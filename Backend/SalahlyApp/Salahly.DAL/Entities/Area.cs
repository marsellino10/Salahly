using System.Collections.Generic;

namespace Salahly.DAL.Entities
{
    public class Area
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string City { get; set; }

        // Navigation - craftsmen service areas that reference this Area
        public ICollection<CraftsmanServiceArea> CraftsmanServiceAreas { get; set; } = new List<CraftsmanServiceArea>();
    }
}
