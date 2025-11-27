using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class CraftsmanOffer
    {
        public int CraftsmanOfferId { get; set; }
        public int ServiceRequestId { get; set; }
        public int CraftsmanId { get; set; }

        // Offer Details
        public decimal OfferedPrice { get; set; }
        public string Description { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        // Availability
        public DateTime PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }

        // Status
        public OfferStatus Status { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation Properties
        public ServiceRequest ServiceRequest { get; set; }
        public Craftsman Craftsman { get; set; }
        public Booking? Booking { get; set; }
    }

    public enum OfferStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Withdrawn = 3,
        Expired = 4
    }
}
