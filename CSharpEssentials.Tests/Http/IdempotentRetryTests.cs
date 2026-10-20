using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CSharpEssentials.Http;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.Tests.Http;

public class IdempotentRetryTests
{
    private const string Key = "order-42";

    [Fact]
    public async Task Retries_Should_Reuse_The_Same_Key()
    {
        using var handler = new ScriptedHandler(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            _ => new HttpResponseMessage(HttpStatusCode.GatewayTimeout),
            _ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        handler.Keys.Should().Equal(Key, Key, Key);
    }

    [Fact]
    public async Task Generated_Key_Should_Be_Reused_Across_Retries()
    {
        using var handler = new ScriptedHandler(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            _ => new HttpResponseMessage(HttpStatusCode.OK));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");
        string key = request.SetIdempotencyKey();

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().Equal(key, key);
    }

    [Fact]
    public async Task Post_Without_Key_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");

        using HttpResponseMessage response = await RunAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Replayed_Response_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => Replayed(HttpStatusCode.ServiceUnavailable));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Replayed_Server_Error_Of_Safe_Method_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => Replayed(HttpStatusCode.InternalServerError));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/orders/1");

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task InFlight_Conflict_Should_Be_Retried_After_Its_Capped_Retry_After()
    {
        List<TimeSpan> delays = [];
        using var handler = new ScriptedHandler(
            _ => InFlight(r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(2))),
            _ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);
        var options = new IdempotentRetryOptions { MaxRetryAfter = TimeSpan.FromMilliseconds(20) };

        using HttpResponseMessage response = await RunAsync(handler, request, Pipeline(delays, options));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        delays.Should().Equal(TimeSpan.FromMilliseconds(20));
        handler.Keys.Should().Equal(Key, Key);
    }

    [Fact]
    public async Task DelayGenerator_Should_Use_Retry_After_Seconds()
    {
        using HttpResponseMessage response = InFlight(r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(2)));

        TimeSpan? delay = await GenerateDelayAsync(response);

        delay.Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task DelayGenerator_Should_Use_Retry_After_Date()
    {
        DateTimeOffset now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        using HttpResponseMessage response = InFlight(r =>
        {
            r.Headers.Date = now;
            r.Headers.RetryAfter = new RetryConditionHeaderValue(now.AddSeconds(5));
        });

        TimeSpan? delay = await GenerateDelayAsync(response);

        delay.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DelayGenerator_Should_Cap_Retry_After()
    {
        using HttpResponseMessage response = InFlight(r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromHours(1)));

        TimeSpan? delay = await GenerateDelayAsync(response, new IdempotentRetryOptions { MaxRetryAfter = TimeSpan.FromSeconds(3) });

        delay.Should().Be(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task DelayGenerator_Should_Defer_To_The_Previous_Generator_For_Other_Statuses()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(9));

        TimeSpan? delay = await GenerateDelayAsync(response, previous: TimeSpan.FromMilliseconds(7));

        delay.Should().Be(TimeSpan.FromMilliseconds(7));
    }

    [Fact]
    public async Task DelayGenerator_Should_Return_Null_Without_Previous_Generator_Or_Retry_After()
    {
        using HttpResponseMessage response = InFlight();

        TimeSpan? delay = await GenerateDelayAsync(response);

        delay.Should().BeNull();
    }

    [Fact]
    public async Task InFlight_Conflict_Should_Keep_Its_Body_Readable()
    {
        using var handler = new ScriptedHandler(_ => InFlight());
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(3);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await response.Content.ReadAsStringAsync()).Should().Contain(IdempotencyErrorCodes.InFlight);
    }

    [Fact]
    public async Task InFlight_Conflict_Without_Key_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => InFlight());
        using var request = new HttpRequestMessage(HttpMethod.Put, "https://api.test/orders/1");

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task Keyed_Post_Should_Be_Retried_On_Transient_Status(HttpStatusCode status)
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(status), _ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.Keys.Should().Equal(Key, Key);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task Keyed_Post_Should_Not_Be_Retried_On_Other_Server_Errors(HttpStatusCode status)
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(status));
        using HttpRequestMessage request = Keyed(HttpMethod.Patch);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Safe_Method_Should_Be_Retried_On_Server_Errors_Without_Key(HttpStatusCode status)
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(status), _ => new HttpResponseMessage(HttpStatusCode.OK));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.test/orders/1");

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().Equal(null, null);
    }

    [Theory]
    [InlineData(HttpStatusCode.UnprocessableEntity, IdempotencyErrorCodes.KeyReused)]
    [InlineData(HttpStatusCode.BadRequest, IdempotencyErrorCodes.KeyInvalid)]
    [InlineData(HttpStatusCode.BadRequest, IdempotencyErrorCodes.KeyMissing)]
    [InlineData(HttpStatusCode.BadRequest, IdempotencyErrorCodes.ScopeRequired)]
    public async Task Key_Errors_Should_Not_Be_Retried(HttpStatusCode status, string code)
    {
        using var handler = new ScriptedHandler(_ => Problem(status, code));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("Orders.Duplicate")]
    [InlineData(null)]
    public async Task Conflict_Without_InFlight_Code_Should_Not_Be_Retried(string? code)
    {
        using var handler = new ScriptedHandler(_ => code is null
            ? new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent("{}", Encoding.UTF8, "application/problem+json") }
            : Problem(HttpStatusCode.Conflict, code));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Conflict_With_Plain_Json_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent("""{"code":"Idempotency.InFlight"}""", Encoding.UTF8, "application/json"),
        });
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Unreadable_Conflict_Body_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => WithStream(HttpStatusCode.Conflict, new FailingStream()));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Unreadable_Success_Body_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => WithStream(HttpStatusCode.OK, new FailingStream()));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Conflict_Body_Read_Timeout_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => WithStream(HttpStatusCode.Conflict, new BlockingStream()));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);
        var options = new IdempotentRetryOptions { ProblemReadTimeout = TimeSpan.FromMilliseconds(50) };

        using HttpResponseMessage response = await RunAsync(handler, request, Pipeline([], options));

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Retryable_Status_With_Stuck_Body_Should_Be_Retried()
    {
        using var handler = new ScriptedHandler(
            _ => WithStream(HttpStatusCode.ServiceUnavailable, new BlockingStream()),
            _ => new HttpResponseMessage(HttpStatusCode.OK));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.Keys.Should().Equal(Key, Key);
    }

    [Fact]
    public async Task Oversized_Conflict_Body_Should_Not_Be_Retried_And_Stay_Readable()
    {
        string body = """{"code":"Idempotency.InFlight","detail":""" + "\"" + new string('x', 5000) + "\"}";
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        using var handler = new ScriptedHandler(_ => WithStream(HttpStatusCode.Conflict, new NonSeekableStream(bytes)));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
        (await response.Content.ReadAsStringAsync()).Should().Be(body);
    }

    [Fact]
    public async Task Conflict_With_Declared_Oversized_Length_Should_Not_Be_Read()
    {
        using var handler = new ScriptedHandler(_ =>
        {
            HttpResponseMessage response = InFlight();
            response.Content.Headers.ContentLength = 5000;
            return response;
        });
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Empty_Success_Json_Body_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"),
        });
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task No_Response_Should_Be_Retried_With_The_Same_Key()
    {
        using var handler = new ScriptedHandler(
            _ => throw new HttpRequestException("connection reset"),
            _ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        handler.Keys.Should().Equal(Key, Key);
    }

    [Fact]
    public async Task Timeout_Before_Headers_Should_Be_Retried()
    {
        using var handler = new ScriptedHandler(
            _ => throw new TaskCanceledException("timed out", new TimeoutException()),
            _ => new HttpResponseMessage(HttpStatusCode.Created));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        using HttpResponseMessage response = await RunAsync(handler, request);

        handler.Keys.Should().Equal(Key, Key);
    }

    [Fact]
    public async Task No_Response_Without_Key_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => throw new HttpRequestException("connection reset"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");

        Func<Task> act = async () =>
        {
            using HttpResponseMessage response = await RunAsync(handler, request);
        };

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Ssrf_Block_Should_Not_Be_Retried()
    {
        using var handler = new ScriptedHandler(_ => throw new SsrfBlockedException("blocked"));
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        Func<Task> act = async () =>
        {
            using HttpResponseMessage response = await RunAsync(handler, request);
        };

        await act.Should().ThrowAsync<SsrfBlockedException>();
        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task Caller_Cancellation_Should_Not_Be_Retried()
    {
        using var cancellation = new CancellationTokenSource();
        using var handler = new ScriptedHandler(_ =>
        {
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        });
        using HttpRequestMessage request = Keyed(HttpMethod.Post);

        Func<Task> act = async () =>
        {
            using HttpResponseMessage response = await RunAsync(handler, request, cancellationToken: cancellation.Token);
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
        handler.Keys.Should().HaveCount(1);
    }

    [Fact]
    public async Task ShouldRetryAsync_Should_Return_False_When_Caller_Cancelled()
    {
        using var request = Keyed(HttpMethod.Post);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        bool retry = await IdempotentRetry.ShouldRetryAsync(
            request, null, new OperationCanceledException(cancellation.Token), cancellationToken: cancellation.Token);

        retry.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldRetryAsync_Should_Return_True_For_Timeout_Without_Caller_Cancellation()
    {
        using var request = Keyed(HttpMethod.Post);

        bool retry = await IdempotentRetry.ShouldRetryAsync(request, null, new TaskCanceledException("timed out"));

        retry.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldRetryAsync_Should_Return_False_For_Unknown_Exception()
    {
        using var request = Keyed(HttpMethod.Post);

        bool retry = await IdempotentRetry.ShouldRetryAsync(request, null, new InvalidOperationException());

        retry.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldRetryAsync_Should_Throw_For_Null_Request()
    {
        Func<Task> act = async () => await IdempotentRetry.ShouldRetryAsync(null!, null, null);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GetRetryAfter_Should_Read_Seconds()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7));

        IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(30)).Should().Be(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void GetRetryAfter_Should_Cap_Seconds()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(120));

        IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(30)).Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void GetRetryAfter_Should_Measure_Date_From_Date_Header()
    {
        DateTimeOffset now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);
        response.Headers.Date = now;
        response.Headers.RetryAfter = new RetryConditionHeaderValue(now.AddSeconds(4));

        IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(30)).Should().Be(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public void GetRetryAfter_Should_Cap_Date()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);
        response.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddHours(1));

        IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(30)).Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void GetRetryAfter_Should_Return_Zero_For_Past_Date()
    {
        DateTimeOffset now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);
        response.Headers.Date = now;
        response.Headers.RetryAfter = new RetryConditionHeaderValue(now.AddSeconds(-10));

        IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(30)).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetRetryAfter_Should_Return_Null_Without_Header()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);

        IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(30)).Should().BeNull();
    }

    [Fact]
    public void GetRetryAfter_Should_Throw_For_Negative_Cap()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Conflict);

        Action act = () => IdempotentRetry.GetRetryAfter(response, TimeSpan.FromSeconds(-1));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Options_Should_Reject_Invalid_Values()
    {
        var options = new IdempotentRetryOptions();

        options.MaxRetryAfter.Should().Be(TimeSpan.FromSeconds(30));
        options.ProblemReadTimeout.Should().Be(TimeSpan.FromSeconds(2));
        ((Action)(() => options.MaxRetryAfter = TimeSpan.FromSeconds(-1))).Should().Throw<ArgumentOutOfRangeException>();
        ((Action)(() => options.ProblemReadTimeout = TimeSpan.Zero)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Wire_Constants_Should_Match_The_Server()
    {
        IdempotencyHttpProtocol.KeyHeaderName.Should().Be("Idempotency-Key");
        IdempotencyHttpProtocol.ReplayedHeaderName.Should().Be("Idempotency-Replayed");
        IdempotencyHttpProtocol.InFlightCode.Should().Be(IdempotencyErrorCodes.InFlight);
    }

    [Theory]
    [InlineData("replayed", 1)]
    [InlineData("in-flight", 3)]
    [InlineData("business-conflict", 1)]
    [InlineData("unavailable", 2)]
    [InlineData("network", 2)]
    public async Task Handler_Key_Should_Survive_Retries_Like_A_Manual_Key(string outcome, int attempts)
    {
        Func<HttpRequestMessage, HttpResponseMessage> first = outcome switch
        {
            "replayed" => _ => Replayed(HttpStatusCode.ServiceUnavailable),
            "in-flight" => _ => InFlight(),
            "business-conflict" => _ => Problem(HttpStatusCode.Conflict, "Orders.Duplicate"),
            "unavailable" => _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            _ => _ => throw new HttpRequestException("connection reset"),
        };
        Func<HttpRequestMessage, HttpResponseMessage> next = outcome == "in-flight" ? first : _ => new HttpResponseMessage(HttpStatusCode.OK);
        using var scripted = new ScriptedHandler(first, next);
        using var resilience = new PipelineHandler(Pipeline([]), scripted);
        using var keyHandler = new IdempotencyKeyHandler(resilience);
        using var invoker = new HttpMessageInvoker(keyHandler, disposeHandler: false);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.test/orders");

        using HttpResponseMessage response = await invoker.SendAsync(request, CancellationToken.None);

        scripted.Keys.Should().HaveCount(attempts);
        scripted.Keys.Distinct().Should().ContainSingle().Which.Should().NotBeNullOrEmpty();
    }

    private static HttpRequestMessage Keyed(HttpMethod method)
    {
        var request = new HttpRequestMessage(method, "https://api.test/orders");
        request.SetIdempotencyKey(Key);
        return request;
    }

    private static HttpResponseMessage Replayed(HttpStatusCode status)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.TryAddWithoutValidation("Idempotency-Replayed", "true");
        return response;
    }

    private static HttpResponseMessage Problem(HttpStatusCode status, string code) =>
        new(status)
        {
            Content = new StringContent(
                "{\"status\":" + ((int)status).ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\"code\":\"" + code + "\"}",
                Encoding.UTF8,
                "application/problem+json"),
        };

    private static HttpResponseMessage InFlight(Action<HttpResponseMessage>? configure = null)
    {
        HttpResponseMessage response = Problem(HttpStatusCode.Conflict, IdempotencyErrorCodes.InFlight);
        configure?.Invoke(response);
        return response;
    }

    private static HttpResponseMessage WithStream(HttpStatusCode status, Stream stream)
    {
        var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/problem+json");
        return new HttpResponseMessage(status) { Content = content };
    }

    private static async Task<TimeSpan?> GenerateDelayAsync(
        HttpResponseMessage response,
        IdempotentRetryOptions? idempotency = null,
        TimeSpan? previous = null)
    {
        var retry = new RetryStrategyOptions<HttpResponseMessage>();
        if (previous is not null)
            retry.DelayGenerator = _ => new ValueTask<TimeSpan?>(previous);
        retry.RetryUnsafeMethodsOnlyWithIdempotencyKey(idempotency);
        ResilienceContext context = ResilienceContextPool.Shared.Get();
        try
        {
            return await retry.DelayGenerator!(
                new RetryDelayGeneratorArguments<HttpResponseMessage>(context, Outcome.FromResult(response), 0));
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private static ResiliencePipeline<HttpResponseMessage> Pipeline(
        List<TimeSpan> delays,
        IdempotentRetryOptions? idempotency = null)
    {
        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();
        var retry = new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 2,
            Delay = TimeSpan.Zero,
            BackoffType = DelayBackoffType.Constant,
            OnRetry = args =>
            {
                delays.Add(args.RetryDelay);
                return default;
            },
        };

        return builder.AddRetry(retry.RetryUnsafeMethodsOnlyWithIdempotencyKey(idempotency)).Build();
    }

    private static async Task<HttpResponseMessage> RunAsync(
        ScriptedHandler handler,
        HttpRequestMessage request,
        ResiliencePipeline<HttpResponseMessage>? pipeline = null,
        CancellationToken cancellationToken = default)
    {
        using var invoker = new HttpMessageInvoker(handler, disposeHandler: false);
        ResiliencePipeline<HttpResponseMessage> resilience = pipeline ?? Pipeline([]);
        ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(IdempotentRetry.RequestMessageKey, request);
        try
        {
            return await resilience.ExecuteAsync(
                async ctx => await invoker.SendAsync(request, ctx.CancellationToken).ConfigureAwait(false),
                context).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }

    private sealed class ScriptedHandler(params Func<HttpRequestMessage, HttpResponseMessage>[] steps) : HttpMessageHandler
    {
        public List<string?> Keys { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Keys.Add(request.Headers.TryGetValues("Idempotency-Key", out IEnumerable<string>? values) ? values.Single() : null);
            HttpResponseMessage response = steps[Math.Min(Keys.Count, steps.Length) - 1](request);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }

    private sealed class PipelineHandler(ResiliencePipeline<HttpResponseMessage> pipeline, HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ResilienceContext context = ResilienceContextPool.Shared.Get(cancellationToken);
            context.Properties.Set(IdempotentRetry.RequestMessageKey, request);
            try
            {
                return await pipeline.ExecuteAsync(
                    async ctx => await base.SendAsync(request, ctx.CancellationToken).ConfigureAwait(false),
                    context).ConfigureAwait(false);
            }
            finally
            {
                ResilienceContextPool.Shared.Return(context);
            }
        }
    }

    private sealed class FailingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("connection reset");
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromException<int>(new IOException("connection reset"));
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class BlockingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return 0;
        }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class NonSeekableStream(byte[] bytes) : Stream
    {
        private readonly MemoryStream _inner = new(bytes);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            _inner.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
