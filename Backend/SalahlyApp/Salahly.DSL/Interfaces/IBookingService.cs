using Salahly.DAL.Entities;
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
        // ===== For Orchestrator (Internal Use) =====
        Task<List<BookingAdminViewDto>> GetAllBookingsAsync();
        /// <summary>
        /// Create booking (simple - no payment logic)
        /// Used by orchestrator - does NOT save (orchestrator handles transaction)
        /// </summary>
        Task<Booking> CreateBookingAsync(
            int customerId,
            int craftsmanId,
            int craftId,
            int serviceRequestId,
            int acceptedOfferId,
            decimal amount,
            DateTime bookingDate);

        /// <summary>
        /// Delete booking (compensation)
        /// Used by orchestrator on rollback - DOES save
        /// </summary>
        Task DeleteBookingAsync(int bookingId);

        /// <summary>
        /// Update booking status (helper method)
        /// </summary>
        Task UpdateBookingStatusAsync(
            int bookingId,
            BookingStatus status);

        // ===== For Webhook & Public Use =====

        /// <summary>
        /// Confirm booking after payment success (called by webhook)
        /// </summary>
        Task<ServiceResponse<bool>> ConfirmBookingAsync(
            int bookingId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel booking with refund calculation
        /// </summary>
        Task<ServiceResponse<CancellationResultDto>> CancelBookingAsync(
            int bookingId,
            string? cancellationReason,
            CancellationToken cancellationToken = default);

        // ===== Query Methods =====

        /// <summary>
        /// Get booking by ID
        /// </summary>
        Task<ServiceResponse<BookingDto>> GetBookingByIdAsync(
            int bookingId,
            int userId);


        /// <summary>
        /// Get all bookings for user (craftsman, customer)
        /// </summary>
        Task<ServiceResponse<IEnumerable<BookingWithServiceRequestDto>>> GetBookingsAsync(
            int Id);

        /// <summary>
        /// Get booking details together with the linked service request and customer contact
        /// Only returns data when the service request status is OfferAccepted
        /// </summary>
        Task<ServiceResponse<BookingWithServiceRequestDto>> GetBookingWithServiceRequestDetailsAsync(
            int bookingId,
            int userId);
    }
}
