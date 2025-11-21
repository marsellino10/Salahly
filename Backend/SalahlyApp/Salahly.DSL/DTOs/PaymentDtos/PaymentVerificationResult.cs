using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.PaymentDtos
{
    public class PaymentVerificationResult
    {
        public bool IsSuccess { get; set; }
        public bool IsPaymentConfirmed { get; set; }
        public decimal PaidAmount { get; set; }
        public string? TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
