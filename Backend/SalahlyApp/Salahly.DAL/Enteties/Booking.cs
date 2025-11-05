using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Enteties
{
    public class Booking
    {
        public int Id { get; set; }

        public string CustomerId { get; set; }  
        public string CraftsmanId { get; set; }  

        public int CraftId { get; set; }       
        public DateTime BookingDate { get; set; } 
        public DateTime? CompletedAt { get; set; } 

        public decimal Price { get; set; }
        public BookingStatus Status { get; set; }  

        // Navigation properties
        public Customer Customer { get; set; }
        public Craftsman Craftsman { get; set; }
        public Craft Craft { get; set; }
        public Payment Payment { get; set; }
        public Review Review { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Completed,
        Canceled
    }
}
