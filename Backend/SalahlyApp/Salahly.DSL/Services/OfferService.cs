using Mapster;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.Booking;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using Salahly.DSL.Interfaces.Orchestrator;
using System;

namespace Salahly.DSL.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OfferService> _logger;

        public OfferService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILogger<OfferService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        // =====================================================================
        // ===== PUBLIC METHODS (For Controllers) =====
        // =====================================================================

        /// <summary>
        /// Get all offers for a specific customer request
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersForCustomerRequestAsync(
            int customerId,
            int requestId)
        {
            try
            {
                var offers = await _unitOfWork.CraftsmanOffers
                    .GetOffersForCustomerRequestAsync(requestId, customerId);

                var dtoList = offers.Select(o =>
                {
                    var dto = o.Adapt<OfferDto>();
                    dto.CraftsmanName = o.Craftsman?.User?.FullName ?? "Unknown";
                    return dto;
                }).ToList();

                if (!dtoList.Any())
                    return ServiceResponse<IEnumerable<OfferDto>>
                        .FailureResponse("No offers found for this request.");

                return ServiceResponse<IEnumerable<OfferDto>>.SuccessResponse(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving offers for request {requestId}");
                return ServiceResponse<IEnumerable<OfferDto>>
                    .FailureResponse($"Error retrieving offers: {ex.Message}");
            }
        }

        /// <summary>
        /// Accept offer - delegates to orchestrator for full workflow
        /// </summary>
        //public async Task<ServiceResponse<BookingPaymentDto>> AcceptOfferAsync(
        //    int customerId,
        //    int offerId,
        //    CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        _logger.LogInformation(
        //            $"Customer {customerId} initiating offer acceptance for offer {offerId}");
                
        //        // Delegate entire workflow to orchestrator
        //        var result = await _orchestrator.ExecuteAsync(
        //            customerId,
        //            offerId,
        //            "card",
        //            cancellationToken);

        //        if (!result.Success)
        //        {
        //            _logger.LogError(
        //                $"Offer acceptance failed for offer {offerId}: {result.ErrorMessage}");
        //            return ServiceResponse<BookingPaymentDto>.FailureResponse(result.ErrorMessage);
        //        }

        //        _logger.LogInformation(
        //            $"✅ Offer {offerId} accepted successfully. " +
        //            $"Booking: {result.Data.BookingId}, Payment link created");

        //        return ServiceResponse<BookingPaymentDto>.SuccessResponse(
        //            result.Data,
        //            "Offer accepted and payment link created successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error accepting offer {offerId}");
        //        return ServiceResponse<BookingPaymentDto>
        //            .FailureResponse($"Error accepting offer: {ex.Message}");
        //    }
        //}

        /// <summary>
        /// Reject an offer
        /// </summary>
        public async Task<ServiceResponse<bool>> RejectOfferAsync(
            int customerId,
            int offerId,
            RejectOfferDto dto)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers
                    .GetOfferForCustomerByIdAsync(offerId, customerId);

                if (offer == null)
                {
                    _logger.LogWarning($"Offer {offerId} not found for customer {customerId}");
                    return ServiceResponse<bool>
                        .FailureResponse("Offer not found or inaccessible.");
                }

                var request = offer.ServiceRequest;

                // Check ServiceRequest status
                if (request.Status == ServiceRequestStatus.Completed ||
                    request.Status == ServiceRequestStatus.Cancelled ||
                    request.Status == ServiceRequestStatus.Expired)
                {
                    return ServiceResponse<bool>
                        .FailureResponse("Cannot reject offer for a completed, cancelled, or expired request.");
                }

                // Check Offer status
                if (offer.Status == OfferStatus.Accepted)
                {
                    return ServiceResponse<bool>
                        .FailureResponse("Cannot reject an offer that has already been accepted.");
                }

                offer.Status = OfferStatus.Rejected;
                offer.RejectedAt = DateTime.UtcNow;
                offer.RejectionReason = dto.RejectionReason;

                await _unitOfWork.SaveAsync();

                await _notificationService.NotifyAsync(new CreateNotificationDto
                {
                    UserIds = new[] { offer.CraftsmanId },
                    Type = NotificationType.OfferRejected,
                    Title = "Your Offer has been Rejected",
                    Message = $" Your Offer for {request.Title} request has been rejected by Customer due to {dto.RejectionReason}",
                    ActionUrl = $"/service-requests/{request.ServiceRequestId}",
                    CraftsmanOfferId = offer.CraftsmanOfferId,
                    ServiceRequestId = request.ServiceRequestId
                });

                _logger.LogInformation($"Customer {customerId} rejected offer {offerId}");

                return ServiceResponse<bool>.SuccessResponse(true, "Offer rejected successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting offer {offerId}");
                return ServiceResponse<bool>
                    .FailureResponse($"Error rejecting offer: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new offer (by craftsman)
        /// </summary>
        public async Task<ServiceResponse<OfferDto>> CreateOfferAsync(
            int craftsmanId,
            CreateOfferDto dto)
        {
            try
            {
                // Validate service request exists
                var request = await _unitOfWork.ServiceRequests.GetByIdAsync(dto.ServiceRequestId);
                if (request == null)
                {
                    _logger.LogWarning($"Service request {dto.ServiceRequestId} not found");
                    return ServiceResponse<OfferDto>
                        .FailureResponse("Service request not found.");
                }

                // Verify craftsman's eligibility
                var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(craftsmanId);
                if (craftsman == null)
                {
                    _logger.LogWarning($"Craftsman {craftsmanId} not found");
                    return ServiceResponse<OfferDto>
                        .FailureResponse("Craftsman not found.");
                }

                if (craftsman.CraftId != request.CraftId)
                {
                    _logger.LogWarning(
                        $"Craftsman {craftsmanId} craft mismatch. " +
                        $"Craftsman craft: {craftsman.CraftId}, Request craft: {request.CraftId}");
                    return ServiceResponse<OfferDto>
                        .FailureResponse("You can only offer for requests in your craft.");
                }

                var offer = new CraftsmanOffer
                {
                    CraftsmanId = craftsmanId,
                    ServiceRequestId = dto.ServiceRequestId,
                    OfferedPrice = dto.OfferedPrice,
                    Description = dto.Description,
                    EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                    AvailableFromDate = dto.AvailableFromDate,
                    AvailableToDate = dto.AvailableToDate,
                    Status = OfferStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CraftsmanOffers.AddAsync(offer);
                request.OffersCount += 1;
                await _unitOfWork.SaveAsync();

                _logger.LogInformation(
                    $"Craftsman {craftsmanId} created offer {offer.CraftsmanOfferId} " +
                    $"for request {dto.ServiceRequestId}");

                // Send notification to customer
                await _notificationService.NotifyAsync(new CreateNotificationDto
                {
                    UserIds = new[] { request.CustomerId },
                    Type = NotificationType.NewOffer,
                    Title = "New Offer Received",
                    Message = $"{craftsman.User?.FullName ?? "A craftsman"} submitted an offer for your request.",
                    ActionUrl = $"/service-requests/{request.ServiceRequestId}",
                    CraftsmanOfferId = offer.CraftsmanOfferId,
                    ServiceRequestId = request.ServiceRequestId
                });

                var offerDto = offer.Adapt<OfferDto>();
                return ServiceResponse<OfferDto>
                    .SuccessResponse(offerDto, "Offer created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating offer for craftsman {craftsmanId}");
                return ServiceResponse<OfferDto>
                    .FailureResponse($"Error creating offer: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all offers by a specific craftsman
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersByCraftsmanAsync(
            int craftsmanId)
        {
            try
            {
                var offers = await _unitOfWork.CraftsmanOffers
                    .GetOffersByCraftsmanAsync(craftsmanId);

                var dto = offers.Adapt<IEnumerable<OfferDto>>();

                return ServiceResponse<IEnumerable<OfferDto>>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving offers for craftsman {craftsmanId}");
                return ServiceResponse<IEnumerable<OfferDto>>
                    .FailureResponse($"Error retrieving offers: {ex.Message}");
            }
        }

        /// <summary>
        /// Get specific offer by ID for craftsman
        /// </summary>
        public async Task<ServiceResponse<OfferDto>> GetOfferByIdForCraftsmanAsync(
            int craftsmanId,
            int offerId)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers
                    .GetOfferByIdForCraftsmanAsync(craftsmanId, offerId);

                if (offer == null)
                {
                    _logger.LogWarning($"Offer {offerId} not found for craftsman {craftsmanId}");
                    return ServiceResponse<OfferDto>
                        .FailureResponse("Offer not found or inaccessible.");
                }

                var dto = offer.Adapt<OfferDto>();
                return ServiceResponse<OfferDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving offer {offerId} for craftsman {craftsmanId}");
                return ServiceResponse<OfferDto>
                    .FailureResponse($"Error retrieving offer: {ex.Message}");
            }
        }

        /// <summary>
        /// Withdraw an offer (by craftsman)
        /// </summary>
        public async Task<ServiceResponse<bool>> WithdrawOfferAsync(
            int craftsmanId,
            int offerId)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers
                    .GetOfferByIdForCraftsmanAsync(craftsmanId, offerId);

                if (offer == null)
                {
                    _logger.LogWarning($"Offer {offerId} not found for craftsman {craftsmanId}");
                    return ServiceResponse<bool>
                        .FailureResponse("Offer not found or inaccessible.");
                }

                if (offer.Status == OfferStatus.Accepted)
                {
                    return ServiceResponse<bool>
                        .FailureResponse("You cannot withdraw an accepted offer.");
                }

                offer.Status = OfferStatus.Withdrawn;
                offer.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveAsync();

                _logger.LogInformation($"Craftsman {craftsmanId} withdrew offer {offerId}");

                return ServiceResponse<bool>.SuccessResponse(true, "Offer withdrawn successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error withdrawing offer {offerId}");
                return ServiceResponse<bool>
                    .FailureResponse($"Error withdrawing offer: {ex.Message}");
            }
        }

        // =====================================================================
        // ===== ORCHESTRATOR HELPER METHODS (Internal Use) =====
        // =====================================================================

        /// <summary>
        /// Get offer for acceptance validation (used by orchestrator)
        /// </summary>
        public async Task<OfferDto?> GetOfferForAcceptanceAsync(
            int offerId,
            int customerId)
        {
            var offer = await _unitOfWork.CraftsmanOffers
                .GetOfferForCustomerByIdAsync(offerId, customerId);

            if (offer == null)
            {
                _logger.LogWarning($"Offer {offerId} not found for customer {customerId}");
                return null;
            }

            if (offer.Status != OfferStatus.Pending)
            {
                _logger.LogWarning(
                    $"Offer {offerId} is not pending (Status: {offer.Status})");
                return null;
            }

            return offer.Adapt<OfferDto>();
        }

        /// <summary>
        /// Reserve offer (update status to Accepted) - used by orchestrator
        /// Does NOT save to database - orchestrator handles transaction
        /// </summary>
        public async Task ReserveOfferAsync(int offerId)
        {
            var offer = await _unitOfWork.CraftsmanOffers.GetByIdWithServiceRequestAsync(offerId);
            offer.Status = OfferStatus.Accepted;

            _logger.LogInformation($"Offer ya a7aaa {offer.CraftsmanOfferId} status set to Accepted");

            //No SaveAsync here - orchestrator manages the transaction
        }

        /// <summary>
        /// Reject other offers for the same service request - used by orchestrator
        /// Does NOT save to database - orchestrator handles transaction
        /// </summary>
        public async Task RejectOtherOffersAsync(
            int serviceRequestId,
            int acceptedOfferId,
            int customerId)
        {
            var allOffers = await _unitOfWork.CraftsmanOffers
                .GetOffersByServiceRequestIdAsync(serviceRequestId);

            var otherOffers = allOffers
                .Where(o => o.CraftsmanOfferId != acceptedOfferId)
                .ToList();

            foreach (var other in otherOffers)
            {
                other.Status = OfferStatus.Rejected;
                other.RejectedAt = DateTime.UtcNow;
                other.RejectionReason = "Another offer was accepted by the customer";
            }

            _logger.LogInformation(
                $"Set {otherOffers.Count} offers to Rejected for service request {serviceRequestId}");

            // No SaveAsync here - orchestrator manages the transaction
        }

        /// <summary>
        /// Revert offer reservation (compensation) - used by orchestrator on failure
        /// This DOES save to database - independent compensation operation
        /// </summary>
        public async Task RevertOfferReservationAsync(int offerId)
        {
            var offer = await _unitOfWork.CraftsmanOffers.GetByIdWithServiceRequestAsync(offerId);

            if (offer != null && offer.Status == OfferStatus.Accepted)
            {
                offer.Status = OfferStatus.Pending;
                offer.AcceptedAt = null;

                // Get all related offers that were rejected
                var relatedOffers = await _unitOfWork.CraftsmanOffers
                    .GetOffersForCustomerRequestAsync(offer.ServiceRequestId, offer.ServiceRequest.CustomerId);

                // Revert them back to Pending too
                foreach (var relatedOffer in relatedOffers.Where(o => o.Status == OfferStatus.Rejected))
                {
                    relatedOffer.Status = OfferStatus.Pending;
                    relatedOffer.RejectedAt = null;
                    relatedOffer.RejectionReason = null;
                }

                //await _unitOfWork.SaveAsync();

                _logger.LogWarning(
                    $"🔄 Reverted offer {offerId} to Pending (compensation). " +
                    $"Also reverted {relatedOffers.Count(o => o.Status == OfferStatus.Pending)} related offers");
            }
        }
    }
}