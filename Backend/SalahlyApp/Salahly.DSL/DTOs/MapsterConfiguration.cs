using Mapster;
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

        }
    }
}
