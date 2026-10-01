using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "Refresh Token is required.")]
        [StringLength(1000, ErrorMessage = "Refresh Token is invalid.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
