using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Orchestrator;

namespace Salahly.DSL.Services.Orchestrator
{
    /// <summary>
    /// Orchestrator for handling payment failure cleanup
    /// Reverts offer reservations and updates statuses in a transaction
    /// </summary>
    public class PaymentFailureOrchestrator : IFailedOrchestrator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOfferService _offerService;
        private readonly ILogger<PaymentFailureOrchestrator> _logger;

        public PaymentFailureOrchestrator(
            IUnitOfWork unitOfWork,
            IOfferService offerService,
            ILogger<PaymentFailureOrchestrator> logger)
        {
            _unitOfWork = unitOfWork;
            _offerService = offerService;
            _logger = logger;
        }

        public async Task<WorkflowResult<bool>> ExecuteAsync(
            int bookingId,
            int paymentId,
            string failureReason,
            CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _logger.LogInformation(
                    $"Starting payment failure cleanup - Booking: {bookingId}, Payment: {paymentId}");

                // Step 1: Load Booking
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return WorkflowResult<bool>.FailureResult(
                        "Booking not found", "LoadBooking");
                }

                // Step 2: Load Payment
                var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
                if (payment == null)
                {
                    return WorkflowResult<bool>.FailureResult(
                        "Payment not found", "LoadPayment");
                }

                // Step 3: Check if already processed (idempotency)
                if (payment.Status == PaymentStatus.Failed &&
                    booking.Status == BookingStatus.failed)
                {
                    _logger.LogInformation($"Payment {paymentId} already marked as failed (idempotent)");
                    return WorkflowResult<bool>.SuccessResult(
                        true, "Payment failure already processed");
                }

                // Step 4: Update Payment and booking Status
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = failureReason;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Status = BookingStatus.failed;

                // step 5: Revert Offer Reservations
                var acceptedOffer = booking.AcceptedOfferId;
                if(acceptedOffer == null)
                {
                    return WorkflowResult<bool>.FailureResult(
                        "No accepted offer to revert", "RevertOfferReservation");
                }
                await _offerService.RevertOfferReservationAsync(acceptedOffer);
                

                await _unitOfWork.SaveAsync(cancellationToken);

                _logger.LogInformation($"Payment failure cleanup completed for Booking {bookingId}");

                return WorkflowResult<bool>.SuccessResult(
                    true, "Payment failure processed successfully");

            }, cancellationToken);
        }

        /// <summary>
        /// Determines if booking should be cancelled based on deadline or max attempts
        /// </summary>
        //private async Task<bool> ShouldCancelBooking(Booking booking)
        //{
        //    // Cancel if payment deadline passed
        //    if (DateTime.UtcNow > booking.PaymentDeadline)
        //    {
        //        _logger.LogWarning($"Payment deadline passed for Booking {booking.BookingId}");
        //        return true;
        //    }

        //    // Cancel if max payment attempts reached (e.g., 3 attempts)
        //    var paymentAttempts = await _unitOfWork.Payments
        //        .GetByBookingIdAsync(booking.BookingId);

        //    var failedAttempts = paymentAttempts.Count(p => p.Status == PaymentStatus.Failed);

        //    const int MAX_ATTEMPTS = 3;
        //    if (failedAttempts >= MAX_ATTEMPTS)
        //    {
        //        _logger.LogWarning($"Max payment attempts ({MAX_ATTEMPTS}) reached for Booking {booking.BookingId}");
        //        return true;
        //    }

        //    return false;
        //}
    }
}
