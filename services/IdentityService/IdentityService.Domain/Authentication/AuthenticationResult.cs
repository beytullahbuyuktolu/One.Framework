namespace IdentityService.Domain.Authentication;

public record AuthenticationResult(
    Guid UserId,
    string Email,
    string Token,
    string RefreshToken,
    DateTime TokenExpiration,
    DateTime RefreshTokenExpiration);
