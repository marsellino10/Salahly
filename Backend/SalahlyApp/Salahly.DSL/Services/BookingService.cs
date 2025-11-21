using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.DTOs.PaymentDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Payments;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentStrategyFactory _paymentStrategyFactory;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IUnitOfWork unitOfWork,
            IPaymentStrategyFactory paymentStrategyFactory,
            ILogger<BookingService> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentStrategyFactory = paymentStrategyFactory;
            _logger = logger;
        }

        /// <summary>
        /// Create Booking from Accepted Offer and Initiate Payment
        /// </summary>
        public async Task<ServiceResponse<BookingPaymentDto>> CreateAndInitiatePaymentAsync(
            int customerId,
            int offerId,
            string paymentMethod = "Card",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers
                    .GetOfferForCustomerByIdAsync(offerId, customerId);

                if (offer == null)
                {
                    _logger.LogWarning($"Offer {offerId} not found for customer {customerId}");
                    return ServiceResponse<BookingPaymentDto>
                        .FailureResponse("Offer not found or inaccessible.");
                }

                if (offer.Status != OfferStatus.Accepted)
                {
                    _logger.LogWarning($"Offer {offerId} has invalid status: {offer.Status}");
                    return ServiceResponse<BookingPaymentDto>
                        .FailureResponse("Offer is not in accepted status.");
                }

                var serviceRequest = await _unitOfWork.ServiceRequests
                    .GetByIdAsync(offer.ServiceRequestId);
                var craftsman = await _unitOfWork.Craftsmen
                    .GetByIdAsync(offer.CraftsmanId);
                var customer = await _unitOfWork.Customers
                    .GetByIdAsync(customerId);

                if (serviceRequest == null || craftsman == null || customer == null)
                {
                    _logger.LogError($"Required data not found. ServiceRequest: {serviceRequest != null}, Craftsman: {craftsman != null}, Customer: {customer != null}");
                    return ServiceResponse<BookingPaymentDto>
                        .FailureResponse("Required data not found.");
                }

                _logger.LogInformation("Basic entities loaded successfully");

                var customerUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(customer.Id);
                var craftsmanUser = await _unitOfWork.ApplicationUsers.GetByIdAsync(craftsman.Id);
                var craft = await _unitOfWork.Crafts.GetByIdAsync(serviceRequest.CraftId);

                if (customerUser == null || craftsmanUser == null || craft == null)
                {
                    _logger.LogError($"User or Craft data not found. CustomerUser: {customerUser != null}, CraftsmanUser: {craftsmanUser != null}, Craft: {craft != null}");
                    return ServiceResponse<BookingPaymentDto>
                        .FailureResponse("User or craft data not found.");
                }

                _logger.LogInformation($"All data loaded. Customer: {customerUser.Email}, Craftsman: {craftsmanUser.Email}, Craft: {craft.Name}");

                var booking = new Booking
                {
                    CustomerId = customerId,
                    CraftsmanId = offer.CraftsmanId,
                    CraftId = serviceRequest.CraftId,
                    ServiceRequestId = offer.ServiceRequestId,
                    AcceptedOfferId = offerId,
                    BookingDate = serviceRequest.PreferredDate != default(DateTime) ? serviceRequest.PreferredDate : DateTime.UtcNow.AddDays(1),
                    Duration = 0,
                    TotalAmount = offer.OfferedPrice,
                    Status = BookingStatus.InProgress,
                    PaymentDeadline = DateTime.UtcNow.AddHours(24),
                    RefundableAmount = offer.OfferedPrice,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation($"Booking created with ID: {booking.BookingId}");

                var paymentStrategy = _paymentStrategyFactory.GetStrategy(paymentMethod);

                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = offer.OfferedPrice,
                    PaymentDate = DateTime.UtcNow,
                    Status = PaymentStatus.Pending,
                    PaymentMethod = paymentMethod,
                    PaymentGateway = paymentStrategy.GetProviderName()
                };

                await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation($"Payment record created with ID: {payment.Id}");

                var paymentInitRequest = new PaymentInitializationRequest
                {
                    BookingId = booking.BookingId,
                    CustomerId = customerId,
                    Amount = offer.OfferedPrice,
                    CustomerEmail = customerUser.Email ?? "customer@test.com",
                    CustomerPhone = customer.PhoneNumber ?? customerUser.PhoneNumber ?? "0100000000",
                    CustomerName = customerUser.FullName ?? "Customer",
                    CustomerAddress = customer.Address ?? "Cairo",
                    CraftName = craft.Name ?? "Service",
                    CraftsmanName = craftsmanUser.FullName ?? "Craftsman",
                    BookingDate = booking.BookingDate
                };

                _logger.LogInformation($"Initiating payment for amount: {paymentInitRequest.Amount}");

                var paymentResult = await paymentStrategy.InitializeAsync(
                    paymentInitRequest,
                    cancellationToken);

                if (!paymentResult.IsSuccess)
                {
                    _logger.LogError($"Payment initialization failed: {paymentResult.ErrorMessage}");
                    return ServiceResponse<BookingPaymentDto>
                        .FailureResponse($"Failed to initialize payment: {paymentResult.ErrorMessage}");
                }

                payment.TransactionId = paymentResult.TransactionId;
                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation($"Payment initialized successfully. Booking: {booking.BookingId}, Transaction: {paymentResult.TransactionId}");

                var response = new BookingPaymentDto
                {
                    BookingId = booking.BookingId,
                    PaymentId = payment.Id,
                    Amount = offer.OfferedPrice,
                    PaymentLink = paymentResult.PaymentLink,
                    PaymentToken = paymentResult.PaymentToken,
                    TransactionId = paymentResult.TransactionId,
                    BookingDate = booking.BookingDate,
                    PaymentDeadline = booking.PaymentDeadline,
                    CraftsmanName = craftsmanUser.FullName ?? "Craftsman",
                    CraftName = craft.Name ?? "Service"
                };

                return ServiceResponse<BookingPaymentDto>
                    .SuccessResponse(response, "Booking created and payment initiated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAndInitiatePaymentAsync");
                return ServiceResponse<BookingPaymentDto>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Confirm Booking after payment success
        /// </summary>
        public async Task<ServiceResponse<bool>> ConfirmBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found");
                    return ServiceResponse<bool>
                        .FailureResponse("Booking not found.");
                }

                if (booking.Status != BookingStatus.InProgress)
                {
                    _logger.LogWarning($"Booking {bookingId} is not awaiting payment. Current status: {booking.Status}");
                    return ServiceResponse<bool>
                        .FailureResponse("Booking is not awaiting payment.");
                }

                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation($"Booking {bookingId} confirmed successfully");

                return ServiceResponse<bool>
                    .SuccessResponse(true, "Booking confirmed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming booking {bookingId}");
                return ServiceResponse<bool>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel Booking with Refund
        /// </summary>
        public async Task<ServiceResponse<CancellationResultDto>> CancelBookingAsync(
            int bookingId,
            string? cancellationReason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found");
                    return ServiceResponse<CancellationResultDto>
                        .FailureResponse("Booking not found.");
                }

                if (booking.Status == BookingStatus.Completed ||
                    booking.Status == BookingStatus.Cancelled)
                {
                    _logger.LogWarning($"Cannot cancel booking {bookingId}. Current status: {booking.Status}");
                    return ServiceResponse<CancellationResultDto>
                        .FailureResponse("Cannot cancel a completed or already cancelled booking.");
                }

                var hoursUntilBooking = (booking.BookingDate - DateTime.UtcNow).TotalHours;
                var (refundAmount, refundPercentage) = CalculateRefundAmount(
                    booking.TotalAmount,
                    hoursUntilBooking);

                _logger.LogInformation($"Cancellation: Booking {bookingId}, Hours until: {hoursUntilBooking:F2}, Refund: {refundAmount} ({refundPercentage}%)");

                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                var payment = allPayments.FirstOrDefault(p => p.BookingId == bookingId);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment record not found for booking {bookingId}");
                    return ServiceResponse<CancellationResultDto>
                        .FailureResponse("Payment record not found.");
                }

                if (payment.Status == PaymentStatus.Completed && refundAmount > 0)
                {
                    var paymentStrategy = _paymentStrategyFactory.GetStrategy(payment.PaymentMethod);

                    var refundRequest = new RefundRequest
                    {
                        BookingId = bookingId,
                        PaymentId = payment.Id,
                        OriginalTransactionId = payment.TransactionId,
                        RefundAmount = refundAmount,
                        Reason = cancellationReason ?? "Customer cancelled booking"
                    };

                    var refundResult = await paymentStrategy.RefundAsync(
                        refundRequest,
                        cancellationToken);

                    if (!refundResult.IsSuccess)
                    {
                        _logger.LogError($"Refund failed for booking {bookingId}: {refundResult.ErrorMessage}");
                        return ServiceResponse<CancellationResultDto>
                            .FailureResponse($"Refund failed: {refundResult.ErrorMessage}");
                    }

                    payment.Status = PaymentStatus.Refunded;
                    payment.RefundTransactionId = refundResult.RefundTransactionId;
                    payment.RefundedAt = refundResult.RefundDate;

                    _logger.LogInformation($"Refund processed successfully: {refundResult.RefundTransactionId}");
                }

                booking.Status = BookingStatus.Cancelled;
                booking.CancellationReason = cancellationReason;
                booking.CancelledAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.RefundableAmount = refundAmount;

                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation($"Booking {bookingId} cancelled successfully. Refund amount: {refundAmount}");

                var result = new CancellationResultDto
                {
                    BookingId = bookingId,
                    CancellationDate = booking.CancelledAt.Value,
                    RefundAmount = refundAmount,
                    RefundPercentage = refundPercentage,
                    Message = $"Booking cancelled. Refund of {refundAmount:C} will be processed."
                };

                return ServiceResponse<CancellationResultDto>
                    .SuccessResponse(result, "Booking cancelled successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling booking {bookingId}");
                return ServiceResponse<CancellationResultDto>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate refund amount based on cancellation timing
        /// </summary>
        private (decimal refundAmount, int refundPercentage) CalculateRefundAmount(
            decimal totalAmount,
            double hoursUntilBooking)
        {
            int refundPercentage = hoursUntilBooking switch
            {
                > 24 => 100,
                > 2 => 75,
                _ => 50
            };

            var refundAmount = totalAmount * refundPercentage / 100;
            return (refundAmount, refundPercentage);
        }
    }
}