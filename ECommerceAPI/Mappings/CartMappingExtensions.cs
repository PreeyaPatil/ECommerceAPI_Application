using ECommerceAPI.DTOs.Requests;
using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;

namespace ECommerceAPI.Mappings
{
    public static class CartMappingExtensions
    {
        // Converts an AddToCartRequestDTO into a new CartItem entity.
        public static CartItem ToEntity(this AddToCartRequestDTO request, int cartId)
        {
            return new CartItem
            {
                // Associate the new cart item with the specified cart.
                CartId = cartId,

                // Copy the selected product and quantity from the request.
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };
        }

        // Converts a CartItem entity into a response DTO.
        public static CartItemResponseDTO ToCartItemResponseDTO(this CartItem cartItem)
        {
            return new CartItemResponseDTO
            {
                // Cart item and product information.
                CartItemId = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                ProductImageUrl = cartItem.Product.ImageUrl,

                // Use the current product price as the unit price.
                UnitPrice = cartItem.Product.Price,

                Quantity = cartItem.Quantity,

                // Calculate the total amount for this cart item.
                LineTotal = cartItem.Product.Price * cartItem.Quantity,

                // Include the currently available product stock.
                AvailableStock = cartItem.Product.StockQuantity
            };
        }

        // Converts a Cart entity into a complete CartResponseDTO.
        public static CartResponseDTO ToResponseDTO(this Cart cart)
        {
            // Convert all CartItem entities into CartItemResponseDTO objects.
            var items = cart.Items
                .Select(item => item.ToCartItemResponseDTO())
                .ToList();

            return new CartResponseDTO
            {
                CartId = cart.Id,

                // Calculate the total number of product units in the cart.
                TotalItems = items.Sum(item => item.Quantity),

                // Calculate the total price of all items in the cart.
                CartTotal = items.Sum(item => item.LineTotal),

                // Add the mapped cart items to the response.
                Items = items
            };
        }

        // Converts a CartItem entity into a DTO used on the checkout page.
        public static CheckoutItemResponseDTO ToCheckoutItemResponseDTO(this CartItem cartItem)
        {
            return new CheckoutItemResponseDTO
            {
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                ProductImageUrl = cartItem.Product.ImageUrl,

                // Use the current product price during checkout preparation.
                UnitPrice = cartItem.Product.Price,

                Quantity = cartItem.Quantity,

                // Calculate the total price for this checkout item.
                LineTotal = cartItem.Product.Price * cartItem.Quantity
            };
        }
    }
}

