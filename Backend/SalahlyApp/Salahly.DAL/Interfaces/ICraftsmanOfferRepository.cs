using Salahly.DAL.Entities;

namespace Salahly.DAL.Interfaces
{
    public interface ICraftsmanOfferRepository : IGenericRepository<CraftsmanOffer> 
    {
        Task<IEnumerable<CraftsmanOffer>> GetOffersForCustomerRequestAsync(int requestId, int customerId);
        Task<CraftsmanOffer?> GetOfferForCustomerByIdAsync(int offerId, int customerId);

        // Craftsman methods
        Task<IEnumerable<CraftsmanOffer>> GetOffersByCraftsmanAsync(int craftsmanId);
        Task<CraftsmanOffer?> GetOfferByIdForCraftsmanAsync(int craftsmanId, int offerId);
    }
}
