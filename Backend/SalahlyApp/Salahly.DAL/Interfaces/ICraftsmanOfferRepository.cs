using Salahly.DAL.Entities;

namespace Salahly.DAL.Interfaces
{
    public interface ICraftsmanOfferRepository 
    {
        Task<IEnumerable<CraftsmanOffer>> GetOffersForCustomerRequestAsync(int requestId, int customerId);
        Task<CraftsmanOffer?> GetOfferForCustomerByIdAsync(int offerId, int customerId);
    }
}
