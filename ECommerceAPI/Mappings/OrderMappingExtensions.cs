using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
using ECommerceAPI.Models;

namespace ECommerceAPI.Mappings
{
    public static class OrderMappingExtensions
    {
        // Converts an Order entity into a pricing summary response.
        public static PricingSummaryResponseDTO ToPricingSummaryResponseDTO(this Order order)
        {
            return new PricingSummaryResponseDTO
            {
                // Copy the complete pricing information from the order.
                Subtotal = order.Subtotal,
                TaxAmount = order.TaxAmount,
                ShippingAmount = order.ShippingAmount,
                GrandTotal = order.GrandTotal,
                Currency = order.Currency
            };
        }

        // Converts an Order entity into the response returned after placing an order.
        public static PlaceOrderResponseDTO ToPlaceOrderResponseDTO(
            this Order order, 
            string? failureMessage = null)
        {
            return new PlaceOrderResponseDTO
            {
                // Basic order information.
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,

                // Use the order status name when available;
                // otherwise, use the status ID as a fallback.
                OrderStatus =
                    order.OrderStatus?.Name
                    ?? order.OrderStatusId.ToString(),

                // Use the payment status name when available;
                // otherwise, use the status ID as a fallback.
                PaymentStatus =
                    order.PaymentStatus?.Name
                    ?? order.PaymentStatusId.ToString(),

                // Order total and currency.
                GrandTotal = order.GrandTotal,
                Currency = order.Currency,

                // Include the failure message if order placement was unsuccessful.
                FailureMessage = failureMessage
            };
        }

        // Converts an Order entity into a lightweight DTO used in order history.
        public static OrderListItemResponseDTO ToListItemResponseDTO(this Order order)
        {
            return new OrderListItemResponseDTO
            {
                // Basic order information.
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDateUtc = order.CreatedAtUtc,

                // Calculate the total number of product units in the order.
                TotalItems = order.Items.Sum(item => item.Quantity),

                // Order amount information.
                GrandTotal = order.GrandTotal,
                Currency = order.Currency,

                // Get the payment method name, with ID as a fallback.
                PaymentMethod =
                    order.PaymentMethod?.Name
                    ?? order.PaymentMethodId.ToString(),

                // Get the payment status name, with ID as a fallback.
                PaymentStatus =
                    order.PaymentStatus?.Name
                    ?? order.PaymentStatusId.ToString(),

                // Get the order status name, with ID as a fallback.
                OrderStatus =
                    order.OrderStatus?.Name
                    ?? order.OrderStatusId.ToString()
            };
        }

        // Converts an OrderItem entity into an OrderItemResponseDTO.
        public static OrderItemResponseDTO ToResponseDTO(this OrderItem orderItem)
        {
            return new OrderItemResponseDTO
            {
                // Copy the product snapshot stored with the order item.
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName,
                ProductSku = orderItem.ProductSku,
                ProductImageUrl = orderItem.ProductImageUrl,

                // Copy the price and quantity information.
                UnitPrice = orderItem.UnitPrice,
                Quantity = orderItem.Quantity,
                LineTotal = orderItem.LineTotal
            };
        }

        // Converts an OrderStatusHistory entity into a response DTO.
        public static OrderStatusHistoryResponseDTO ToResponseDTO(this OrderStatusHistory history)
        {
            return new OrderStatusHistoryResponseDTO
            {
                Id = history.Id,

                // Previous status can be null, for example when the order is first created.
                PreviousStatus = history.PreviousStatus?.Name,

                // Get the new status name, with ID as a fallback.
                NewStatus =
                    history.NewStatus?.Name
                    ?? history.NewStatusId.ToString(),

                // Include remarks describing the status change.
                Remarks = history.Remarks,

                // Date and time when the order status was changed.
                ChangedAtUtc = history.CreatedAtUtc
            };
        }

        // Converts an Order entity into a complete order details response.
        public static OrderDetailsResponseDTO ToDetailsResponseDTO(this Order order)
        {
            return new OrderDetailsResponseDTO
            {
                // Basic order information.
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDateUtc = order.CreatedAtUtc,

                // Payment method information.
                PaymentMethod =
                    order.PaymentMethod?.Name
                    ?? order.PaymentMethodId.ToString(),

                // Current payment status.
                PaymentStatus =
                    order.PaymentStatus?.Name
                    ?? order.PaymentStatusId.ToString(),

                // Current order status.
                OrderStatus =
                    order.OrderStatus?.Name
                    ?? order.OrderStatusId.ToString(),

                // Convert the order pricing into a pricing summary DTO.
                Pricing = order.ToPricingSummaryResponseDTO(),

                // Include the shipping and billing address snapshots stored with the order.
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,

                // Convert all order items into response DTOs.
                Items = order.Items
                    .Select(item => item.ToResponseDTO())
                    .ToList(),

                // Convert all payment transactions into response DTOs.
                Payments = order.Payments
                    .Select(payment => payment.ToResponseDTO())
                    .ToList(),

                // Convert the complete order status change history into response DTOs.
                StatusHistory = order.StatusHistories
                    .Select(history => history.ToResponseDTO())
                    .ToList()
            };
        }

        // Converts a paginated collection of Order entities into
        // a paginated collection of OrderListItemResponseDTO objects.
        public static PagedResult<OrderListItemResponseDTO> ToOrderHistoryPagedResult(this PagedResult<Order> pagedResult)
        {
            return new PagedResult<OrderListItemResponseDTO>
            {
                // Preserve the pagination information.
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,

                // Convert each Order entity into an order history list item DTO.
                Items = pagedResult.Items
                    .Select(order => order.ToListItemResponseDTO())
                    .ToList()
            };
        }
    }
}
