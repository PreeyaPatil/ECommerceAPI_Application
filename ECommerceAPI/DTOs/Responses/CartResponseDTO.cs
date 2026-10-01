namespace ECommerceAPI.DTOs.Responses
{
    public sealed class CartResponseDTO
    {
        public int CartId { get; set; }
        public int TotalItems { get; set; }
        public decimal CartTotal { get; set; }
        public List<CartItemResponseDTO> Items { get; set; } = new List<CartItemResponseDTO>();
    }
}
