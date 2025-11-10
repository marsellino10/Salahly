using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; } 
        public int CraftsmanId { get; set; }

        [Range(1, 5)]  
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public string? CraftsmanResponse { get; set; }  // Craftsman can reply
        public DateTime? ResponsedAt { get; set; }
        public bool IsVerified { get; set; }  // Admin verified review

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Booking Booking { get; set; }
        public Customer Customer { get; set; }
        public Craftsman Craftsman { get; set; }
    }
}
