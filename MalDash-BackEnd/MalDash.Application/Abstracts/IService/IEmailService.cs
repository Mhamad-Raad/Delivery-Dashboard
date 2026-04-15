namespace MalDash.Application.Abstracts.IService
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}