using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SalahlyProject.Api.Hubs
{
   [Authorize] // Hub requires authenticated users
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            //tell adel where it the user id
            var userId = Context.UserIdentifier;

            // For debugging
            Console.WriteLine($"User {userId} connected to NotificationHub");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User {userId} disconnected from NotificationHub");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
