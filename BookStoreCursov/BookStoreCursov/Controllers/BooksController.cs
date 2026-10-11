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
    public class BooksController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public BooksController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books.ToListAsync();
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.IdBook)
            {
                return BadRequest(new { message = "Несоответствие идентификатора книги" });
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                errors.Add("Название книги обязательно для заполнения");
            }
            else if (book.Title.Length > 255)
            {
                errors.Add("Название книги не может превышать 255 символов");
            }

            var authorExists = await _context.Authors.AnyAsync(a => a.IdAuthor == book.AuthorId);
            if (book.AuthorId <= 0 || !authorExists)
            {
                errors.Add("Выбранный автор не существует в системе");
            }

            var publisherExists = await _context.PublishingHouses.AnyAsync(p => p.IdPublishingHouse == book.PublishingHouseId);
            if (book.PublishingHouseId <= 0 || !publisherExists)
            {
                errors.Add("Выбранное издательство не существует в системе");
            }

            var genreExists = await _context.Genres.AnyAsync(g => g.IdGenre == book.GenreId);
            if (book.GenreId <= 0 || !genreExists)
            {
                errors.Add("Выбранный жанр не существует в системе");
            }

            if (string.IsNullOrWhiteSpace(book.Isbn))
            {
                errors.Add("ISBN книги обязателен для заполнения");
            }
            else
            {
                var isbnDigits = new string(book.Isbn.Where(char.IsDigit).ToArray());
                if (isbnDigits.Length != 10 && isbnDigits.Length != 13)
                {
                    errors.Add("ISBN должен содержать 10 или 13 цифр");
                }
                else
                {
                    var isbnExists = await _context.Books.AnyAsync(b => b.Isbn == book.Isbn && b.IdBook != id);
                    if (isbnExists)
                    {
                        errors.Add("Книга с таким ISBN уже существует в системе");
                    }
                }
            }

            if (book.YearOfPublication < 1000 || book.YearOfPublication > DateTime.Now.Year)
            {
                errors.Add($"Год издания должен быть от 1000 до {DateTime.Now.Year}");
            }

            if (book.Price <= 0)
            {
                errors.Add("Цена книги должна быть больше 0");
            }
            else if (book.Price > 999999.99m)
            {
                errors.Add("Цена книги не может превышать 999 999.99 рублей");
            }

            if (book.Quantity < 0)
            {
                errors.Add("Количество книг на складе не может быть отрицательным");
            }
            else if (book.Quantity > 9999)
            {
                errors.Add("Количество книг на складе не может превышать 9 999 штук");
            }

            if (string.IsNullOrWhiteSpace(book.BookDescription))
            {
                errors.Add("Описание книги обязательно для заполнения");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
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

        // POST: api/Books
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                errors.Add("Название книги обязательно для заполнения");
            }
            else if (book.Title.Length > 255)
            {
                errors.Add("Название книги не может превышать 255 символов");
            }

            var authorExists = await _context.Authors.AnyAsync(a => a.IdAuthor == book.AuthorId);
            if (book.AuthorId <= 0 || !authorExists)
            {
                errors.Add("Выбранный автор не существует в системе");
            }

            var publisherExists = await _context.PublishingHouses.AnyAsync(p => p.IdPublishingHouse == book.PublishingHouseId);
            if (book.PublishingHouseId <= 0 || !publisherExists)
            {
                errors.Add("Выбранное издательство не существует в системе");
            }

            var genreExists = await _context.Genres.AnyAsync(g => g.IdGenre == book.GenreId);
            if (book.GenreId <= 0 || !genreExists)
            {
                errors.Add("Выбранный жанр не существует в системе");
            }

            if (string.IsNullOrWhiteSpace(book.Isbn))
            {
                errors.Add("ISBN книги обязателен для заполнения");
            }
            else
            {
                var isbnDigits = new string(book.Isbn.Where(char.IsDigit).ToArray());
                if (isbnDigits.Length != 10 && isbnDigits.Length != 13)
                {
                    errors.Add("ISBN должен содержать 10 или 13 цифр");
                }
                else
                {
                    var isbnExists = await _context.Books.AnyAsync(b => b.Isbn == book.Isbn);
                    if (isbnExists)
                    {
                        errors.Add("Книга с таким ISBN уже существует в системе");
                    }
                }
            }

            if (book.YearOfPublication < 1000 || book.YearOfPublication > DateTime.Now.Year)
            {
                errors.Add($"Год издания должен быть от 1000 до {DateTime.Now.Year}");
            }

            if (book.Price <= 0)
            {
                errors.Add("Цена книги должна быть больше 0");
            }
            else if (book.Price > 999999.99m)
            {
                errors.Add("Цена книги не может превышать 999 999.99 рублей");
            }

            if (book.Quantity < 0)
            {
                errors.Add("Количество книг на складе не может быть отрицательным");
            }
            else if (book.Quantity > 9999)
            {
                errors.Add("Количество книг на складе не может превышать 9 999 штук");
            }

            if (string.IsNullOrWhiteSpace(book.BookDescription))
            {
                errors.Add("Описание книги обязательно для заполнения");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            AppMetrics.BooksInCatalog.Inc();

            return CreatedAtAction("GetBook", new { id = book.IdBook }, book);
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var hasOrderPositions = await _context.OrderPositions.AnyAsync(op => op.BookId == id);
            if (hasOrderPositions)
            {
                return BadRequest(new
                {
                    message = "Невозможно удалить книгу",
                    errors = new List<string> { "Книга присутствует в заказах и не может быть удалена" }
                });
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            AppMetrics.BooksInCatalog.Dec();

            return NoContent();
        }

        // POST: api/Books/upload-cover
        [HttpPost("upload-cover")]
        public async Task<IActionResult> UploadCover(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Ошибка валидации", errors = new List<string> { "Файл не выбран" } });
                }

                var errors = new List<string>();

                if (file.Length > 5 * 1024 * 1024)
                {
                    errors.Add("Размер файла не должен превышать 5 МБ");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    errors.Add("Допустимые форматы: JPEG, PNG, WEBP");
                }

                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
                {
                    errors.Add("Неверный тип файла. Допустимы только изображения.");
                }

                if (errors.Any())
                {
                    return BadRequest(new { message = "Ошибка валидации", errors = errors });
                }

                var originalName = Path.GetFileNameWithoutExtension(file.FileName);
                var cleanName = string.Join("_", originalName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"{cleanName}{extension}";

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "covers");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    fileName = $"{cleanName}_{DateTime.Now.Ticks}{extension}";
                    filePath = Path.Combine(uploadPath, fileName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/images/covers/{fileName}";
                return Ok(new { path = fileUrl, success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка сервера", errors = new List<string> { ex.Message } });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.PublishingHouse)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Isbn.Contains(search) ||
                    (b.Author != null && (b.Author.AuthorSurname.Contains(search) || b.Author.AuthorName.Contains(search))));
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Book, string>>
            {
                ["ID"] = b => b.IdBook.ToString(),
                ["Название"] = b => b.Title,
                ["Автор"] = b => b.Author != null ? $"{b.Author.AuthorSurname} {b.Author.AuthorName}" : "",
                ["Жанр"] = b => b.Genre?.Title ?? "",
                ["Издательство"] = b => b.PublishingHouse?.Title ?? "",
                ["ISBN"] = b => b.Isbn,
                ["Год издания"] = b => b.YearOfPublication.ToString(),
                ["Цена"] = b => b.Price.ToString("F2"),
                ["Количество"] = b => b.Quantity.ToString(),
                ["Описание"] = b => b.BookDescription ?? "",
                ["Обложка"] = b => b.CoverPath ?? "",
                ["Дата добавления"] = b => b.AddDate.ToString("yyyy-MM-dd")
            };

            var bytes = _excelService.ExportToExcel(data, "Книги", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "books.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null)
        {
            var query = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.PublishingHouse)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Isbn.Contains(search) ||
                    (b.Author != null && (b.Author.AuthorSurname.Contains(search) || b.Author.AuthorName.Contains(search))));
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<Book, string>>
            {
                ["ID"] = b => b.IdBook.ToString(),
                ["Название"] = b => b.Title,
                ["Автор"] = b => b.Author != null ? $"{b.Author.AuthorSurname} {b.Author.AuthorName}" : "",
                ["Жанр"] = b => b.Genre?.Title ?? "",
                ["Издательство"] = b => b.PublishingHouse?.Title ?? "",
                ["ISBN"] = b => b.Isbn,
                ["Год издания"] = b => b.YearOfPublication.ToString(),
                ["Цена"] = b => b.Price.ToString("F2"),
                ["Количество"] = b => b.Quantity.ToString(),
                ["Описание"] = b => b.BookDescription ?? "",
                ["Обложка"] = b => b.CoverPath ?? "",
                ["Дата добавления"] = b => b.AddDate.ToString("yyyy-MM-dd")
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "books.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var authors = await _context.Authors.ToListAsync();
            var genres = await _context.Genres.ToListAsync();
            var publishingHouses = await _context.PublishingHouses.ToListAsync();

            var result = await _excelService.ImportFromExcelAsync<Book>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var title = row["Название"]?.ToString();
                    if (string.IsNullOrEmpty(title))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Название'", null);

                    var isbn = row["ISBN"]?.ToString();
                    if (string.IsNullOrEmpty(isbn))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'ISBN'", null);

                    if (await _context.Books.AnyAsync(b => b.Isbn == isbn))
                        return (false, $"Строка {rowNumber}: Книга с ISBN '{isbn}' уже существует", null);

                    var authorName = row["Автор"]?.ToString();
                    if (string.IsNullOrEmpty(authorName))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Автор'", null);

                    var author = FindAuthor(authors, authorName);
                    if (author == null)
                        return (false, $"Строка {rowNumber}: Автор '{authorName}' не найден в базе данных", null);

                    var genreName = row["Жанр"]?.ToString();
                    if (string.IsNullOrEmpty(genreName))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Жанр'", null);

                    var genre = genres.FirstOrDefault(g => g.Title == genreName);
                    if (genre == null)
                        return (false, $"Строка {rowNumber}: Жанр '{genreName}' не найден в базе данных", null);

                    var publisherName = row["Издательство"]?.ToString();
                    if (string.IsNullOrEmpty(publisherName))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Издательство'", null);

                    var publisher = publishingHouses.FirstOrDefault(ph => ph.Title == publisherName);
                    if (publisher == null)
                        return (false, $"Строка {rowNumber}: Издательство '{publisherName}' не найдено в базе данных", null);

                    if (!int.TryParse(row["Год издания"]?.ToString(), out int year) || year < 1000 || year > DateTime.Now.Year)
                        return (false, $"Строка {rowNumber}: Год издания должен быть от 1000 до {DateTime.Now.Year}", null);

                    if (!decimal.TryParse(row["Цена"]?.ToString(), out decimal price) || price <= 0)
                        return (false, $"Строка {rowNumber}: Цена должна быть больше 0", null);

                    if (!int.TryParse(row["Количество"]?.ToString(), out int quantity) || quantity < 0)
                        return (false, $"Строка {rowNumber}: Количество не может быть отрицательным", null);

                    var description = row["Описание"]?.ToString();
                    if (string.IsNullOrEmpty(description))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Описание'", null);

                    var book = new Book
                    {
                        Title = title,
                        AuthorId = author.IdAuthor,
                        GenreId = genre.IdGenre,
                        PublishingHouseId = publisher.IdPublishingHouse,
                        Isbn = isbn,
                        YearOfPublication = year,
                        Price = price,
                        Quantity = quantity,
                        BookDescription = description,
                        CoverPath = row["Обложка"]?.ToString(),
                        AddDate = DateOnly.FromDateTime(DateTime.Now)
                    };

                    return (true, null, book);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private Author FindAuthor(List<Author> authors, string fullName)
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                return authors.FirstOrDefault(a =>
                    a.AuthorSurname == parts[0] && a.AuthorName == parts[1]);
            }
            else if (parts.Length == 3)
            {
                return authors.FirstOrDefault(a =>
                    a.AuthorSurname == parts[0] && a.AuthorName == parts[1] && a.AuthorPatronymic == parts[2]);
            }
            return null;
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.IdBook == id);
        }
    }
}
