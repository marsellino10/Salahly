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

        Task<ServiceRequestResponseDto> UpdateAsync(int id, UpdateServiceRequestDto dto, int customerId);
    }
}
