
using Salahly.DSL.Interfaces;

namespace Salahly.API.BackgroundJobs
{
    public class BookingCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCleanupHostedService> _logger;
        private readonly TimeSpan _interval;

        public BookingCleanupHostedService(
            IServiceProvider serviceProvider,
            ILogger<BookingCleanupHostedService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Read interval from appsettings (default: 60 minutes)
            var intervalMinutes = configuration.GetValue<int>(
                "BackgroundJobs:CleanupIntervalMinutes",
                60
            );
            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Booking Cleanup Background Service started. Running every {Interval} minutes.",
                _interval.TotalMinutes
            );

            // Wait a bit before first execution (give app time to fully start)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error in Booking Cleanup Background Service"
                    );
                }

                // Wait for next interval
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup job triggered at {Time}", DateTime.UtcNow);

            using (var scope = _serviceProvider.CreateScope())
            {
                var cleanupService = scope.ServiceProvider
                    .GetRequiredService<IBookingCleanupService>();

                await cleanupService.CleanupUnpaidBookingsAsync();
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Cleanup Background Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}