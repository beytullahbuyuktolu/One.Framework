using FluentValidation;
using IdentityService.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace IdentityService.Application.Authentication.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; init; } = null!;
    public Guid TenantId { get; init; }
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(v => v.TenantId)
            .NotEmpty();
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IIdentityDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IIdentityDbContext context,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == request.TenantId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent user: {Email}", request.Email);
            return true; // Return true to prevent email enumeration
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(24);

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            user.FirstName,
            token,
            cancellationToken);

        return true;
    }
}
