using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public interface IServiceRequestService
    {
        Task<ServiceRequestResponseDto> CreateAsync(CreateServiceRequestDto dto, int customerId);
        Task<ServiceRequestDto?> GetByIdAsync(int id, int customerId);
        Task<IEnumerable<ServiceRequestDto>> GetAllByCustomerAsync(int customerId);
        Task<bool> DeleteAsync(int id, int customerId);
        Task<bool> ChangeStatusAsync(int id, ServiceRequestStatus status);

        Task<ServiceRequestResponseDto> UpdateAsync(int id, UpdateServiceRequestDto dto, int customerId);
        Task<ServiceResponse<IEnumerable<ServiceRequestDto>>> GetAvailableOpportunitiesAsync(int craftsmanId);
        Task<ServiceResponse<IEnumerable<ServiceRequestDto>>> GetRequestsWithCraftsmanOffersAsync(int craftsmanId);
        Task<ServiceResponse<ServiceRequestDto>> GetServiceRequestForCraftsmanAsync(int craftsmanId, int requestId);
    }
}
