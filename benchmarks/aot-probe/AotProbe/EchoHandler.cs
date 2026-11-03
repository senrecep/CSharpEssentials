using System.Net;
using System.Net.Http.Json;

namespace AotProbe;

internal sealed class EchoHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ProbeDto body = request.Content is null
            ? new ProbeDto("get", 1)
            : (await request.Content.ReadFromJsonAsync(ProbeJsonContext.Default.ProbeDto, cancellationToken))! with { Name = "echo" };
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(body, ProbeJsonContext.Default.ProbeDto) };
    }
}
