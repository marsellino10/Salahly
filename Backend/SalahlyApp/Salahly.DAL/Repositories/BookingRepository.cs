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
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<Booking?> GetByOfferIdAsync(int offerId)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.AcceptedOfferId == offerId);
        }

        public async Task<int> CleanupUnpaidBookingsAsync()
        {
            try
            {
                var result = await _context.Database
                    .SqlQueryRaw<int>("EXEC SP_CleanupUnpaidBookings")
                    .ToListAsync();

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing cleanup stored procedure: {ex.Message}", ex);
            }
        }
    }
}
