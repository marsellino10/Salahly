using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class CraftsmanServiceArea
    {
        public int CraftsmanServiceAreaId { get; set; }
        public string CraftsmanId { get; set; } 

        // Area Coverage
        public string City { get; set; }
        public string Area { get; set; }
        public int ServiceRadiusKm { get; set; }

      
        public bool IsActive { get; set; } = true;  // Can disable area temporarily

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public Craftsman Craftsman { get; set; }
    }
}
