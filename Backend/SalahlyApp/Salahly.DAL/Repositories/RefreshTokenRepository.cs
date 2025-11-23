using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;

namespace Salahly.DAL.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _context.Set<RefreshToken>().AddAsync(token);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeAsync(RefreshToken token)
        {
            token.Revoked = true;
            _context.Set<RefreshToken>().Update(token);
            await Task.CompletedTask;
        }

        public IQueryable<RefreshToken> GetAll()
        {
            return _context.Set<RefreshToken>();
        }
    }
}