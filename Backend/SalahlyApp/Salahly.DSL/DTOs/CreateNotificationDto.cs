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
       public IEnumerable<int> UserIds { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? ActionUrl { get; set; } = null;
        public int? ServiceRequestId { get; set; } = null;
        public int? CraftsmanOfferId { get; set; } = null;
        public int? BookingId { get; set; } = null;
    }
}
