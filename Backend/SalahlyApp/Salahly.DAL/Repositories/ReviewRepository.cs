using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _context.Reviews.Where(r => r.TargetUserId == userId).ToListAsync();
        }
        public async Task<IEnumerable<Review>> GetReviewsByBookingIdAsync(int BookingId)
        {
            return await _context.Reviews.Where(r => r.BookingId == BookingId).ToListAsync();
        }

        public async Task<bool> HasUserReviewedAsync(int reviewerId, int BookingId)
        {
            return await _context.Reviews.AnyAsync(r => r.ReviewerUserId == reviewerId && r.BookingId == BookingId);
        }

        public async Task<double> GetAverageRatingForUser(int userId)
        {
            return await _context.Reviews.Where(r => r.TargetUserId == userId).AverageAsync(r => r.Rating);
        }
    }
}
