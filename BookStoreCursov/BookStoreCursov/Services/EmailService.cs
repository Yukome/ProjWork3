using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace BookStoreCursov.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string email, string newPassword)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"] ?? "Книжный магазин";

            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f3ef; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #5E2A2B; padding: 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 24px; }}
                        .content {{ padding: 30px; }}
                        .password-box {{ background-color: #C9A99A; border-radius: 12px; padding: 20px; text-align: center; margin: 20px 0; }}
                        .password {{ font-size: 28px; font-weight: bold; color: #5E2A2B; letter-spacing: 2px; font-family: monospace; }}
                        .warning {{ background-color: #FFF3CD; border-left: 4px solid #FFC107; padding: 15px; margin: 20px 0; border-radius: 8px; }}
                        .footer {{ background-color: #3F3F50; padding: 20px; text-align: center; color: #C9A99A; font-size: 12px; }}
                        .button {{ background-color: #5E2A2B; color: white; padding: 12px 24px; text-decoration: none; border-radius: 50px; display: inline-block; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Книжный магазин</h1>
                        </div>
                        <div class='content'>
                            <h2>Восстановление пароля</h2>
                            <p>Здравствуйте!</p>
                            <p>Вы запросили восстановление пароля для вашей учетной записи.</p>
                            <div class='password-box'>
                                <div style='margin-bottom: 8px; color: #5E2A2B;'>Ваш новый пароль:</div>
                                <div class='password'>{newPassword}</div>
                            </div>
                            <div class='warning'>
                                <strong>Важно:</strong> После входа в систему обязательно смените пароль в настройках профиля.
                            </div>
                            <p style='margin-top: 20px;'>Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.</p>
                        </div>
                        <div class='footer'>
                            <p>2026 Книжный магазин. Все права защищены.</p>
                            <p>Это автоматическое сообщение, пожалуйста, не отвечайте на него.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;
                client.Timeout = 10000;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Восстановление пароля - Книжный магазин",
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"SMTP Error: {ex.Message}");
                    throw new Exception("Не удалось отправить email. Проверьте настройки почты.");
                }
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"] ?? "Книжный магазин";

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    client.EnableSsl = true;
                    client.Timeout = 10000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
                return false;
            }
        }
    }
}