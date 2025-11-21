using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Create booking from accepted offer and initiate payment
        /// </summary>
        Task<ServiceResponse<BookingPaymentDto>> CreateAndInitiatePaymentAsync(
            int customerId,
            int offerId,
            string paymentMethod = "Card",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Confirm booking after successful payment
        /// </summary>
        Task<ServiceResponse<bool>> ConfirmBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel booking with refund
        /// </summary>
        Task<ServiceResponse<CancellationResultDto>> CancelBookingAsync(
            int bookingId,
            string? cancellationReason,
            CancellationToken cancellationToken = default);
    }
}
