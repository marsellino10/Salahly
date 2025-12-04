using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CustomerServicecs> _logger;
        public CustomerServicecs(IUnitOfWork unitOfWork, ILogger<CustomerServicecs> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CustomerResponseDto?> GetByIdAsync(int id, int currentCustomerId)
        {
            if (id != currentCustomerId)
                return null;

            var customer = await _unitOfWork.Customers.GetAll()
                .Include(c=> c.User).FirstOrDefaultAsync(c=> c.Id == id);
            if (customer == null)
                return null;

            return customer.Adapt<CustomerResponseDto>();
        }
        public async Task<CustomerResponseDto?> UpdateAsync(int id, CustomerUpdateDto dto, int currentCustomerId)
        {
            if (id != currentCustomerId)
                return null;

            var customer = await _unitOfWork.Customers.GetAll()
                .Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
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
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.UserId <= 0)
                throw new ArgumentException("Invalid user ID", nameof(dto.UserId));

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(dto.UserId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");

            var customer = new Customer
            {
                Id = dto.UserId,
                Address = dto.Address,
                City = dto.City,
                Area = dto.Area,
                PhoneNumber = dto.PhoneNumber,
                DateOfBirth = dto.DateOfBirth
            };
            //dto.FullName = customer.User.FullName;
            await _unitOfWork.Customers.AddAsync(customer);
            user.IsProfileCompleted = true;
            await _unitOfWork.ApplicationUsers.UpdateAsync(user);
            await _unitOfWork.SaveAsync();
            return customer.Adapt<CustomerResponseDto>();
        }
        private async Task<Customer?> GetByIdWithIncludesAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _unitOfWork.Customers
                .GetAll()
                .Include(c => c.User)
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CustomerResponseDto> UpdateCustomerImageAsync(int customerId, string profileImageUrl)
        {
            if (customerId <= 0)
                throw new ArgumentException("Invalid customer ID", nameof(customerId));

            if (string.IsNullOrWhiteSpace(profileImageUrl))
                throw new ArgumentException("Profile image URL cannot be empty", nameof(profileImageUrl));

            var customer = await GetByIdWithIncludesAsync(customerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            // Update user profile image if user exists
            if (customer.User != null)
            {
                customer.User.ProfileImageUrl = profileImageUrl;

                // User is already tracked because of includes
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Profile image updated for customer ID: {CustomerId}", customerId);
            }
            else
            {
                _logger.LogWarning("No associated user found for customer ID: {CustomerId}", customerId);
            }

            // Map to DTO and return result
            var result = customer.Adapt<CustomerResponseDto>();
            return result ?? throw new InvalidOperationException("Failed to retrieve updated customer");
        }

    }
}
