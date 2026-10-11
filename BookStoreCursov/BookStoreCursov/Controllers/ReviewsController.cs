using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreCursov.Models;
using BookStoreCursov.Services;
using BookStoreCursov;

namespace BookStoreCursov.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public ReviewsController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // PUT: api/Reviews/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReview(int id, Review review)
        {
            if (id != review.IdReview)
            {
                return BadRequest(new { message = "Несоответствие идентификатора отзыва" });
            }

            var errors = new List<string>();

            var bookExists = await _context.Books.AnyAsync(b => b.IdBook == review.BookId);
            if (review.BookId <= 0 || !bookExists)
            {
                errors.Add("Выбранная книга не существует в системе");
            }

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == review.UsersId);
            if (review.UsersId <= 0 || !userExists)
            {
                errors.Add("Выбранный пользователь не существует в системе");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                errors.Add("Оценка должна быть от 1 до 5");
            }

            if (string.IsNullOrWhiteSpace(review.TextReview))
            {
                errors.Add("Текст отзыва обязателен для заполнения");
            }
            else if (review.TextReview.Length < 3)
            {
                errors.Add("Текст отзыва должен содержать не менее 3 символов");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
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

        // POST: api/Reviews
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            var errors = new List<string>();

            var bookExists = await _context.Books.AnyAsync(b => b.IdBook == review.BookId);
            if (review.BookId <= 0 || !bookExists)
            {
                errors.Add("Выбранная книга не существует в системе");
            }

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == review.UsersId);
            if (review.UsersId <= 0 || !userExists)
            {
                errors.Add("Выбранный пользователь не существует в системе");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                errors.Add("Оценка должна быть от 1 до 5");
            }

            if (string.IsNullOrWhiteSpace(review.TextReview))
            {
                errors.Add("Текст отзыва обязателен для заполнения");
            }
            else if (review.TextReview.Length < 3)
            {
                errors.Add("Текст отзыва должен содержать не менее 3 символов");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            AppMetrics.ReviewsCreatedTotal.Inc();

            return CreatedAtAction("GetReview", new { id = review.IdReview }, review);
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null, [FromQuery] int? rating = null)
        {
            var query = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    (r.Book != null && r.Book.Title.Contains(search)) ||
                    (r.Users != null && (r.Users.UserLogin.Contains(search) || r.Users.UsersName.Contains(search) || r.Users.UsersSurname.Contains(search))) ||
                    r.TextReview.Contains(search));
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Review, string>>
            {
                ["ID"] = r => r.IdReview.ToString(),
                ["Книга"] = r => r.Book != null ? r.Book.Title : "",
                ["Пользователь"] = r => r.Users != null ? $"{r.Users.UsersSurname} {r.Users.UsersName} ({r.Users.UserLogin})" : "",
                ["Оценка"] = r => r.Rating.ToString(),
                ["Текст отзыва"] = r => r.TextReview,
                ["Дата"] = r => r.AddDate.ToString("yyyy-MM-dd")
            };

            var bytes = _excelService.ExportToExcel(data, "Отзывы", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reviews.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null, [FromQuery] int? rating = null)
        {
            var query = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    (r.Book != null && r.Book.Title.Contains(search)) ||
                    (r.Users != null && (r.Users.UserLogin.Contains(search) || r.Users.UsersName.Contains(search) || r.Users.UsersSurname.Contains(search))) ||
                    r.TextReview.Contains(search));
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Review, string>>
            {
                ["ID"] = r => r.IdReview.ToString(),
                ["Книга"] = r => r.Book != null ? r.Book.Title : "",
                ["Пользователь"] = r => r.Users != null ? $"{r.Users.UsersSurname} {r.Users.UsersName} ({r.Users.UserLogin})" : "",
                ["Оценка"] = r => r.Rating.ToString(),
                ["Текст отзыва"] = r => r.TextReview,
                ["Дата"] = r => r.AddDate.ToString("yyyy-MM-dd")
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "reviews.csv");
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
            var users = await _context.Users.ToListAsync();

            var result = await _excelService.ImportFromExcelAsync<Review>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var bookTitle = row["Книга"]?.ToString();
                    if (string.IsNullOrEmpty(bookTitle))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Книга'", null);

                    var book = books.FirstOrDefault(b => b.Title == bookTitle);
                    if (book == null)
                        return (false, $"Строка {rowNumber}: Книга '{bookTitle}' не найдена в базе данных", null);

                    var userName = row["Пользователь"]?.ToString();
                    if (string.IsNullOrEmpty(userName))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Пользователь'", null);

                    var user = FindUserByFullName(users, userName);
                    if (user == null)
                        return (false, $"Строка {rowNumber}: Пользователь '{userName}' не найден в базе данных", null);

                    if (!int.TryParse(row["Оценка"]?.ToString(), out int rating) || rating < 1 || rating > 5)
                        return (false, $"Строка {rowNumber}: Оценка должна быть числом от 1 до 5", null);

                    var textReview = row["Текст отзыва"]?.ToString();
                    if (string.IsNullOrEmpty(textReview))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Текст отзыва'", null);

                    if (textReview.Length < 3)
                        return (false, $"Строка {rowNumber}: Текст отзыва должен содержать не менее 3 символов", null);

                    var review = new Review
                    {
                        BookId = book.IdBook,
                        UsersId = user.IdUser,
                        Rating = rating,
                        TextReview = textReview,
                        AddDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    return (true, null, review);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private User FindUserByFullName(List<User> users, string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                return users.FirstOrDefault(u =>
                    u.UsersSurname == parts[0] && u.UsersName == parts[1]);
            }
            else if (parts.Length == 3)
            {
                return users.FirstOrDefault(u =>
                    u.UsersSurname == parts[0] && u.UsersName == parts[1] && u.UsersPatronymic == parts[2]);
            }

            return users.FirstOrDefault(u => u.UserLogin == fullName);
        }


        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.IdReview == id);
        }
    }
}
