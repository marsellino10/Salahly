using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;
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
        private readonly ICraftsManService _craftsManService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationHubSender hubSender,
            ILogger<NotificationService> logger,
            ICraftsManService craftsManService)
        {
            _unitOfWork = unitOfWork;
            _hubSender = hubSender;
            _logger = logger;
            _craftsManService = craftsManService;
        }
        public async Task NotifyAsync(CreateNotificationDto dto)
        {
            try
            {
                var notifications = new List<Notification>();

                foreach (var userId in dto.UserIds)
                {
                    var notif = new Notification
                    {
                        UserId = userId,
                        Type = dto.Type,
                        Title = dto.Title,
                        Message = dto.Message,
                        ActionUrl = dto.ActionUrl,
                        ServiceRequestId = dto.ServiceRequestId,
                        CraftsmanOfferId = dto.CraftsmanOfferId,
                        BookingId = dto.BookingId,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    notifications.Add(notif);
                    await _unitOfWork.Notifications.AddAsync(notif);
                }

                await _unitOfWork.SaveAsync();

                // Send in real-time
                foreach (var n in notifications)
                {
                    await _hubSender.SendToUserAsync(n.UserId.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                throw;
            }
        }

        public async Task NotifyCraftsmenInAreaAsync(ServiceRequest request)
        {
            var users = await _craftsManService.CraftsmenInAreaWithCraftAsync(request.AreaId, request.CraftId);

            if (!users.Any())
                return;

            await NotifyAsync(new CreateNotificationDto
            {
                UserIds = users,
                Type = NotificationType.NewServiceRequest,
                Title = "New Service Request",
                Message = $"A new request was posted in your area: {request.Title}",
                ActionUrl = $"/craftsman/requests/{request.ServiceRequestId}",
                ServiceRequestId = request.ServiceRequestId
            });
        }

        public async Task NotifyPaymentSuccessAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            await NotifyAsync(new CreateNotificationDto
            {
                UserIds = new[] { booking.CustomerId },
                Type = NotificationType.BookingConfirmed,
                Title = "Your Booking is Confirmed",
                Message = $"This is a confirmation that the booking for {booking.ServiceRequest.Title} on {booking.BookingDate} " +
                $"has been successfully scheduled for {booking.Customer.User.FullName} at {booking.Customer.Address}." +
                $"\r\n\r\nWe look forward to a successful service.",
                ActionUrl = $"/history",
                BookingId = bookingId
            });
            await NotifyAsync(new CreateNotificationDto
            {
                UserIds = new[] {  booking.CraftsmanId },
                Type = NotificationType.BookingConfirmed,
                Title = "Your Booking is Confirmed",
                Message = $"This is a confirmation that the booking for {booking.ServiceRequest.Title} on {booking.BookingDate} " +
                $"has been successfully scheduled for {booking.Customer.User.FullName} at {booking.Customer.Address}." +
                $"\r\n\r\nWe look forward to a successful service.",
                ActionUrl = $"/service-request-details/{booking.ServiceRequestId}",
                BookingId = bookingId
            });
        }


        public async Task<List<NotificationDto>> GetNotificationsForUserAsync(int userId)
        {
            try
            {
                var list = await _unitOfWork.Notifications.GetForUserAsync(userId);
                return list.Adapt<List<NotificationDto>>();
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
        public async Task MarkAllAsRead(int userId)
        {
            var notifications = await _unitOfWork.Notifications
            .GetAll()
             .Where(n => n.UserId == userId && !n.IsRead)
             .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;
            await _unitOfWork.SaveAsync();
        }
    }
}
