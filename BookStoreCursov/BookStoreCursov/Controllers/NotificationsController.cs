using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreCursov.Models;
using BookStoreCursov.Services;

namespace BookStoreCursov.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public NotificationsController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return await _context.Notifications.ToListAsync();
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            return notification;
        }

        // PUT: api/Notifications/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotification(int id, Notification notification)
        {
            if (id != notification.IdNotification)
            {
                return BadRequest(new { message = "Несоответствие идентификатора уведомления" });
            }

            var errors = new List<string>();

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == notification.UsersId);
            if (notification.UsersId <= 0 || !userExists)
            {
                errors.Add("Выбранный пользователь не существует в системе");
            }

            if (string.IsNullOrWhiteSpace(notification.NotificationText))
            {
                errors.Add("Текст уведомления обязателен для заполнения");
            }
            else if (notification.NotificationText.Length > 255)
            {
                errors.Add("Текст уведомления не может превышать 255 символов");
            }

            if (notification.AddDate > DateOnly.FromDateTime(DateTime.Now))
            {
                errors.Add("Дата уведомления не может быть позже текущей даты");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(notification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Notifications
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            var errors = new List<string>();

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == notification.UsersId);
            if (notification.UsersId <= 0 || !userExists)
            {
                errors.Add("Выбранный пользователь не существует в системе");
            }

            if (string.IsNullOrWhiteSpace(notification.NotificationText))
            {
                errors.Add("Текст уведомления обязателен для заполнения");
            }
            else if (notification.NotificationText.Length > 255)
            {
                errors.Add("Текст уведомления не может превышать 255 символов");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            notification.AddDate = DateOnly.FromDateTime(DateTime.Now);
            notification.StatusN = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNotification", new { id = notification.IdNotification }, notification);
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.Notifications
                .Include(n => n.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n =>
                    (n.Users != null && (n.Users.UserLogin.Contains(search) || n.Users.UsersName.Contains(search) || n.Users.UsersSurname.Contains(search))) ||
                    n.NotificationText.Contains(search));
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Notification, string>>
            {
                ["ID"] = n => n.IdNotification.ToString(),
                ["Пользователь"] = n => n.Users != null ? $"{n.Users.UsersSurname} {n.Users.UsersName} ({n.Users.UserLogin})" : "",
                ["Текст уведомления"] = n => n.NotificationText,
                ["Дата"] = n => n.AddDate.ToString("yyyy-MM-dd"),
                ["Статус"] = n => n.StatusN ? "Прочитано" : "Не прочитано"
            };

            var bytes = _excelService.ExportToExcel(data, "Уведомления", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "notifications.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.Notifications
                .Include(n => n.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n =>
                    (n.Users != null && (n.Users.UserLogin.Contains(search) || n.Users.UsersName.Contains(search) || n.Users.UsersSurname.Contains(search))) ||
                    n.NotificationText.Contains(search));
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Notification, string>>
            {
                ["ID"] = n => n.IdNotification.ToString(),
                ["Пользователь"] = n => n.Users != null ? $"{n.Users.UsersSurname} {n.Users.UsersName} ({n.Users.UserLogin})" : "",
                ["Текст уведомления"] = n => n.NotificationText,
                ["Дата"] = n => n.AddDate.ToString("yyyy-MM-dd"),
                ["Статус"] = n => n.StatusN ? "Прочитано" : "Не прочитано"
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "notifications.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            return BadRequest(new { message = "Импорт уведомлений запрещен в веб-приложении" });
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.IdNotification == id);
        }
    }
}
