using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salahly.DAL.Entities;

namespace Salahly.DSL.DTOs
{
    public class CreateNotificationDto
    {
       public int userId { get; set; }
        public NotificationType type { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public string? actionUrl { get; set; } = null;
        public int? serviceRequestId { get; set; } = null;
        public int? craftsmanOfferId { get; set; } = null;
        public int? bookingId { get; set; } = null;
    }
}
