using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class UpdateCartItemQuantityRequestDTO
    {
        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
        public int Quantity { get; set; }
    }
}
