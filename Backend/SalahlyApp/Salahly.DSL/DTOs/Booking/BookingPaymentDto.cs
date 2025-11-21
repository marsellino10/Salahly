using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.Booking
{
    public class BookingPaymentDto
    {
        public int BookingId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentLink { get; set; }
        public string? PaymentToken { get; set; }
        public string? TransactionId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? PaymentDeadline { get; set; }
        public string? CraftsmanName { get; set; }
        public string? CraftName { get; set; }
    }
}
