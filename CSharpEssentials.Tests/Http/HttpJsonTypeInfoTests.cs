using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpJsonTypeInfoTests
{
    private static readonly Uri Items = new("https://test.com/items");

    [Fact]
    public async Task GetFromJsonAsResultAsync_WithTypeInfo_Should_Return_Value()
    {
        using var handler = new EchoHandler();
        using var client = new HttpClient(handler);

        Result<TypeInfoDto> result = await client.GetFromJsonAsResultAsync(Items, TypeInfoJsonContext.Default.TypeInfoDto);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new TypeInfoDto("get", 1));
    }

    [Fact]
    public async Task PostPutPatch_WithTypeInfo_Should_Send_And_Read_Body()
    {
        using var handler = new EchoHandler();
        using var client = new HttpClient(handler);
        var sent = new TypeInfoDto("sent", 3);
        var info = TypeInfoJsonContext.Default.TypeInfoDto;

        Result<TypeInfoDto> posted = await client.PostAsJsonAsResultAsync(Items, sent, info, info);
        Result<TypeInfoDto> put = await client.PutAsJsonAsResultAsync(Items, sent, info, info);
        Result<TypeInfoDto> patched = await client.PatchAsJsonAsResultAsync(Items, sent, info, info);

        posted.Value.Should().Be(new TypeInfoDto("echo POST", 3));
        put.Value.Should().Be(new TypeInfoDto("echo PUT", 3));
        patched.Value.Should().Be(new TypeInfoDto("echo PATCH", 3));
    }

    [Fact]
    public async Task SendAsResultAsync_WithTypeInfo_Should_Return_Value()
    {
        using var handler = new EchoHandler();
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, Items);

        Result<TypeInfoDto> result = await client.SendAsResultAsync(request, TypeInfoJsonContext.Default.TypeInfoDto);

        result.Value.Should().Be(new TypeInfoDto("get", 1));
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithTypeInfo_Should_Follow_Redirect()
    {
        using var handler = new EchoHandler();
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));

        Result<TypeInfoDto> result = await client.SendWithRedirectsAsResultAsync(request, TypeInfoJsonContext.Default.TypeInfoDto);

        result.Value.Should().Be(new TypeInfoDto("get", 1));
    }

    [Fact]
    public async Task ReadFromJsonAsResultAsync_WithTypeInfo_Should_Return_Value()
    {
        using HttpContent content = JsonContent.Create(new TypeInfoDto("body", 2), TypeInfoJsonContext.Default.TypeInfoDto);

        Result<TypeInfoDto> result = await content.ReadFromJsonAsResultAsync(TypeInfoJsonContext.Default.TypeInfoDto);

        result.Value.Should().Be(new TypeInfoDto("body", 2));
    }

    [Fact]
    public async Task Builder_WithJsonTypeInfoContent_Should_Send_Body_And_Follow_Redirects()
    {
        using var handler = new EchoHandler();
        using var client = new HttpClient(handler);
        var info = TypeInfoJsonContext.Default.TypeInfoDto;

        Result<TypeInfoDto> direct = await HttpRequestBuilder.Post(Items)
            .WithJsonTypeInfoContent(new TypeInfoDto("sent", 4), info)
            .AsResultAsync(client, info);
        Result<TypeInfoDto> redirected = await HttpRequestBuilder.Get(new Uri("https://test.com/redirect"))
            .FollowRedirects()
            .AsResultAsync(client, info);

        direct.Value.Should().Be(new TypeInfoDto("echo POST", 4));
        redirected.Value.Should().Be(new TypeInfoDto("get", 1));
    }

    [Fact]
    public void Builder_WithJsonTypeInfoContent_WithNullTypeInfo_Should_Throw()
    {
        Action act = () => HttpRequestBuilder.Post(Items).WithJsonTypeInfoContent(new TypeInfoDto("x", 1), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("typeInfo");
    }

    [Fact]
    public async Task Builder_WithJsonContent_WithNullOptions_Should_Use_Default_Options()
    {
        Result<HttpRequestMessage> built = HttpRequestBuilder.Post(Items)
            .WithJsonContent(new TypeInfoDto("sent", 5), null)
            .Build();

        built.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = built.Value;
        string body = await request.Content!.ReadAsStringAsync();
        body.Should().Contain("sent").And.Contain("5");
    }

    private sealed class EchoHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath == "/redirect")
            {
                var redirect = new HttpResponseMessage(HttpStatusCode.Found);
                redirect.Headers.Location = new Uri("/final", UriKind.Relative);
                return redirect;
            }

            TypeInfoDto body = request.Content is null
                ? new TypeInfoDto("get", 1)
                : (await request.Content.ReadFromJsonAsync(TypeInfoJsonContext.Default.TypeInfoDto, cancellationToken))! with { Name = $"echo {request.Method}" };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(body, TypeInfoJsonContext.Default.TypeInfoDto) };
        }
    }
}

public sealed record TypeInfoDto(string Name, int Count);

[JsonSerializable(typeof(TypeInfoDto))]
internal sealed partial class TypeInfoJsonContext : JsonSerializerContext;
