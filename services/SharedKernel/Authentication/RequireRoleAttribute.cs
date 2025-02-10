using Microsoft.AspNetCore.Authorization;

namespace SharedKernel.Authentication;

public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(string role) : base($"Role_{role}")
    {
    }
}
