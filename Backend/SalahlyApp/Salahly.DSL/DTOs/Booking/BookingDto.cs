using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.Booking
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? PaymentDeadline { get; set; }

        // Customer Info
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }

        // Craftsman Info
        public int CraftsmanId { get; set; }
        public string CraftsmanName { get; set; }
        public string? CraftsmanPhone { get; set; }
        public decimal? CraftsmanRating { get; set; }

        // Service Info
        public int CraftId { get; set; }
        public string CraftName { get; set; }
        public string? ServiceDescription { get; set; }

        // Payment Info
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public bool CanRetryPayment { get; set; }
        public string? LastPaymentFailureReason { get; set; }

        // Additional
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; internal set; }
    }
}
