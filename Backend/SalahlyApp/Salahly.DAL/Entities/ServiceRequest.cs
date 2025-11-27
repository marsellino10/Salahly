using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salahly.DAL.Entities
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }
        public int CustomerId { get; set; }
        public int CraftId { get; set; }

        // Service Details
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(2000)]
        public string Description { get; set; }

        // Location
        [Required, MaxLength(500)]
        public string Address { get; set; }

        [Required]
        public int AreaId { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Timing
        public DateTime AvailableFromDate { get; set; } 
        public DateTime AvailableToDate { get; set; }

        // Budget
        public decimal? CustomerBudget { get; set; }

        // Images
        public string? ImagesJson { get; set; }

        public int MaxOffers { get; set; } = 10;
        public string ? PaymentMethod { get; set; }

        // Status & Tracking
        public ServiceRequestStatus Status { get; set; }
        public int OffersCount { get; set; }
        public DateTime ExpiresAt { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; }
        public Craft Craft { get; set; }
        public ICollection<CraftsmanOffer> CraftsmanOffers { get; set; }
        public Booking? Booking { get; set; }

        public Area AreaData { get; set; }
    }

    public enum ServiceRequestStatus
    {
        Open = 0,
        OfferAccepted = 2,
        Completed = 4,
        Cancelled = 5,
        Expired = 6
    }
}