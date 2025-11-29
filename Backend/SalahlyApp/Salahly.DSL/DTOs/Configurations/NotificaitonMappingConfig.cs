using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Salahly.DAL.Entities;

namespace Salahly.DSL.DTOs.Configurations
{
    public static class NotificaitonMappingConfig
    {
        public static void RegisterNotificationMappings(TypeAdapterConfig config)
        {
            config.NewConfig<Notification, NotificationDto>()
                .Map(dest => dest.NotificationId, src => src.NotificationId);
        }
    }
}
