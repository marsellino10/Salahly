using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using System;

namespace Salahly.DAL.Repositories
{
    public class CraftsmanOfferRepository : GenericRepository<CraftsmanOffer>, ICraftsmanOfferRepository
    {
        private readonly ApplicationDbContext _context;

        public CraftsmanOfferRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CraftsmanOffer>> GetOffersForCustomerRequestAsync(int requestId, int customerId)
        {
            try
            {
                return await _context.CraftsmanOffers
                    .AsNoTracking()
                    .Include(o => o.Craftsman).ThenInclude(c => c.User)
                    .Include(o => o.ServiceRequest)
                    .Where(o => o.ServiceRequestId == requestId &&
                                o.ServiceRequest.CustomerId == customerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch
            {
                return Enumerable.Empty<CraftsmanOffer>();
            }
        }

        public async Task<CraftsmanOffer?> GetOfferForCustomerByIdAsync(int offerId, int customerId)
        {
            try
            {
                return await _context.CraftsmanOffers
                    .Include(o => o.ServiceRequest)
                    .FirstOrDefaultAsync(o =>
                        o.CraftsmanOfferId == offerId &&
                        o.ServiceRequest.CustomerId == customerId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<CraftsmanOffer>> GetOffersByCraftsmanAsync(int craftsmanId)
        {
            try
            {
                return await _context.CraftsmanOffers
                    .AsNoTracking()
                    .Include(o => o.ServiceRequest).ThenInclude(sr => sr.Customer).ThenInclude(c => c.User)
                    .Include(o => o.ServiceRequest).ThenInclude(sr => sr.Craft)
                    .Where(o => o.CraftsmanId == craftsmanId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch
            {
                return Enumerable.Empty<CraftsmanOffer>();
            }
        }

        public async Task<CraftsmanOffer?> GetOfferByIdForCraftsmanAsync(int craftsmanId, int offerId)
        {
            try
            {
                return await _context.CraftsmanOffers
                    .Include(o => o.ServiceRequest)
                    .FirstOrDefaultAsync(o => o.CraftsmanOfferId == offerId && o.CraftsmanId == craftsmanId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<CraftsmanOffer?> GetByIdWithServiceRequestAsync(int id)
        {
            return await _context.CraftsmanOffers
                .Include(o => o.ServiceRequest)
                .FirstOrDefaultAsync(o => o.CraftsmanOfferId == id);
        }
    }
}
