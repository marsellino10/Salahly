using Microsoft.Extensions.Logging;
using Salahly.DAL.Interfaces;
using Salahly.DSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Services
{
    public class BookingCleanupService : IBookingCleanupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BookingCleanupService> _logger;

        public BookingCleanupService(
            IUnitOfWork unitOfWork,
            ILogger<BookingCleanupService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> CleanupUnpaidBookingsAsync()
        {
            try
            {
                _logger.LogInformation(
                    "[Cleanup Job] Starting cleanup of unpaid bookings at {Time}",
                    DateTime.UtcNow
                );

                var affectedCount = await _unitOfWork.Bookings.CleanupUnpaidBookingsAsync();

                if (affectedCount > 0)
                {
                    _logger.LogWarning(
                        "[Cleanup Job] Successfully marked {Count} booking(s) as unpaid at {Time}",
                        affectedCount,
                        DateTime.UtcNow
                    );
                }
                else
                {
                    _logger.LogInformation(
                        "[Cleanup Job] No bookings to cleanup at {Time}",
                        DateTime.UtcNow
                    );
                }

                return affectedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[Cleanup Job] Error occurred during cleanup at {Time}",
                    DateTime.UtcNow
                );
                throw;
            }
        }
    }
}
