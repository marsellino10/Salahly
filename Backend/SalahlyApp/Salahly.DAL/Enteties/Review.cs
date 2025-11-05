using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Review
    {
        public int Id { get; set; }

        public int BookingId { get; set; }       
        public string CustomerId { get; set; }    
        public string CraftsmanId { get; set; }   

        public int Rating { get; set; }          
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Booking Booking { get; set; }
        public Customer Customer { get; set; }
        public Craftsman Craftsman { get; set; }
    }
}
