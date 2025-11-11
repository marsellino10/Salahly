using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.ServiceRequstDtos
{
    public class UpdateServiceRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public DateTime? PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }
        public decimal? CustomerBudget { get; set; }
    }

}
