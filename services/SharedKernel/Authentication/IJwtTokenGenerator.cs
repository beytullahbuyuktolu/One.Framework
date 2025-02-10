namespace SharedKernel.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string username, IEnumerable<string> permissions);
}
