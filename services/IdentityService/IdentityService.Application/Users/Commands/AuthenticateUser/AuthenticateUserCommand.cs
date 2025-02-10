using FluentValidation;
using IdentityService.Domain.Authentication;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Repository;
using SharedKernel.Domain;
using SharedKernel.Messaging;

namespace IdentityService.Application.Users.Commands.AuthenticateUser;

public record AuthenticateUserCommand(string Email, string Password) : IRequest<AuthenticationResult>;

public record AuthenticateUserResult
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
    public string Token { get; init; } = null!;
}

public class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(v => v.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthenticateUserCommandHandler> _logger;
    private readonly IMessageBus _messageBus;

    public AuthenticateUserCommandHandler(
        IRepository<User, Guid> userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        ILogger<AuthenticateUserCommandHandler> logger,
        IMessageBus messageBus)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with email: {Email}", request.Email);
            throw new InvalidOperationException("Invalid email or password");
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user: {Email}", request.Email);
            throw new InvalidOperationException("Invalid email or password");
        }

        var (token, tokenExpiration) = _jwtTokenGenerator.GenerateToken(user);
        var (refreshToken, refreshTokenExpiration) = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = refreshTokenExpiration;
        user.LastLoginAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _messageBus.PublishAsync(new UserAuthenticatedEvent(user.Id), "user-authenticated", cancellationToken);

        return new AuthenticateUserResult
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token
        };
    }
}

public record UserAuthenticatedEvent(Guid UserId);
