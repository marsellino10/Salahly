using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.PaymentDtos
{
    public class RefundResult
    {
        public bool IsSuccess { get; set; }
        public string? RefundTransactionId { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime RefundDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
