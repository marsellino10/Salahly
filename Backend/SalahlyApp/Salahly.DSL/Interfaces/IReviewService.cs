using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salahly.DSL.DTOs;

namespace Salahly.DSL.Interfaces
{

        public interface IReviewService
        {
            // Create a review (customer → craftsman OR craftsman → customer)
            Task<bool> CreateReviewAsync(CreateReviewDto dto);

            // Delete a review (for moderation or user removal)
            Task<bool> DeleteReviewAsync(int reviewId, int requestingUserId);

            // Get reviews for a specific user (craftsman or customer)
            Task<IEnumerable<CreateReviewDto>> GetReviewsForUserAsync(int userId);

            // Get reviews for a specific job (max 2 reviews: each direction)
            Task<IEnumerable<CreateReviewDto>> GetReviewsForBookingAsync(int BookingId);
            Task<double> GetAverageRatingForUser(int userId);

            // Check if reviewer already reviewed target for a job
            Task<bool> HasUserReviewedAsync(int reviewerId, int BookingId);
        }
}
