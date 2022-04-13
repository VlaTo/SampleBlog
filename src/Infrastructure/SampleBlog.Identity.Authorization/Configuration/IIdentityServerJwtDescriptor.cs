namespace SampleBlog.Identity.Authorization.Configuration;

internal interface IIdentityServerJwtDescriptor
{
    IDictionary<string, ResourceDefinition> GetResourceDefinitions();
}