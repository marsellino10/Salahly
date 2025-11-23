using Salahly.DAL.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace Salahly.DAL.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAsync(RefreshToken token);
        IQueryable<RefreshToken> GetAll();
    }
}