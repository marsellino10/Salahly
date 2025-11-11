using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.ServiceRequstDtos
{
    public class ServiceRequestResponseDto
    {
        public int ServiceRequestId { get; set; }
        public int CustomerId { get; set; }
        public int CraftId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Area { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }

        public decimal? CustomerBudget { get; set; }
        public string? ImagesJson { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
