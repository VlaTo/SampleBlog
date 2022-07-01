namespace SampleBlog.IdentityServer.Models;

public enum BearerTokenUsageType
{
    AuthorizationHeader = 0,
    PostBody = 1,
    QueryString = 2
}