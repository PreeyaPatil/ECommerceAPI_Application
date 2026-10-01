using ECommerceAPI.Entities;
using ECommerceAPI.Enums;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class PaymentService : IPaymentService
    {
        // Used to retrieve Payment Method information
        // and store PaymentTransaction records.
        private readonly IPaymentRepository _paymentRepository;

        // Used to save database changes.
        private readonly IUnitOfWork _unitOfWork;

        // Used to simulate Card and UPI payment results.
        private readonly IPaymentSimulator _paymentSimulator;

        // Used to write application logs.
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            IPaymentSimulator paymentSimulator,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _paymentSimulator = paymentSimulator;
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            int orderId,
            PaymentMethod paymentMethod,
            decimal amount,
            string currency)
        {
            // Log the start of the payment-processing operation.
            _logger.LogInformation(
                "Processing Payment for Order {OrderId} using {PaymentMethod}.",
                orderId, paymentMethod);

            // Retrieve the selected Payment Method from master data.
            var paymentMethodMaster = await _paymentRepository.GetPaymentMethodAsync(paymentMethod);

            // The Payment Method must exist and be active.
            if (paymentMethodMaster == null || !paymentMethodMaster.IsActive)
            {
                _logger.LogWarning(
                    "Payment processing failed for Order {OrderId}. Payment Method {PaymentMethod} is unavailable.",
                    orderId, paymentMethod);

                throw new BusinessException("Selected Payment Method is not available.");
            }

            // Create the PaymentTransaction with Pending status first.
            // This gives us a record of the payment attempt before the final result is known.
            var paymentTransaction = new PaymentTransaction
            {
                OrderId = orderId,
                PaymentMethodId = paymentMethod,

                // Every payment attempt starts as Pending.
                PaymentStatusId = PaymentStatus.Pending,

                // For online payments, we use the simulator.
                // For COD, no online provider is required.
                Provider = paymentMethodMaster.RequiresOnlinePayment
                            ? "ECommerce Payment Simulator"
                            : "Cash On Delivery",

                Amount = amount,
                Currency = currency
            };

            // Add the PaymentTransaction to the DbContext.
            await _paymentRepository.AddAsync(paymentTransaction);

            // Save first so the payment attempt is recorded.
            await _unitOfWork.SaveChangesAsync();

            // Cash On Delivery does not require online payment processing.
            if (!paymentMethodMaster.RequiresOnlinePayment)
            {
                _logger.LogInformation("Cash On Delivery Payment recorded as Pending for Order {OrderId}.", orderId);

                // COD remains Pending until payment is collected later.
                return new PaymentResult
                {
                    Status = PaymentStatus.Pending,
                    Provider = "Cash On Delivery"
                };
            }

            // Process Card or UPI payment using
            // our simulated Payment Gateway.
            var paymentResult =
                await _paymentSimulator.ProcessAsync(paymentMethod, amount, currency);

            _logger.LogInformation(
                "Payment processing completed for Order {OrderId} with Status {PaymentStatus}.",
                orderId, paymentResult.Status);

            // Update the PaymentTransaction with the
            // result returned by the Payment Simulator.
            paymentTransaction.PaymentStatusId = paymentResult.Status;
            paymentTransaction.Provider = paymentResult.Provider;
            paymentTransaction.ProviderTransactionId = paymentResult.ProviderTransactionId;
            paymentTransaction.MaskedInstrument = paymentResult.MaskedInstrument;
            paymentTransaction.FailureReason = paymentResult.FailureReason;

            // Paid and Failed are final payment states, so record the completion time.
            // Pending is not completed yet, therefore CompletedAtUtc remains null.
            if (paymentResult.Status == PaymentStatus.Paid || paymentResult.Status == PaymentStatus.Failed)
            {
                paymentTransaction.CompletedAtUtc = DateTime.UtcNow;
            }

            // Save the final PaymentTransaction status
            // and related provider information.
            await _unitOfWork.SaveChangesAsync();

            // Return the payment result to the calling Service.
            return paymentResult;
        }
    }
}
