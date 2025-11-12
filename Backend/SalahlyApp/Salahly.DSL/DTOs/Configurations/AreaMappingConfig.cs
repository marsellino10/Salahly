using Mapster;
using Salahly.DAL.Entities;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class AreaMappingConfig
    {
        public static void RegisterAreaMappings(TypeAdapterConfig config)
        {
            // Area to AreaDto
            config.NewConfig<Area, AreaDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Region, src => src.Region)
                .Map(dest => dest.City, src => src.City);

            // CreateAreaDto to Area
            config.NewConfig<CreateAreaDto, Area>()
                .Map(dest => dest.Region, src => src.Region)
                .Map(dest => dest.City, src => src.City);

            // UpdateAreaDto to Area
            config.NewConfig<UpdateAreaDto, Area>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Region, src => src.Region)
                .Map(dest => dest.City, src => src.City);
        }
    }
}
