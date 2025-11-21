using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Salahly.DAL.Entities;
using Salahly.DAL.Interfaces;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
namespace Salahly.DSL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubSender _hubSender;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationHubSender hubSender,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubSender = hubSender;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(CreateNotificationDto dto)
        {
            try
            {
                var notif = new Notification
                {
                    UserId = dto.userId,
                    Type = dto.type,
                    Title = dto.title,
                    Message = dto.message,
                    ActionUrl = dto.actionUrl,
                    ServiceRequestId = dto.serviceRequestId,
                    CraftsmanOfferId = dto.craftsmanOfferId,
                    BookingId = dto.bookingId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notif);
                await _unitOfWork.SaveAsync();

                // Send via SignalR
                var payload = new
                {
                    type = (int)dto.type,
                    dto.title,
                    dto.message,
                    dto.actionUrl,
                    dto.serviceRequestId,
                    dto.craftsmanOfferId,
                    dto.bookingId,
                    notificationId = notif.NotificationId,
                    createdAt = notif.CreatedAt
                };

                await _hubSender.SendToUserAsync(dto.userId.ToString(), payload);
                //Clients.User(dto.userId.ToString())
                    //.SendAsync("ReceiveNotification", payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", dto.userId);
                // optionally rethrow or swallow
                throw;
            }
        }

        public async Task<List<Notification>> GetNotificationsForUserAsync(int userId)
        {
            try
            {
                var list = await _unitOfWork.Notifications.GetForUserAsync(userId);
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            try
            {
                var notif = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
                if (notif == null || notif.UserId != userId)
                {
                    throw new InvalidOperationException("Notification not found or access denied.");
                }

                if (!notif.IsRead)
                {
                    notif.IsRead = true;
                    notif.ReadAt = DateTime.UtcNow;
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                throw;
            }
        }
    }
}
