using BookStoreCursov.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookStoreCursov.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IExcelService _excelService;

        public UsersController(BookStoreCursContext context, IExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.IdUser)
            {
                return BadRequest(new { message = "Несоответствие идентификатора пользователя" });
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.UserLogin))
            {
                errors.Add("Логин обязателен для заполнения");
            }
            else if (user.UserLogin.Length < 3 || user.UserLogin.Length > 50)
            {
                errors.Add("Логин должен содержать от 3 до 50 символов");
            }
            else if (!Regex.IsMatch(user.UserLogin, @"^[a-zA-Z0-9_]+$"))
            {
                errors.Add("Логин может содержать только латинские буквы, цифры и подчеркивание");
            }
            else
            {
                var loginExists = await _context.Users.AnyAsync(u => u.UserLogin == user.UserLogin && u.IdUser != id);
                if (loginExists)
                {
                    errors.Add("Пользователь с таким логином уже существует");
                }
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errors.Add("Email обязателен для заполнения");
            }
            else if (user.Email.Length > 100)
            {
                errors.Add("Email не может превышать 100 символов");
            }
            else if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errors.Add("Неверный формат email");
            }
            else
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email && u.IdUser != id);
                if (emailExists)
                {
                    errors.Add("Пользователь с таким email уже существует");
                }
            }

            if (string.IsNullOrWhiteSpace(user.UsersName))
            {
                errors.Add("Имя обязательно для заполнения");
            }
            else if (user.UsersName.Length > 50)
            {
                errors.Add("Имя не может превышать 50 символов");
            }
            else if (!Regex.IsMatch(user.UsersName, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Имя может содержать только буквы, пробелы и дефисы");
            }

            if (string.IsNullOrWhiteSpace(user.UsersSurname))
            {
                errors.Add("Фамилия обязательна для заполнения");
            }
            else if (user.UsersSurname.Length > 50)
            {
                errors.Add("Фамилия не может превышать 50 символов");
            }
            else if (!Regex.IsMatch(user.UsersSurname, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Фамилия может содержать только буквы, пробелы и дефисы");
            }

            if (!string.IsNullOrEmpty(user.UsersPatronymic))
            {
                if (user.UsersPatronymic.Length > 50)
                {
                    errors.Add("Отчество не может превышать 50 символов");
                }
                else if (!Regex.IsMatch(user.UsersPatronymic, @"^[а-яА-Яa-zA-Z\s\-]*$"))
                {
                    errors.Add("Отчество может содержать только буквы, пробелы и дефисы");
                }
            }

            if (user.DateOfBirth.HasValue && user.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                errors.Add("Дата рождения не может быть позже текущей даты");
            }

            if (user.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var birthDate = user.DateOfBirth.Value;
                int age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;

                if (age < 12)
                {
                    errors.Add("Возраст должен быть не менее 12 лет");
                }
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.IdRole == user.RoleId);
            if (user.RoleId <= 0 || !roleExists)
            {
                errors.Add("Выбранная роль не существует в системе");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            existingUser.UserLogin = user.UserLogin;
            existingUser.Email = user.Email;
            existingUser.UsersName = user.UsersName;
            existingUser.UsersSurname = user.UsersSurname;
            existingUser.UsersPatronymic = user.UsersPatronymic;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.RoleId = user.RoleId;
            existingUser.AvatarPath = user.AvatarPath ?? existingUser.AvatarPath;

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.PasswordHash = user.PasswordHash;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(user.UserLogin))
            {
                errors.Add("Логин обязателен для заполнения");
            }
            else if (user.UserLogin.Length < 3 || user.UserLogin.Length > 50)
            {
                errors.Add("Логин должен содержать от 3 до 50 символов");
            }
            else if (!Regex.IsMatch(user.UserLogin, @"^[a-zA-Z0-9_]+$"))
            {
                errors.Add("Логин может содержать только латинские буквы, цифры и подчеркивание");
            }
            else
            {
                var loginExists = await _context.Users.AnyAsync(u => u.UserLogin == user.UserLogin);
                if (loginExists)
                {
                    errors.Add("Пользователь с таким логином уже существует");
                }
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                errors.Add("Email обязателен для заполнения");
            }
            else if (user.Email.Length > 100)
            {
                errors.Add("Email не может превышать 100 символов");
            }
            else if (!Regex.IsMatch(user.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errors.Add("Неверный формат email");
            }
            else
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (emailExists)
                {
                    errors.Add("Пользователь с таким email уже существует");
                }
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                errors.Add("Пароль обязателен для заполнения");
            }
            else if (user.PasswordHash.Length < 5 || !user.PasswordHash.Any(char.IsDigit))
            {
                errors.Add("Пароль должен содержать не менее 5 символов и хотя бы одну цифру");
            }

            if (string.IsNullOrWhiteSpace(user.UsersName))
            {
                errors.Add("Имя обязательно для заполнения");
            }
            else if (user.UsersName.Length > 50)
            {
                errors.Add("Имя не может превышать 50 символов");
            }
            else if (!Regex.IsMatch(user.UsersName, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Имя может содержать только буквы, пробелы и дефисы");
            }

            if (string.IsNullOrWhiteSpace(user.UsersSurname))
            {
                errors.Add("Фамилия обязательна для заполнения");
            }
            else if (user.UsersSurname.Length > 50)
            {
                errors.Add("Фамилия не может превышать 50 символов");
            }
            else if (!Regex.IsMatch(user.UsersSurname, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Фамилия может содержать только буквы, пробелы и дефисы");
            }

            if (!string.IsNullOrEmpty(user.UsersPatronymic))
            {
                if (user.UsersPatronymic.Length > 50)
                {
                    errors.Add("Отчество не может превышать 50 символов");
                }
                else if (!Regex.IsMatch(user.UsersPatronymic, @"^[а-яА-Яa-zA-Z\s\-]*$"))
                {
                    errors.Add("Отчество может содержать только буквы, пробелы и дефисы");
                }
            }

            if (user.DateOfBirth.HasValue && user.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                errors.Add("Дата рождения не может быть позже текущей даты");
            }

            if (user.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var birthDate = user.DateOfBirth.Value;
                int age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;

                if (age < 12)
                {
                    errors.Add("Возраст должен быть не менее 12 лет");
                }
            }

            var roleExists = await _context.Roles.AnyAsync(r => r.IdRole == user.RoleId);
            if (user.RoleId <= 0 || !roleExists)
            {
                errors.Add("Выбранная роль не существует в системе");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.Length < 64)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.IdUser }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var hasActiveOrders = await _context.Orders
                .Include(o => o.OrderStatus)
                .AnyAsync(o => o.UsersId == id &&
                    o.OrderStatus.Title != "Отменен" &&
                    o.OrderStatus.Title != "Доставлен");

            if (hasActiveOrders)
            {
                return BadRequest(new { message = "Невозможно удалить аккаунт с активными заказами" });
            }

            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                var avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarPath.TrimStart('/'));
                if (System.IO.File.Exists(avatarPath))
                {
                    System.IO.File.Delete(avatarPath);
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Users/5/profile
        [HttpPatch("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден" });
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.UserLogin))
            {
                errors.Add("Логин обязателен для заполнения");
            }
            else if (model.UserLogin.Length < 3 || model.UserLogin.Length > 50)
            {
                errors.Add("Логин должен содержать от 3 до 50 символов");
            }
            else if (!Regex.IsMatch(model.UserLogin, @"^[a-zA-Z0-9_]+$"))
            {
                errors.Add("Логин может содержать только латинские буквы, цифры и подчеркивание");
            }

            if (user.UserLogin != model.UserLogin && await _context.Users.AnyAsync(u => u.UserLogin == model.UserLogin))
            {
                errors.Add("Логин уже занят");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                errors.Add("Email обязателен для заполнения");
            }
            else if (model.Email.Length > 100)
            {
                errors.Add("Email не может превышать 100 символов");
            }
            else if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errors.Add("Неверный формат email");
            }

            if (user.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                errors.Add("Email уже занят");
            }

            if (string.IsNullOrWhiteSpace(model.UsersName))
            {
                errors.Add("Имя обязательно для заполнения");
            }
            else if (model.UsersName.Length > 50)
            {
                errors.Add("Имя не может превышать 50 символов");
            }
            else if (!Regex.IsMatch(model.UsersName, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Имя может содержать только буквы, пробелы и дефисы");
            }

            if (string.IsNullOrWhiteSpace(model.UsersSurname))
            {
                errors.Add("Фамилия обязательна для заполнения");
            }
            else if (model.UsersSurname.Length > 50)
            {
                errors.Add("Фамилия не может превышать 50 символов");
            }
            else if (!Regex.IsMatch(model.UsersSurname, @"^[а-яА-Яa-zA-Z\s\-]+$"))
            {
                errors.Add("Фамилия может содержать только буквы, пробелы и дефисы");
            }

            if (!string.IsNullOrEmpty(model.UsersPatronymic))
            {
                if (model.UsersPatronymic.Length > 50)
                {
                    errors.Add("Отчество не может превышать 50 символов");
                }
                else if (!Regex.IsMatch(model.UsersPatronymic, @"^[а-яА-Яa-zA-Z\s\-]*$"))
                {
                    errors.Add("Отчество может содержать только буквы, пробелы и дефисы");
                }
            }

            if (model.DateOfBirth.HasValue && model.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Now))
            {
                errors.Add("Дата рождения не может быть позже текущей даты");
            }

            if (model.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var birthDate = model.DateOfBirth.Value;
                int age = today.Year - birthDate.Year;
                if (birthDate > today.AddYears(-age)) age--;

                if (age < 12)
                {
                    errors.Add("Возраст должен быть не менее 12 лет");
                }
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            user.UserLogin = model.UserLogin;
            user.Email = model.Email;
            user.UsersName = model.UsersName;
            user.UsersSurname = model.UsersSurname;
            user.UsersPatronymic = model.UsersPatronymic;
            user.DateOfBirth = model.DateOfBirth;

            await _context.SaveChangesAsync();

            var updatedUser = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.IdUser == id);

            return Ok(new
            {
                user = new
                {
                    updatedUser!.IdUser,
                    updatedUser.UserLogin,
                    updatedUser.Email,
                    updatedUser.UsersName,
                    updatedUser.UsersSurname,
                    updatedUser.UsersPatronymic,
                    updatedUser.DateOfBirth,
                    updatedUser.AvatarPath,
                    updatedUser.RoleId,
                    roleTitle = updatedUser.Role?.Title ?? ""
                },
                success = true
            });
        }

        // PATCH: api/Users/5/avatar
        [HttpPatch("{id}/avatar")]
        public async Task<IActionResult> UpdateAvatar(int id, IFormFile? avatar)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден", errors = new List<string> { "Пользователь не найден" } });
            }

            if (avatar == null || avatar.Length == 0)
            {
                return BadRequest(new { message = "Ошибка валидации", errors = new List<string> { "Файл не выбран" } });
            }

            var errors = new List<string>();

            if (avatar.Length > 5 * 1024 * 1024)
            {
                errors.Add("Размер файла не должен превышать 5 МБ");
            }

            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                errors.Add("Допустимые форматы: JPEG, PNG");
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedMimeTypes.Contains(avatar.ContentType.ToLower()))
            {
                errors.Add("Неверный тип файла. Допустимы только изображения.");
            }

            if (errors.Any())
            {
                return BadRequest(new { message = "Ошибка валидации", errors = errors });
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarPath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            var fileName = $"avatar_{id}_{DateTime.Now.Ticks}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.AvatarPath = $"/images/avatars/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { avatarPath = user.AvatarPath, success = true });
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string search = null, [FromQuery] int? roleId = null)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.UserLogin.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.UsersName.Contains(search) ||
                    u.UsersSurname.Contains(search));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<User, string>>
            {
                ["ID"] = u => u.IdUser.ToString(),
                ["Логин"] = u => u.UserLogin,
                ["Email"] = u => u.Email,
                ["Имя"] = u => u.UsersName,
                ["Фамилия"] = u => u.UsersSurname,
                ["Отчество"] = u => u.UsersPatronymic ?? "",
                ["Дата рождения"] = u => u.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                ["Роль"] = u => u.Role?.Title ?? "",
                ["Аватар"] = u => u.AvatarPath ?? ""
            };

            var bytes = _excelService.ExportToExcel(data, "Пользователи", customFields);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] string search = null, [FromQuery] int? roleId = null)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.UserLogin.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.UsersName.Contains(search) ||
                    u.UsersSurname.Contains(search));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            var data = await query.ToListAsync();

            var customFields = new Dictionary<string, Func<User, string>>
            {
                ["ID"] = u => u.IdUser.ToString(),
                ["Логин"] = u => u.UserLogin,
                ["Email"] = u => u.Email,
                ["Имя"] = u => u.UsersName,
                ["Фамилия"] = u => u.UsersSurname,
                ["Отчество"] = u => u.UsersPatronymic ?? "",
                ["Дата рождения"] = u => u.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                ["Роль"] = u => u.Role?.Title ?? "",
                ["Аватар"] = u => u.AvatarPath ?? ""
            };

            var bytes = _excelService.ExportToCsv(data, customFields);
            return File(bytes, "text/csv", "users.csv");
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не выбран" });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".csv")
                return BadRequest(new { message = "Поддерживаются только файлы .xlsx и .csv" });

            var roles = await _context.Roles.ToListAsync();

            var result = await _excelService.ImportFromExcelAsync<User>(
                file.OpenReadStream(),
                _context,
                async (row, rowNumber) =>
                {
                    var login = row["Логин"]?.ToString();
                    if (string.IsNullOrEmpty(login))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Логин'", null);

                    if (login.Length < 3 || login.Length > 50)
                        return (false, $"Строка {rowNumber}: Логин должен содержать от 3 до 50 символов", null);

                    if (!Regex.IsMatch(login, @"^[a-zA-Z0-9_]+$"))
                        return (false, $"Строка {rowNumber}: Логин может содержать только латинские буквы, цифры и подчеркивание", null);

                    if (await _context.Users.AnyAsync(u => u.UserLogin == login))
                        return (false, $"Строка {rowNumber}: Пользователь с логином '{login}' уже существует", null);

                    var email = row["Email"]?.ToString();
                    if (string.IsNullOrEmpty(email))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Email'", null);

                    if (email.Length > 100)
                        return (false, $"Строка {rowNumber}: Email не может превышать 100 символов", null);

                    if (await _context.Users.AnyAsync(u => u.Email == email))
                        return (false, $"Строка {rowNumber}: Пользователь с email '{email}' уже существует", null);

                    if (!email.Contains("@") || !email.Contains("."))
                        return (false, $"Строка {rowNumber}: Неверный формат email", null);

                    var name = row["Имя"]?.ToString();
                    if (string.IsNullOrEmpty(name))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Имя'", null);

                    if (name.Length > 50)
                        return (false, $"Строка {rowNumber}: Имя не может превышать 50 символов", null);

                    var surname = row["Фамилия"]?.ToString();
                    if (string.IsNullOrEmpty(surname))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Фамилия'", null);

                    if (surname.Length > 50)
                        return (false, $"Строка {rowNumber}: Фамилия не может превышать 50 символов", null);

                    var password = row["Пароль"]?.ToString();
                    if (string.IsNullOrEmpty(password))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Пароль'", null);

                    if (password.Length < 5 || !password.Any(char.IsDigit))
                        return (false, $"Строка {rowNumber}: Пароль должен содержать не менее 5 символов и хотя бы одну цифру", null);

                    var roleName = row["Роль"]?.ToString();
                    if (string.IsNullOrEmpty(roleName))
                        return (false, $"Строка {rowNumber}: Не заполнено обязательное поле 'Роль'", null);

                    var role = roles.FirstOrDefault(r => r.Title == roleName);
                    if (role == null)
                        return (false, $"Строка {rowNumber}: Роль '{roleName}' не найдена в базе данных", null);

                    DateOnly? birthDate = null;
                    var birthDateStr = row["Дата рождения"]?.ToString();
                    if (!string.IsNullOrEmpty(birthDateStr))
                    {
                        if (!DateOnly.TryParse(birthDateStr, out var parsedDate))
                            return (false, $"Строка {rowNumber}: Неверный формат даты рождения", null);
                        if (parsedDate > DateOnly.FromDateTime(DateTime.Now))
                            return (false, $"Строка {rowNumber}: Дата рождения не может быть позже текущей даты", null);

                        var today = DateOnly.FromDateTime(DateTime.Now);
                        var age = today.Year - parsedDate.Year;
                        if (parsedDate > today.AddYears(-age)) age--;

                        if (age < 12)
                            return (false, $"Строка {rowNumber}: Возраст должен быть не менее 12 лет", null);

                        birthDate = parsedDate;
                    }

                    var patronymic = row["Отчество"]?.ToString();
                    if (!string.IsNullOrEmpty(patronymic) && patronymic.Length > 50)
                        return (false, $"Строка {rowNumber}: Отчество не может превышать 50 символов", null);

                    var user = new User
                    {
                        UserLogin = login,
                        PasswordHash = password,
                        Email = email,
                        UsersName = name,
                        UsersSurname = surname,
                        UsersPatronymic = row["Отчество"]?.ToString(),
                        DateOfBirth = birthDate,
                        RoleId = role.IdRole,
                        AvatarPath = row["Аватар"]?.ToString()
                    };

                    return (true, null, user);
                }
            );

            var message = $"Импорт завершен. Добавлено: {result.Added}, Пропущено: {result.Skipped}";

            return Ok(new { message = message, added = result.Added, skipped = result.Skipped, errors = result.Errors });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private bool IsPasswordValid(string password)
        {
            return password.Length >= 5 && password.Any(char.IsDigit);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.IdUser == id);
        }

    }
}

public class UpdateProfileModel
{
    public string UserLogin { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UsersName { get; set; } = null!;
    public string UsersSurname { get; set; } = null!;
    public string? UsersPatronymic { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}

