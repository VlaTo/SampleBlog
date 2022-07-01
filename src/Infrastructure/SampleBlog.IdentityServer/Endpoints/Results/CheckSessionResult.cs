using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SampleBlog.IdentityServer.Core;
using SampleBlog.IdentityServer.DependencyInjection.Options;
using SampleBlog.IdentityServer.Extensions;
using SampleBlog.IdentityServer.Hosting;

namespace SampleBlog.IdentityServer.Endpoints.Results;

[EmbeddedResource("form", ResourceName = "SampleBlog.IdentityServer.Scripts.ChechSession.template.html")]
internal sealed class CheckSessionResult : EmbeddedResourceProvider, IEndpointResult
{
    private IdentityServerOptions options;
    private static volatile string FormattedHtml;
    private static readonly object Lock = new object();
    private static volatile string LastCheckSessionCookieName;

    public CheckSessionResult()
    {
    }

    internal CheckSessionResult(IdentityServerOptions options)
    {
        this.options = options;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        Init(context);

        AddCspHeaders(context);

        var html = await GetHtmlAsync(options.Authentication.CheckSessionCookieName);
        await context.Response.WriteHtmlAsync(html);
    }

    private void Init(HttpContext context)
    {
        options = options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
    }
    
    private void AddCspHeaders(HttpContext context)
    {
        context.Response.AddScriptCspHeaders(options.Csp, "sha256-fa5rxHhZ799izGRP38+h4ud5QXNT0SFaFlh4eqDumBI=");
    }

    private async Task<string> GetHtmlAsync(string cookieName)
    {
        if (cookieName != LastCheckSessionCookieName)
        {
            
            //WARNING: lock (Lock)
            //{
                if (cookieName != LastCheckSessionCookieName)
                {
                    using (var stream = GetResourceStream("form"))
                    {
                        var reader = new StreamReader(stream, Encoding.UTF8);
                        var content = await reader.ReadToEndAsync();
                        FormattedHtml = content.Replace("{cookieName}", cookieName);
                    }

                    //FormattedHtml = Html.Replace("{cookieName}", cookieName);
                    LastCheckSessionCookieName = cookieName;
                }
            //}
        }

        return FormattedHtml;
    }
}