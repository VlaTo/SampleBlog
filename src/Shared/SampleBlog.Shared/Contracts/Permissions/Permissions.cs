using System.Reflection;

namespace SampleBlog.Shared.Contracts.Permissions;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
    }

    public static class Blog
    {
        public const string View = "Permissions.Blog.View";
        public const string Create = "Permissions.Blog.Create";
        public const string Edit = "Permissions.Blog.Edit";
    }

    public static List<string> GetRegisteredPermissions()
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        var permissions = new List<string>();
        var selectMany = typeof(Permissions)
            .GetNestedTypes()
            .SelectMany(nestedType => nestedType.GetFields(bindingFlags));

        foreach (var prop in selectMany)
        {
            var propertyValue = prop.GetValue(null);

            if (null != propertyValue)
            {
                permissions.Add(propertyValue.ToString());
            }
        }

        return permissions;
    }
}