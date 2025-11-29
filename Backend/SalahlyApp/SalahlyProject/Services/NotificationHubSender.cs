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

        public async Task SendToUserAsync(string userId)
        {
            // Just send a simple flag or empty object
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", true);
            // or even simpler: SendAsync("ReceiveNotification")
        }
    }

}
