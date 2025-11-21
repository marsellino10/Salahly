using Salahly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DAL.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetForUserAsync(int userId);
        Task<Notification?> GetByIdAsync(int notificationId);
    }
}
