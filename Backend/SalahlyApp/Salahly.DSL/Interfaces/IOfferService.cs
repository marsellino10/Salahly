using Salahly.DSL.DTOs.OffersDtos;
using Salahly.DSL.DTOs.ServiceRequstDtos;

namespace Salahly.DSL.Interfaces
{
    public interface IOfferService
    {
        Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersForCustomerRequestAsync(int customerId, int requestId);
        Task<ServiceResponse<bool>> AcceptOfferAsync(int customerId, int offerId);
        Task<ServiceResponse<bool>> RejectOfferAsync(int customerId, int offerId, RejectOfferDto dto);


        //Craftsman methods
        Task<ServiceResponse<OfferDto>> CreateOfferAsync(int craftsmanId, CreateOfferDto dto);
        Task<ServiceResponse<IEnumerable<OfferDto>>> GetOffersByCraftsmanAsync(int craftsmanId);
        Task<ServiceResponse<OfferDto>> GetOfferByIdForCraftsmanAsync(int craftsmanId, int offerId);
        Task<ServiceResponse<bool>> WithdrawOfferAsync(int craftsmanId, int offerId);
    }
}
