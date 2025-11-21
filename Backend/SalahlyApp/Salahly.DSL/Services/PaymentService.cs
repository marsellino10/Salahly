using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Payments;
using System.Security.Cryptography;
using System.Text;

namespace Salahly.DSL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentStrategyFactory _paymentStrategyFactory;
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymentStrategyFactory paymentStrategyFactory,
            IBookingService bookingService,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _paymentStrategyFactory = paymentStrategyFactory;
            _bookingService = bookingService;
            _logger = logger;
            _configuration = configuration;
        }

        // ---------------------------------------------------------------------
        // VERIFY PAYMENT
        // ---------------------------------------------------------------------
        public async Task<ServiceResponse<PaymentVerificationResult>> VerifyPaymentAsync(
            string transactionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Verifying payment: {transactionId}");

                // Get payment from DB
                var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(transactionId);
                if (payment == null)
                    return ServiceResponse<PaymentVerificationResult>.FailureResponse("Payment not found");

                // Get correct strategy based on payment method
                var strategy = _paymentStrategyFactory.GetStrategy(payment.PaymentMethod);

                var verificationResult = await strategy.VerifyAsync(transactionId, cancellationToken);

                if (!verificationResult.IsSuccess)
                {
                    return ServiceResponse<PaymentVerificationResult>
                        .FailureResponse($"Verification failed: {verificationResult.ErrorMessage}");
                }

                return ServiceResponse<PaymentVerificationResult>
                    .SuccessResponse(verificationResult, "Payment verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return ServiceResponse<PaymentVerificationResult>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }

        // ---------------------------------------------------------------------
        // PROCESS WEBHOOK
        // ---------------------------------------------------------------------
        public async Task<ServiceResponse<bool>> ProcessWebhookAsync(
            PaymobWebhookDto webhookData,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Processing webhook for transaction: {webhookData.TransactionId}");

                // 1. Verify HMAC
                if (!VerifyHmacSignature(webhookData))
                {
                    return ServiceResponse<bool>.FailureResponse("Invalid webhook signature");
                }

                // 2. Find payment record
                var payment = await _unitOfWork.Payments
                        .GetByTransactionIdAsync(webhookData.OrderId.ToString());

                if (payment == null)
                    return ServiceResponse<bool>.FailureResponse("Payment not found");

                // 3. Idempotency
                if (payment.Status == PaymentStatus.Completed)
                {
                    return ServiceResponse<bool>
                        .SuccessResponse(true, "Payment already processed");
                }

                // 4. Pick correct strategy
                var strategy = _paymentStrategyFactory.GetStrategy(payment.PaymentMethod);

                // Some strategies (like Paymob) may need custom webhook handling
                // you can add strategy.HandleWebhookAsync if needed.
                // For now we process normally.

                if (webhookData.Success)
                {
                    // Payment succeeded
                    payment.Status = PaymentStatus.Completed;
                    payment.PaymentDate = DateTime.UtcNow;
                    payment.TransactionId = webhookData.TransactionId.ToString();

                    await _unitOfWork.SaveAsync(cancellationToken);

                    // Confirm Booking
                    await _bookingService.ConfirmBookingAsync(payment.BookingId, cancellationToken);

                    return ServiceResponse<bool>.SuccessResponse(true, "Payment completed and booking confirmed");
                }
                else
                {
                    // Payment failed
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = webhookData.ErrorOccurred ? "Gateway error" : "Payment declined";

                    await _unitOfWork.SaveAsync(cancellationToken);

                    return ServiceResponse<bool>.SuccessResponse(true, "Payment failure recorded");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return ServiceResponse<bool>.FailureResponse($"Error: {ex.Message}");
            }
        }

        // ---------------------------------------------------------------------
        // HMAC VERIFICATION
        // ---------------------------------------------------------------------
        private bool VerifyHmacSignature(PaymobWebhookDto webhookData)
        {
            try
            {
                var secret = _configuration["Paymob:HmacSecret"];
                if (string.IsNullOrEmpty(secret))
                    return true; // skip in development

                var concatenated =
                    $"{webhookData.AmountCents}" +
                    $"{webhookData.CreatedAt}" +
                    $"{webhookData.Currency}" +
                    $"{webhookData.ErrorOccurred}" +
                    $"{webhookData.HasParentTransaction}" +
                    $"{webhookData.TransactionId}" +
                    $"{webhookData.IntegrationId}" +
                    $"{webhookData.IsAuth}" +
                    $"{webhookData.IsCapture}" +
                    $"{webhookData.IsRefunded}" +
                    $"{webhookData.IsStandalonePayment}" +
                    $"{webhookData.IsVoided}" +
                    $"{webhookData.OrderId}" +
                    $"{webhookData.Owner}" +
                    $"{webhookData.Pending}" +
                    $"{webhookData.SourceDataPan}" +
                    $"{webhookData.SourceDataSubType}" +
                    $"{webhookData.SourceDataType}" +
                    $"{webhookData.Success}";

                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
                var computed = BitConverter.ToString(hash).Replace("-", "").ToLower();

                return computed == webhookData.Hmac?.ToLower();
            }
            catch
            {
                return false;
            }
        }
    }
}
