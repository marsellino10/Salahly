using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Entities
{
    public class PortfolioItem
    {
        public int Id { get; set; }
        public int CraftsmanId { get; set; } 

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int DisplayOrder { get; set; }  // For sorting
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Craftsman Craftsman { get; set; }
    }
}
