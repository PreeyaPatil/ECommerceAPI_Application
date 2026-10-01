using ECommerceAPI.Enums;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class PaymentSimulator : IPaymentSimulator
    {
        // Used to write payment simulation logs.
        private readonly ILogger<PaymentSimulator> _logger;

        public PaymentSimulator(ILogger<PaymentSimulator> logger)
        {
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessAsync(
            PaymentMethod paymentMethod, 
            decimal amount, 
            string currency)
        {
            // Log the payment method, amount, and currency
            // before starting the payment simulation.
            _logger.LogInformation(
                "Simulating Payment using {PaymentMethod} for Amount {Amount} {Currency}.",
                paymentMethod, amount, currency);

            // Simulate the time normally required to communicate with a real Payment Gateway.
            await Task.Delay(TimeSpan.FromSeconds(1));

            // Generate a random value between 1 and 100
            // to decide the simulated payment result.
            var randomValue = Random.Shared.Next(1, 101);

            // Simulate the Payment Status:
            // 1 - 80   => Paid
            // 81 - 90  => Pending
            // 91 - 100 => Failed
            var status =
                randomValue <= 80
                    ? PaymentStatus.Paid
                    : randomValue <= 90 ? PaymentStatus.Pending : PaymentStatus.Failed;

            // Create the simulated Payment Result.
            var result = new PaymentResult
            {
                // Store the simulated payment status.
                Status = status,

                // Name of the simulated payment provider.
                Provider = "ECommerce Payment Simulator",

                // Generate a unique Provider Transaction Id
                // similar to what a real Payment Gateway would return.
                ProviderTransactionId = $"SIM-{Guid.NewGuid():N}".ToUpperInvariant(),

                // Store only safe and masked payment information.
                // Never store the actual Card Number or UPI PIN.
                MaskedInstrument =
                    paymentMethod == PaymentMethod.Card
                        ? "**** **** **** 1234"
                        : "d*****@upi",

                // Failure reason is populated only
                // when the simulated payment fails.
                FailureReason = status == PaymentStatus.Failed ? "Simulated payment failure." : null
            };

            // Log the final simulated Payment Status.
            _logger.LogInformation("Payment simulation completed with Status {PaymentStatus}.", result.Status);

            // Return the simulated payment result to the calling Service.
            return result;
        }
    }
}
