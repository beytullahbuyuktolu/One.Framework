using Microsoft.AspNetCore.Authorization;

namespace SharedKernel.Authentication;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "roles" && 
            c.Value.Split(',').Contains(requirement.Role)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public RoleRequirement(string role)
    {
        Role = role;
    }
}
