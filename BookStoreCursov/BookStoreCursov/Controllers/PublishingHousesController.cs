using BookStoreCursov.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookStoreCursov.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishingHousesController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public PublishingHousesController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/PublishingHouses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublishingHouse>>> GetPublishingHouses()
        {
            return await _context.PublishingHouses.ToListAsync();
        }

        // GET: api/PublishingHouses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PublishingHouse>> GetPublishingHouse(int id)
        {
            var publishingHouse = await _context.PublishingHouses.FindAsync(id);

            if (publishingHouse == null)
            {
                return NotFound();
            }

            return publishingHouse;
        }

        // PUT: api/PublishingHouses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPublishingHouse(int id, PublishingHouse publishingHouse)
        {
            if (id != publishingHouse.IdPublishingHouse)
            {
                return BadRequest(new { message = "Несоответствие идентификатора издательства" });
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(publishingHouse.Title))
            {
                errors.Add("Название издательства обязательно для заполнения");
            }
            else if (publishingHouse.Title.Length > 200)
            {
                errors.Add("Название издательства не может превышать 200 символов");
            }
            else
            {
                var titleExists = await _context.PublishingHouses.AnyAsync(ph => ph.Title == publishingHouse.Title && ph.IdPublishingHouse != id);
                if (titleExists)
                {
                    errors.Add("Издательство с таким названием уже существует в системе");
                }
            }

            if (string.IsNullOrWhiteSpace(publishingHouse.PublishingHouseAddress))
            {
                errors.Add("Адрес издательства обязателен для заполнения");
            }
            else if (publishingHouse.PublishingHouseAddress.Length > 300)
            {
                errors.Add("Адрес издательства не может превышать 300 символов");
            }
            else if (publishingHouse.PublishingHouseAddress.Length < 5)
            {
                errors.Add("Адрес издательства должен содержать не менее 5 символов");
            }

            if (string.IsNullOrWhiteSpace(publishingHouse.Phone))
            {
                errors.Add("Контактный телефон обязателен для заполнения");
            }
            else if (publishingHouse.Phone.Length > 20)
            {
                errors.Add("Номер телефона не может превышать 20 символов");
            }
            else
            {
                var phoneDigits = new string(publishingHouse.Phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length != 11)
                {
                    errors.Add("Телефон должен содержать 11 цифр");
                }
                else if (!publishingHouse.Phone.StartsWith("+7") && !publishingHouse.Phone.StartsWith("8"))
                {
                    errors.Add("Телефон должен начинаться с +7 или 8");
                }
            }

            if (!string.IsNullOrEmpty(publishingHouse.Email))
            {
                if (publishingHouse.Email.Length > 100)
                {
                    errors.Add("Email не может превышать 100 символов");
                }
                else if (!Regex.IsMatch(publishingHouse.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errors.Add("Неверный формат email");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(publishingHouse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublishingHouseExists(id))
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

        // POST: api/PublishingHouses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PublishingHouse>> PostPublishingHouse(PublishingHouse publishingHouse)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(publishingHouse.Title))
            {
                errors.Add("Название издательства обязательно для заполнения");
            }
            else if (publishingHouse.Title.Length > 200)
            {
                errors.Add("Название издательства не может превышать 200 символов");
            }
            else
            {
                var titleExists = await _context.PublishingHouses.AnyAsync(ph => ph.Title == publishingHouse.Title);
                if (titleExists)
                {
                    errors.Add("Издательство с таким названием уже существует в системе");
                }
            }

            if (string.IsNullOrWhiteSpace(publishingHouse.PublishingHouseAddress))
            {
                errors.Add("Адрес издательства обязателен для заполнения");
            }
            else if (publishingHouse.PublishingHouseAddress.Length > 300)
            {
                errors.Add("Адрес издательства не может превышать 300 символов");
            }
            else if (publishingHouse.PublishingHouseAddress.Length < 5)
            {
                errors.Add("Адрес издательства должен содержать не менее 5 символов");
            }

            if (string.IsNullOrWhiteSpace(publishingHouse.Phone))
            {
                errors.Add("Контактный телефон обязателен для заполнения");
            }
            else if (publishingHouse.Phone.Length > 20)
            {
                errors.Add("Номер телефона не может превышать 20 символов");
            }
            else
            {
                var phoneDigits = new string(publishingHouse.Phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length != 11)
                {
                    errors.Add("Телефон должен содержать 11 цифр");
                }
                else if (!publishingHouse.Phone.StartsWith("+7") && !publishingHouse.Phone.StartsWith("8"))
                {
                    errors.Add("Телефон должен начинаться с +7 или 8");
                }
            }

            if (!string.IsNullOrEmpty(publishingHouse.Email))
            {
                if (publishingHouse.Email.Length > 100)
                {
                    errors.Add("Email не может превышать 100 символов");
                }
                else if (!Regex.IsMatch(publishingHouse.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errors.Add("Неверный формат email");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.PublishingHouses.Add(publishingHouse);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPublishingHouse", new { id = publishingHouse.IdPublishingHouse }, publishingHouse);
        }

        // DELETE: api/PublishingHouses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublishingHouse(int id)
        {
            var publishingHouse = await _context.PublishingHouses.FindAsync(id);
            if (publishingHouse == null)
            {
                return NotFound();
            }

            var hasBooks = await _context.Books.AnyAsync(b => b.PublishingHouseId == id);
            if (hasBooks)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить издательство",
                    errors = new List<string> { "Существуют книги, привязанные к этому издательству. Сначала удалите или переназначьте книги." }
                });
            }

            _context.PublishingHouses.Remove(publishingHouse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.PublishingHouses.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ph => ph.Title.Contains(search) || ph.Phone.Contains(search));
            }
            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<PublishingHouse, string>>
            {
                ["ID"] = ph => ph.IdPublishingHouse.ToString(),
                ["Название"] = ph => ph.Title,
                ["Адрес"] = ph => ph.PublishingHouseAddress,
                ["Телефон"] = ph => ph.Phone,
                ["Email"] = ph => ph.Email ?? ""
            };

            var bytes = _excelService.ExportToExcel(data, "Издательства", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "publishing_houses.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.PublishingHouses.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(ph => ph.Title.Contains(search) || ph.Phone.Contains(search));
            }
            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<PublishingHouse, string>>
            {
                ["ID"] = ph => ph.IdPublishingHouse.ToString(),
                ["Название"] = ph => ph.Title,
                ["Адрес"] = ph => ph.PublishingHouseAddress,
                ["Телефон"] = ph => ph.Phone,
                ["Email"] = ph => ph.Email ?? ""
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "publishing_houses.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var result = await _excelService.ImportFromExcelAsync<PublishingHouse>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var title = row["Название"]?.ToString();
                    if (string.IsNullOrEmpty(title))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Название'", null);

                    if (title.Length > 200)
                        return (false, $"Строка {rowNumber}: Название издательства не может превышать 200 символов", null);

                    var address = row["Адрес"]?.ToString();
                    if (string.IsNullOrEmpty(address))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Адрес'", null);

                    if (address.Length > 300)
                        return (false, $"Строка {rowNumber}: Адрес не может превышать 300 символов", null);

                    var phone = row["Телефон"]?.ToString();
                    if (string.IsNullOrEmpty(phone))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Телефон'", null);

                    var phoneDigits = new string(phone.Where(char.IsDigit).ToArray());
                    if (phoneDigits.Length != 11)
                        return (false, $"Строка {rowNumber}: Телефон должен содержать 11 цифр", null);

                    if (await _context.PublishingHouses.AnyAsync(ph => ph.Title == title))
                        return (false, $"Строка {rowNumber}: Издательство '{title}' уже существует", null);

                    var email = row["Email"]?.ToString();
                    if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        return (false, $"Строка {rowNumber}: Неверный формат email", null);

                    var publishingHouse = new PublishingHouse
                    {
                        Title = title,
                        PublishingHouseAddress = address,
                        Phone = phone,
                        Email = row["Email"]?.ToString()
                    };

                    return (true, null, publishingHouse);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private bool PublishingHouseExists(int id)
        {
            return _context.PublishingHouses.Any(e => e.IdPublishingHouse == id);
        }
    }
}
