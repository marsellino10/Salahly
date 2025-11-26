using Mapster;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;
using System.Text.Json;

namespace Salahly.DSL.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ServiceRequestService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<ServiceRequestResponseDto> CreateAsync(CreateServiceRequestDto dto, int customerId)
        {
            try
            {
                // Map from DTO to Entity using Mapster
                var entity = dto.Adapt<ServiceRequest>();
                entity.CustomerId = customerId;
                entity.Status = ServiceRequestStatus.Open;
                entity.CreatedAt = DateTime.UtcNow;
                entity.ExpiresAt = DateTime.UtcNow.AddDays(7);

                // Add to DB via UnitOfWork
                await _unitOfWork.ServiceRequests.AddAsync(entity);
                await _unitOfWork.SaveAsync();
                await _notificationService.NotifyCraftsmenInAreaAsync(entity);
                // Map back to response DTO
                var response = entity.Adapt<ServiceRequestResponseDto>();
                response.Status = entity.Status.ToString();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating service request: " + ex.Message);
            }
        }

        public async Task<ServiceRequestDto?> GetByIdAsync(int id, int customerId)
        {
            try
            {
                var request = await _unitOfWork.ServiceRequests.GetServiceRequestByIdWithIncludesAsync(id);

                if (request == null || request.CustomerId != customerId)
                    return null;


                var dto = request.Adapt<ServiceRequestDto>();

                return dto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving service request: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAllByCustomerAsync(int customerId)
        {
            var requests = await _unitOfWork.ServiceRequests.GetAllByCustomerAsync(customerId);
            return requests.Adapt<IEnumerable<ServiceRequestDto>>();
        }

        public async Task<bool> DeleteAsync(int id, int customerId)
        {
            try
            {
                var request = await _unitOfWork.ServiceRequests.GetByIdAsync(id);
                if (request == null || request.CustomerId != customerId)
                    return false;
                await _unitOfWork.ServiceRequests.DeleteByCustomerAsync(id,customerId);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting service request: {ex.Message}");
            }
        }

        public async Task<ServiceRequestResponseDto?> UpdateAsync(int id, UpdateServiceRequestDto dto, int customerId)
        {
            var request = await _unitOfWork.ServiceRequests.GetByIdAsync(id);
            if (request == null || request.CustomerId != customerId)
                return null;

            if (request.Status != ServiceRequestStatus.Open)
                throw new Exception("Cannot update a request that is not open.");

            if (!string.IsNullOrEmpty(dto.Title)) request.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) request.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Address)) request.Address = dto.Address;

            var area = await _unitOfWork.Areas.GetByIdAsync(dto.AreaId);
            if (area is not null) request.AreaId = dto.AreaId;
            if (dto.PreferredDate.HasValue) request.PreferredDate = dto.PreferredDate.Value;
            if (!string.IsNullOrEmpty(dto.PreferredTimeSlot)) request.PreferredTimeSlot = dto.PreferredTimeSlot;
            if (dto.CustomerBudget.HasValue) request.CustomerBudget = dto.CustomerBudget.Value;

            request.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
            return request.Adapt<ServiceRequestResponseDto>();
        }

        public async Task<ServiceResponse<IEnumerable<ServiceRequestDto>>> GetRequestsWithCraftsmanOffersAsync(int craftsmanId)
        {
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequests
                    .GetServiceRequestsWithCraftsmanOffersAsync(craftsmanId);

                var serviceRequestDtos = serviceRequests
                    .Select(sr => MapToDto(sr))
                    .ToList();

                if (!serviceRequestDtos.Any())
                {
                    return ServiceResponse<IEnumerable<ServiceRequestDto>>.FailureResponse(
                        "This craftsman has not made any offers yet."
                    );
                }

                return ServiceResponse<IEnumerable<ServiceRequestDto>>.SuccessResponse(
                    serviceRequestDtos,
                    $"Found {serviceRequestDtos.Count} service requests with offers from this craftsman."
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<ServiceRequestDto>>.FailureResponse(
                    $"Error retrieving service requests with craftsman offers: {ex.Message}"
                );
            }
        }

        public async Task<ServiceResponse<ServiceRequestDto>> GetServiceRequestForCraftsmanAsync(int craftsmanId, int requestId)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests
                    .GetServiceRequestForCraftsmanByIdAsync(craftsmanId, requestId);

                if (serviceRequest == null)
                    return ServiceResponse<ServiceRequestDto>.FailureResponse("Request not accessible by this craftsman.");

                var dto = MapToDto(serviceRequest);
                return ServiceResponse<ServiceRequestDto>.SuccessResponse(dto, "Service request retrieved successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<ServiceRequestDto>.FailureResponse($"Error retrieving service request: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<ServiceRequestDto>>> GetAvailableOpportunitiesAsync(int craftsmanId)
        {
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequests
                    .GetActiveServiceRequestsForCraftsmanAsync(craftsmanId);

                var serviceRequestDtos = serviceRequests
                    .Select(sr => MapToDto(sr))
                    .ToList();

                return ServiceResponse<IEnumerable<ServiceRequestDto>>.SuccessResponse(
                    serviceRequestDtos,
                    $"Found {serviceRequestDtos.Count} available opportunities"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<ServiceRequestDto>>.FailureResponse(
                    $"Error retrieving opportunities: {ex.Message}"
                );
            }
        }
        
        public async Task ChangeStatusAsync(int id, ServiceRequestStatus status)
        {
             await _unitOfWork.ServiceRequests.ChangeStatusAsync(id, status);
        }


        private ServiceRequestDto MapToDto(ServiceRequest sr)
        {
            var dto = sr.Adapt<ServiceRequestDto>();

            dto.Status = sr.Status.ToString();
            dto.CraftName = sr.Craft?.Name;
            dto.CustomerName = sr.Customer?.User?.FullName ?? "Unknown";
            dto.Images = string.IsNullOrEmpty(sr.ImagesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(sr.ImagesJson);

            return dto;
        }
    }
}