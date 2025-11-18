using Mapster;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersForCustomerRequestAsync(int customerId, int requestId)
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
                    return ServiceResponse<IEnumerable<OfferDto>>.FailureResponse("No offers found for this request.");

                return ServiceResponse<IEnumerable<OfferDto>>.SuccessResponse(dtoList);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<OfferDto>>.FailureResponse($"Error retrieving offers: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> AcceptOfferAsync(int customerId, int offerId)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers.GetOfferForCustomerByIdAsync(offerId, customerId);
                if (offer == null)
                    return ServiceResponse<bool>.FailureResponse("Offer not found or inaccessible.");

                offer.Status = OfferStatus.Accepted;
                offer.AcceptedAt = DateTime.UtcNow;

                // reject other offers on the same request
                var allOffers = await _unitOfWork.CraftsmanOffers.GetOffersForCustomerRequestAsync(offer.ServiceRequestId, customerId);
                var otherOffers = allOffers.Where(o => o.CraftsmanOfferId != offer.CraftsmanOfferId);

                foreach (var other in otherOffers)
                {
                    other.Status = OfferStatus.Rejected;
                    other.RejectedAt = DateTime.UtcNow;
                    other.RejectionReason = "Another offer was accepted by the customer.";
                }

                await _unitOfWork.SaveAsync();
                return ServiceResponse<bool>.SuccessResponse(true, "Offer accepted successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.FailureResponse($"Error accepting offer: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> RejectOfferAsync(int customerId, int offerId, RejectOfferDto dto)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers.GetOfferForCustomerByIdAsync(offerId, customerId);
                if (offer == null)
                    return ServiceResponse<bool>.FailureResponse("Offer not found or inaccessible.");

                var request = offer.ServiceRequest;

                // Check ServiceRequest status
                if (request.Status == ServiceRequestStatus.Completed ||
                    request.Status == ServiceRequestStatus.Cancelled ||
                    request.Status == ServiceRequestStatus.Expired)
                {
                    return ServiceResponse<bool>.FailureResponse("Cannot reject offer for a completed, cancelled, or expired request.");
                }

                // Check Offer status
                if (offer.Status == OfferStatus.Accepted)
                {
                    return ServiceResponse<bool>.FailureResponse("Cannot reject an offer that has already been accepted.");
                }

                offer.Status = OfferStatus.Rejected;
                offer.RejectedAt = DateTime.UtcNow;
                offer.RejectionReason = dto.RejectionReason;

                await _unitOfWork.SaveAsync();
                return ServiceResponse<bool>.SuccessResponse(true, "Offer rejected successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.FailureResponse($"Error rejecting offer: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<OfferDto>> CreateOfferAsync(int craftsmanId, CreateOfferDto dto)
        {
            try
            {
                // Validate service request exists (and open)
                var request = await _unitOfWork.ServiceRequests.GetByIdAsync(dto.ServiceRequestId);
                if (request == null)
                    return ServiceResponse<OfferDto>.FailureResponse("Service request not found.");

                // Verify the craftsman’s eligibility (same craft & area)
                var craftsman = await _unitOfWork.Craftsmen.GetByIdAsync(craftsmanId);
                if (craftsman == null)
                    return ServiceResponse<OfferDto>.FailureResponse("Craftsman not found.");

                if (craftsman.CraftId != request.CraftId)
                    return ServiceResponse<OfferDto>.FailureResponse("You can only offer for requests in your craft.");

                // TODO: optionally verify area match

                var offer = new CraftsmanOffer
                {
                    CraftsmanId = craftsmanId,
                    ServiceRequestId = dto.ServiceRequestId,
                    OfferedPrice = dto.OfferedPrice,
                    Description = dto.Description,
                    EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                    AvailableFromDate = dto.AvailableFromDate,
                    AvailableToDate = dto.AvailableToDate,
                    Status = OfferStatus.Pending
                };

                await _unitOfWork.CraftsmanOffers.AddAsync(offer);
                await _unitOfWork.SaveAsync();

                var offerDto = offer.Adapt<OfferDto>();
                return ServiceResponse<OfferDto>.SuccessResponse(offerDto, "Offer created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<OfferDto>.FailureResponse($"Error creating offer: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersByCraftsmanAsync(int craftsmanId)
        {
            try
            {
                var offers = await _unitOfWork.CraftsmanOffers.GetOffersByCraftsmanAsync(craftsmanId);
                var dto = offers.Adapt<IEnumerable<OfferDto>>();
                return ServiceResponse<IEnumerable<OfferDto>>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<OfferDto>>.FailureResponse($"Error retrieving offers: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<OfferDto>> GetOfferByIdForCraftsmanAsync(int craftsmanId, int offerId)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers.GetOfferByIdForCraftsmanAsync(craftsmanId, offerId);
                if (offer == null)
                    return ServiceResponse<OfferDto>.FailureResponse("Offer not found or inaccessible.");

                var dto = offer.Adapt<OfferDto>();
                return ServiceResponse<OfferDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                return ServiceResponse<OfferDto>.FailureResponse($"Error retrieving offer: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> WithdrawOfferAsync(int craftsmanId, int offerId)
        {
            try
            {
                var offer = await _unitOfWork.CraftsmanOffers.GetOfferByIdForCraftsmanAsync(craftsmanId, offerId);
                if (offer == null)
                    return ServiceResponse<bool>.FailureResponse("Offer not found or inaccessible.");

                if (offer.Status == OfferStatus.Accepted)
                    return ServiceResponse<bool>.FailureResponse("You cannot withdraw an accepted offer.");

                offer.Status = OfferStatus.Withdrawn;
                offer.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveAsync();
                return ServiceResponse<bool>.SuccessResponse(true, "Offer withdrawn successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.FailureResponse($"Error withdrawing offer: {ex.Message}");
            }
        }

    }
}
