using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.Booking
{
    public class BookingAdminViewDto
    {
        public int BookingId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; internal set; }
    }
}
