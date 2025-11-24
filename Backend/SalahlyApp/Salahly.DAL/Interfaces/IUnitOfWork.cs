using Microsoft.EntityFrameworkCore.Storage;
using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    public interface IUnitOfWork: IDisposable
    {
        IGenericRepository<Admin> Admins { get; }
        IGenericRepository<ApplicationUser> ApplicationUsers { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<Craft> Crafts { get; }
        IGenericRepository<Craftsman> Craftsmen { get; }
        ICraftsmanOfferRepository CraftsmanOffers { get; }
        IGenericRepository<CraftsmanServiceArea> CraftsmanServiceAreas { get; }
        IGenericRepository<Customer> Customers { get; }
        INotificationRepository Notifications { get; }
        IPaymentRepository Payments { get; }
        IGenericRepository<PortfolioItem> PortfolioItems { get; }
        IReviewRepository Reviews { get; }

        IGenericRepository<Area> Areas { get; }
        IServiceRequestRepository ServiceRequests { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        Task<int> SaveAsync(CancellationToken cancellationToken = default);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }

}
