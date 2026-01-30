using Application.DTOs.Auth;
using Application.DTOs.CustomerDtos;
using Application.Interfaces.ICustomer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class CustomerSettingsController : ControllerBase
    {
        private readonly IUserSettingsService _settings;

        public CustomerSettingsController(IUserSettingsService settings)
        {
            _settings = settings;
        }

        private long GetUserIdOrThrow()
        {
            var userIdStr = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdStr, out var userId)) throw new UnauthorizedAccessException("Unauthorized");
            return userId;
        }

        // Notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications(CancellationToken ct)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _settings.GetNotificationsAsync(userId, ct));
        }

        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotifications([FromBody] NotificationsSettingsDto dto, CancellationToken ct)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _settings.UpdateNotificationsAsync(userId, dto, ct));
        }

        // Preferences
        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences(CancellationToken ct)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _settings.GetPreferencesAsync(userId, ct));
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] PreferencesSettingsDto dto, CancellationToken ct)
        {
            var userId = GetUserIdOrThrow();
            return Ok(await _settings.UpdatePreferencesAsync(userId, dto, ct));
        }
    }
}
