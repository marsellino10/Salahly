using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.ServiceRequstDtos
{
    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }

        // Basic Info
        public string Title { get; set; }
        public string Description { get; set; }

        // Location
        public string Address { get; set; }
        public string City { get; set; }
        public string Area { get; set; }

        // Timing
        public DateTime PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }

        // Budget
        public decimal? CustomerBudget { get; set; }

        // Images
        public string? ImagesJson { get; set; }

        // Status
        public string Status { get; set; }

        // Other Info
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
