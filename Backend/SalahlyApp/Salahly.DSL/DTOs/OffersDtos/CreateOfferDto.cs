using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.OffersDtos
{
    public class CreateOfferDto
    {
        public int ServiceRequestId { get; set; }
        public decimal OfferedPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public int EstimatedDurationMinutes { get; set; }
        public DateTime AvailableFromDate { get; set; }
        public DateTime AvailableToDate { get; set; }
    }
}
