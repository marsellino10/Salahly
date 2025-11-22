using Salahly.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace Salahly.DAL.Entities
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int CraftsmanId { get; set; }
        public int CraftId { get; set; }

        // Links to Request & Offer
        public int? ServiceRequestId { get; set; }
        public int? AcceptedOfferId { get; set; }

        // Booking Details
        public DateTime BookingDate { get; set; }
        public int Duration { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
        public decimal RefundableAmount { get; set; } 
        public DateTime? PaymentDeadline { get; set; }

        public string? CancellationReason { get; set; }
        public string? CompletionNotes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; }
        public Craftsman Craftsman { get; set; }
        public Craft Craft { get; set; }
        public ServiceRequest? ServiceRequest { get; set; }
        public CraftsmanOffer? AcceptedOffer { get; set; }
        public Payment? Payment { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }

    public enum BookingStatus
    {
        Confirmed = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}