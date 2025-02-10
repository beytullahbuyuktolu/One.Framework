using IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using SharedKernel.Configuration;

namespace IdentityService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string name, string token, CancellationToken cancellationToken)
    {
        var subject = "Password Reset Request";
        var body = $"Hi {name},\n\nYou have requested to reset your password. Please use the following token to reset your password: {token}\n\nIf you did not request this, please ignore this email.";
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendEmailVerificationAsync(string email, string name, string token, CancellationToken cancellationToken)
    {
        var subject = "Email Verification";
        var body = $"Hi {name},\n\nPlease verify your email address by using the following token: {token}";
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken)
    {
        var subject = "Welcome!";
        var body = $"Hi {name},\n\nWelcome to our platform! We're excited to have you on board.";
        await SendEmailAsync(email, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new System.Net.NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
