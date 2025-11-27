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
            config.NewConfig<CreateServiceRequestDto, ServiceRequest>()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Address, src => src.Address)
                .Map(dest => dest.AreaId, src => src.AreaId)
                .Map(dest => dest.Latitude, src => src.Latitude)
                .Map(dest => dest.Longitude, src => src.Longitude)
                .Map(dest => dest.AvailableFromDate, src => src.AvailableFromDate)
                .Map(dest => dest.AvailableToDate, src => src.AvailableToDate)
                .Map(dest => dest.CustomerBudget, src => src.CustomerBudget)
                .Map(dest => dest.ImagesJson, src => src.ImagesJson)
                .Map(dest => dest.MaxOffers, src => src.MaxOffers)
                .Map(dest => dest.CraftId, src => src.CraftId);

            config.NewConfig<ServiceRequest, ServiceRequestDto>()
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.CraftName, src => src.Craft != null ? src.Craft.Name : null)
                .Map(dest => dest.City, src => src.AreaData != null ? src.AreaData.City : null)
                .Map(dest => dest.Area, src => src.AreaData != null ? src.AreaData.Region : null)
                .Map(dest => dest.CustomerName, src => src.Customer != null && src.Customer.User != null ? src.Customer.User.FullName : null)
                .Map(dest => dest.Images, src => ParseImages(src.ImagesJson));
        }

        private static List<string> ParseImages(string? imagesJson)
        {
            if (string.IsNullOrWhiteSpace(imagesJson))
            {
                return new List<string>();
            }

            try
            {
                var images = JsonSerializer.Deserialize<List<string>>(imagesJson);
                return images ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}