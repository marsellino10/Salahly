using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }         
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public PaymentStatus Status { get; set; }     
        public string TransactionId { get; set; }   
        public string PaymentMethod { get; set; }  

        // Navigation
        public Booking Booking { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }
}
