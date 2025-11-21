using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.PaymentDtos
{
    public class PaymentInitializationRequest
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerName { get; set; }
        public string? CustomerAddress { get; set; }

        // Metadata
        public string CraftName { get; set; }
        public string CraftsmanName { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
