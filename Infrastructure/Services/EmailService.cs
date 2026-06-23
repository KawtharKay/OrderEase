using Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        public async Task SendVerificationEmailAsync(string toEmail, string verificationToken)
        {
            var baseUrl = configuration["AppSettings:BaseUrl"];
            var subject = "Verify your OrderEase account";
            var body = $@"
                <h2>Welcome to OrderEase</h2>
                <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                <a href='{baseUrl}/verify-email?token={verificationToken}'>
                    Verify Email Address
                </a>
                <p>This link expires in 24 hours.</p>
                <p>If you did not create an account, please ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var subject = "OrderEase password reset code";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p> Use the code below to reset your password:</p>
                <h1 style='letter-spacing: 8px;'>{resetToken}</h1>
                <p>This code expires in 15 minutes.</p>
                <p>If you did not request a password reset, please ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("OrderEase", configuration["EmailSettings:FromEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                configuration["EmailSettings:Host"],
                int.Parse(configuration["EmailSettings:Port"]!),
                SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                configuration["EmailSettings:UserName"],
                configuration["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendGenericEmailAsync(string toEmail, string subject, string body)
        {
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}