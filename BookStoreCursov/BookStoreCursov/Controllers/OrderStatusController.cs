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
    public class OrderStatusController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public OrderStatusController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/OrderStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderStatus>>> GetOrderStatuses()
        {
            return await _context.OrderStatuses.ToListAsync();
        }

        // GET: api/OrderStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderStatus>> GetOrderStatus(int id)
        {
            var orderStatus = await _context.OrderStatuses.FindAsync(id);

            if (orderStatus == null)
            {
                return NotFound();
            }

            return orderStatus;
        }

        // PUT: api/OrderStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderStatus(int id, OrderStatus orderStatus)
        {
            if (id != orderStatus.IdOrderStatus)
            {
                return BadRequest(new { message = "Несоответствие идентификатора статуса заказа" });
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(orderStatus.Title))
            {
                errors.Add("Название статуса обязательно для заполнения");
            }
            else if (orderStatus.Title.Length > 50)
            {
                errors.Add("Название статуса не может превышать 50 символов");
            }
            else
            {
                var titleExists = await _context.OrderStatuses.AnyAsync(s => s.Title == orderStatus.Title && s.IdOrderStatus != id);
                if (titleExists)
                {
                    errors.Add("Статус с таким названием уже существует в системе");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(orderStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderStatusExists(id))
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

        // POST: api/OrderStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OrderStatus>> PostOrderStatus(OrderStatus orderStatus)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(orderStatus.Title))
            {
                errors.Add("Название статуса обязательно для заполнения");
            }
            else if (orderStatus.Title.Length > 50)
            {
                errors.Add("Название статуса не может превышать 50 символов");
            }
            else
            {
                var titleExists = await _context.OrderStatuses.AnyAsync(s => s.Title == orderStatus.Title);
                if (titleExists)
                {
                    errors.Add("Статус с таким названием уже существует в системе");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.OrderStatuses.Add(orderStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrderStatus", new { id = orderStatus.IdOrderStatus }, orderStatus);
        }

        // DELETE: api/OrderStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderStatus(int id)
        {
            var orderStatus = await _context.OrderStatuses.FindAsync(id);
            if (orderStatus == null)
            {
                return NotFound();
            }

            var hasOrders = await _context.Orders.AnyAsync(o => o.OrderStatusId == id);
            if (hasOrders)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить статус",
                    errors = new List<string> { "Существуют заказы с этим статусом. Сначала измените статус заказов или удалите их." }
                });
            }

            _context.OrderStatuses.Remove(orderStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.OrderStatuses.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(os => os.Title.Contains(search));
            }
            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<OrderStatus, string>>
            {
                ["ID"] = os => os.IdOrderStatus.ToString(),
                ["Название статуса"] = os => os.Title
            };

            var bytes = _excelService.ExportToExcel(data, "Статусы заказов", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "order_status.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.OrderStatuses.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(os => os.Title.Contains(search));
            }
            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<OrderStatus, string>>
            {
                ["ID"] = os => os.IdOrderStatus.ToString(),
                ["Название статуса"] = os => os.Title
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "order_status.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var result = await _excelService.ImportFromExcelAsync<OrderStatus>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var title = row["Название статуса"]?.ToString();
                    if (string.IsNullOrEmpty(title))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Название статуса'", null);

                    if (title.Length > 50)
                        return (false, $"Строка {rowNumber}: Название статуса не может превышать 50 символов", null);

                    if (await _context.OrderStatuses.AnyAsync(os => os.Title == title))
                        return (false, $"Строка {rowNumber}: Статус '{title}' уже существует", null);

                    var orderStatus = new OrderStatus { Title = title };
                    return (true, null, orderStatus);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private bool OrderStatusExists(int id)
        {
            return _context.OrderStatuses.Any(e => e.IdOrderStatus == id);
        }
    }
}
