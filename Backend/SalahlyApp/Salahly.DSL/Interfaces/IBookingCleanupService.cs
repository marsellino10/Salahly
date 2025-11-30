using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces
{
    public interface IBookingCleanupService
    {
        Task<int> CleanupUnpaidBookingsAsync();
    }
}
