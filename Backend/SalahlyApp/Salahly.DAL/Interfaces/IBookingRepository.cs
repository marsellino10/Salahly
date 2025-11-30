using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<Booking?> GetByOfferIdAsync(int offerId);
        Task<int> CleanupUnpaidBookingsAsync();
    }
}
