using Microsoft.AspNetCore.Authorization;

namespace HexagonalArchitecture.Domain.Permissions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = $"Permission_{permission}";
    }
} 