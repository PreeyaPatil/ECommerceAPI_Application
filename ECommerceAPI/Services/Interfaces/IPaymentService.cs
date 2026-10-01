using ECommerceAPI.Enums;
using ECommerceAPI.Models;
namespace ECommerceAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(
            int orderId,
            PaymentMethod paymentMethod,
            decimal amount,
            string currency);
    }
}
