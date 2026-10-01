namespace ECommerceAPI.DTOs.Responses
{
    public sealed class LoginResponseDTO
    {
        public bool RequiresTwoFactor { get; set; }
        public CustomerResponseDTO? Customer { get; set; }
        public TokenResponseDTO? Tokens { get; set; }
    }
}
