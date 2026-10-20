using System.Net;
using CSharpEssentials.Http;
using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Http;

public class IdempotencyKeyTests
{
    [Fact]
    public void SetIdempotencyKey_Should_Generate_A_Valid_Key_When_None_Given()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");

        string key = request.SetIdempotencyKey();

        IdempotencyKeys.IsValid(key).Should().BeTrue();
        request.Headers.GetValues("Idempotency-Key").Should().Equal(key);
    }

    [Fact]
    public void SetIdempotencyKey_Should_Replace_An_Existing_Key()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");
        request.SetIdempotencyKey("first");

        request.SetIdempotencyKey("second");

        request.Headers.GetValues("Idempotency-Key").Should().Equal("second");
    }

    [Fact]
    public void SetIdempotencyKey_Should_Accept_A_Deterministic_Key()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");
        string expected = IdempotencyKeys.For("create-order", "42");

        string key = request.SetIdempotencyKey(expected);

        key.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("has space")]
    [InlineData("quote\"")]
    [InlineData("back\\slash")]
    [InlineData("ünicode")]
    public void SetIdempotencyKey_Should_Reject_Invalid_Keys(string key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");

        Action act = () => request.SetIdempotencyKey(key);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetIdempotencyKey_Should_Throw_For_Null_Request()
    {
        Action act = () => ((HttpRequestMessage)null!).SetIdempotencyKey();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Builder_Should_Use_The_Same_Key_For_Every_Build()
    {
        HttpRequestBuilder builder = HttpRequestBuilder.Post("https://api.test/orders").WithIdempotencyKey();

        Result<HttpRequestMessage> first = builder.Build();
        Result<HttpRequestMessage> second = builder.Build();

        using HttpRequestMessage firstRequest = first.Value;
        using HttpRequestMessage secondRequest = second.Value;
        firstRequest.Headers.GetValues("Idempotency-Key").Should().Equal(secondRequest.Headers.GetValues("Idempotency-Key"));
    }

    [Fact]
    public void Builder_Should_Generate_A_New_Key_Per_Builder()
    {
        using HttpRequestMessage first = HttpRequestBuilder.Post("https://api.test/orders").WithIdempotencyKey().Build().Value;
        using HttpRequestMessage second = HttpRequestBuilder.Post("https://api.test/orders").WithIdempotencyKey().Build().Value;

        first.Headers.GetValues("Idempotency-Key").Should().NotEqual(second.Headers.GetValues("Idempotency-Key"));
    }

    [Fact]
    public void Builder_Should_Use_The_Given_Key()
    {
        using HttpRequestMessage request = HttpRequestBuilder.Patch("https://api.test/orders/1").WithIdempotencyKey("patch-1").Build().Value;

        request.Headers.GetValues("Idempotency-Key").Should().Equal("patch-1");
    }

    [Fact]
    public void Builder_Should_Reject_An_Invalid_Key()
    {
        Action act = () => HttpRequestBuilder.Post("https://api.test/orders").WithIdempotencyKey("bad key");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Builder_Should_Not_Add_A_Key_By_Default()
    {
        using HttpRequestMessage request = HttpRequestBuilder.Post("https://api.test/orders").Build().Value;

        request.Headers.Contains("Idempotency-Key").Should().BeFalse();
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PATCH")]
    public async Task Handler_Should_Add_A_Key_To_Unsafe_Methods(string method)
    {
        using var recorder = new RecordingHandler();
        using var keyHandler = new IdempotencyKeyHandler(recorder);
        using var invoker = new HttpMessageInvoker(keyHandler, disposeHandler: false);
        using var request = new HttpRequestMessage(new HttpMethod(method), "https://api.test/orders");

        using HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        IdempotencyKeys.IsValid(recorder.Keys.Single()).Should().BeTrue();
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Handler_Should_Not_Add_A_Key_To_Other_Methods(string method)
    {
        using var recorder = new RecordingHandler();
        using var keyHandler = new IdempotencyKeyHandler(recorder);
        using var invoker = new HttpMessageInvoker(keyHandler, disposeHandler: false);
        using var request = new HttpRequestMessage(new HttpMethod(method), "https://api.test/orders");

        using HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        recorder.Keys.Should().ContainSingle().Which.Should().BeNull();
    }

    [Fact]
    public async Task Handler_Should_Keep_An_Existing_Key()
    {
        using var recorder = new RecordingHandler();
        using var keyHandler = new IdempotencyKeyHandler(recorder);
        using var invoker = new HttpMessageInvoker(keyHandler, disposeHandler: false);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");
        request.SetIdempotencyKey("caller-key");

        using HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        recorder.Keys.Should().Equal("caller-key");
    }

    [Fact]
    public async Task AddIdempotencyKeys_Should_Add_The_Handler_To_The_Client()
    {
        using var recorder = new RecordingHandler();
        var services = new ServiceCollection();
        services.AddHttpClient("orders")
            .AddIdempotencyKeys()
            .ConfigurePrimaryHttpMessageHandler(() => recorder);
        using ServiceProvider provider = services.BuildServiceProvider();
        HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("orders");

        using HttpResponseMessage response = await client.PostAsync(new Uri("https://api.test/orders"), null);

        IdempotencyKeys.IsValid(recorder.Keys.Single()).Should().BeTrue();
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string?> Keys { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Keys.Add(request.Headers.TryGetValues("Idempotency-Key", out IEnumerable<string>? values) ? values.Single() : null);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request });
        }
    }
}
