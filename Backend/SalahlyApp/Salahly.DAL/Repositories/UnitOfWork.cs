using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Salahly.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        public IGenericRepository<Admin> Admins { get; }
        public IGenericRepository<ApplicationUser> ApplicationUsers { get; }
        public IBookingRepository Bookings { get; }
        public IGenericRepository<Craft> Crafts { get; }
        public IGenericRepository<Craftsman> Craftsmen { get; }
        public ICraftsmanOfferRepository CraftsmanOffers { get; }
        public IGenericRepository<CraftsmanServiceArea> CraftsmanServiceAreas { get; }
        public IGenericRepository<Customer> Customers { get; }
        public INotificationRepository Notifications { get; }
        public IPaymentRepository Payments { get; }
        public IGenericRepository<PortfolioItem> PortfolioItems { get; }
        //public IGenericRepository<Review> Reviews { get; }
        public IReviewRepository Reviews { get; }

        public IGenericRepository<Area> Areas { get; }
        public IServiceRequestRepository ServiceRequests { get; }
        public IRefreshTokenRepository RefreshTokens { get; }


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Admins = new GenericRepository<Admin>(_context);
            ApplicationUsers = new GenericRepository<ApplicationUser>(_context);
            Bookings = new BookingRepository(_context);
            Crafts = new GenericRepository<Craft>(_context);
            Craftsmen = new GenericRepository<Craftsman>(_context);
            CraftsmanOffers = new CraftsmanOfferRepository(_context);
            CraftsmanServiceAreas = new GenericRepository<CraftsmanServiceArea>(_context);
            Customers = new GenericRepository<Customer>(_context);
            Notifications = new NotificationRepository(_context);
            Payments = new PaymentRepositery(_context);
            PortfolioItems = new GenericRepository<PortfolioItem>(_context);
            Reviews = new ReviewRepository(_context);

            Areas = new GenericRepository<Area>(_context);
            ServiceRequests = new ServiceRequestRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);
        }

        public Task<int> SaveAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            });

            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var result = await operation();
                    await SaveAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}
