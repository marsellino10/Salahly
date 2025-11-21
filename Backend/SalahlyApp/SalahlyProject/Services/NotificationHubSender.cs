using Microsoft.AspNetCore.SignalR;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;
using SalahlyProject.Api.Hubs;

namespace SalahlyProject.Services
{
    public class NotificationHubSender : INotificationHubSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(string userId, object payload)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", payload);
        }
    }

}
