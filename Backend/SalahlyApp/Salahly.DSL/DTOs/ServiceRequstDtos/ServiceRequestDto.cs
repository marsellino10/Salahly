using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.ServiceRequstDtos
{
    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Address { get; set; }
        public DateTime AvailableFromDate { get; set; }
        public DateTime AvailableToDate { get; set; }
        public decimal? CustomerBudget { get; set; }
        public string Status { get; set; }
        public int OffersCount { get; set; }
        public int MaxOffers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string CraftName { get; set; }
        public string CustomerName { get; set; }
        public List<string> Images { get; set; }
    }
}
