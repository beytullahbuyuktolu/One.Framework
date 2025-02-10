namespace IdentityService.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string name, string token, CancellationToken cancellationToken);
    Task SendEmailVerificationAsync(string email, string name, string token, CancellationToken cancellationToken);
    Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken);
}
