using Microsoft.Extensions.Logging;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.Interfaces.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Services.Payments
{
    public class CashPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<CashPaymentStrategy> _logger;

        public CashPaymentStrategy(ILogger<CashPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public string GetProviderName() => "Cash";

        public async Task<PaymentInitializationResult> InitializeAsync(
            PaymentInitializationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Initializing Cash payment for Booking: {request.BookingId}");

                // For cash payments - no external gateway needed
                return new PaymentInitializationResult
                {
                    IsSuccess = true,
                    TransactionId = $"CASH_{Guid.NewGuid()}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                    PaymentLink = null, // No online payment link
                    PaymentToken = null,
            
                    MetaData = new Dictionary<string, object>
                    {
                        { "PaymentType", "Cash" },
                        { "Amount", request.Amount },
                        { "CraftsmanName", request.CraftsmanName },
                        { "BookingDate", request.BookingDate }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Cash payment");
                return new PaymentInitializationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Cash payment initialization failed: {ex.Message}"
                };
            }
        }

        public async Task<PaymentVerificationResult> VerifyAsync(
            string transactionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Verifying Cash transaction: {transactionId}");

                // For cash payments, we assume payment is confirmed when craftsman marks it as paid
                return new PaymentVerificationResult
                {
                    IsSuccess = true,
                    IsPaymentConfirmed = false, // Will be confirmed by craftsman later
                    TransactionId = transactionId,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying Cash transaction: {transactionId}");
                return new PaymentVerificationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Cash verification failed: {ex.Message}"
                };
            }
        }

        public async Task<RefundResult> RefundAsync(
            RefundRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Processing Cash refund for transaction: {request.OriginalTransactionId}");

                // For cash payments, refunds are handled manually
                return new RefundResult
                {
                    IsSuccess = true,
                    RefundTransactionId = $"CASH_REFUND_{Guid.NewGuid()}",
                    RefundAmount = request.RefundAmount,
                    RefundDate = DateTime.UtcNow,
                    
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Cash refund");
                return new RefundResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Cash refund failed: {ex.Message}"
                };
            }
        }
    }
}
