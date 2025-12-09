using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.OffersDtos
{
    public class OfferDto
    {
        public int ServiceRequestId { get; set; }
        public int CraftsmanId { get; set; }
        public int CraftsmanOfferId { get; set; }
        public decimal OfferedPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public int EstimatedDurationMinutes { get; set; }
        public DateTime PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }
        public string CraftsmanName { get; set; } = string.Empty;
        public OfferStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string craftsmanProfileImageUrl { get; set; } = string.Empty;
    }
}
