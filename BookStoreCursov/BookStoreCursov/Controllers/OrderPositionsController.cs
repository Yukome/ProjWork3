using BookStoreCursov.Models;
using BookStoreCursov.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreCursov.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderPositionsController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public OrderPositionsController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/OrderPositions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderPosition>>> GetOrderPositions()
        {
            return await _context.OrderPositions.ToListAsync();
        }

        // GET: api/OrderPositions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderPosition>> GetOrderPosition(int id)
        {
            var orderPosition = await _context.OrderPositions.FindAsync(id);

            if (orderPosition == null)
            {
                return NotFound();
            }

            return orderPosition;
        }

        // PUT: api/OrderPositions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderPosition(int id, OrderPosition orderPosition)
        {
            if (id != orderPosition.IdOrderPosition)
            {
                return BadRequest(new { message = "Несоответствие идентификатора позиции заказа" });
            }

            var errors = new List<string>();

            var bookExists = await _context.Books.AnyAsync(b => b.IdBook == orderPosition.BookId);
            if (orderPosition.BookId <= 0 || !bookExists)
            {
                errors.Add("Выбранная книга не существует в системе");
            }

            if (orderPosition.Quantity < 1)
            {
                errors.Add("Количество товара должно быть не менее 1");
            }
            else if (orderPosition.Quantity > 99)
            {
                errors.Add("Количество товара не может превышать 99");
            }

            if (orderPosition.PurchasePrice <= 0)
            {
                errors.Add("Цена покупки должна быть больше 0");
            }
            else if (orderPosition.PurchasePrice > 999999.99m)
            {
                errors.Add("Цена покупки не может превышать 999 999.99 рублей");
            }

            if (orderPosition.OrderId <= 0)
            {
                errors.Add("Заказ не выбран");
            }
            else
            {
                var orderExists = await _context.Orders.AnyAsync(o => o.IdOrder == orderPosition.OrderId);
                if (!orderExists)
                {
                    errors.Add("Выбранный заказ не существует в системе");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(orderPosition).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderPositionExists(id))
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

        // POST: api/OrderPositions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OrderPosition>> PostOrderPosition(OrderPosition orderPosition)
        {
            var errors = new List<string>();

            var bookExists = await _context.Books.AnyAsync(b => b.IdBook == orderPosition.BookId);
            if (orderPosition.BookId <= 0 || !bookExists)
            {
                errors.Add("Выбранная книга не существует в системе");
            }
            else
            {
                var book = await _context.Books.FindAsync(orderPosition.BookId);
                if (book != null && book.Quantity < orderPosition.Quantity)
                {
                    errors.Add($"Недостаточно книг на складе. Доступно: {book.Quantity}, запрошено: {orderPosition.Quantity}");
                }
            }

            if (orderPosition.Quantity < 1)
            {
                errors.Add("Количество товара должно быть не менее 1");
            }
            else if (orderPosition.Quantity > 99)
            {
                errors.Add("Количество товара не может превышать 99");
            }

            if (orderPosition.PurchasePrice <= 0)
            {
                errors.Add("Цена покупки должна быть больше 0");
            }
            else if (orderPosition.PurchasePrice > 999999.99m)
            {
                errors.Add("Цена покупки не может превышать 999 999.99 рублей");
            }

            var orderExists = await _context.Orders.AnyAsync(o => o.IdOrder == orderPosition.OrderId);
            if (orderPosition.OrderId <= 0 || !orderExists)
            {
                errors.Add("Выбранный заказ не существует в системе");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.OrderPositions.Add(orderPosition);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrderPosition", new { id = orderPosition.IdOrderPosition }, orderPosition);
        }

        // DELETE: api/OrderPositions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderPosition(int id)
        {
            var orderPosition = await _context.OrderPositions.FindAsync(id);
            if (orderPosition == null)
            {
                return NotFound();
            }

            _context.OrderPositions.Remove(orderPosition);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null, [FromQuery] int? orderId = null)
        {
            var query = _context.OrderPositions
                .Include(p => p.Book)
                .Include(p => p.Order)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    (p.Book != null && p.Book.Title.Contains(search)) ||
                    (p.Order != null && p.Order.IdOrder.ToString().Contains(search)));
            }

            if (orderId.HasValue)
            {
                query = query.Where(p => p.OrderId == orderId.Value);
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<OrderPosition, string>>
            {
                ["ID позиции"] = p => p.IdOrderPosition.ToString(),
                ["ID заказа"] = p => p.OrderId.ToString(),
                ["ID книги"] = p => p.BookId.ToString(),
                ["Книга"] = p => p.Book?.Title ?? "",
                ["Количество"] = p => p.Quantity.ToString(),
                ["Цена покупки"] = p => p.PurchasePrice.ToString("F2"),
                ["Цена за единицу"] = p => (p.PurchasePrice / p.Quantity).ToString("F2")
            };

            var bytes = _excelService.ExportToExcel(data, "Позиции заказов", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "order_positions.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null, [FromQuery] int? orderId = null)
        {
            var query = _context.OrderPositions
                .Include(p => p.Book)
                .Include(p => p.Order)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    (p.Book != null && p.Book.Title.Contains(search)) ||
                    (p.Order != null && p.Order.IdOrder.ToString().Contains(search)));
            }

            if (orderId.HasValue)
            {
                query = query.Where(p => p.OrderId == orderId.Value);
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<OrderPosition, string>>
            {
                ["ID позиции"] = p => p.IdOrderPosition.ToString(),
                ["ID заказа"] = p => p.OrderId.ToString(),
                ["ID книги"] = p => p.BookId.ToString(),
                ["Книга"] = p => p.Book?.Title ?? "",
                ["Количество"] = p => p.Quantity.ToString(),
                ["Цена покупки"] = p => p.PurchasePrice.ToString("F2"),
                ["Цена за единицу"] = p => (p.PurchasePrice / p.Quantity).ToString("F2")
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "order_positions.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var books = await _context.Books.ToListAsync();
            var orders = await _context.Orders.ToListAsync();
            var existingOrderIds = new HashSet<int>(orders.Select(o => o.IdOrder));
            var affectedOrderIds = new HashSet<int>();

            var result = await _excelService.ImportFromExcelAsync<OrderPosition>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var orderIdStr = row["ID заказа"]?.ToString();
                    var bookIdStr = row["ID книги"]?.ToString();

                    if (string.IsNullOrWhiteSpace(orderIdStr) && string.IsNullOrWhiteSpace(bookIdStr))
                    {
                        return (false, $"Строка {rowNumber}: Пустая строка. Заполните данные или удалите строку.", null);
                    }

                    if (string.IsNullOrEmpty(orderIdStr))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'ID заказа'", null);

                    if (!int.TryParse(orderIdStr, out int orderId))
                        return (false, $"Строка {rowNumber}: 'ID заказа' должен быть числом", null);

                    if (!existingOrderIds.Contains(orderId))
                        return (false, $"Строка {rowNumber}: Заказ с ID {orderId} не найден в системе. Сначала импортируйте заказы.", null);

                    if (string.IsNullOrEmpty(bookIdStr))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'ID книги'", null);

                    if (!int.TryParse(bookIdStr, out int bookId))
                        return (false, $"Строка {rowNumber}: 'ID книги' должен быть числом", null);

                    var book = books.FirstOrDefault(b => b.IdBook == bookId);
                    if (book == null)
                        return (false, $"Строка {rowNumber}: Книга с ID {bookId} не найдена", null);

                    if (!int.TryParse(row["Количество"]?.ToString(), out int quantity) || quantity < 1 || quantity > 99)
                        return (false, $"Строка {rowNumber}: Количество должно быть от 1 до 99", null);

                    decimal purchasePrice = book.Price * quantity;
                    var existingPosition = await _context.OrderPositions
                        .FirstOrDefaultAsync(p => p.OrderId == orderId && p.BookId == bookId);

                    if (existingPosition != null)
                        return (false, $"Строка {rowNumber}: Позиция с книгой ID {bookId} уже существует в заказе {orderId}", null);

                    affectedOrderIds.Add(orderId);

                    var orderPosition = new OrderPosition
                    {
                        OrderId = orderId,
                        BookId = bookId,
                        Quantity = quantity,
                        PurchasePrice = purchasePrice
                    };

                    return (true, null, orderPosition);
                }
            );

            if (affectedOrderIds.Any())
            {
                foreach (var orderId in affectedOrderIds)
                {
                    var order = await _context.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        var totalSum = await _context.OrderPositions
                            .Where(p => p.OrderId == orderId)
                            .SumAsync(p => p.PurchasePrice);

                        order.TotalSum = totalSum;
                    }
                }
                await _context.SaveChangesAsync();
            }

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";
            if (result.Errors.Count > 0)
            {
                message += $"\n\nОшибки ({result.Errors.Count}):\n" + string.Join("\n", result.Errors.Take(10));
                if (result.Errors.Count > 10)
                    message += $"\n... и ещё {result.Errors.Count - 10} ошибок";
            }

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private bool OrderPositionExists(int id)
        {
            return _context.OrderPositions.Any(e => e.IdOrderPosition == id);
        }
    }
}
