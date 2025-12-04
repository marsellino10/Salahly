using Mapster;
using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.CustomerDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class CustomerMappingConfig
    {
        public static void RegisterCustomerMappings(TypeAdapterConfig config)
        {
            // Currently, there are no specific mappings defined for Customer.
            // This method serves as a placeholder for future Customer-related mapping configurations.

            // from Customer to CustomerResponseDto
            config.NewConfig<Customer, CustomerResponseDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullName, src => src.User.FullName)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Address, src => src.Address)
                .Map(dest => dest.City, src => src.City)
                .Map(dest => dest.Area, src => src.Area)
                .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
                .Map(dest => dest.ProfileImageUrl, src => src.User.ProfileImageUrl)
                .IgnoreNullValues(true);
        }
    }
}
