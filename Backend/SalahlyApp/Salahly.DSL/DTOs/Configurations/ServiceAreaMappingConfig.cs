using Mapster;
using Salahly.DAL.Entities;
using System;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class ServiceAreaMappingConfig
    {
        public static void RegisterServiceAreaMappings(TypeAdapterConfig config)
        {
            // CraftsmanServiceArea to ServiceAreaDto (simplified DTO for responses)
            config.NewConfig<CraftsmanServiceArea, ServiceAreaDto>()
                .Map(dest => dest.AreaId, src => src.AreaId)
                .Map(dest => dest.Region, src => src.Area == null ? null : src.Area.Region)
                .Map(dest => dest.City, src => src.Area == null ? null : src.Area.City)
                .Map(dest => dest.ServiceRadiusKm, src => src.ServiceRadiusKm)
                .Map(dest => dest.IsActive, src => src.IsActive);

            // CraftsmanServiceArea to CraftsmanServiceAreaDto (detailed DTO with nested Area)
            config.NewConfig<CraftsmanServiceArea, CraftsmanServiceAreaDto>()
                .Map(dest => dest.AreaId, src => src.AreaId)
                .Map(dest => dest.Area, src => src.Area == null ? null : src.Area.Adapt<AreaDto>())
                .Map(dest => dest.ServiceRadiusKm, src => src.ServiceRadiusKm)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            // AddServiceAreaDto to CraftsmanServiceArea
            config.NewConfig<AddServiceAreaDto, CraftsmanServiceArea>()
                .Map(dest => dest.AreaId, src => src.AreaId)
                .Map(dest => dest.ServiceRadiusKm, src => src.ServiceRadiusKm)
                .Map(dest => dest.IsActive, src => true)
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);
        }
    }
}
