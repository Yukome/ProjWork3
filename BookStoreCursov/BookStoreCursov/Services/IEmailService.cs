namespace BookStoreCursov.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string newPassword);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}