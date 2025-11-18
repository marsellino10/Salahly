using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs.ServiceRequstDtos
{
    public class CreateServiceRequestDto
    {
        [Required]
        public int CraftId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(2000)]
        public string Description { get; set; }

        [Required, MaxLength(500)]
        public string Address { get; set; }

        //[Required, MaxLength(100)]
        //public string City { get; set; }

        //[Required, MaxLength(100)]
        //public string Area { get; set; }
        [Required]
        public int AreaId { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public DateTime PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }

        public decimal? CustomerBudget { get; set; }
        public string? ImagesJson { get; set; }

        public int MaxOffers { get; set; } = 10;
    }
}
