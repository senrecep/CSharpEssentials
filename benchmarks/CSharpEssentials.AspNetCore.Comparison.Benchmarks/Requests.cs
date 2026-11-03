using System.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>Builds the request messages every variant receives; a message is sent once, so each call builds a new one.</summary>
internal static class Requests
{
    public static HttpRequestMessage Get(string path, string? ifNoneMatch = null)
    {
        HttpRequestMessage request = new(HttpMethod.Get, new Uri(path, UriKind.Relative));
        if (ifNoneMatch is not null)
            request.Headers.TryAddWithoutValidation("If-None-Match", ifNoneMatch);
        return request;
    }

    public static HttpRequestMessage PostJson(string path, byte[] body, string? idempotencyKey = null)
    {
        ByteArrayContent content = new(body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        HttpRequestMessage request = new(HttpMethod.Post, new Uri(path, UriKind.Relative)) { Content = content };
        if (idempotencyKey is not null)
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        return request;
    }
}
