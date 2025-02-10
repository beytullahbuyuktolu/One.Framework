using Microsoft.AspNetCore.Authorization;

namespace SharedKernel.Authentication;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(policy: $"Permission_{permission}")
    {
    }
}
