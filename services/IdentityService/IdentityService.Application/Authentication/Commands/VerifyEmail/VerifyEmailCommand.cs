using FluentValidation;
using IdentityService.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Authentication.Commands.VerifyEmail;

public record VerifyEmailCommand : IRequest<bool>
{
    public string Token { get; init; } = null!;
    public Guid TenantId { get; init; }
}

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(v => v.Token)
            .NotEmpty();

        RuleFor(v => v.TenantId)
            .NotEmpty();
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IIdentityDbContext _context;
    private readonly ILogger<VerifyEmailCommandHandler> _logger;

    public VerifyEmailCommandHandler(
        IIdentityDbContext context,
        ILogger<VerifyEmailCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => 
                u.EmailVerificationToken == request.Token && 
                u.TenantId == request.TenantId &&
                u.EmailVerificationTokenExpiryTime > DateTime.UtcNow, 
                cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Invalid or expired email verification token used");
            return false;
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiryTime = null;
        user.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
