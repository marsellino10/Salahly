using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Craftsman
    {
        public string Id { get; set; }
        public int CraftId { get; set; }
        public decimal RatingAverage { get; set; }
        public int TotalCompletedBookings { get; set; }

        public ApplicationUser User { get; set; }
        public Craft Craft { get; set; }

        public ICollection<PortfolioItem> Portfolio { get; set; } = new List<PortfolioItem>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
