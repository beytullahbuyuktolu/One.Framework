namespace SharedKernel.Authorization
{
    public class PermissionDefinition
    {
        public string Name { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string Group { get; }

        public PermissionDefinition(
            string name,
            string displayName = null,
            string description = null,
            string group = null)
        {
            Name = name;
            DisplayName = displayName ?? name;
            Description = description;
            Group = group;
        }
    }

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
    }
}
