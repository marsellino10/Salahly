using Microsoft.EntityFrameworkCore;
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

        public IGenericRepository<Admin> Admins { get; }
        public IGenericRepository<ApplicationUser> ApplicationUsers { get; }
        public IGenericRepository<Booking> Bookings { get; }
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
            Bookings = new GenericRepository<Booking>(_context);
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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
