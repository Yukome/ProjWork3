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
    public class GenresController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public GenresController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Genres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Genre>>> GetGenres()
        {
            return await _context.Genres.ToListAsync();
        }

        // GET: api/Genres/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Genre>> GetGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return NotFound();
            }

            return genre;
        }

        // PUT: api/Genres/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGenre(int id, Genre genre)
        {
            if (id != genre.IdGenre)
            {
                return BadRequest(new { message = "Несоответствие идентификатора жанра" });
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(genre.Title))
            {
                errors.Add("Название жанра обязательно для заполнения");
            }
            else if (genre.Title.Length > 100)
            {
                errors.Add("Название жанра не может превышать 100 символов");
            }
            else
            {
                var titleExists = await _context.Genres.AnyAsync(g => g.Title == genre.Title && g.IdGenre != id);
                if (titleExists)
                {
                    errors.Add("Жанр с таким названием уже существует в системе");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(id))
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

        // POST: api/Genres
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Genre>> PostGenre(Genre genre)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(genre.Title))
            {
                errors.Add("Название жанра обязательно для заполнения");
            }
            else if (genre.Title.Length > 100)
            {
                errors.Add("Название жанра не может превышать 100 символов");
            }
            else
            {
                var titleExists = await _context.Genres.AnyAsync(g => g.Title == genre.Title);
                if (titleExists)
                {
                    errors.Add("Жанр с таким названием уже существует в системе");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGenre", new { id = genre.IdGenre }, genre);
        }

        // DELETE: api/Genres/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            var hasBooks = await _context.Books.AnyAsync(b => b.GenreId == id);
            if (hasBooks)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить жанр",
                    errors = new List<string> { "Существуют книги, привязанные к этому жанру. Сначала удалите или переназначьте книги." }
                });
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Genres/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.Genres.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(g => g.Title.Contains(search));
            }

            var genres = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Genre, string>>
            {
                ["ID"] = g => g.IdGenre.ToString(),
                ["Название"] = g => g.Title,
                ["Описание"] = g => g.GenreDescription ?? ""
            };

            var bytes = _excelService.ExportToExcel(genres, "Жанры", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "genres.xlsx");
        }

        // GET: api/Genres/export/csv
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.Genres.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(g => g.Title.Contains(search));
            }

            var genres = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Genre, string>>
            {
                ["ID"] = g => g.IdGenre.ToString(),
                ["Название"] = g => g.Title,
                ["Описание"] = g => g.GenreDescription ?? ""
            };

            var bytes = _excelService.ExportToCsv(genres, customFields);
            return File(bytes, "text/csv", "genres.csv");
        }

        // POST: api/Genres/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var result = await _excelService.ImportFromExcelAsync<Genre>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var title = row["Название"]?.ToString();
                    if (string.IsNullOrEmpty(title))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Название'", null);

                    if (title.Length > 100)
                        return (false, $"Строка {rowNumber}: Название жанра не может превышать 100 символов", null);

                    if (await _context.Genres.AnyAsync(g => g.Title == title))
                        return (false, $"Строка {rowNumber}: Жанр '{title}' уже существует", null);

                    var genre = new Genre
                    {
                        Title = title,
                        GenreDescription = row["Описание"]?.ToString()
                    };

                    return (true, null, genre);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new
            {
                message = message,
                added = result.Added,
                skipped = result.Skipped,
                errors = result.Errors
            });
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.IdGenre == id);
        }
    }
}
