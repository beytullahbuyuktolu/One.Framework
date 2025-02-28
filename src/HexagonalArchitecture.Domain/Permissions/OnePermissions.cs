using System.Reflection;

namespace HexagonalArchitecture.Domain.Permissions;

public static class OnePermissions
{
    public const string GroupName = "One";
    public const string AdminPolicy = "admin";
    public const string UserPolicy = "user";

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Read = Default + ".Read";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string Export = Default + ".Export";
    }

    public static IEnumerable<string> GetAllPermissions()
    {
        return new[]
        {
            Products.Create,
            Products.Read,
            Products.Update,
            Products.Delete
        };
    }

    public static IEnumerable<(string Module, string Action)> GetPermissionPairs()
    {
        return GetAllPermissions().Select(p =>
        {
            var parts = p.Split(':');
            return (parts[0], parts[1]);
        });
    }
}