using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Hosting;

namespace SampleBlog.IdentityServer.Extensions;

public static class EndpointOptionsExtensions
{
    public static bool IsEndpointEnabled(this EndpointsOptions options, Endpoint endpoint)
    {
        return endpoint?.Name switch
        {
            Constants.EndpointNames.Authorize => options.EnableAuthorizeEndpoint,
            Constants.EndpointNames.CheckSession => options.EnableCheckSessionEndpoint,
            Constants.EndpointNames.DeviceAuthorization => options.EnableDeviceAuthorizationEndpoint,
            Constants.EndpointNames.Discovery => options.EnableDiscoveryEndpoint,
            Constants.EndpointNames.EndSession => options.EnableEndSessionEndpoint,
            Constants.EndpointNames.Introspection => options.EnableIntrospectionEndpoint,
            Constants.EndpointNames.Revocation => options.EnableTokenRevocationEndpoint,
            Constants.EndpointNames.Token => options.EnableTokenEndpoint,
            Constants.EndpointNames.UserInfo => options.EnableUserInfoEndpoint,
            _ => true
        };
    }
}