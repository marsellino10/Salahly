using Salahly.DAL.Entities;
using Salahly.DSL.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int craftsmanId);
        Task<IEnumerable<Review>> GetReviewsByBookingIdAsync(int BookingId);
        Task<double> GetAverageRatingForUser(int userId);
        Task<bool> HasUserReviewedAsync(int reviewerId, int BookingId);
    }
}
