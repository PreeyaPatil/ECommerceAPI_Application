using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class OrderHistoryRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page Number must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page Size must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
