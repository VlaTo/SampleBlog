using HashidsNet;
using Microsoft.Extensions.Options;
using SampleBlog.Web.Client.Core.Configuration;

namespace SampleBlog.Web.Client.Core.Services;

internal sealed class HashidsHashProvider : IHashProvider
{
    private readonly ClientOptions options;

    public HashidsHashProvider(IOptions<ClientOptions> options)
    {
        this.options = options.Value;
    }

    public string GetHash(DateTime value)
    {
        var hash = new Hashids(options.HashIdOptions.Salt, options.HashIdOptions.MinHashLength);
        return hash.EncodeLong(value.ToBinary());
    }
}