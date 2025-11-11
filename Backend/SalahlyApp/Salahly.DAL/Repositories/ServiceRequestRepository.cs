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
    }
}
