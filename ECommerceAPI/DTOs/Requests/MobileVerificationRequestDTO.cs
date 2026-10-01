using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class MobileVerificationRequestDTO
    {
        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^\+?[1-9]\d{7,14}$", ErrorMessage = "Please provide a valid Mobile Number.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification Code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification Code must contain exactly 6 digits.")]
        public string Code { get; set; } = string.Empty;
    }
}
