using Mapster;
using Salahly.DAL.Entities;
using Salahly.DSL.DTOs.PortfolioDtos;
using System;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class PortfolioItemMappingConfig
    {
        public static void RegisterPortfolioItemMappings(TypeAdapterConfig config)
        {
            // PortfolioItem to PortfolioItemResponseDto
            config.NewConfig<PortfolioItem, PortfolioItemResponseDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CraftsmanId, src => src.CraftsmanId)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            // CreatePortfolioItemDto to PortfolioItem
            config.NewConfig<CreatePortfolioItemDto, PortfolioItem>()
                .Map(dest => dest.CraftsmanId, src => src.CraftsmanId)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => true)
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);

            // UpdatePortfolioItemDto to PortfolioItem
            config.NewConfig<UpdatePortfolioItemDto, PortfolioItem>()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.ImageUrl, src => src.ImageUrl)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}
