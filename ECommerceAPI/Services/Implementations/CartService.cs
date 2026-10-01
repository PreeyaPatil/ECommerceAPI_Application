using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Mappings;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class CartService : ICartService
    {
        // Used to retrieve and update Shopping Cart data.
        private readonly ICartRepository _cartRepository;

        // Used to retrieve Product information such as
        // availability, status, and stock quantity.
        private readonly IProductRepository _productRepository;

        // Used to save all database changes.
        private readonly IUnitOfWork _unitOfWork;

        // Used to write application logs.
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CartResponseDTO> GetCartAsync(int customerId)
        {
            // Retrieve the Customer's Cart for display.
            var cart = await _cartRepository.GetByCustomerIdAsync(customerId);

            // If the Customer does not have a Cart yet, return an empty Cart response.
            if (cart == null)
            {
                return new CartResponseDTO
                {
                    CartId = 0,
                    TotalItems = 0,
                    CartTotal = 0
                };
            }

            // Convert the Cart Entity into CartResponseDTO.
            return cart.ToResponseDTO();
        }

        public async Task<CartResponseDTO> AddToCartAsync(
            int customerId, 
            AddToCartRequestDTO request)
        {
            _logger.LogInformation(
                "Adding Product {ProductId} with Quantity {Quantity} to Customer {CustomerId} Cart.",
                request.ProductId,
                request.Quantity,
                customerId);

            // Retrieve the Product requested by the Customer.
            var product = await _productRepository.GetByIdAsync(request.ProductId);

            // Product must exist and be active.
            if (product == null || !product.IsActive)
            {
                throw new NotFoundException("Product was not found.");
            }

            // Product must have available stock.
            if (product.StockQuantity <= 0)
            {
                throw new BusinessException("Product is currently out of stock.");
            }

            // Retrieve the Customer's Cart as a tracked Entity
            // because we may modify the Cart or its Items.
            var cart = await _cartRepository.GetByCustomerIdForUpdateAsync(customerId);

            // Create a new Cart if the Customer does not have one yet.
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,

                    // CheckoutToken changes whenever the Cart changes.
                    // It can later be used to detect an outdated Checkout.
                    CheckoutToken = Guid.NewGuid()
                };

                await _cartRepository.AddAsync(cart);

                // Save first so that the database generates
                // the Cart Id before adding Cart Items.
                await _unitOfWork.SaveChangesAsync();
            }

            // Check whether the selected Product already exists inside the Cart.
            var cartItem = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (cartItem == null)
            {
                // For a new Cart Item, make sure the requested
                // quantity does not exceed the available stock.
                if (request.Quantity > product.StockQuantity)
                {
                    throw new BusinessException($"Only {product.StockQuantity} unit(s) are available.");
                }

                // Convert the AddToCart Request DTO into a CartItem Entity
                // and add it to the Cart.
                cart.Items.Add(request.ToEntity(cart.Id));
            }
            else
            {
                // If the Product already exists in the Cart, increase the existing quantity.
                var newQuantity = cartItem.Quantity + request.Quantity;

                // Validate the new total quantity against the available stock.
                if (newQuantity > product.StockQuantity)
                {
                    throw new BusinessException($"Only {product.StockQuantity} unit(s) are available.");
                }

                // Update the existing Cart Item quantity.
                cartItem.Quantity = newQuantity;
            }

            // Generate a new CheckoutToken because the Cart contents have changed.
            RefreshCheckoutToken(cart);

            // Save the Cart changes.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Product {ProductId} added or updated successfully in Customer {CustomerId} Cart.",
                request.ProductId,
                customerId);

            // Reload the Cart with all required navigation data
            // so that a complete response can be returned.
            var updatedCart = await _cartRepository.GetByCustomerIdAsync(customerId);

            // Convert the updated Cart into the response DTO.
            return updatedCart!.ToResponseDTO();
        }

        public async Task<CartResponseDTO> UpdateQuantityAsync(
            int customerId,
            int cartItemId,
            UpdateCartItemQuantityRequestDTO request)
        {
            _logger.LogInformation(
                "Updating Cart Item {CartItemId} for Customer {CustomerId}.",
                cartItemId,
                customerId);

            // Retrieve the Cart as a tracked Entity
            // because the Cart Item quantity will be changed.
            var cart = await _cartRepository.GetByCustomerIdForUpdateAsync(customerId);

            if (cart == null)
            {
                throw new NotFoundException("Shopping Cart was not found.");
            }

            // Find the requested Cart Item inside the authenticated Customer's Cart.
            var cartItem = cart.Items.FirstOrDefault(x => x.Id == cartItemId);

            if (cartItem == null)
            {
                throw new NotFoundException("Cart Item was not found.");
            }

            // The Product must still be active.
            if (!cartItem.Product.IsActive)
            {
                throw new BusinessException($"{cartItem.Product.Name} is no longer available.");
            }

            // Requested quantity must not exceed the current available stock.
            if (request.Quantity > cartItem.Product.StockQuantity)
            {
                throw new BusinessException($"Only {cartItem.Product.StockQuantity} unit(s) are available.");
            }

            // Update the Cart Item quantity.
            cartItem.Quantity = request.Quantity;

            // Refresh the CheckoutToken because the Cart quantity has changed.
            RefreshCheckoutToken(cart);

            // Save the changes.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Cart Item {CartItemId} updated successfully for Customer {CustomerId}.",
                cartItemId,
                customerId);

            // Reload the Cart for the latest response data.
            var updatedCart = await _cartRepository.GetByCustomerIdAsync(customerId);

            return updatedCart!.ToResponseDTO();
        }

        public async Task RemoveItemAsync(int customerId, int cartItemId)
        {
            // Retrieve the Customer's Cart for update.
            var cart = await _cartRepository.GetByCustomerIdForUpdateAsync(customerId);

            if (cart == null)
            {
                throw new NotFoundException("Shopping Cart was not found.");
            }

            // Find the Cart Item that should be removed.
            var cartItem = cart.Items.FirstOrDefault(x => x.Id == cartItemId);

            if (cartItem == null)
            {
                throw new NotFoundException("Cart Item was not found.");
            }

            // Mark the Cart Item for deletion.
            _cartRepository.RemoveItem(cartItem);

            // Refresh the CheckoutToken because the Cart contents have changed.
            RefreshCheckoutToken(cart);

            // Save the deletion.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Cart Item {CartItemId} removed successfully for Customer {CustomerId}.",
                cartItemId,
                customerId);
        }

        public async Task ClearCartAsync(int customerId)
        {
            // Retrieve the Customer's Cart for update.
            var cart = await _cartRepository.GetByCustomerIdForUpdateAsync(customerId);

            // If there is no Cart or the Cart is already empty, there is nothing to clear.
            if (cart == null || cart.Items.Count == 0)
            {
                return;
            }

            // Mark all Cart Items for deletion.
            _cartRepository.ClearItems(cart.Items.ToList());

            // Refresh the CheckoutToken because the Cart contents have changed.
            RefreshCheckoutToken(cart);

            // Save all deletions.
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Shopping Cart cleared successfully for Customer {CustomerId}.",
                customerId);
        }

        private static void RefreshCheckoutToken(Cart cart)
        {
            // Generate a new token whenever the Cart changes.
            // This helps identify whether a previously prepared
            // Checkout is still based on the current Cart state.
            cart.CheckoutToken = Guid.NewGuid();

            // Record when the Cart was last modified.
            cart.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
