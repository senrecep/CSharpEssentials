using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// In-memory handler: drains every request body (so lazy and eager serializers do the same work), answers GET /users,
/// POST /users and GET /missing with a new response per call, and records the request when <see cref="Capture"/> is on.
/// </summary>
public sealed class StubHandler(byte[] usersBody) : HttpMessageHandler
{
    public bool Capture { get; set; }

    public string? LastMethod { get; private set; }

    public string? LastUri { get; private set; }

    public string? LastBody { get; private set; }

    public string? LastContentType { get; private set; }

    public string? LastAccept { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RecordHeaders(request);

        if (request.Content is not null)
        {
            if (Capture)
            {
                using var sink = new MemoryStream();
                await request.Content.CopyToAsync(sink, cancellationToken).ConfigureAwait(false);
                LastBody = Encoding.UTF8.GetString(sink.ToArray());
            }
            else
            {
                await request.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
            }
        }

        return Respond(request);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RecordHeaders(request);

        if (request.Content is not null)
        {
            if (Capture)
            {
                using var sink = new MemoryStream();
                request.Content.CopyTo(sink, null, cancellationToken);
                LastBody = Encoding.UTF8.GetString(sink.ToArray());
            }
            else
            {
                request.Content.CopyTo(Stream.Null, null, cancellationToken);
            }
        }

        return Respond(request);
    }

    private void RecordHeaders(HttpRequestMessage request)
    {
        if (Capture)
        {
            LastMethod = request.Method.Method;
            LastUri = request.RequestUri?.AbsoluteUri;
            LastAccept = string.Join(",", request.Headers.Accept);
            LastBody = null;
            LastContentType = request.Content?.Headers.ContentType?.ToString();
        }
    }

    private HttpResponseMessage Respond(HttpRequestMessage request)
    {
        string path = request.RequestUri!.AbsolutePath;
        (HttpStatusCode status, byte[] body, string mediaType) = (request.Method.Method, path) switch
        {
            ("GET", "/users") => (HttpStatusCode.OK, usersBody, "application/json"),
            ("POST", "/users") => (HttpStatusCode.Created, Inputs.SingleUserBody, "application/json"),
            ("GET", "/missing") => (HttpStatusCode.NotFound, Inputs.NotFoundBody, "application/problem+json"),
            _ => throw new InvalidOperationException($"No stub route for {request.Method} {path}."),
        };

        var content = new ByteArrayContent(body);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType) { CharSet = "utf-8" };
        return new HttpResponseMessage(status) { RequestMessage = request, Content = content };
    }
}
