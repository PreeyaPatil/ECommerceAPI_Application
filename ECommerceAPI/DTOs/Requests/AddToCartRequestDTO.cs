using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class AddToCartRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be greater than 0.")]
        public int ProductId { get; set; }

        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
        public int Quantity { get; set; } = 1;
    }
}
