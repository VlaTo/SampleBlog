using SampleBlog.Web.Shared.Models.Menu;

namespace SampleBlog.Web.Client.Core.Services;

internal sealed class HttpMenuClient : IMenuClient
{
    private readonly HttpClient httpClient;
    private readonly IHashProvider hashProvider;

    public HttpMenuClient(HttpClient httpClient, IHashProvider hashProvider)
    {
        this.httpClient = httpClient;
        this.hashProvider = hashProvider;
    }

    public async Task<Menu?> FetchOriginalMenuAsync(DateTime dateTime)
    {
        var hash = hashProvider.GetHash(dateTime);
        var message = new HttpRequestMessage(HttpMethod.Get, new Uri($"http://localhost:5026/api/v1/Menu/{hash}"));

        try
        {
            using (var response = await httpClient.SendAsync(message, CancellationToken.None))
            {
                var result = response.EnsureSuccessStatusCode();

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    var menu = await System.Text.Json.JsonSerializer.DeserializeAsync<Menu>(stream, cancellationToken: CancellationToken.None);

                    if (null != menu)
                    {
                        return menu;
                    }
                }
            }
        }
        catch (HttpRequestException exception)
        {
            Console.WriteLine(exception);
            throw;
        }

        return null;
    }
}