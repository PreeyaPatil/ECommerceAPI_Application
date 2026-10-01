using ECommerceAPI.Enums;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IPaymentSimulator
    {
        Task<PaymentResult> ProcessAsync(PaymentMethod paymentMethod, decimal amount, string currency);
    }
}
