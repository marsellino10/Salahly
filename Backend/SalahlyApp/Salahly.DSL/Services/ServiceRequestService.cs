using Mapster;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using Salahly.DSL.Interfaces;

namespace Salahly.DSL.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                var request = await _unitOfWork.ServiceRequests.GetByIdAsync(id);

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
            if (!string.IsNullOrEmpty(dto.City)) request.City = dto.City;
            if (!string.IsNullOrEmpty(dto.Area)) request.Area = dto.Area;
            if (dto.PreferredDate.HasValue) request.PreferredDate = dto.PreferredDate.Value;
            if (!string.IsNullOrEmpty(dto.PreferredTimeSlot)) request.PreferredTimeSlot = dto.PreferredTimeSlot;
            if (dto.CustomerBudget.HasValue) request.CustomerBudget = dto.CustomerBudget.Value;

            request.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
            return request.Adapt<ServiceRequestResponseDto>();
        }
    }
}