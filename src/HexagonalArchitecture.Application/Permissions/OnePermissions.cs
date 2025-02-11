namespace HexagonalArchitecture.Application.Permissions;
public static class OnePermissions
{
    public const string GroupName = "One";
    public const string AdminPolicy = "AdminPolicy";
    public const string UserPolicy = "UserPolicy";

    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }   
}