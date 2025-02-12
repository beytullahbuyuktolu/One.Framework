using System.Reflection;

namespace HexagonalArchitecture.Application.Permissions;
public static class OnePermissions
{
    public const string GroupName = "One";
    public const string AdminPolicy = "admin";
    public const string UserPolicy = "user";

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = ".Create";
        public const string Read = ".Read";
        public const string Update = ".Update";
        public const string Delete = ".Delete";
        public const string Export = ".Export";
    }
    public static IEnumerable<string> GetAllPermissions()
    {
        var permissions = new List<string>();
        var nestedTypes = typeof(OnePermissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        foreach (var type in nestedTypes)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            permissions.AddRange(fields.Select(f => f.GetValue(null).ToString()));
        }
        return permissions;
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