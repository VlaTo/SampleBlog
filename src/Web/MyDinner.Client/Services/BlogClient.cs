using System.Net.Http.Headers;
using System.Net.Mime;
using SampleBlog.Web.Shared.Models.Blog;

namespace SampleBlog.Web.Application.MyDinner.Client.Services;

internal sealed class BlogClient : IBlogClient
{
    private readonly HttpClient httpClient;

    public BlogClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task GetBlogAsync(string blogId, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{blogId}"));

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        using (var response = await httpClient.SendAsync(request, cancellationToken))
        {
            var message = response.EnsureSuccessStatusCode();

            using (var stream = await message.Content.ReadAsStreamAsync(cancellationToken))
            {
                var entry = await System.Text.Json.JsonSerializer.DeserializeAsync<BlogEntry>(stream, cancellationToken: cancellationToken);
            }
        }
    }
}