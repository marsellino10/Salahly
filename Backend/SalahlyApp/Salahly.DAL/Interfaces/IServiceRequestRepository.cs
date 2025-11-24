using Salahly.DAL.Entities;

namespace Salahly.DAL.Interfaces
{
    public interface IServiceRequestRepository : IGenericRepository<ServiceRequest>
    {
        // Related to customer
        Task<IEnumerable<ServiceRequest>> GetAllByCustomerAsync(int customerId);
        Task<bool> DeleteByCustomerAsync(int id, int customerId);        
        Task<ServiceRequest?> GetServiceRequestByIdWithIncludesAsync(int id);


        // Related to craftsman
        Task<IEnumerable<ServiceRequest>> GetActiveServiceRequestsForCraftsmanAsync(int craftsmanId);
        Task<List<ServiceRequest>> GetServiceRequestsWithCraftsmanOffersAsync(int craftsmanId);
        Task<ServiceRequest?> GetServiceRequestForCraftsmanByIdAsync(int craftsmanId, int requestId);

        // General
        Task ChangeStatusAsync(int requestId, ServiceRequestStatus newStatus);
    }
}
