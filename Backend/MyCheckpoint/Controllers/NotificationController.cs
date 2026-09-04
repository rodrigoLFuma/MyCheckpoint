using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine(
            [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            return Ok(await _notificationService.GetMyNotificationsAsync(this.GetUserId(), skip, take));
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            return Ok(await _notificationService.GetUnreadCountAsync(this.GetUserId()));
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllAsReadAsync(this.GetUserId());
            return NoContent();
        }
    }
}
