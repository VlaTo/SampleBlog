using System.Net.Http.Headers;
using Fluxor;
using HashidsNet;
using Microsoft.Extensions.Options;
using SampleBlog.Web.Client.Core.Configuration;
using SampleBlog.Web.Client.Store.Menu.Actions;
using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Store.Menu.Effects;

// ReSharper disable once UnusedMember.Global
public sealed class GetMenuActionEffect : Effect<GetMenuAction>
{
    private readonly HttpClient httpClient;
    private readonly ClientOptions options;

    public GetMenuActionEffect(HttpClient httpClient, IOptions<ClientOptions> options)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
    }

    public override async Task HandleAsync(GetMenuAction action, IDispatcher dispatcher)
    {
        var hash = GetHashedDate(action.DateTime);
        var message = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://localhost:5026/api/v1/Menu/{hash}"));

        try
        {
            using (var response = await httpClient.SendAsync(message, CancellationToken.None))
            {
                var result = response.EnsureSuccessStatusCode();

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    var menu = await System.Text.Json.JsonSerializer.DeserializeAsync<MenuEntry>(stream, cancellationToken: CancellationToken.None);
                    dispatcher.Dispatch(new MenuAcquiredAction(action.DateTime, menu?.Dishes));
                }
            }
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine(exception);
            dispatcher.Dispatch(new NoMenuAction(action.DateTime));
        }
    }

    private string GetHashedDate(DateTime dateTime)
    {
        var hash = new Hashids(options.HashIdOptions.Salt, options.HashIdOptions.MinHashLength);
        return hash.EncodeLong(dateTime.ToBinary());
    }
}