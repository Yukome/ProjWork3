using BookStoreCursov.Models;
using BookStoreCursov.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStoreCursov;

namespace BookStoreCursov.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public OrdersController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.IdOrder)
            {
                return BadRequest(new { message = "Несоответствие идентификатора заказа" });
            }

            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.IdOrder == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            int oldStatusId = existingOrder.OrderStatusId;
            int newStatusId = order.OrderStatusId;

            var errors = new List<string>();

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == order.UsersId);
            if (order.UsersId <= 0 || !userExists)
            {
                errors.Add("Выбранный пользователь не существует в системе");
            }

            if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
            {
                errors.Add("Адрес доставки обязателен для заполнения");
            }
            else if (order.DeliveryAddress.Length < 5)
            {
                errors.Add("Адрес доставки должен содержать не менее 5 символов");
            }

            var validPaymentMethods = new[] { "Наличные", "Карта", "Перевод" };
            if (string.IsNullOrWhiteSpace(order.PaymentMethod))
            {
                errors.Add("Способ оплаты обязателен для выбора");
            }
            else if (!validPaymentMethods.Contains(order.PaymentMethod))
            {
                errors.Add("Способ оплаты должен быть выбран из списка: Наличные, Карта, Перевод");
            }

            if (string.IsNullOrWhiteSpace(order.Phone))
            {
                errors.Add("Контактный телефон обязателен для заполнения");
            }
            else
            {
                var phoneDigits = new string(order.Phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length != 11)
                {
                    errors.Add("Телефон должен содержать 11 цифр");
                }
                else if (!order.Phone.StartsWith("+7") && !order.Phone.StartsWith("8"))
                {
                    errors.Add("Телефон должен начинаться с +7 или 8");
                }
            }

            if (order.TotalSum <= 0)
            {
                errors.Add("Сумма заказа должна быть больше 0");
            }
            else if (order.TotalSum > 999999.99m)
            {
                errors.Add("Сумма заказа не может превышать 999 999.99 рублей");
            }

            var statusExists = await _context.OrderStatuses.AnyAsync(s => s.IdOrderStatus == order.OrderStatusId);
            if (order.OrderStatusId <= 0 || !statusExists)
            {
                errors.Add("Выбранный статус заказа не существует в системе");
            }

            if (order.AddDate > DateOnly.FromDateTime(DateTime.Now))
            {
                errors.Add("Дата заказа не может быть позже текущей даты");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            existingOrder.UsersId = order.UsersId;
            existingOrder.DeliveryAddress = order.DeliveryAddress;
            existingOrder.PaymentMethod = order.PaymentMethod;
            existingOrder.Phone = order.Phone;
            existingOrder.TotalSum = order.TotalSum;
            existingOrder.OrderStatusId = order.OrderStatusId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (oldStatusId != newStatusId)
            {
                var newStatus = await _context.OrderStatuses.FindAsync(newStatusId);

                if (newStatus != null && newStatus.Title != "Оформлен")
                {
                    string notificationText = "";

                    switch (newStatus.Title)
                    {
                        case "Доставлен":
                            notificationText = $"Заказ №{existingOrder.IdOrder} доставлен и готов к получению {DateTime.Now:dd.MM.yyyy}";
                            break;
                        case "Оплачен":
                            notificationText = $"Заказ №{existingOrder.IdOrder} успешно оплачен";
                            break;
                        case "Отменен":
                            notificationText = $"Заказ №{existingOrder.IdOrder} был отменен";
                            break;
                        default:
                            notificationText = $"Статус заказа №{existingOrder.IdOrder} изменен на '{newStatus.Title}'";
                            break;
                    }

                    var notification = new Notification
                    {
                        UsersId = existingOrder.UsersId,
                        NotificationText = notificationText,
                        AddDate = DateOnly.FromDateTime(DateTime.Now),
                        StatusN = false
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            var errors = new List<string>();

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == order.UsersId);
            if (order.UsersId <= 0 || !userExists)
            {
                errors.Add("Выбранный пользователь не существует в системе");
            }

            if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
            {
                errors.Add("Адрес доставки обязателен для заполнения");
            }
            else if (order.DeliveryAddress.Length < 5)
            {
                errors.Add("Адрес доставки должен содержать не менее 5 символов");
            }

            var validPaymentMethods = new[] { "Наличные", "Карта", "Перевод" };
            if (string.IsNullOrWhiteSpace(order.PaymentMethod))
            {
                errors.Add("Способ оплаты обязателен для выбора");
            }
            else if (!validPaymentMethods.Contains(order.PaymentMethod))
            {
                errors.Add("Способ оплаты должен быть выбран из списка: Наличные, Карта, Перевод");
            }

            if (string.IsNullOrWhiteSpace(order.Phone))
            {
                errors.Add("Контактный телефон обязателен для заполнения");
            }
            else
            {
                var phoneDigits = new string(order.Phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length != 11)
                {
                    errors.Add("Телефон должен содержать 11 цифр");
                }
                else if (!order.Phone.StartsWith("+7") && !order.Phone.StartsWith("8"))
                {
                    errors.Add("Телефон должен начинаться с +7 или 8");
                }
            }

            if (order.TotalSum <= 0)
            {
                errors.Add("Сумма заказа должна быть больше 0");
            }
            else if (order.TotalSum > 999999.99m)
            {
                errors.Add("Сумма заказа не может превышать 999 999.99 рублей");
            }

            var statusExists = await _context.OrderStatuses.AnyAsync(s => s.IdOrderStatus == order.OrderStatusId);
            if (order.OrderStatusId <= 0 || !statusExists)
            {
                errors.Add("Выбранный статус заказа не существует в системе");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            order.AddDate = DateOnly.FromDateTime(DateTime.Now);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            AppMetrics.OrdersCreatedTotal.Inc();

            var notification = new Notification
            {
                UsersId = order.UsersId,
                NotificationText = $"Заказ №{order.IdOrder} оформлен и готов к обработке",
                AddDate = DateOnly.FromDateTime(DateTime.Now),
                StatusN = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.IdOrder }, order);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var hasPositions = await _context.OrderPositions.AnyAsync(p => p.OrderId == id);
            if (hasPositions)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить заказ",
                    errors = new List<string> { "В заказе есть позиции. Используйте удаление с позициями (/with-positions) или сначала удалите позиции." }
                });
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Orders/5/with-positions:
        [HttpDelete("{id}/with-positions")]
        public async Task<IActionResult> DeleteOrderWithPositions(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.OrderStatusId != null)
            {
                var status = await _context.OrderStatuses.FindAsync(order.OrderStatusId);
                if (status != null && (status.Title == "Доставлен" || status.Title == "Оплачен"))
                {
                    return BadRequest(new
                    {
                        message = "Невозможно удалить заказ",
                        errors = new List<string> { $"Нельзя удалить заказ со статусом '{status.Title}'" }
                    });
                }
            }

            var positions = _context.OrderPositions.Where(p => p.OrderId == id);
            _context.OrderPositions.RemoveRange(positions);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusModel model)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.IdOrder == id);

            if (order == null)
            {
                return NotFound(new { message = "Заказ не найден" });
            }

            var newStatus = await _context.OrderStatuses.FindAsync(model.OrderStatusId);
            if (newStatus == null)
            {
                return BadRequest(new { message = "Статус не найден" });
            }

            int oldStatusId = order.OrderStatusId;
            int newStatusId = model.OrderStatusId;

            if (oldStatusId == newStatusId)
            {
                return Ok(new { message = "Статус не изменился", status = newStatus.Title });
            }

            order.OrderStatusId = newStatusId;
            await _context.SaveChangesAsync();

            string notificationText = "";

            switch (newStatus.Title)
            {
                case "Доставлен":
                    notificationText = $"Заказ №{order.IdOrder} доставлен и готов к получению {DateTime.Now:dd.MM.yyyy}";
                    break;
                case "Оплачен":
                    notificationText = $"Заказ №{order.IdOrder} успешно оплачен";
                    break;
                case "Отменен":
                    notificationText = $"Заказ №{order.IdOrder} был отменен";
                    break;
                default:
                    notificationText = $"Статус заказа №{order.IdOrder} изменен на '{newStatus.Title}'";
                    break;
            }

            var notification = new Notification
            {
                UsersId = order.UsersId,
                NotificationText = notificationText,
                AddDate = DateOnly.FromDateTime(DateTime.Now),
                StatusN = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Статус заказа изменен на '{newStatus.Title}'",
                newStatus = newStatus.Title,
                orderId = order.IdOrder
            });
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.Orders
                .Include(o => o.Users)
                .Include(o => o.OrderStatus)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.IdOrder.ToString().Contains(search) ||
                    (o.Users != null && (o.Users.UserLogin.Contains(search) || o.Users.UsersName.Contains(search))) ||
                    o.DeliveryAddress.Contains(search));
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Order, string>>
            {
                ["ID заказа"] = o => o.IdOrder.ToString(),
                ["ID пользователя"] = o => o.UsersId.ToString(),
                ["Покупатель"] = o => o.Users != null ? $"{o.Users.UsersSurname} {o.Users.UsersName} ({o.Users.UserLogin})" : "",
                ["Дата заказа"] = o => o.AddDate.ToString("yyyy-MM-dd"),
                ["Статус"] = o => o.OrderStatus?.Title ?? "",
                ["Сумма"] = o => o.TotalSum.ToString("F2"),
                ["Способ оплаты"] = o => o.PaymentMethod,
                ["Адрес доставки"] = o => o.DeliveryAddress,
                ["Телефон"] = o => o.Phone
            };

            var bytes = _excelService.ExportToExcel(data, "Заказы", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.Orders
                .Include(o => o.Users)
                .Include(o => o.OrderStatus)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o =>
                    o.IdOrder.ToString().Contains(search) ||
                    (o.Users != null && (o.Users.UserLogin.Contains(search) || o.Users.UsersName.Contains(search))) ||
                    o.DeliveryAddress.Contains(search));
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Order, string>>
            {
                ["ID заказа"] = o => o.IdOrder.ToString(),
                ["ID пользователя"] = o => o.UsersId.ToString(),
                ["Покупатель"] = o => o.Users != null ? $"{o.Users.UsersSurname} {o.Users.UsersName} ({o.Users.UserLogin})" : "",
                ["Дата заказа"] = o => o.AddDate.ToString("yyyy-MM-dd"),
                ["Статус"] = o => o.OrderStatus?.Title ?? "",
                ["Сумма"] = o => o.TotalSum.ToString("F2"),
                ["Способ оплаты"] = o => o.PaymentMethod,
                ["Адрес доставки"] = o => o.DeliveryAddress,
                ["Телефон"] = o => o.Phone
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "orders.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Файл не выбран" });

                var extension = Path.GetExtension(file.FileName).ToLower();
                if (extension != ".xlsx" && extension != ".csv")
                    return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

                var users = await _context.Users.ToListAsync();
                var statuses = await _context.OrderStatuses.ToListAsync();
                var existingOrderIds = new HashSet<int>(await _context.Orders.Select(o => o.IdOrder).ToListAsync());

                var result = await _excelService.ImportFromExcelAsync<Order>(
                    file.OpenReadStream(),
                    _context,
                    async (row, rowNumber) =>
                    {
                        var userIdStr = row["ID пользователя"]?.ToString();
                        var deliveryAddress = row["Адрес доставки"]?.ToString();

                        if (string.IsNullOrWhiteSpace(userIdStr) && string.IsNullOrWhiteSpace(deliveryAddress))
                        {
                            return (false, $"Строка {rowNumber}: Пустая строка. Заполните данные или удалите строку.", null);
                        }

                        int? orderId = null;
                        var orderIdStr = row["ID заказа"]?.ToString();
                        if (!string.IsNullOrEmpty(orderIdStr))
                        {
                            if (!int.TryParse(orderIdStr, out int parsedId))
                                return (false, $"Строка {rowNumber}: 'ID заказа' должен быть числом", null);

                            orderId = parsedId;

                            if (existingOrderIds.Contains(orderId.Value))
                                return (false, $"Строка {rowNumber}: Заказ с ID {orderId} уже существует в системе", null);
                        }

                        if (string.IsNullOrEmpty(userIdStr))
                            return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'ID пользователя'", null);

                        if (!int.TryParse(userIdStr, out int userId))
                            return (false, $"Строка {rowNumber}: 'ID пользователя' должен быть числом", null);

                        var user = users.FirstOrDefault(u => u.IdUser == userId);
                        if (user == null)
                            return (false, $"Строка {rowNumber}: Пользователь с ID {userId} не найден", null);

                        if (string.IsNullOrEmpty(deliveryAddress))
                            return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Адрес доставки'", null);

                        if (deliveryAddress.Length < 5)
                            return (false, $"Строка {rowNumber}: Адрес доставки должен содержать не менее 5 символов", null);

                        var paymentMethod = row["Способ оплаты"]?.ToString();
                        var validPaymentMethods = new[] { "Наличные", "Карта", "Перевод" };
                        if (string.IsNullOrEmpty(paymentMethod) || !validPaymentMethods.Contains(paymentMethod))
                            return (false, $"Строка {rowNumber}: Способ оплаты должен быть: Наличные, Карта, Перевод", null);

                        var phone = row["Телефон"]?.ToString();
                        if (string.IsNullOrEmpty(phone))
                            return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Телефон'", null);

                        var phoneDigits = new string(phone.Where(char.IsDigit).ToArray());
                        if (phoneDigits.Length != 11)
                            return (false, $"Строка {rowNumber}: Телефон должен содержать 11 цифр", null);

                        decimal totalSum = 0;
                        var totalSumStr = row["Сумма"]?.ToString();
                        if (!string.IsNullOrEmpty(totalSumStr))
                        {
                            if (!decimal.TryParse(totalSumStr, out totalSum) || totalSum < 0)
                                return (false, $"Строка {rowNumber}: Сумма должна быть положительным числом", null);
                        }

                        var statusTitle = row["Статус"]?.ToString();
                        var status = statuses.FirstOrDefault(s => s.Title == (statusTitle ?? "Оформлен"));
                        if (status == null)
                            status = statuses.FirstOrDefault(s => s.Title == "Оформлен");

                        DateOnly addDate = DateOnly.FromDateTime(DateTime.Now);
                        var addDateStr = row["Дата заказа"]?.ToString();
                        if (!string.IsNullOrEmpty(addDateStr))
                        {
                            if (!DateOnly.TryParse(addDateStr, out var parsedDate))
                                return (false, $"Строка {rowNumber}: Неверный формат даты. Ожидается ГГГГ-ММ-ДД", null);
                            addDate = parsedDate;
                        }

                        var order = new Order
                        {
                            IdOrder = orderId ?? 0,
                            UsersId = userId,
                            DeliveryAddress = deliveryAddress,
                            PaymentMethod = paymentMethod,
                            Phone = phone,
                            TotalSum = totalSum,
                            OrderStatusId = status.IdOrderStatus,
                            AddDate = addDate
                        };

                        return (true, null, order);
                    }
                );

                var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";
                if (result.Errors.Count > 0)
                {
                    message += $"\n\nОшибки ({result.Errors.Count}):\n" + string.Join("\n", result.Errors.Take(10));
                    if (result.Errors.Count > 10)
                        message += $"\n... и ещё {result.Errors.Count - 10} ошибок";
                }

                return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
            }
            catch (Exception ex)
            {
                // Логируем полную ошибку в консоль
                Console.WriteLine($"Ошибка импорта: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Возвращаем понятную ошибку
                return StatusCode(500, new
                {
                    message = $"Ошибка импорта: {ex.Message}",
                    details = ex.Message.Contains("Cannot insert") ? "Возможно, дублирование ключей" : ex.Message
                });
            }
            ;
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.IdOrder == id);
        }
    }

    public class UpdateStatusModel
    {
        public int OrderStatusId { get; set; }
    }
}
