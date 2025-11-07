using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class Admin
    {
        public string Id { get; set; } 
        public string? Department { get; set; }
        public DateTime? HiredAt { get; set; }

        public ApplicationUser User { get; set; }
    }
}
