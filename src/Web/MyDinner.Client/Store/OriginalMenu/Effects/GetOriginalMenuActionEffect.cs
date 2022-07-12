using Fluxor;
using HashidsNet;
using Microsoft.Extensions.Options;
using SampleBlog.Web.Client.Core.Configuration;
using SampleBlog.Web.Client.Store.OriginalMenu.Actions;

namespace SampleBlog.Web.Client.Store.OriginalMenu.Effects;

// ReSharper disable once UnusedMember.Global
public sealed class GetOriginalMenuActionEffect : Effect<GetOriginalMenuAction>
{
    private readonly HttpClient httpClient;
    private readonly ClientOptions options;

    public GetOriginalMenuActionEffect(HttpClient httpClient, IOptions<ClientOptions> options)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
    }

    public override async Task HandleAsync(GetOriginalMenuAction action, IDispatcher dispatcher)
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
                    try
                    {
                        var menu = await System.Text.Json.JsonSerializer.DeserializeAsync<Web.Shared.Models.Menu.Menu>(stream, cancellationToken: CancellationToken.None);

                        if (null != menu)
                        {
                            dispatcher.Dispatch(new OriginalMenuAcquiredAction(menu.Date, menu.Dishes, menu.IsOpen));
                            return;
                        }

                        dispatcher.Dispatch(new NoOriginalMenuAction(action.DateTime));
                    }
                    catch (Exception exception)
                    {
                        dispatcher.Dispatch(new NoOriginalMenuAction(action.DateTime));
                    }
                }
            }
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine(exception);
            dispatcher.Dispatch(new NoOriginalMenuAction(action.DateTime));
        }
    }

    private string GetHashedDate(DateTime dateTime)
    {
        var hash = new Hashids(options.HashIdOptions.Salt, options.HashIdOptions.MinHashLength);
        return hash.EncodeLong(dateTime.ToBinary());
    }
}