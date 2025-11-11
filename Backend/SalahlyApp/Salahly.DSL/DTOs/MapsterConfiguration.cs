using Mapster;
using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    public class MapsterConfiguration
    {
        public static void RegisterMappings()
        {
            // Create or use the global config
            var config = TypeAdapterConfig.GlobalSettings;

            // Map Craft -> CraftDto
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

            // Map CreateCraftDto -> Craft
            config.NewConfig<CreateCraftDto, Craft>()
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive)
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow);

            // Map UpdateCraftDto -> Craft
            config.NewConfig<UpdateCraftDto, Craft>()
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DisplayOrder, src => src.DisplayOrder)
                .Map(dest => dest.IsActive, src => src.IsActive);
        }
    }
}
