using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Customer
    {
        public string Id { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }

        // Relations
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
