using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salahly.DAL.Entities;
using Salahly.DSL.DTOs;

namespace Salahly.DSL.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(CreateNotificationDto dto);

        Task<List<Notification>> GetNotificationsForUserAsync(int userId);

        Task MarkAsReadAsync(int notificationId, int userId);
    }
}
