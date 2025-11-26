using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class Craftsman
    {
        public int Id { get; set; } 
        public int CraftId { get; set; }
        public int TotalCompletedBookings { get; set; }

        public bool IsAvailable { get; set; } = true; 
        public decimal? HourlyRate { get; set; }  
        public string? Bio { get; set; }  
        public int YearsOfExperience { get; set; }
        public bool IsVerified { get; set; } = false;
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public ApplicationUser User { get; set; }
        public Craft Craft { get; set; }

        // Relations
        public ICollection<CraftsmanServiceArea> CraftsmanServiceAreas { get; set; }
        public ICollection<CraftsmanOffer> CraftsmanOffers { get; set; }
        public ICollection<PortfolioItem> Portfolio { get; set; } = new List<PortfolioItem>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        //public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
    public enum VerificationStatus
    {
        Pending=0,
        Rejected=1,
        Verified=2
    }
}
