using Mapster;
using Salahly.DAL.Entities;
using System;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class PortfolioMappingConfig
    {
        public static void RegisterPortfolioMappings(TypeAdapterConfig config)
        {
            // PortfolioItem to PortfolioItemDto
            config.NewConfig<PortfolioItem, PortfolioItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive);

            // AddPortfolioItemDto to PortfolioItem
            config.NewConfig<AddPortfolioItemDto, PortfolioItem>()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => true)
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);
        }
    }
}
