using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; }

        [MaxLength(200)]
        public string? TransactionId { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        public string? PaymentGateway { get; set; } 
        public string? FailureReason { get; set; } 

        // Navigation
        public Booking Booking { get; set; }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3  
    }
}
