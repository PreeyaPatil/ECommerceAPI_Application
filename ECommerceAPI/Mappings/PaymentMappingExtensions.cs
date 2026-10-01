using ECommerceAPI.DTOs.Responses;
using ECommerceAPI.Entities;
namespace ECommerceAPI.Mappings
{
    public static class PaymentMappingExtensions
    {
        // Converts a PaymentMethodMaster entity into a PaymentMethodResponseDTO.
        public static PaymentMethodResponseDTO ToResponseDTO(this PaymentMethodMaster paymentMethod)
        {
            return new PaymentMethodResponseDTO
            {
                // Convert the payment method ID to an integer value.
                Id = (int)paymentMethod.Id,

                // Copy the payment method details.
                Name = paymentMethod.Name,
                Description = paymentMethod.Description,

                // Indicates whether this payment method requires online payment processing.
                RequiresOnlinePayment = paymentMethod.RequiresOnlinePayment
            };
        }

        // Converts a PaymentTransaction entity into a PaymentTransactionResponseDTO.
        public static PaymentTransactionResponseDTO ToResponseDTO(this PaymentTransaction payment)
        {
            return new PaymentTransactionResponseDTO
            {
                Id = payment.Id,

                // Use the related payment method name when available.
                // Otherwise, fall back to the payment method ID.
                PaymentMethod =
                    payment.PaymentMethod?.Name
                    ?? payment.PaymentMethodId.ToString(),

                // Use the related payment status name when available.
                // Otherwise, fall back to the payment status ID.
                PaymentStatus =
                    payment.PaymentStatus?.Name
                    ?? payment.PaymentStatusId.ToString(),

                // Payment provider details.
                Provider = payment.Provider,
                ProviderTransactionId = payment.ProviderTransactionId,

                // Payment amount and currency.
                Amount = payment.Amount,
                Currency = payment.Currency,

                // Masked payment instrument information, such as card details.
                MaskedInstrument = payment.MaskedInstrument,

                // Contains the reason when the payment fails.
                FailureReason = payment.FailureReason,

                // Date and time when the payment was completed.
                CompletedAtUtc = payment.CompletedAtUtc,

                // Date and time when the payment transaction was created.
                CreatedAtUtc = payment.CreatedAtUtc
            };
        }
    }
}
