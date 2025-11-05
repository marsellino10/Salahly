using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Admin
    {
        public string Id { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
    }
}
