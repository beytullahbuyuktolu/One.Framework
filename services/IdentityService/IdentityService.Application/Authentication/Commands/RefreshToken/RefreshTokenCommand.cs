using FluentValidation;
using IdentityService.Domain.Authentication;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Repository;
using SharedKernel.Domain;

namespace IdentityService.Application.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthenticationResult>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRepository<User, Guid> userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token: {RefreshToken}", request.RefreshToken);
            throw new InvalidOperationException("Invalid or expired refresh token");
        }

        var (token, tokenExpiration) = _jwtTokenGenerator.GenerateToken(user);
        var (refreshToken, refreshTokenExpiration) = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = refreshTokenExpiration;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthenticationResult(
            user.Id,
            user.Email,
            token,
            refreshToken,
            tokenExpiration,
            refreshTokenExpiration);
    }
}
