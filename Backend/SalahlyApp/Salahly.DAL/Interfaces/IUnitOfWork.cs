using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<Admin> Admins { get; }
        IGenericRepository<ApplicationUser> ApplicationUsers { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Craft> Crafts { get; }
        IGenericRepository<Craftsman> Craftsmen { get; }
        IGenericRepository<CraftsmanOffer> CraftsmanOffers { get; }
        IGenericRepository<CraftsmanServiceArea> CraftsmanServiceAreas { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<PortfolioItem> PortfolioItems { get; }
        IGenericRepository<Review> Reviews { get; }
        IServiceRequestRepository ServiceRequests { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);
    }

}
