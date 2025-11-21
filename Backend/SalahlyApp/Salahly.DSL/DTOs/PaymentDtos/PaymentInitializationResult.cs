using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.PaymentDtos
{
    public class PaymentInitializationResult
    {
        public bool IsSuccess { get; set; }
        public string? PaymentLink { get; set; }
        public string? PaymentToken { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? MetaData { get; set; }
    }
}
