namespace HexagonalArchitecture.Application.Permissions;
public static class OnePermissions
{
    public const string GroupName = "One";
    public const string AdminPolicy = "admin";
    public const string UserPolicy = "user";

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }   
}