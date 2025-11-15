using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerResponseDto?> GetByIdAsync(int id, int currentCustomerId);
        Task<CustomerResponseDto?> UpdateAsync(int id, CustomerUpdateDto dto, int currentCustomerId);
        Task<CustomerResponseDto?> CreateAsync(CreateCustomerDto dto);
    }
}
