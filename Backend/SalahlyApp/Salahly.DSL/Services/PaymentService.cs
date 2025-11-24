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
        private readonly IServiceRequestService _serviceRequestService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymentStrategyFactory paymentStrategyFactory,
            IBookingService bookingService,
            ILogger<PaymentService> logger,
            IConfiguration configuration,
            IServiceRequestService serviceRequestService)
        {
            _unitOfWork = unitOfWork;
            _paymentStrategyFactory = paymentStrategyFactory;
            _bookingService = bookingService;
            _logger = logger;
            _configuration = configuration;
            _serviceRequestService = serviceRequestService;
        }

        // ✅ =========================================================
        // ✅ NEW METHODS FOR ORCHESTRATOR
        // ✅ =========================================================

        /// <summary>
        /// Create payment record in DB (used by CreatePaymentRecordStep)
        /// </summary>
        public async Task<Payment> CreatePaymentRecordAsync(
            int bookingId,
            decimal amount,
            string paymentMethod,
            string gateway)
        {
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending,
                PaymentMethod = paymentMethod,
                PaymentGateway = gateway
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation($"✅ Payment record created: {payment.Id} for Booking: {bookingId}");
            return payment;
        }

        /// <summary>
        /// Initialize payment with gateway (used by InitializePaymentGatewayStep)
        /// </summary>
        public async Task<PaymentInitializationResult> InitializePaymentGatewayAsync(
            Payment payment,
            PaymentInitializationRequest request,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Initializing payment gateway for Payment: {payment.Id}");

            var strategy = _paymentStrategyFactory.GetStrategy(payment.PaymentMethod);
            var result = await strategy.InitializeAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                payment.TransactionId = result.TransactionId;
                await _unitOfWork.SaveAsync(cancellationToken);
                _logger.LogInformation($"✅ Payment initialized: {result.TransactionId}");
            }
            else
            {
                _logger.LogError($"❌ Payment initialization failed: {result.ErrorMessage}");
            }

            return result;
        }

        /// <summary>
        /// Delete payment record (used in compensation)
        /// </summary>
        public async Task DeletePaymentAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment != null)
            {
                await _unitOfWork.Payments.DeleteAsync(payment);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation($"🔄 Payment {paymentId} deleted (compensation)");
            }
        }


        public async Task<ServiceResponse<PaymentVerificationResult>> VerifyPaymentAsync(
            string transactionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Verifying payment: {transactionId}");

                var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(transactionId);
                if (payment == null)
                    return ServiceResponse<PaymentVerificationResult>.FailureResponse("Payment not found");

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
                    _logger.LogWarning("Invalid webhook HMAC signature");
                    return ServiceResponse<bool>.FailureResponse("Invalid webhook signature");
                }

                // 2. Find payment record
                var payment = await _unitOfWork.Payments
                        .GetByTransactionIdAsync(webhookData.OrderId.ToString());

                if (payment == null)
                {
                    _logger.LogWarning($"Payment not found for order: {webhookData.OrderId}");
                    return ServiceResponse<bool>.FailureResponse("Payment not found");
                }

                // 3. Idempotency check
                if (payment.Status == PaymentStatus.Completed)
                {
                    _logger.LogInformation($"Payment {payment.Id} already processed (idempotent)");
                    return ServiceResponse<bool>
                        .SuccessResponse(true, "Payment already processed");
                }

                // 4. Process payment result
                if (webhookData.Success)
                {
                    // Payment succeeded
                    payment.Status = PaymentStatus.Completed;
                    payment.PaymentDate = DateTime.UtcNow;
                    payment.TransactionId = webhookData.TransactionId.ToString();

                    await _unitOfWork.SaveAsync(cancellationToken);

                    // Confirm Booking
                    await _bookingService.ConfirmBookingAsync(payment.BookingId, cancellationToken);
                    var SR = await _unitOfWork.Bookings.GetByIdAsync(payment.BookingId);
                    var SRId = SR.ServiceRequestId ?? 0;
                    await _serviceRequestService.ChangeStatusAsync(SRId, ServiceRequestStatus.OfferAccepted);

                    _logger.LogInformation($"Payment {payment.Id} completed, Booking {payment.BookingId} confirmed");
                    return ServiceResponse<bool>.SuccessResponse(true, "Payment completed and booking confirmed");
                }
                else
                {
                    // Payment failed
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = webhookData.ErrorOccurred ? "Gateway error" : "Payment declined";

                    await _unitOfWork.SaveAsync(cancellationToken);

                    _logger.LogWarning($"Payment {payment.Id} failed: {payment.FailureReason}");
                    return ServiceResponse<bool>.SuccessResponse(true, "Payment failure recorded");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return ServiceResponse<bool>.FailureResponse($"Error: {ex.Message}");
            }
        }

        // ✅ =========================================================
        // ✅ PRIVATE HELPER METHODS
        // ✅ =========================================================

        private bool VerifyHmacSignature(PaymobWebhookDto webhookData)
        {
            try
            {
                var secret = _configuration["Paymob:HmacSecret"];
                if (string.IsNullOrEmpty(secret))
                {
                    _logger.LogWarning("HMAC secret not configured - skipping verification");
                    return true; // skip in development
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying HMAC");
                return false;
            }
        }
    }
}
