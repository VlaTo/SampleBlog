using System.Reflection;

namespace SampleBlog.IdentityServer.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class EmbeddedResourceAttribute : Attribute
{
    public string ResourceKey
    {
        get;
    }

    public string? ResourceName
    {
        get;
        set;
    }

    public EmbeddedResourceAttribute(string resourceKey)
    {
        ResourceKey = resourceKey;
    }
}

internal class EmbeddedResourceProvider
{
    protected Stream? GetResourceStream(string key)
    {
        var currentType = GetType();
        var attributes = currentType.GetCustomAttributes<EmbeddedResourceAttribute>();

        foreach (var attribute in attributes)
        {
            if (false == String.Equals(key, attribute.ResourceKey))
            {
                continue;
            }

            var resourceName = attribute.ResourceName ?? attribute.ResourceKey;
            var resourceManifest = currentType.Assembly.GetManifestResourceInfo(resourceName);

            if (null != resourceManifest)
            {
                if (ResourceLocation.Embedded == resourceManifest.ResourceLocation)
                {
                    ;
                }

                var assembly = resourceManifest.ReferencedAssembly ?? currentType.Assembly;
                var stream = assembly.GetManifestResourceStream(resourceManifest.FileName ?? resourceName);

                return stream;
            }

            break;
        }

        return null;
    }
}