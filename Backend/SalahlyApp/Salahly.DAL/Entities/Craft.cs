using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class Craft
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? NameAr { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Full URL of the craft icon from Cloudinary
        /// </summary>
        public string? IconUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }  // For sorting in UI

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public ICollection<ServiceRequest> ServiceRequests { get; set; }
        public ICollection<Craftsman> Craftsmen { get; set; } = new List<Craftsman>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();  // ✅ Now Booking has CraftId
    }
}
