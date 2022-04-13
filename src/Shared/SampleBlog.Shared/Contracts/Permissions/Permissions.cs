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
}