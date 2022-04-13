using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace SampleBlog.Web.Server.Extensions;

internal static class HttpResponseExtensions
{
    public static Task WriteJsonAsync<T>(this HttpResponse response, HttpStatusCode statusCode, T obj)
    {
        response.StatusCode = (int)statusCode;
        response.ContentType = MediaTypeNames.Application.Json;

        return JsonSerializer.SerializeAsync(response.Body, obj);
    }
}