using Mapster;
using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.DTOs.CustomerDtos;
using Salahly.DSL.Interfaces;

namespace Salahly.DSL.Services
{
    public class CustomerServicecs : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CustomerServicecs(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(int id, int currentCustomerId)
        {
            if (id != currentCustomerId)
                return null;

            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return null;

            return customer.Adapt<CustomerResponseDto>();
        }
        public async Task<CustomerResponseDto?> UpdateAsync(int id, CustomerUpdateDto dto, int currentCustomerId)
        {
            if (id != currentCustomerId)
                return null;

            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
                return null;

            if (!string.IsNullOrEmpty(dto.Address)) customer.Address = dto.Address;
            if (!string.IsNullOrEmpty(dto.City)) customer.City = dto.City;
            if (!string.IsNullOrEmpty(dto.Area)) customer.Area = dto.Area;
            if (!string.IsNullOrEmpty(dto.PhoneNumber)) customer.PhoneNumber = dto.PhoneNumber;
            if (dto.DateOfBirth.HasValue) customer.DateOfBirth = dto.DateOfBirth.Value;

            await _unitOfWork.Customers.UpdateAsync(customer);
            await _unitOfWork.SaveAsync();

            return customer.Adapt<CustomerResponseDto>();
        }
        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Id = int.Parse(dto.UserId),
                Address = dto.Address,
                City = dto.City,
                Area = dto.Area,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth
            };
            _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveAsync();
            dto.FullName = customer.User.FullName;
            return customer.Adapt<CustomerResponseDto>();
        }

    }
}
