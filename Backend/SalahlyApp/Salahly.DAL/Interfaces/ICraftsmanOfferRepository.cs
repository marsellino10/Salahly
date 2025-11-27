using Salahly.DAL.Entities;
using System;

namespace Salahly.DAL.Interfaces
{
    public interface ICraftsmanOfferRepository : IGenericRepository<CraftsmanOffer> 
    {
        Task<IEnumerable<CraftsmanOffer>> GetOffersForCustomerRequestAsync(int requestId, int customerId);
        Task<CraftsmanOffer?> GetOfferForCustomerByIdAsync(int offerId, int customerId);

        // Craftsman methods
        Task<IEnumerable<CraftsmanOffer>> GetOffersByCraftsmanAsync(int craftsmanId);
        Task<CraftsmanOffer?> GetOfferByIdForCraftsmanAsync(int craftsmanId, int offerId);

        // General methods
        Task<CraftsmanOffer> GetByIdWithServiceRequestAsync(int id);

        Task<IQueryable<CraftsmanOffer>> GetOffersByServiceRequestIdAsync(int serviceRequestId);
    }
}
