using Microsoft.AspNetCore.Http;

namespace SampleBlog.IdentityServer.Hosting;

public class Endpoint
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    /// <value>
    /// The path.
    /// </value>
    public PathString Path
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the handler.
    /// </summary>
    /// <value>
    /// The handler.
    /// </value>
    public Type Handler
    {
        get;
        set;
    }

    public Endpoint(string name, string path, Type handlerType)
    {
        Name = name;
        Path = path;
        Handler = handlerType;
    }
}