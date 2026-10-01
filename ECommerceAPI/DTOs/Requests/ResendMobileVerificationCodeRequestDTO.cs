using System.ComponentModel.DataAnnotations;
namespace ECommerceAPI.DTOs.Requests
{
    public sealed class ResendMobileVerificationCodeRequestDTO
    {
        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^\+?[1-9]\d{7,14}$", ErrorMessage = "Please provide a valid Mobile Number.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
