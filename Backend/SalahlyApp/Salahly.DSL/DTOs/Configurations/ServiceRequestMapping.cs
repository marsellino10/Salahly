using Mapster;
using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.ServiceRequstDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class ServiceRequestMapping
    {
        public static void RegisterServiceAreaMappings(TypeAdapterConfig config)
        {
            // Mapping configurations for ServiceRequest related DTOs can be added here in the future
            //1. CreateServiceRequestDto to ServiceRequest entity
            config.NewConfig<CreateServiceRequestDto, ServiceRequest>()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Address, src => src.Address)
                .Map(dest => dest.AreaId, src => src.AreaId)
                .Map(dest => dest.Latitude, src => src.Latitude)
                .Map(dest => dest.Longitude, src => src.Longitude)
                .Map(dest => dest.PreferredDate, src => src.PreferredDate)
                .Map(dest => dest.PreferredTimeSlot, src => src.PreferredTimeSlot)
                .Map(dest => dest.CustomerBudget, src => src.CustomerBudget)
                .Map(dest => dest.ImagesJson, src => src.ImagesJson)
                .Map(dest => dest.MaxOffers, src => src.MaxOffers)
                .Map(dest => dest.CraftId, src => src.CraftId);
                

            //2. ServiceRequest to ServiceRequestDto
            config.NewConfig<ServiceRequest, ServiceRequestDto>()
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.CraftName, src => src.Craft.Name)
                .Map(dest => dest.City, src => src.AreaData.City)
                .Map(dest => dest.Area, src => src.AreaData.Region)
                .Map(dest => dest.CustomerName, src => src.Customer.User.FullName)
                .Map(dest => dest.Images, src => new List<string>())
                .AfterMapping((src, dest) =>
                {
                    if (!string.IsNullOrWhiteSpace(src.ImagesJson))
                    {
                        dest.Images = JsonSerializer
                            .Deserialize<List<string>>(src.ImagesJson) ?? new List<string>();
                    }
                    else
                    {
                        dest.Images = new List<string>();
                    }
                });

            
        }
    }
}
