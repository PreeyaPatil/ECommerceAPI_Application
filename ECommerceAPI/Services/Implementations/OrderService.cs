using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Helpers;
using ECommerceAPI.Mappings;
using ECommerceAPI.Models;
using ECommerceAPI.Options;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class OrderService : IOrderService
    {
        // Used to retrieve Customer information.
        private readonly ICustomerRepository _customerRepository;

        // Used to retrieve and update the Customer's Shopping Cart.
        private readonly ICartRepository _cartRepository;

        // Used to validate Shipping and Billing Addresses.
        private readonly ICustomerAddressRepository _addressRepository;

        // Used to retrieve Products as tracked Entities
        // so stock quantity can be updated safely.
        private readonly IProductRepository _productRepository;

        // Used to create and retrieve Orders and Order Status History.
        private readonly IOrderRepository _orderRepository;

        // Used to save changes and manage database transactions.
        private readonly IUnitOfWork _unitOfWork;

        // Used to process COD, Card, or UPI payments.
        private readonly IPaymentService _paymentService;

        // Used to queue Email and SMS notifications.
        private readonly INotificationService _notificationService;

        // Contains Tax, Shipping, Free Shipping Threshold,
        // and Currency configuration.
        private readonly PricingOptions _pricingOptions;

        // Used to write application logs.
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ICustomerRepository customerRepository,
            ICartRepository cartRepository,
            ICustomerAddressRepository addressRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            INotificationService notificationService,
            IOptions<PricingOptions> pricingOptions,
            ILogger<OrderService> logger)
        {
            _customerRepository = customerRepository;
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _notificationService = notificationService;

            // Read PricingOptions values from configuration.
            _pricingOptions = pricingOptions.Value;

            _logger = logger;
        }

        public async Task<PlaceOrderResponseDTO> PlaceOrderAsync(int customerId, PlaceOrderRequestDTO request)
        {
            _logger.LogInformation("Order Placement started for Customer {CustomerId}.", customerId);

            // CheckoutToken must be a valid non-empty Guid.
            if (request.CheckoutToken == Guid.Empty)
            {
                _logger.LogWarning(
                    "Order Placement rejected for Customer {CustomerId} because the Checkout Token is invalid.",
                    customerId);

                throw new BusinessException("Invalid Checkout Token.");
            }

            // Load the Customer and make sure the account is active.
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null || !customer.IsActive)
            {
                throw new BusinessException("Customer account is not available.");
            }

            // Check whether this CheckoutToken has already been used to create an Order.
            // This helps prevent duplicate Order creation
            // when the client submits the same request again.
            var existingOrder = await _orderRepository.GetByCheckoutTokenAsync(customerId, request.CheckoutToken);

            if (existingOrder != null)
            {
                _logger.LogInformation(
                    "Existing Order {OrderId} found for Customer {CustomerId}. Duplicate Order creation avoided.",
                    existingOrder.Id,
                    customerId);

                // Return the already-created Order instead of creating another Order.
                return existingOrder.ToPlaceOrderResponseDTO(GetFailureMessage(existingOrder));
            }

            // Begin a database transaction because Order Placement updates several related tables.
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Load the Cart as a tracked Entity because Cart Items may later be cleared.
                var cart = await _cartRepository.GetByCustomerIdForUpdateAsync(customerId);

                // Order cannot be placed with an empty Cart.
                if (cart == null || cart.Items.Count == 0)
                {
                    throw new BusinessException("Shopping Cart is empty.");
                }

                // Make sure the CheckoutToken from the client still matches the current Cart.
                // If the Cart changed after Checkout was prepared,
                // the Checkout information is considered outdated.
                if (cart.CheckoutToken != request.CheckoutToken)
                {
                    throw new BusinessException(
                        "The Checkout information is no longer valid. " +
                        "Please refresh the Checkout page.");
                }

                // Validate the selected Shipping Address.
                var shippingAddress = await _addressRepository.GetByIdAsync(request.ShippingAddressId, customerId);

                if (shippingAddress == null)
                {
                    throw new BusinessException("Invalid Shipping Address.");
                }

                // Validate the selected Billing Address.
                var billingAddress = await _addressRepository.GetByIdAsync(request.BillingAddressId, customerId);

                if (billingAddress == null)
                {
                    throw new BusinessException("Invalid Billing Address.");
                }

                // Collect all distinct Product Ids from the Cart.
                var productIds = cart.Items
                        .Select(x => x.ProductId)
                        .Distinct()
                        .ToList();

                // Load Products as tracked Entities.
                // These Products may have their stock reduced later.
                var products = await _productRepository.GetProductsByIdsForUpdateAsync(productIds);

                // If fewer Products are returned than requested,
                // one or more Products no longer exist.
                if (products.Count != productIds.Count)
                {
                    throw new BusinessException("One or more Products are no longer available.");
                }

                // Revalidate Product status and stock using the latest database values.
                foreach (var cartItem in cart.Items)
                {
                    var product = products.First(x => x.Id == cartItem.ProductId);

                    // Product must still be active.
                    if (!product.IsActive)
                    {
                        throw new BusinessException($"{product.Name} is no longer available.");
                    }

                    // Requested quantity must still be available.
                    if (cartItem.Quantity > product.StockQuantity)
                    {
                        throw new BusinessException(
                            $"Only {product.StockQuantity} unit(s) " +
                            $"of {product.Name} are available.");
                    }
                }

                // Recalculate the Subtotal using the latest
                // Product Prices from the database.
                var subtotal = cart.Items.Sum(cartItem =>
                {
                    var product = products.First(x => x.Id == cartItem.ProductId);
                    return product.Price * cartItem.Quantity;
                });

                // Calculate Tax, Shipping, Grand Total,
                // and Currency using PricingOptions.
                var pricing = PricingHelper.Calculate(subtotal, _pricingOptions);

                // Create the Order using only server-validated
                // and server-calculated values.
                var order =
                    new Order
                    {
                        OrderNumber = OrderNumberGenerator.Generate(),
                        CustomerId = customerId,

                        // Store the CheckoutToken with the Order.
                        // This provides idempotency for Order Placement.
                        CheckoutToken = request.CheckoutToken,
                        PaymentMethodId = request.PaymentMethod,
                        PaymentStatusId = PaymentStatus.Pending,
                        OrderStatusId = OrderStatus.Pending,
                        Subtotal = pricing.Subtotal,
                        TaxAmount = pricing.TaxAmount,
                        ShippingAmount = pricing.ShippingAmount,
                        GrandTotal = pricing.GrandTotal,
                        Currency = pricing.Currency,

                        // Store Address snapshots so future Address
                        // changes do not affect historical Orders.
                        ShippingAddress = BuildAddressSnapshot(shippingAddress),
                        BillingAddress = BuildAddressSnapshot(billingAddress)
                    };

                // Create OrderItem snapshots from the
                // current Product and Cart information.
                foreach (var cartItem in cart.Items)
                {
                    var product = products.First(x => x.Id == cartItem.ProductId);
                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductSku = product.Sku,
                        ProductImageUrl = product.ImageUrl,
                        UnitPrice = product.Price,
                        Quantity = cartItem.Quantity,
                        LineTotal = product.Price * cartItem.Quantity
                    });
                }

                // Add the Order and its OrderItems.
                await _orderRepository.AddAsync(order);

                // Save first so the database generates OrderId.
                // A UNIQUE constraint on CustomerId + CheckoutToken
                // can also protect against concurrent duplicate requests.
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Order {OrderId} created for Customer {CustomerId}.",
                    order.Id, customerId);

                // Process the selected Payment Method.
                var paymentResult = await _paymentService.ProcessPaymentAsync(
                            order.Id,
                            request.PaymentMethod,
                            order.GrandTotal,
                            order.Currency);

                _logger.LogInformation(
                    "Payment processing completed for Order {OrderId} with Status {PaymentStatus}.",
                    order.Id, paymentResult.Status);

                string? failureMessage = null;

                // Update Order and Payment status based on the Payment Result.
                switch (paymentResult.Status)
                {
                    case PaymentStatus.Paid:

                        // Online payment completed successfully.
                        order.PaymentStatusId = PaymentStatus.Paid;
                        order.OrderStatusId = OrderStatus.Confirmed;
                        // Reduce stock because the Order is confirmed.
                        ReduceStock(cart, products);

                        // Clear Cart because the Order is complete.
                        ClearCart(cart);
                        break;

                    case PaymentStatus.Pending:
                        order.PaymentStatusId = PaymentStatus.Pending;

                        // COD has Pending payment because the Customer
                        // will pay later, but the Order itself is confirmed.
                        if (request.PaymentMethod == PaymentMethod.CashOnDelivery)
                        {
                            order.OrderStatusId = OrderStatus.Confirmed;
                            // Reserve the purchase by reducing stock.
                            ReduceStock(cart, products);
                        }
                        else
                        {
                            // Online payment is still pending,
                            // so the Order remains Pending.
                            order.OrderStatusId = OrderStatus.Pending;
                        }

                        // The Order already exists, so clear the Cart.
                        ClearCart(cart);
                        break;

                    case PaymentStatus.Failed:
                        // Online payment failed.
                        order.PaymentStatusId = PaymentStatus.Failed;
                        order.OrderStatusId = OrderStatus.PaymentFailed;

                        failureMessage = paymentResult.FailureReason ?? "Payment failed.";

                        // Do not reduce stock.
                        // Do not clear the Cart.
                        // Payment retry should happen against this existing Order.
                        break;

                    default:
                        throw new BusinessException("Invalid Payment Status.");
                }

                // Create the first Order Status History record.
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    PreviousStatusId = null,
                    NewStatusId = order.OrderStatusId,
                    Remarks = "Order placed."
                };

                // Store Order Status History.
                await _orderRepository.AddStatusHistoryAsync(statusHistory);

                // Save Order Status, stock changes, Cart changes, and Status History.
                await _unitOfWork.SaveChangesAsync();

                // Queue Email and SMS notifications.
                await QueueOrderNotificationAsync(customer, order);

                _logger.LogInformation("Order Notifications queued for Order {OrderId}.", order.Id);

                // Commit all database changes.
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Order {OrderId} completed with Order Status {OrderStatus} and Payment Status {PaymentStatus}.",
                    order.Id,
                    order.OrderStatusId,
                    order.PaymentStatusId);

                // Return the Order Placement result.
                return order.ToPlaceOrderResponseDTO(failureMessage);
            }
            catch (DbUpdateException)
            {
                _logger.LogWarning(
                    "Database update conflict occurred while placing an Order for Customer {CustomerId}. Transaction is being rolled back.", customerId);

                // Roll back all changes from this transaction.
                await _unitOfWork.RollbackTransactionAsync();

                // Another request may have created the same Order
                // using the same CheckoutToken at the same time.
                var duplicateOrder = await _orderRepository.GetByCheckoutTokenAsync(customerId, request.CheckoutToken);

                if (duplicateOrder != null)
                {
                    // Return the already-created Order instead of treating it as a duplicate failure.
                    return duplicateOrder.ToPlaceOrderResponseDTO(GetFailureMessage(duplicateOrder));
                }

                // If it was not a duplicate Order,
                // propagate the original database exception.
                throw;
            }
            catch
            {
                _logger.LogError(
                    "Order Placement failed for Customer {CustomerId}. Transaction is being rolled back.",
                    customerId);

                // Roll back all changes when any other error occurs.
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }

        public async Task<PagedResult<OrderListItemResponseDTO>> GetOrderHistoryAsync(int customerId, OrderHistoryRequestDTO request)
        {
            _logger.LogInformation("Retrieving Order History for Customer {CustomerId}.", customerId);

            // Retrieve the Customer's Orders using paging.
            var result = await _orderRepository.GetCustomerOrdersAsync(customerId, request.PageNumber, request.PageSize);

            // Convert paged Order Entities into paged Order History Response DTOs.
            return result.ToOrderHistoryPagedResult();
        }

        public async Task<OrderDetailsResponseDTO> GetOrderDetailsAsync(int customerId, int orderId)
        {
            _logger.LogInformation("Retrieving Order {OrderId} for Customer {CustomerId}.",
                orderId, customerId);

            // Retrieve complete Order details only if the Order belongs to the Customer.
            var order = await _orderRepository.GetDetailsAsync(orderId, customerId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} was not found for Customer {CustomerId}.", orderId, customerId);
                throw new NotFoundException("Order was not found.");
            }

            _logger.LogInformation(
                "Order {OrderId} retrieved successfully for Customer {CustomerId}.",
                orderId, customerId);

            // Convert the complete Order into OrderDetailsResponseDTO.
            return order.ToDetailsResponseDTO();
        }

        private async Task QueueOrderNotificationAsync(Customer customer, Order order)
        {
            // Prepare the Email body containing
            // Order, Payment, and Total information.
            var emailBody = $"""
                <h3>Order Update</h3>

                <p>Hello {customer.FirstName},</p>

                <p>
                    Your Order
                    <strong>{order.OrderNumber}</strong>
                    has been created successfully.
                </p>

                <p>
                    Order Status:
                    <strong>{order.OrderStatusId}</strong>
                </p>

                <p>
                    Payment Status:
                    <strong>{order.PaymentStatusId}</strong>
                </p>

                <p>
                    Grand Total:
                    <strong>
                        {order.Currency} {order.GrandTotal:F2}
                    </strong>
                </p>
                """;

            // Prepare a shorter SMS message.
            var smsBody =
                $"Order {order.OrderNumber} created. " +
                $"Order Status: {order.OrderStatusId}. " +
                $"Payment Status: {order.PaymentStatusId}. " +
                $"Total: {order.Currency} " +
                $"{order.GrandTotal:F2}.";

            // Add both Email and SMS messages
            // to the Notification Queue.
            await _notificationService.QueueEmailAndSmsAsync(
                    customer.Email,
                    customer.PhoneNumber,
                    $"Order {order.OrderNumber}",
                    emailBody,
                    smsBody);
        }

        private static void ReduceStock(Cart cart, List<Product> products)
        {
            // Reduce stock for every Product included in the confirmed Order.
            foreach (var cartItem in cart.Items)
            {
                var product = products.First(x => x.Id == cartItem.ProductId);
                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        private void ClearCart(Cart cart)
        {
            // Remove all Cart Items.
            _cartRepository.ClearItems(cart.Items.ToList());

            // Generate a new CheckoutToken because
            // the current Checkout has finished.
            // This prevents the old token from being reused.
            cart.CheckoutToken = Guid.NewGuid();
            cart.UpdatedAtUtc = DateTime.UtcNow;
        }

        private static string? GetFailureMessage(Order order)
        {
            // Failure message is needed only when Payment Status is Failed.
            if (order.PaymentStatusId != PaymentStatus.Failed)
            {
                return null;
            }

            // Return the most recent non-empty Payment failure reason.
            return order.Payments
                .OrderByDescending(payment => payment.CreatedAtUtc)
                .Select(payment => payment.FailureReason)
                .FirstOrDefault(failureReason => !string.IsNullOrWhiteSpace(failureReason));
        }

        private static string BuildAddressSnapshot(CustomerAddress address)
        {
            // Build a single formatted Address string
            // that will be stored permanently with the Order.
            var parts = new List<string>
            {
                address.FullName,
                address.PhoneNumber,
                address.AddressLine1
            };

            // AddressLine2 is optional.
            if (!string.IsNullOrWhiteSpace(address.AddressLine2))
            {
                parts.Add(address.AddressLine2);
            }

            parts.Add(address.City);
            parts.Add(address.State);
            parts.Add(address.PostalCode);
            parts.Add(address.Country);

            // Join all Address parts into one snapshot string.
            return string.Join(", ", parts);
        }
    }
}
