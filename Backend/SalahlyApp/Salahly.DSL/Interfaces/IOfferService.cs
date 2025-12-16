using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using System;

namespace Salahly.DSL.Interfaces
{
    public interface IOfferService
    {
        // ===== For Customer =====

        /// <summary>
        /// Get all offers for a specific customer request
        /// </summary>
        Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersForCustomerRequestAsync(
            int customerId,
            int requestId);

        /// <summary>
        /// Accept offer - delegates to orchestrator for full workflow
        /// </summary>
        //Task<ServiceResponse<BookingPaymentDto>> AcceptOfferAsync(
        //    int customerId,
        //    int offerId,
        //    CancellationToken cancellationToken = default);

        /// <summary>
        /// Reject an offer
        /// </summary>
        Task<ServiceResponse<bool>> RejectOfferAsync(
            int customerId,
            int offerId,
            RejectOfferDto dto);

        Task<ServiceResponse<bool>> ResetOffersAsync(
            int customerId,
            int offerId);
        Task ResetOtherOffersAsync(
            int serviceRequestId,
            int customerId);
        // ===== For Craftsman =====

        /// <summary>
        /// Create a new offer (by craftsman)
        /// </summary>
        Task<ServiceResponse<OfferDto>> CreateOfferAsync(
            int craftsmanId,
            CreateOfferDto dto);

        /// <summary>
        /// Get all offers by a specific craftsman
        /// </summary>
        Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersByCraftsmanAsync(
            int craftsmanId);

        /// <summary>
        /// Get specific offer by ID for craftsman
        /// </summary>
        Task<ServiceResponse<OfferDto>> GetOfferByIdForCraftsmanAsync(
            int craftsmanId,
            int offerId);

        /// <summary>
        /// Get specific offer by serviceRequestId for craftsman
        /// </summary>
        Task<ServiceResponse<OfferDto>> GetOfferForServiceRequestAsync(
            int craftsmanId,
            int serviceRequestId);

        /// <summary>
        /// Withdraw an offer (by craftsman)
        /// </summary>
        Task<ServiceResponse<bool>> WithdrawOfferAsync(
            int craftsmanId,
            int offerId);

        // ===== For Orchestrator (Internal Use) =====

        /// <summary>
        /// Get offer for acceptance validation
        /// </summary>
        Task<OfferDto?> GetOfferForAcceptanceAsync(
            int offerId,
            int customerId);

        /// <summary>
        /// Reserve offer (update status to Accepted)
        /// </summary>
        Task ReserveOfferAsync(int offerId);

        /// <summary>
        /// Reject other offers for the same service request
        /// </summary>
        Task RejectOtherOffersAsync(
            int serviceRequestId,
            int acceptedOfferId,
            int customerId);

        /// <summary>
        /// Revert offer reservation (compensation)
        /// </summary>
        Task RevertOfferReservationAsync(int offerId);
    }
}
