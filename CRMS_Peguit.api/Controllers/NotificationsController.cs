using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly RealEstateDbContext _db;

        public NotificationsController(RealEstateDbContext db)
        {
            _db = db;
        }

        private int GetCurrentUserId(int fallbackUserId = 0)
        {
            var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out var uid) && uid > 0)
            {
                return uid;
            }
            return fallbackUserId;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy([FromQuery] int userId, [FromQuery] int take = 50)
        {
            int resolvedUserId = GetCurrentUserId(userId);
            if (resolvedUserId <= 0) return BadRequest("A valid userId is required.");

            var items = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.RecipientUserId == resolvedUserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] int userId)
        {
            int resolvedUserId = GetCurrentUserId(userId);
            if (resolvedUserId <= 0) return BadRequest("A valid userId is required.");

            var count = await _db.Notifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientUserId == resolvedUserId && !n.IsRead);

            return Ok(new { unreadCount = count });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var item = await _db.Notifications.SingleOrDefaultAsync(n => n.NotificationId == id);
            if (item == null) return NotFound();

            if (!item.IsRead)
            {
                item.IsRead = true;
                item.ReadAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int userId)
        {
            int resolvedUserId = GetCurrentUserId(userId);
            if (resolvedUserId <= 0) return BadRequest("A valid userId is required.");

            var unread = await _db.Notifications
                .Where(n => n.RecipientUserId == resolvedUserId && !n.IsRead)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var item in unread)
            {
                item.IsRead = true;
                item.ReadAt = now;
            }

            await _db.SaveChangesAsync();
            return Ok(new { markedCount = unread.Count });
        }

        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences([FromQuery] int userId)
        {
            int resolvedUserId = GetCurrentUserId(userId);
            if (resolvedUserId <= 0) return BadRequest("A valid userId is required.");

            var rows = await _db.NotificationPreferences
                .AsNoTracking()
                .Where(p => p.UserId == resolvedUserId)
                .ToListAsync();

            var dict = new Dictionary<string, bool>();
            foreach (NotificationType t in Enum.GetValues<NotificationType>())
            {
                var match = rows.FirstOrDefault(r => r.Type == t);
                dict[t.ToString()] = match?.IsEnabled ?? true;
            }

            return Ok(dict);
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromQuery] int userId, [FromBody] Dictionary<string, bool> preferences)
        {
            int resolvedUserId = GetCurrentUserId(userId);
            if (resolvedUserId <= 0) return BadRequest("A valid userId is required.");

            var existing = await _db.NotificationPreferences
                .Where(p => p.UserId == resolvedUserId)
                .ToListAsync();

            foreach (var kvp in preferences)
            {
                if (Enum.TryParse<NotificationType>(kvp.Key, out var typeEnum))
                {
                    var match = existing.FirstOrDefault(e => e.Type == typeEnum);
                    if (match != null)
                    {
                        match.IsEnabled = kvp.Value;
                    }
                    else
                    {
                        _db.NotificationPreferences.Add(new NotificationPreference
                        {
                            UserId = resolvedUserId,
                            Type = typeEnum,
                            IsEnabled = kvp.Value
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("prune")]
        public async Task<IActionResult> PruneOld([FromQuery] int userId, [FromQuery] int daysOld = 90)
        {
            int resolvedUserId = GetCurrentUserId(userId);
            if (resolvedUserId <= 0) return BadRequest("A valid userId is required.");

            var cutoff = DateTime.UtcNow.AddDays(-daysOld);
            var oldNotifications = await _db.Notifications
                .Where(n => n.RecipientUserId == resolvedUserId && n.IsRead && n.CreatedAt < cutoff)
                .ToListAsync();

            _db.Notifications.RemoveRange(oldNotifications);
            await _db.SaveChangesAsync();

            return Ok(new { deletedCount = oldNotifications.Count });
        }
    }
}
