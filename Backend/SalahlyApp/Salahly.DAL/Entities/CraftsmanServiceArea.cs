using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class CraftsmanServiceArea
    {
        //public int CraftsmanServiceAreaId { get; set; }
        public int CraftsmanId { get; set; } 

        // Link to canonical Area entity
        public int? AreaId { get; set; }
        public Area? Area { get; set; }

        // Coverage radius
        public int ServiceRadiusKm { get; set; }

        
        public bool IsActive { get; set; } = true;  // Can disable area temporarily

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public Craftsman Craftsman { get; set; }
    }
}
