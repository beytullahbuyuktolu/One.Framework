namespace HexagonalArchitecture.Domain.Permissions;

public static class StandardPermissions
{
    public static class Users
    {
        public const string Default = "Users";
        public const string Create = "Users.Create";
        public const string Read = "Users.Read";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
        public const string ManagePermissions = "Users.ManagePermissions";
    }

    public static class Roles
    {
        public const string Default = "Roles";
        public const string Create = "Roles.Create";
        public const string Read = "Roles.Read";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
        public const string ManagePermissions = "Roles.ManagePermissions";
    }

    public static class Tenants
    {
        public const string Default = "Tenants";
        public const string Create = "Tenants.Create";
        public const string Read = "Tenants.Read";
        public const string Update = "Tenants.Update";
        public const string Delete = "Tenants.Delete";
        public const string ManageFeatures = "Tenants.ManageFeatures";
    }

    public static IEnumerable<string> GetAllPermissions()
    {
        var permissions = new List<string>();

        // Users permissions
        permissions.Add(Users.Default);
        permissions.Add(Users.Create);
        permissions.Add(Users.Read);
        permissions.Add(Users.Update);
        permissions.Add(Users.Delete);
        permissions.Add(Users.ManagePermissions);

        // Roles permissions
        permissions.Add(Roles.Default);
        permissions.Add(Roles.Create);
        permissions.Add(Roles.Read);
        permissions.Add(Roles.Update);
        permissions.Add(Roles.Delete);
        permissions.Add(Roles.ManagePermissions);

        // Tenants permissions
        permissions.Add(Tenants.Default);
        permissions.Add(Tenants.Create);
        permissions.Add(Tenants.Read);
        permissions.Add(Tenants.Update);
        permissions.Add(Tenants.Delete);
        permissions.Add(Tenants.ManageFeatures);

        return permissions;
    }
} 