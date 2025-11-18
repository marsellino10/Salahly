using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;

namespace Salahly.DAL.Repositories
{
    public class ServiceRequestRepository : GenericRepository<ServiceRequest>, IServiceRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceRequest>> GetAllByCustomerAsync(int customerId)
        {
            return await _context.ServiceRequests
                .Include(r => r.AreaData)
                .Where(r => r.CustomerId == customerId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> DeleteByCustomerAsync(int id, int customerId)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.ServiceRequestId == id && r.CustomerId == customerId);
            if (request == null)
                return false;
            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        // Retrieves active service requests for a given craftsman based on their service areas and availability
        public async Task<IEnumerable<ServiceRequest>> GetActiveServiceRequestsForCraftsmanAsync(int craftsmanId)
        {
            try
            {
                var craftsman = await _context.Craftsmen
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == craftsmanId);

                if (craftsman == null || !craftsman.IsAvailable)
                    return new List<ServiceRequest>();

                // This stays IQueryable = no DB hit yet
                var craftsmanAreasQuery = _context.CraftsmanServiceAreas
                    .Where(csa => csa.CraftsmanId == craftsmanId)
                    .Select(csa => new { csa.Area.City, csa.Area.Region })
                    .AsQueryable();

                var hasAreas = await craftsmanAreasQuery.AnyAsync();
                if (!hasAreas)
                    return new List<ServiceRequest>();

                // Final query also stays IQueryable until ToListAsync()
                var query = _context.ServiceRequests
                    .AsNoTracking()
                    .Include(sr => sr.Customer).ThenInclude(c => c.User)
                    .Include(sr => sr.Craft)
                    .Include(sr => sr.AreaData)
                    .Where(sr =>
                        sr.CraftId == craftsman.CraftId &&
                        (sr.Status == ServiceRequestStatus.Open ||
                         sr.Status == ServiceRequestStatus.HasOffers) &&
                        sr.ExpiresAt > DateTime.UtcNow &&
                        sr.OffersCount < sr.MaxOffers &&
                        craftsmanAreasQuery.Any(area =>
                            area.City.Trim().ToLower() == sr.AreaData.City.Trim().ToLower() &&
                            area.Region.Trim().ToLower() == sr.AreaData.Region.Trim().ToLower()
                        )
                    )
                    .OrderByDescending(sr => sr.CreatedAt);

                return await query.ToListAsync();
            }
            catch
            {
                return new List<ServiceRequest>();
            }
        }

        //get all service requests craftsman has offered on

        public async Task<List<ServiceRequest>> GetServiceRequestsWithCraftsmanOffersAsync(int craftsmanId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsNoTracking()
                    .Include(sr => sr.Customer)
                        .ThenInclude(c => c.User)
                    .Include(sr => sr.Craft)
                    .Include(sr => sr.AreaData)
                    .Include(sr => sr.CraftsmanOffers.Where(o => o.CraftsmanId == craftsmanId))
                    .Where(sr => sr.CraftsmanOffers.Any(o => o.CraftsmanId == craftsmanId))
                    .OrderByDescending(sr => sr.CreatedAt)
                    .ToListAsync();

                return requests;
            }
            catch (Exception)
            {
                return new List<ServiceRequest>();
            }
        }

        public async Task<ServiceRequest?> GetServiceRequestForCraftsmanByIdAsync(int craftsmanId, int requestId)
        {
            try
            {
                var craftsman = await _context.Craftsmen
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == craftsmanId);

                if (craftsman == null)
                    return null;

                // Craftsman's service areas
                var craftsmanAreasQuery = _context.CraftsmanServiceAreas
                    .Where(csa => csa.CraftsmanId == craftsmanId)
                    .Select(csa => new { csa.Area.City, csa.Area.Region });

                var serviceRequest = await _context.ServiceRequests
                    .AsNoTracking()
                    .Include(sr => sr.Customer).ThenInclude(c => c.User)
                    .Include(sr => sr.Craft)
                    .Include(sr => sr.AreaData)
                    .FirstOrDefaultAsync(sr =>
                        sr.ServiceRequestId == requestId &&
                        sr.CraftId == craftsman.CraftId &&
                        craftsmanAreasQuery.Any(area =>
                            area.City.Trim().ToLower() == sr.AreaData.City.Trim().ToLower() &&
                            area.Region.Trim().ToLower() == sr.AreaData.Region.Trim().ToLower()
                        )
                    );

                return serviceRequest;
            }
            catch
            {
                return null;
            }
        }

        public Task<ServiceRequest?> GetServiceRequestByIdWithIncludesAsync(int id)
        {
            try
            {
                var serviceRequest = _context.ServiceRequests
                    .AsNoTracking()
                    .Include(sr => sr.Customer).ThenInclude(c => c.User)
                    .Include(sr => sr.Craft)
                    .Include(sr => sr.AreaData)
                    .FirstOrDefaultAsync(sr => sr.ServiceRequestId == id);
                return serviceRequest;
            }
            catch (Exception)
            {
                return Task.FromResult<ServiceRequest?>(null);
            }
        }
    }
}

