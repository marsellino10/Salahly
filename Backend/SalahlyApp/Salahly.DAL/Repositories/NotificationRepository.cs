using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salahly.DAL.Data;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;

namespace Salahly.DAL.Repositories
{
    public class NotificationRepository:GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        { }

        public async Task<List<Notification>> GetForUserAsync(int userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

    }
}
