using Mapster;
using Salahly.DSL.DTOs.Configurations;

namespace Salahly.DSL.DTOs
{
    /// <summary>
    /// Central orchestrator for Mapster configurations.
    /// Registers all entity-specific mapping configurations from the Configurations folder.
    /// </summary>
    public class MapsterConfiguration
    {
        public static void RegisterMappings()
        {
            var config = TypeAdapterConfig.GlobalSettings;

            // Register entity-specific configurations
            CraftMappingConfig.RegisterCraftMappings(config);
            AreaMappingConfig.RegisterAreaMappings(config);
            ServiceAreaMappingConfig.RegisterServiceAreaMappings(config);
            CraftsmanMappingConfig.RegisterCraftsmanMappings(config);
            PortfolioItemMappingConfig.RegisterPortfolioItemMappings(config);
        }
    }
}
