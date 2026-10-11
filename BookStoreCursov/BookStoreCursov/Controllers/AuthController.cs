using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    public class AuthController : ControllerBase
    {
        private readonly BookStoreCursContext _context;
        private readonly IEmailService _emailService;

        public AuthController(BookStoreCursContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] Auth.LoginModel loginModel)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserLogin == loginModel.Login);

                if (user == null)
                {
                    return Unauthorized(new { message = "Неверный логин или пароль" });
                }

                var hashedPassword = HashPassword(loginModel.Password);

                if (user.PasswordHash != hashedPassword)
                {
                    return Unauthorized(new { message = "Неверный логин или пароль" });
                }

                var response = new
                {
                    user = new
                    {
                        user.IdUser,
                        user.UserLogin,
                        user.Email,
                        user.UsersName,
                        user.UsersSurname,
                        user.UsersPatronymic,
                        user.DateOfBirth,
                        user.AvatarPath,
                        user.RoleId,
                        roleTitle = user.Role?.Title ?? ""
                    },
                    success = true
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] Auth.RegisterModel registerModel)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.UserLogin == registerModel.Login))
                {
                    return BadRequest(new { message = "Пользователь с таким логином уже существует" });
                }

                if (await _context.Users.AnyAsync(u => u.Email == registerModel.Email))
                {
                    return BadRequest(new { message = "Пользователь с таким email уже существует" });
                }

                if (!IsPasswordValid(registerModel.Password))
                {
                    return BadRequest(new { message = "Пароль должен содержать не менее 5 символов и хотя бы одну цифру" });
                }

                if (registerModel.DateOfBirth.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    var birthDate = registerModel.DateOfBirth.Value;
                    int age = today.Year - birthDate.Year;
                    if (birthDate > today.AddYears(-age)) age--;

                    if (age < 12)
                    {
                        return BadRequest(new { message = "Возраст должен быть не менее 12 лет" });
                    }
                }

                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Title == "Покупатель");
                if (customerRole == null)
                {
                    return BadRequest(new { message = "Роль 'Покупатель' не найдена" });
                }

                var user = new User
                {
                    UserLogin = registerModel.Login,
                    PasswordHash = HashPassword(registerModel.Password),
                    Email = registerModel.Email,
                    UsersName = registerModel.Name,
                    UsersSurname = registerModel.Surname,
                    UsersPatronymic = registerModel.Patronymic,
                    DateOfBirth = registerModel.DateOfBirth,
                    RoleId = customerRole.IdRole
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.IdUser == user.IdUser);

                var response = new
                {
                    user = new
                    {
                        user!.IdUser,
                        user.UserLogin,
                        user.Email,
                        user.UsersName,
                        user.UsersSurname,
                        user.UsersPatronymic,
                        user.DateOfBirth,
                        user.AvatarPath,
                        user.RoleId,
                        roleTitle = user.Role?.Title ?? ""
                    },
                    success = true
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var hashedOldPassword = HashPassword(model.OldPassword);

                if (user.PasswordHash != hashedOldPassword)
                {
                    return BadRequest(new { message = "Неверный текущий пароль" });
                }

                if (!IsPasswordValid(model.NewPassword))
                {
                    return BadRequest(new { message = "Пароль должен содержать не менее 5 символов и хотя бы одну цифру" });
                }

                user.PasswordHash = HashPassword(model.NewPassword);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Пароль успешно изменен" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Login))
                {
                    return BadRequest(new { message = "Введите логин" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == model.Login);

                if (user == null)
                {
                    return Ok(new { message = "Если пользователь с таким логином существует, новый пароль будет отправлен на email" });
                }

                var newPassword = GenerateRandomPassword();
                var hashedPassword = HashPassword(newPassword);
                user.PasswordHash = hashedPassword;

                await _context.SaveChangesAsync();
                await _emailService.SendPasswordResetEmailAsync(user.Email, newPassword);
                return Ok(new { message = "Новый пароль отправлен на вашу электронную почту" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        // GET: api/Auth/check-role
        [HttpGet("check-role")]
        public async Task<ActionResult<object>> CheckRole(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.IdUser == userId);

                if (user == null)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }

                return Ok(new
                {
                    roleId = user.RoleId,
                    roleTitle = user.Role?.Title ?? ""
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
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

        private string GenerateRandomPassword()
        {
            const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            var random = new Random();

            var password = new string(Enumerable.Repeat(letters, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            password += new string(Enumerable.Repeat(digits, 2)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return new string(password.ToCharArray().OrderBy(x => random.Next()).ToArray());
        }
    }

    public class ChangePasswordModel
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ForgotPasswordModel
    {
        public string Login { get; set; } = null!;
    }
}