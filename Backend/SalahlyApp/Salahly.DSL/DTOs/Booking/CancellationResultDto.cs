using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.Booking
{
    public class CancellationResultDto
    {
        public int BookingId { get; set; }
        public DateTime CancellationDate { get; set; }
        public decimal RefundAmount { get; set; }
        public int RefundPercentage { get; set; }
        public string? Message { get; set; }
    }
}
