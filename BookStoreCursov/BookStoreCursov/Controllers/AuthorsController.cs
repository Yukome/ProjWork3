using BookStoreCursov.Models;
using BookStoreCursov.Services;
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
    public class AuthorsController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public AuthorsController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Authors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            return await _context.Authors.ToListAsync();
        }

        // GET: api/Authors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> GetAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);

            if (author == null)
            {
                return NotFound();
            }

            return author;
        }

        // PUT: api/Authors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuthor(int id, Author author)
        {
            if (id != author.IdAuthor)
            {
                return BadRequest(new { message = "Несоответствие идентификатора автора" });
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(author.AuthorSurname))
            {
                errors.Add("Фамилия автора обязательна для заполнения");
            }
            else if (author.AuthorSurname.Length < 2 || author.AuthorSurname.Length > 50)
            {
                errors.Add("Фамилия автора должна содержать от 2 до 50 символов");
            }
            else if (!Regex.IsMatch(author.AuthorSurname, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Фамилия может содержать только буквы, пробелы и дефисы");
            }

            if (string.IsNullOrWhiteSpace(author.AuthorName))
            {
                errors.Add("Имя автора обязательно для заполнения");
            }
            else if (author.AuthorName.Length < 2 || author.AuthorName.Length > 50)
            {
                errors.Add("Имя автора должна содержать от 2 до 50 символов");
            }
            else if (!Regex.IsMatch(author.AuthorName, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Имя может содержать только буквы, пробелы и дефисы");
            }

            if (!string.IsNullOrEmpty(author.AuthorPatronymic))
            {
                if (author.AuthorPatronymic.Length > 50)
                {
                    errors.Add("Отчество не может быть длиннее 50 символов");
                }
                else if (!Regex.IsMatch(author.AuthorPatronymic, @"^[а-яА-Яa-zA-Z\s\-]*$"))
                {
                    errors.Add("Отчество может содержать только буквы, пробелы и дефисы");
                }
            }

            var countryExists = await _context.Countries.AnyAsync(c => c.IdCountry == author.CountryId);
            if (!countryExists)
            {
                errors.Add("Выбранная страна не существует в системе");
            }

            if (author.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var minDate = new DateOnly(today.Year - 12, today.Month, today.Day);

                if (author.DateOfBirth.Value > today)
                {
                    errors.Add("Дата рождения не может быть позже текущей даты");
                }
                else if (author.DateOfBirth.Value > minDate)
                {
                    errors.Add("Автор должен быть не младше 12 лет");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(author).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthorExists(id))
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

        // POST: api/Authors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Author>> PostAuthor(Author author)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(author.AuthorSurname))
            {
                errors.Add("Фамилия автора обязательна для заполнения");
            }
            else if (author.AuthorSurname.Length < 2 || author.AuthorSurname.Length > 50)
            {
                errors.Add("Фамилия автора должна содержать от 2 до 50 символов");
            }
            else if (!Regex.IsMatch(author.AuthorSurname, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Фамилия может содержать только буквы, пробелы и дефисы");
            }

            if (string.IsNullOrWhiteSpace(author.AuthorName))
            {
                errors.Add("Имя автора обязательно для заполнения");
            }
            else if (author.AuthorName.Length < 2 || author.AuthorName.Length > 50)
            {
                errors.Add("Имя автора должна содержать от 2 до 50 символов");
            }
            else if (!Regex.IsMatch(author.AuthorName, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Имя может содержать только буквы, пробелы и дефисы");
            }

            if (!string.IsNullOrEmpty(author.AuthorPatronymic))
            {
                if (author.AuthorPatronymic.Length > 50)
                {
                    errors.Add("Отчество не может быть длиннее 50 символов");
                }
                else if (!Regex.IsMatch(author.AuthorPatronymic, @"^[а-яА-Яa-zA-Z\s\-]*$"))
                {
                    errors.Add("Отчество может содержать только буквы, пробелы и дефисы");
                }
            }

            var countryExists = await _context.Countries.AnyAsync(c => c.IdCountry == author.CountryId);
            if (!countryExists)
            {
                errors.Add("Выбранная страна не существует в системе");
            }

            if (author.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var minDate = new DateOnly(today.Year - 12, today.Month, today.Day);

                if (author.DateOfBirth.Value > today)
                {
                    errors.Add("Дата рождения не может быть позже текущей даты");
                }
                else if (author.DateOfBirth.Value > minDate)
                {
                    errors.Add("Автор должен быть не младше 12 лет");
                }
            }

            var authorExists = await _context.Authors.AnyAsync(a =>
                a.AuthorSurname == author.AuthorSurname &&
                a.AuthorName == author.AuthorName &&
                (a.AuthorPatronymic ?? "") == (author.AuthorPatronymic ?? ""));

            if (authorExists)
            {
                errors.Add("Автор с таким ФИО уже существует в системе");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAuthor", new { id = author.IdAuthor }, author);
        }

        // DELETE: api/Authors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            var hasBooks = await _context.Books.AnyAsync(b => b.AuthorId == id);
            if (hasBooks)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить автора",
                    errors = new List<string> { "У автора есть книги в каталоге. Сначала удалите или переназначьте книги." }
                });
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.Authors.Include(a => a.Country).AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.AuthorSurname.Contains(search) ||
                    a.AuthorName.Contains(search));
            }
            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Author, string>>
            {
                ["ID"] = a => a.IdAuthor.ToString(),
                ["Фамилия"] = a => a.AuthorSurname,
                ["Имя"] = a => a.AuthorName,
                ["Отчество"] = a => a.AuthorPatronymic ?? "",
                ["Дата рождения"] = a => a.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                ["Страна"] = a => a.Country?.Title ?? "",
                ["Биография"] = a => a.Biography ?? ""
            };

            var bytes = _excelService.ExportToExcel(data, "Авторы", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "authors.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.Authors.Include(a => a.Country).AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.AuthorSurname.Contains(search) ||
                    a.AuthorName.Contains(search));
            }
            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Author, string>>
            {
                ["ID"] = a => a.IdAuthor.ToString(),
                ["Фамилия"] = a => a.AuthorSurname,
                ["Имя"] = a => a.AuthorName,
                ["Отчество"] = a => a.AuthorPatronymic ?? "",
                ["Дата рождения"] = a => a.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                ["Страна"] = a => a.Country?.Title ?? "",
                ["Биография"] = a => a.Biography ?? ""
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "authors.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var countries = await _context.Countries.ToListAsync();

            var result = await _excelService.ImportFromExcelAsync<Author>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var surname = row["Фамилия"]?.ToString();
                    if (string.IsNullOrEmpty(surname))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Фамилия'", null);

                    var name = row["Имя"]?.ToString();
                    if (string.IsNullOrEmpty(name))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Имя'", null);

                    var countryName = row["Страна"]?.ToString();
                    if (string.IsNullOrEmpty(countryName))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Страна'", null);

                    var country = countries.FirstOrDefault(c => c.Title == countryName);
                    if (country == null)
                        return (false, $"Строка {rowNumber}: Страна '{countryName}' не найдена в базе данных", null);

                    var patronymic = row["Отчество"]?.ToString();
                    var exists = await _context.Authors.AnyAsync(a =>
                        a.AuthorSurname == surname &&
                        a.AuthorName == name &&
                        (a.AuthorPatronymic ?? "") == (patronymic ?? ""));

                    if (exists)
                        return (false, $"Строка {rowNumber}: Автор '{surname} {name} {patronymic}' уже существует", null);

                    DateOnly? birthDate = null;
                    var birthDateStr = row["Дата рождения"]?.ToString();
                    if (!string.IsNullOrEmpty(birthDateStr))
                    {
                        if (!DateOnly.TryParse(birthDateStr, out var parsedDate))
                            return (false, $"Строка {rowNumber}: Неверный формат даты рождения. Ожидается ГГГГ-ММ-ДД", null);

                        var today = DateOnly.FromDateTime(DateTime.Now);
                        var minDate = new DateOnly(today.Year - 12, today.Month, today.Day);

                        if (parsedDate > today)
                            return (false, $"Строка {rowNumber}: Дата рождения не может быть позже текущей даты", null);

                        if (parsedDate > minDate)
                            return (false, $"Строка {rowNumber}: Автор должен быть не младше 12 лет", null);

                        birthDate = parsedDate;
                    }

                    var author = new Author
                    {
                        AuthorSurname = surname,
                        AuthorName = name,
                        AuthorPatronymic = string.IsNullOrEmpty(patronymic) ? null : patronymic,
                        DateOfBirth = birthDate,
                        CountryId = country.IdCountry,
                        Biography = row["Биография"]?.ToString()
                    };

                    return (true, null, author);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.IdAuthor == id);
        }
    }
}
