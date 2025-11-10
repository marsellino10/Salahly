using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; } 
        public NotificationType Type { get; set; } 

        // Content
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        // Links
        public int? ServiceRequestId { get; set; }
        public int? CraftsmanOfferId { get; set; }
        public int? BookingId { get; set; }

        // Status
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser User { get; set; }
        public ServiceRequest? ServiceRequest { get; set; }
        public CraftsmanOffer? CraftsmanOffer { get; set; }
        public Booking? Booking { get; set; }
    }

   
    public enum NotificationType
    {
        NewServiceRequest = 1,
        NewOffer = 2,
        OfferAccepted = 3,
        OfferRejected = 4,
        BookingConfirmed = 5,
        BookingCancelled = 6,
        BookingCompleted = 7,
        NewReview = 8,
        PaymentReceived = 9
    }
}
