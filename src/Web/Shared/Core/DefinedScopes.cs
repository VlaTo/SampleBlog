namespace SampleBlog.Web.Shared.Core;

public static class DefinedScopes
{
    public static class Blog
    {
        private const string Prefix = "blog";

        public static class Api
        {
            private static readonly string Prefix = Concat(Blog.Prefix, "api");

            public static readonly string Blogs = Concat(Prefix, "blogs");
            public static readonly string Comments = Concat(Prefix, "comments");
        }
    }

    private static string Concat(string prefix, string tail)
    {
        return prefix + '.' + tail;
    }
}