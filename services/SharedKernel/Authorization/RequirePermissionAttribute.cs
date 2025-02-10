using Microsoft.AspNetCore.Authorization;

namespace SharedKernel.Authorization
{
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public const string POLICY_PREFIX = "PERMISSION_";

        public RequirePermissionAttribute(string permission) : base(POLICY_PREFIX + permission)
        {
        }
    }
}
