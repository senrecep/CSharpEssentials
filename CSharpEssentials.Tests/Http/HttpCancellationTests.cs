using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpCancellationTests
{
    private static readonly Uri RequestUri = new("https://test.com/resource");

    public static TheoryData<string> ClientMethods =>
    [
        "GetFromJson",
        "PostAsJson",
        "Post",
        "PutAsJson",
        "Put",
        "PatchAsJson",
        "Delete",
        "Send",
        "SendT",
        "SendWithRedirects",
        "SendWithRedirectsT",
    ];

    public static TheoryData<string> ContentMethods =>
    [
        "ReadAsString",
        "ReadFromJson",
    ];

    [Theory]
    [MemberData(nameof(ClientMethods))]
    public async Task ClientMethod_Should_Throw_OperationCanceled_With_Caller_Token_When_Caller_Cancels(string method)
    {
        using var cts = new CancellationTokenSource();
        using var handler = new BlockingHandler(cts.Cancel);
        using var client = new HttpClient(handler);

        Func<Task> act = async () => await InvokeClient(method, client, cts.Token);

        (await act.Should().ThrowAsync<OperationCanceledException>())
            .Which.CancellationToken.Should().Be(cts.Token);
    }

    [Theory]
    [MemberData(nameof(ClientMethods))]
    public async Task ClientMethod_Should_Return_Timeout_Failure_When_HttpClient_Timeout_Elapses(string method)
    {
        using var cts = new CancellationTokenSource();
        using var handler = new BlockingHandler(onEnter: null);
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(50) };

        IResultBase result = await InvokeClient(method, client, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
        result.Errors[0].Code.Should().Be("Http.Timeout");
    }

    [Theory]
    [MemberData(nameof(ClientMethods))]
    public async Task ClientMethod_Should_Return_Failure_When_Non_Caller_Token_Is_Cancelled(string method)
    {
        using var cts = new CancellationTokenSource();
        using var other = new CancellationTokenSource();
        await other.CancelAsync();
        using var handler = new ThrowingHandler(new OperationCanceledException(other.Token));
        using var client = new HttpClient(handler);

        IResultBase result = await InvokeClient(method, client, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
        result.Errors[0].Code.Should().Be(nameof(OperationCanceledException));
    }

    [Theory]
    [MemberData(nameof(ClientMethods))]
    public async Task ClientMethod_Should_Return_Failure_When_Transport_Throws(string method)
    {
        using var cts = new CancellationTokenSource();
        using var handler = new ThrowingHandler(new HttpRequestException("connection refused"));
        using var client = new HttpClient(handler);

        IResultBase result = await InvokeClient(method, client, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
        result.Errors[0].Code.Should().Be(nameof(HttpRequestException));
    }

    [Theory]
    [MemberData(nameof(ContentMethods))]
    public async Task ContentMethod_Should_Throw_OperationCanceled_With_Caller_Token_When_Caller_Cancels(string method)
    {
        using var cts = new CancellationTokenSource();
        using var content = new BlockingContent(cts.Cancel);

        Func<Task> act = async () => await InvokeContent(method, content, cts.Token);

        (await act.Should().ThrowAsync<OperationCanceledException>())
            .Which.CancellationToken.Should().Be(cts.Token);
    }

    [Theory]
    [MemberData(nameof(ContentMethods))]
    public async Task ContentMethod_Should_Return_Failure_When_Non_Caller_Token_Is_Cancelled(string method)
    {
        using var cts = new CancellationTokenSource();
        using var other = new CancellationTokenSource();
        await other.CancelAsync();
        using var content = new ThrowingContent(new OperationCanceledException(other.Token));

        IResultBase result = await InvokeContent(method, content, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
        result.Errors[0].Code.Should().Be(nameof(OperationCanceledException));
    }

    [Theory]
    [MemberData(nameof(ContentMethods))]
    public async Task ContentMethod_Should_Return_Failure_When_Read_Throws(string method)
    {
        using var cts = new CancellationTokenSource();
        using var content = new ThrowingContent(new InvalidOperationException("read failed"));

        IResultBase result = await InvokeContent(method, content, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
        result.Errors[0].Code.Should().Be(nameof(InvalidOperationException));
    }

    private static async Task<IResultBase> InvokeClient(string method, HttpClient client, CancellationToken cancellationToken)
    {
        var dto = new TestDto(1, "Test");
        using var content = new StringContent("body");
        using var request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
        return method switch
        {
            "GetFromJson" => await client.GetFromJsonAsResultAsync<TestDto>(RequestUri, cancellationToken: cancellationToken),
            "PostAsJson" => await client.PostAsJsonAsResultAsync<TestDto>(RequestUri, dto, cancellationToken: cancellationToken),
            "Post" => await client.PostAsResultAsync(RequestUri, content, cancellationToken),
            "PutAsJson" => await client.PutAsJsonAsResultAsync<TestDto>(RequestUri, dto, cancellationToken: cancellationToken),
            "Put" => await client.PutAsResultAsync(RequestUri, content, cancellationToken),
            "PatchAsJson" => await client.PatchAsJsonAsResultAsync<TestDto>(RequestUri, dto, cancellationToken: cancellationToken),
            "Delete" => await client.DeleteAsResultAsync(RequestUri, cancellationToken),
            "Send" => await client.SendAsResultAsync(request, cancellationToken),
            "SendT" => await client.SendAsResultAsync<TestDto>(request, cancellationToken: cancellationToken),
            "SendWithRedirects" => await client.SendWithRedirectsAsResultAsync(request, cancellationToken: cancellationToken),
            "SendWithRedirectsT" => await client.SendWithRedirectsAsResultAsync<TestDto>(request, cancellationToken: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
        };
    }

    private static async Task<IResultBase> InvokeContent(string method, HttpContent content, CancellationToken cancellationToken) =>
        method switch
        {
            "ReadAsString" => await content.ReadAsStringAsResultAsync(cancellationToken),
            "ReadFromJson" => await content.ReadFromJsonAsResultAsync<TestDto>(cancellationToken: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
        };

    private sealed record TestDto(int Id, string Name);

    private sealed class BlockingHandler(Action? onEnter) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onEnter?.Invoke();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromException<HttpResponseMessage>(exception);
    }

    private sealed class BlockingContent(Action onEnter) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        {
            onEnter();
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class ThrowingContent(Exception exception) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => Task.FromException(exception);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
            => Task.FromException(exception);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
