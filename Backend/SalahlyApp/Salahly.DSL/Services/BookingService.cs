using Mapster;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<BookingAdminViewDto>> GetAllBookingsAsync()
        {
            var list=await _unitOfWork.Bookings.GetAll().ToListAsync();
            return list.Adapt<List<BookingAdminViewDto>>();
        }
        // =====================================================================
        // ===== ORCHESTRATOR HELPER METHODS (Internal Use) =====
        // =====================================================================

        /// <summary>
        /// Create booking (simple - no payment logic)
        /// Used by orchestrator - does NOT save (orchestrator handles transaction)
        /// </summary>
        public async Task<Booking> CreateBookingAsync(
            int customerId,
            int craftsmanId,
            int craftId,
            int serviceRequestId,
            int acceptedOfferId,
            decimal amount,
            DateTime bookingDate)
        {
            var existingBooking = await _unitOfWork.Bookings.GetByOfferIdAsync(acceptedOfferId);
            if (existingBooking != null) { 
                return existingBooking;
            }

            var booking = new Booking
            {
                CustomerId = customerId,
                CraftsmanId = craftsmanId,
                CraftId = craftId,
                ServiceRequestId = serviceRequestId,
                AcceptedOfferId = acceptedOfferId,
                BookingDate = bookingDate,
                Duration = 0,
                TotalAmount = amount,
                RefundableAmount = 0,
                Status = BookingStatus.InProgress, 
                PaymentDeadline = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Bookings.AddAsync(booking);

            _logger.LogInformation(
                $"Booking created for offer {acceptedOfferId}. " +
                $"Status: Pending, Deadline: {booking.PaymentDeadline}");


            return booking;
        }

        /// <summary>
        /// Delete booking (compensation)
        /// Used by orchestrator on rollback - DOES save
        /// </summary>
        public async Task DeleteBookingAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking != null)
            {
                await _unitOfWork.Bookings.DeleteAsync(booking);
                await _unitOfWork.SaveAsync();

                _logger.LogWarning(
                    $"🔄 Deleted booking {bookingId} (compensation)");
            }
        }

        /// <summary>
        /// Update booking status (helper method)
        /// </summary>
        public async Task UpdateBookingStatusAsync(
            int bookingId,
            BookingStatus status)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking != null)
            {
                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;

                if (status == BookingStatus.Cancelled)
                    booking.CancelledAt = DateTime.UtcNow;
                else if (status == BookingStatus.Completed)
                    booking.CompletedAt = DateTime.UtcNow;

                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Booking {bookingId} status updated to {status}");
            }
        }

        // =====================================================================
        // ===== PUBLIC METHODS (For Webhooks & Controllers) =====
        // =====================================================================

        /// <summary>
        /// Confirm booking after payment success (called by webhook)
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

                // Accept Pending status (not InProgress anymore)
                if (booking.Status != BookingStatus.InProgress)
                {
                    _logger.LogWarning(
                        $"Booking {bookingId} cannot be confirmed. Current status: {booking.Status}");
                    return ServiceResponse<bool>
                        .FailureResponse("Booking is not in pending status.");
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
        /// Cancel booking with refund calculation
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

                // Check if booking can be cancelled
                if (booking.Status == BookingStatus.Completed ||
                    booking.Status == BookingStatus.Cancelled)
                {
                    _logger.LogWarning(
                        $"Cannot cancel booking {bookingId}. Current status: {booking.Status}");
                    return ServiceResponse<CancellationResultDto>
                        .FailureResponse("Cannot cancel a completed or already cancelled booking.");
                }

                // Calculate refund based on cancellation timing
                var hoursUntilBooking = (booking.BookingDate - DateTime.UtcNow).TotalHours;
                var (refundAmount, refundPercentage) = CalculateRefundAmount(
                    booking.TotalAmount,
                    hoursUntilBooking);

                _logger.LogInformation(
                    $"Cancellation calculation for booking {bookingId}: " +
                    $"Hours until booking: {hoursUntilBooking:F2}, " +
                    $"Refund: {refundAmount} EGP ({refundPercentage}%)");

                // Find payment record
                var allPayments = await _unitOfWork.Payments.GetAllAsync();
                var payment = allPayments.FirstOrDefault(p => p.BookingId == bookingId);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment record not found for booking {bookingId}");
                    return ServiceResponse<CancellationResultDto>
                        .FailureResponse("Payment record not found.");
                }

                // Process refund if payment was completed and refund amount > 0
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

                    _logger.LogInformation($"Initiating refund for booking {bookingId}");

                    var refundResult = await paymentStrategy.RefundAsync(
                        refundRequest,
                        cancellationToken);

                    if (!refundResult.IsSuccess)
                    {
                        _logger.LogError(
                            $"Refund failed for booking {bookingId}: {refundResult.ErrorMessage}");
                        return ServiceResponse<CancellationResultDto>
                            .FailureResponse($"Refund failed: {refundResult.ErrorMessage}");
                    }

                    payment.Status = PaymentStatus.Refunded;
                    payment.RefundTransactionId = refundResult.RefundTransactionId;
                    payment.RefundedAt = refundResult.RefundDate;

                    _logger.LogInformation(
                        $"Refund processed successfully: {refundResult.RefundTransactionId}");
                }

                // Update booking status
                booking.Status = BookingStatus.Cancelled;
                booking.CancellationReason = cancellationReason;
                booking.CancelledAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.RefundableAmount = refundAmount;

                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation(
                    $"Booking {bookingId} cancelled successfully. Refund amount: {refundAmount} EGP");

                var result = new CancellationResultDto
                {
                    BookingId = bookingId,
                    CancellationDate = booking.CancelledAt.Value,
                    RefundAmount = refundAmount,
                    RefundPercentage = refundPercentage,
                    Message = refundAmount > 0
                        ? $"Booking cancelled. Refund of {refundAmount:C} EGP will be processed."
                        : "Booking cancelled. No refund applicable."
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
                > 24 => 100,  // Full refund if cancelled 24+ hours before
                > 12 => 75,   // 75% refund if 12-24 hours before
                > 2 => 50,    // 50% refund if 2-12 hours before
                _ => 0        // No refund if less than 2 hours
            };

            var refundAmount = totalAmount * refundPercentage / 100;
            return (refundAmount, refundPercentage);
        }

        // =====================================================================
        // ===== QUERY METHODS =====
        // =====================================================================

        /// <summary>
        /// Get booking by ID
        /// </summary>
        public async Task<ServiceResponse<BookingDto>> GetBookingByIdAsync(
            int bookingId,
            int userId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

                if (booking == null)
                {
                    _logger.LogWarning($"Booking {bookingId} not found");
                    return ServiceResponse<BookingDto>
                        .FailureResponse("Booking not found.");
                }

                // Verify user has access (either customer or craftsman)
                if (booking.CustomerId != userId && booking.CraftsmanId != userId)
                {
                    _logger.LogWarning(
                        $"User {userId} attempted to access booking {bookingId} without permission");
                    return ServiceResponse<BookingDto>
                        .FailureResponse("You don't have access to this booking.");
                }

                // Map to DTO (you can use Mapster or manual mapping)
                var dto = new BookingDto
                {
                    BookingId = booking.BookingId,
                    CustomerId = booking.CustomerId,
                    CraftsmanId = booking.CraftsmanId,
                    CraftId = booking.CraftId,
                    BookingDate = booking.BookingDate,
                    TotalAmount = booking.TotalAmount,
                    Status = booking.Status.ToString(),
                    PaymentDeadline = booking.PaymentDeadline,
                    CreatedAt = booking.CreatedAt,
                    CancellationReason = booking.CancellationReason
                };

                return ServiceResponse<BookingDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving booking {bookingId}");
                return ServiceResponse<BookingDto>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all bookings for customer
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<BookingDto>>> GetCustomerBookingsAsync(
            int customerId)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var customerBookings = allBookings
                    .Where(b => b.CustomerId == customerId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                var dtos = customerBookings.Select(b => new BookingDto
                {
                    BookingId = b.BookingId,
                    CustomerId = b.CustomerId,
                    CraftsmanId = b.CraftsmanId,
                    CraftId = b.CraftId,
                    BookingDate = b.BookingDate,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status.ToString(),
                    PaymentDeadline = b.PaymentDeadline,
                    CreatedAt = b.CreatedAt,
                    CancellationReason = b.CancellationReason
                }).ToList();

                return ServiceResponse<IEnumerable<BookingDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for customer {customerId}");
                return ServiceResponse<IEnumerable<BookingDto>>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all bookings for craftsman
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<BookingDto>>> GetCraftsmanBookingsAsync(
            int craftsmanId)
        {
            try
            {
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                var craftsmanBookings = allBookings
                    .Where(b => b.CraftsmanId == craftsmanId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                var dtos = craftsmanBookings.Select(b => new BookingDto
                {
                    BookingId = b.BookingId,
                    CustomerId = b.CustomerId,
                    CraftsmanId = b.CraftsmanId,
                    CraftId = b.CraftId,
                    BookingDate = b.BookingDate,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status.ToString(),
                    PaymentDeadline = b.PaymentDeadline,
                    CreatedAt = b.CreatedAt,
                    CancellationReason = b.CancellationReason
                }).ToList();

                return ServiceResponse<IEnumerable<BookingDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving bookings for craftsman {craftsmanId}");
                return ServiceResponse<IEnumerable<BookingDto>>
                    .FailureResponse($"Error: {ex.Message}");
            }
        }
    }
}