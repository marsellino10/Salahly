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

        public int ReviewerUserId { get; set; }
        public ApplicationUser? Reviewer { get; set; }

        public int TargetUserId { get; set; }
        public ApplicationUser? Target { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
