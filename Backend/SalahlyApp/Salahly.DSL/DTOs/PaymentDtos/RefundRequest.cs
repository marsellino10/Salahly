using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.PaymentDtos
{
    public class RefundRequest
    {
        public int BookingId { get; set; }
        public int PaymentId { get; set; }
        public string? OriginalTransactionId { get; set; }
        public decimal RefundAmount { get; set; }
        public string? Reason { get; set; }
    }
}
