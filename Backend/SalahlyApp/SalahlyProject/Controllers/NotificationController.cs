using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salahly.DSL.DTOs;
using Salahly.DSL.Interfaces;

namespace SalahlyProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var list = await _notificationService.GetNotificationsForUserAsync(userId);
            return Ok(list);
        }
        [Authorize]
        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _notificationService.MarkAsReadAsync(id, userId);
            return Ok();
        }
        [Authorize]
        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _notificationService.MarkAllAsRead(userId);
            return Ok();
        }


    }
}
