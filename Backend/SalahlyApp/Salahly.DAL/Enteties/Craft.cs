using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Craft
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public ICollection<Craftsman> Craftsmen { get; set; } = new List<Craftsman>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
