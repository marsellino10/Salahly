using Mapster;
using Salahly.DAL.Entities;
using System;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class CraftMappingConfig
    {
        public static void RegisterCraftMappings(TypeAdapterConfig config)
        {
            // Craft to CraftDto
            config.NewConfig<Craft, CraftDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.IconUrl, src => src.IconUrl)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.CraftsmenCount, src => 0)
                .Map(dest => dest.ActiveServiceRequestsCount, src => 0);

            // CreateCraftDto to Craft
            config.NewConfig<CreateCraftDto, Craft>()
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);

            // UpdateCraftDto to Craft
            config.NewConfig<UpdateCraftDto, Craft>()
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}
