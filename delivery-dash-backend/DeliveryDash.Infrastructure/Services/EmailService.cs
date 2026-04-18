using System.Net;
using System.Net.Mail;
using DeliveryDash.Application.Abstracts.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeliveryDash.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl)
        {
            var subject = "Password Reset Request - DeliveryDash";
            var resetLink = $"{resetUrl}?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";
            var htmlBody = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta name='color-scheme' content='light dark'>
    <meta name='supported-color-schemes' content='light dark'>
    <style>
        :root {{ color-scheme: light dark; }}
        @media (prefers-color-scheme: dark) {{
            .email-body {{ background-color: #0f0f23 !important; }}
            .email-container {{ background-color: #1a1a2e !important; }}
            .content-text {{ color: #e0e0e0 !important; }}
            .heading-text {{ color: #ffffff !important; }}
            .muted-text {{ color: #888888 !important; }}
            .info-box {{ background-color: #252542 !important; border-color: #3a3a5c !important; }}
            .warning-box {{ background-color: #3d2e0a !important; border-color: #5c4a1a !important; }}
            .warning-text {{ color: #fbbf24 !important; }}
            .link-box {{ background-color: #252542 !important; }}
            .footer-section {{ background-color: #141428 !important; border-color: #252542 !important; }}
        }}
        @media only screen and (max-width: 520px) {{
            .email-container {{ width: 100% !important; border-radius: 0 !important; }}
            .email-wrapper {{ padding: 0 !important; }}
            .header-section {{ padding: 24px 20px !important; }}
            .content-section {{ padding: 24px 20px 20px !important; }}
            .notice-section {{ padding: 0 20px 20px !important; }}
            .link-section {{ padding: 0 20px 24px !important; }}
            .footer-section {{ padding: 16px 20px !important; }}
            .cta-button {{ padding: 14px 32px !important; font-size: 14px !important; }}
            .header-icon {{ width: 48px !important; height: 48px !important; line-height: 48px !important; }}
            .header-icon span {{ font-size: 24px !important; }}
            .header-title {{ font-size: 22px !important; }}
            .main-heading {{ font-size: 20px !important; }}
        }}
    </style>
</head>
<body class='email-body' style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #eef2f7;'>
    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: inherit;'>
        <tr>
            <td class='email-wrapper' align='center' style='padding: 24px 12px;'>
                <!--[if mso]>
                <table role='presentation' width='480' cellspacing='0' cellpadding='0' align='center'>
                <tr><td>
                <![endif]-->
                <table class='email-container' role='presentation' cellspacing='0' cellpadding='0' style='width: 100%; max-width: 480px; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.12);'>
                    <!-- Header -->
                    <tr>
                        <td class='header-section' style='background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 50%, #a855f7 100%); padding: 28px 24px; text-align: center;'>
                            <div class='header-icon' style='width: 52px; height: 52px; background-color: rgba(255,255,255,0.2); border-radius: 12px; margin: 0 auto 12px; line-height: 52px;'>
                                <span style='font-size: 26px;'>🔐</span>
                            </div>
                            <h1 class='header-title' style='margin: 0; color: #ffffff; font-size: 24px; font-weight: 700;'>DeliveryDash</h1>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td class='content-section' style='padding: 28px 24px 20px;'>
                            <h2 class='heading-text main-heading' style='margin: 0 0 6px; color: #1f2937; font-size: 20px; font-weight: 700; text-align: center;'>Reset Your Password</h2>
                            <p class='muted-text' style='margin: 0 0 20px; color: #6b7280; font-size: 14px; text-align: center;'>
                                We received a password reset request for your account
                            </p>
                            <!-- CTA Button -->
                            <table role='presentation' width='100%' cellspacing='0' cellpadding='0'>
                                <tr>
                                    <td align='center' style='padding: 4px 0 20px;'>
                                        <a class='cta-button' href='{resetLink}' style='display: inline-block; background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%); color: #ffffff; text-decoration: none; font-size: 15px; font-weight: 600; padding: 14px 40px; border-radius: 10px; box-shadow: 0 4px 14px rgba(99, 102, 241, 0.4);'>
                                            Reset Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <!-- Expiry Info -->
                            <table class='info-box' role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 10px;'>
                                <tr>
                                    <td style='padding: 14px;'>
                                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td width='32' valign='top' style='padding-right: 10px;'>
                                                    <span style='font-size: 18px;'>⏰</span>
                                                </td>
                                                <td>
                                                    <p class='content-text' style='margin: 0; color: #374151; font-size: 13px; line-height: 1.4;'>
                                                        <strong>Expires in 1 hour</strong><br/>
                                                        <span class='muted-text' style='color: #6b7280; font-size: 12px;'>For security, this link will expire soon</span>
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Warning -->
                    <tr>
                        <td class='notice-section' style='padding: 0 24px 20px;'>
                            <table class='info-box warning-box' role='presentation' width='100%' cellspacing='0' cellpadding='0' style='background-color: #fef3c7; border: 1px solid #fcd34d; border-radius: 10px;'>
                                <tr>
                                    <td style='padding: 12px 14px;'>
                                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0'>
                                            <tr>
                                                <td width='28' valign='middle' style='padding-right: 8px;'>
                                                    <span style='font-size: 16px;'>⚠️</span>
                                                </td>
                                                <td>
                                                    <p class='warning-text' style='margin: 0; color: #92400e; font-size: 12px; line-height: 1.4;'>
                                                        <strong>Didn't request this?</strong> Ignore this email.
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Fallback Link -->
                    <tr>
                        <td class='link-section' style='padding: 0 24px 24px;'>
                            <p class='muted-text' style='margin: 0 0 6px; color: #9ca3af; font-size: 11px;'>Or copy this link:</p>
                            <p class='link-box' style='margin: 0; padding: 10px; background-color: #f3f4f6; border-radius: 6px; word-break: break-all; overflow-wrap: anywhere;'>
                                <a href='{resetLink}' style='color: #6366f1; font-size: 11px; text-decoration: none;'>{resetLink}</a>
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td class='footer-section' style='background-color: #f9fafb; padding: 18px 24px; text-align: center; border-top: 1px solid #e5e7eb;'>
                            <p class='muted-text' style='margin: 0 0 4px; color: #6b7280; font-size: 13px;'><strong>The DeliveryDash Team</strong></p>
                            <p class='muted-text' style='margin: 0; color: #9ca3af; font-size: 11px;'>&copy; {DateTime.UtcNow.Year} DeliveryDash</p>
                        </td>
                    </tr>
                </table>
                <!--[if mso]>
                </td></tr>
                </table>
                <![endif]-->
            </td>
        </tr>
    </table>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "DeliveryDash";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}