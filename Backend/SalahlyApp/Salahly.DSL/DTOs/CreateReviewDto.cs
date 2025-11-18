using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.DTOs
{
    public class CreateReviewDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Reviewer User ID must be greater than 0")]
        public int ReviewerUserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Target User ID must be greater than 0")]
        public int TargetUserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Booking ID must be greater than 0")]
        public int BookingId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; }
    }
}
