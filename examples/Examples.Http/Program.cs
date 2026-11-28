using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

// FakeApi answers every request in memory, so nothing here touches the network.
var api = new FakeApi();
using var client = new HttpClient(api) { BaseAddress = new Uri("https://api.example.com") };
Uri Rel(string path) => new(path, UriKind.Relative);

Section("1. Result calls on HttpClient");
{
    Result<User> get = await client.GetFromJsonAsResultAsync<User>(Rel("/users/1"));
    Result<User> post = await client.PostAsJsonAsResultAsync<User>(Rel("/users"), new { Name = "Linus" });
    Result<User> put = await client.PutAsJsonAsResultAsync<User>(Rel("/users/1"), new { Name = "Ada L." });
    Result<User> patch = await client.PatchAsJsonAsResultAsync<User>(Rel("/users/1"), new { Name = "Ada" });
    Result delete = await client.DeleteAsResultAsync(Rel("/users/1"));
    Result postContent = await client.PostAsResultAsync(Rel("/audit"), new StringContent("login"));
    Result putContent = await client.PutAsResultAsync(Rel("/audit"), new StringContent("logout"));
    Result send = await client.SendAsResultAsync(new HttpRequestMessage(HttpMethod.Head, Rel("/users/1")));
    Result<User> sendTyped = await client.SendAsResultAsync<User>(new HttpRequestMessage(HttpMethod.Get, Rel("/users/1")));
    Console.WriteLine($"GET {get.Value.Name}, POST {post.Value.Id}, PUT {put.Value.Name}, PATCH {patch.Value.Name}");
    Console.WriteLine($"DELETE {delete.IsSuccess}, POST content {postContent.IsSuccess}, PUT content {putContent.IsSuccess}, SEND {send.IsSuccess}/{sendTyped.Value.Name}");
}

Section("2. Status codes and other failures");
{
    foreach (string path in new[] { "/status/400", "/status/401", "/status/404", "/status/409", "/status/422", "/status/503", "/null", "/broken-json", "/no-content", "/down" })
    {
        Result<User> result = await client.GetFromJsonAsResultAsync<User>(Rel(path));
        Console.WriteLine($"{path,-13} {result.FirstError.Type,-12} {result.FirstError.Code}");
    }

    using var impatient = new HttpClient(api) { BaseAddress = client.BaseAddress, Timeout = TimeSpan.FromMilliseconds(100) };
    Result<User> timeout = await impatient.GetFromJsonAsResultAsync<User>(Rel("/slow"));
    Console.WriteLine($"{"/slow",-13} {timeout.FirstError.Type,-12} {timeout.FirstError.Code}");

    Error mapped = HttpStatusCodeMapper.ToError(HttpStatusCode.TooManyRequests, "Slow down.");
    Console.WriteLine($"ToError(429): {mapped.Code} {mapped.Type} '{mapped.Description}', ToErrorType(410): {HttpStatusCodeMapper.ToErrorType(HttpStatusCode.Gone)}");

    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
    try
    {
        await client.GetFromJsonAsResultAsync<User>(Rel("/slow"), cancellationToken: cts.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("own token cancelled: OperationCanceledException is thrown");
    }
}

Section("3. Reading HttpContent");
{
    using var content = new StringContent("""{"id":5,"name":"Grace"}""", Encoding.UTF8, "application/json");
    Result<User> user = await content.ReadFromJsonAsResultAsync<User>();
    Result<string> text = await content.ReadAsStringAsResultAsync();
    Console.WriteLine($"{user.Value.Name} / {text.Value}");
}

Section("4. HttpRequestBuilder");
{
    Result<User> routed = await HttpRequestBuilder
        .Get("/teams/{team}/users/{id}")
        .WithRoute("team", "core team")
        .WithRoute("id", 1)
        .WithQuery("include", "roles")
        .WithQuery("tag", new[] { "a", "b" })
        .WithQuery(new Dictionary<string, string?> { ["page"] = "1", ["skip"] = null })
        .WithHeader("Accept-Language", "en")
        .WithHeaders(new Dictionary<string, string> { ["X-Trace"] = "42" })
        .AsResultAsync<User>(client);
    Console.WriteLine($"routed GET -> {routed.Value.Name}, server saw {api.LastUri}");

    Result posted = await HttpRequestBuilder.Post("/users").WithJsonContent(new { Name = "Barbara" }).AsResultAsync(client);
    Console.WriteLine($"JSON POST -> {posted.IsSuccess}, body {api.LastBody}");

    using var csv = new StringContent("id,name\n1,Ada", Encoding.UTF8);
    HttpRequestBuilder upload = HttpRequestBuilder.Get("/placeholder")
        .WithMethod(HttpMethod.Put)
        .WithUri("/imports")
        .WithHeader("Content-Type", "text/csv")
        .WithContent(csv);
    Result first = await upload.AsResultAsync(client);
    Result second = await upload.AsResultAsync(client); // the builder keeps the content, so it can send again
    Console.WriteLine($"PUT with own content twice -> {first.IsSuccess}/{second.IsSuccess}, content type {api.LastContentType}");

    Result<User> patched = await HttpRequestBuilder.Patch("/users/1").WithJsonContent(new { Name = "Ada" }).AsResultAsync<User>(client);
    Console.WriteLine($"PATCH via builder -> {patched.Value.Name}");

    Result<HttpRequestMessage> built = HttpRequestBuilder.Delete("/users/{id}").WithRoute("id", 3).WithQuery("force", true).Build();
    Result<HttpRequestMessage> noRoute = HttpRequestBuilder.Get("/users").WithRoute("id", 3).Build();
    Result noClient = await HttpRequestBuilder.Get("/users").AsResultAsync(null);
    Console.WriteLine($"Build: {built.Value.Method} {built.Value.RequestUri}, errors: {noRoute.FirstError.Code}, {noClient.FirstError.Code}");
}

Section("5. Enums in route and query");
{
    Uri names = HttpRequestBuilder.Get("https://api.example.com/orders/{status}")
        .WithRoute("status", OrderStatus.PendingApproval)
        .WithQuery("permissions", Permissions.Read | Permissions.Write)
        .Build().Value.RequestUri!;
    Uri numbers = HttpRequestBuilder.Get("https://api.example.com/orders/{status}")
        .WithEnumConventions(EnumConventions.Default, EnumWireFormat.Number)
        .WithRoute("status", OrderStatus.PendingApproval)
        .Build().Value.RequestUri!;
    Result<HttpRequestMessage> undefined = HttpRequestBuilder.Get("/orders/{status}").WithRoute("status", (OrderStatus)99).Build();
    Console.WriteLine($"names: {names}\nnumbers: {numbers}\nundefined: {undefined.FirstError.Code}");
}

Section("6. Query strings");
{
    Uri search = new("https://api.example.com/search?lang=en");
    Console.WriteLine(search.WithQueryString("q", "c# tips").Value.AbsoluteUri);
    Console.WriteLine(search.WithQueryString("status", OrderStatus.Draft, EnumConventions.Default).Value.AbsoluteUri);
    Console.WriteLine(search.WithQueryString(new Dictionary<string, string?> { ["page"] = "2" }).Value.AbsoluteUri);
    Console.WriteLine(search.WithQueryString(new { Q = "csharp", Page = 2 }).Value.AbsoluteUri);
    Console.WriteLine(new Dictionary<string, string?> { ["a"] = "1", ["b"] = "x y" }.ToQueryString().Value);
    Console.WriteLine(new { From = 1, To = 9 }.ToQueryString().Value);
    Console.WriteLine(search.WithQueryString("", "x").FirstError.Code);
}

Section("7. Source-generated JSON (trimming and Native AOT)");
{
    Result<User> read = await client.GetFromJsonAsResultAsync(Rel("/users/1"), AppJson.Default.User);
    Result<User> posted = await client.PostAsJsonAsResultAsync(Rel("/users"), new NewUser("Linus"), AppJson.Default.NewUser, AppJson.Default.User);
    Result<User> built = await HttpRequestBuilder.Post("/users")
        .WithJsonTypeInfoContent(new NewUser("Linus"), AppJson.Default.NewUser)
        .AsResultAsync(client, AppJson.Default.User);
    Console.WriteLine($"{read.Value.Name}, {posted.Value.Id}, {built.Value.Name}");
}

Section("8. Redirects");
{
    Result<User> moved = await client.SendWithRedirectsAsResultAsync<User>(new HttpRequestMessage(HttpMethod.Get, Rel("/old-user")), maxRedirects: 3);
    Result viaBuilder = await HttpRequestBuilder.Get("/old-user").FollowRedirects().AsResultAsync(client);
    Result loop = await HttpRequestBuilder.Get("/loop").FollowRedirects(maxRedirects: 2).AsResultAsync(client);
    Console.WriteLine($"301 -> {moved.Value.Name}, builder -> {viaBuilder.IsSuccess}, loop -> {loop.FirstError.Code}");

    using var withAuth = new HttpClient(api) { BaseAddress = client.BaseAddress };
    withAuth.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
    Result crossOrigin = await HttpRequestBuilder.Get("/elsewhere").FollowRedirects().AsResultAsync(withAuth);
    Console.WriteLine($"cross-origin with default Authorization -> {crossOrigin.FirstError.Code}");
}

Section("9. Retry helpers");
{
    api.FlakyFailures = 2;
    ResiliencePolicy policy = HttpClientResilienceExtensions.CreateResiliencePolicy(maxRetryAttempts: 3, timeout: TimeSpan.FromSeconds(5), retryDelay: TimeSpan.FromMilliseconds(10));
    Result<User> viaPolicy = await policy.ExecuteAsync(ct => client.GetFromJsonAsResultAsync<User>(Rel("/flaky"), cancellationToken: ct));

    api.FlakyFailures = 2;
    ResiliencePipeline<Result<User>> pipeline = HttpClientResilienceExtensions.CreateRetryPipeline<User>(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(10));
    Result<User> viaPipeline = await pipeline.ExecuteAsResultAsync(ct => client.GetFromJsonAsResultAsync<User>(Rel("/flaky"), cancellationToken: ct));

    ResiliencePolicy<User> typed = HttpClientResilienceExtensions.CreateRetryPolicy<User>(maxRetryAttempts: 3, delay: TimeSpan.FromMilliseconds(10));
    Result<User> notRetried = await typed.ExecuteAsync(ct => client.GetFromJsonAsResultAsync<User>(Rel("/status/404"), cancellationToken: ct));
    Console.WriteLine($"policy -> {viaPolicy.Value.Name}, pipeline -> {viaPipeline.Value.Name}, 404 not retried -> {notRetried.FirstError.Code}");

    ResiliencePolicy timeoutPolicy = HttpClientResilienceExtensions.CreateTimeoutPolicy(TimeSpan.FromSeconds(1));
    Result<User> timedOut = await timeoutPolicy.ExecuteAsync(ct => client.GetFromJsonAsResultAsync<User>(Rel("/slow"), cancellationToken: ct));

    ResiliencePipeline breakerPipeline = HttpClientResilienceExtensions.CreateCircuitBreakerPipeline(minimumThroughput: 2);
    string[] breakerCodes = new string[3];
    for (int i = 0; i < 3; i++)
        breakerCodes[i] = (await breakerPipeline.ExecuteAsResultAsync(ct => client.GetFromJsonAsResultAsync<User>(Rel("/status/503"), cancellationToken: ct))).FirstError.Code;

    ResiliencePipeline timeoutPipeline = HttpClientResilienceExtensions.CreateTimeoutPipeline(TimeSpan.FromSeconds(5));
    ResiliencePipeline combined = HttpClientResilienceExtensions.CreateResiliencePipeline(maxRetryAttempts: 2, retryDelay: TimeSpan.FromMilliseconds(10));
    _ = HttpClientResilienceExtensions.CreateCircuitBreakerPolicy(minimumThroughput: 5);
    Result<User> viaTimeoutPipeline = await timeoutPipeline.ExecuteAsResultAsync(ct => client.GetFromJsonAsResultAsync<User>(Rel("/users/1"), cancellationToken: ct));
    Console.WriteLine($"timeout policy -> {timedOut.FirstError.Code}, circuit breaker pipeline -> {string.Join(", ", breakerCodes)}, timeout pipeline -> {viaTimeoutPipeline.Value.Name}, combined pipeline built: {combined != null}");
}

Section("10. Idempotency keys and safe retries");
{
    ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
        .AddRetry(new RetryStrategyOptions<HttpResponseMessage> { MaxRetryAttempts = 3, Delay = TimeSpan.FromMilliseconds(10) }
            .RetryUnsafeMethodsOnlyWithIdempotencyKey(new IdempotentRetryOptions { MaxRetryAfter = TimeSpan.FromSeconds(1) }))
        .Build();

    api.PaymentFailures = 1;
    api.PaymentKeys.Clear();
    HttpRequestBuilder payment = HttpRequestBuilder.Post("/payments").WithJsonContent(new { Amount = 10 }).WithIdempotencyKey();
    using HttpResponseMessage paid = await pipeline.ExecuteAsync(ct => new ValueTask<HttpResponseMessage>(client.SendAsync(payment.Build().Value, ct)));
    Console.WriteLine($"POST with key: {(int)paid.StatusCode}, keys seen: {string.Join(", ", api.PaymentKeys.Select(k => k[..8]))}");

    api.PaymentFailures = 1;
    api.PaymentKeys.Clear();
    using HttpResponseMessage unkeyed = await pipeline.ExecuteAsync(ct =>
        new ValueTask<HttpResponseMessage>(client.SendAsync(HttpRequestBuilder.Post("/payments").Build().Value, ct)));
    Console.WriteLine($"POST without key: {(int)unkeyed.StatusCode} after {api.PaymentKeys.Count} attempt");

    // No response at all: the strategy finds the request through IdempotentRetry.RequestMessageKey.
    api.ResetFailures = 1;
    api.PaymentKeys.Clear();
    HttpRequestBuilder resetPayment = HttpRequestBuilder.Post("/reset").WithIdempotencyKey();
    ResilienceContext context = ResilienceContextPool.Shared.Get();
    try
    {
        using HttpResponseMessage afterReset = await pipeline.ExecuteAsync(async ctx =>
        {
            HttpRequestMessage attempt = resetPayment.Build().Value;
            ctx.Properties.Set(IdempotentRetry.RequestMessageKey, attempt);
            return await client.SendAsync(attempt, ctx.CancellationToken);
        }, context);
        Console.WriteLine($"connection reset, then retried: {(int)afterReset.StatusCode}");
    }
    finally
    {
        ResilienceContextPool.Shared.Return(context);
    }

    api.PaymentKeys.Clear();
    using var keyed = new HttpClient(new IdempotencyKeyHandler(api)) { BaseAddress = client.BaseAddress };
    using HttpResponseMessage auto = await keyed.PostAsync("/payments", new StringContent("{}"));
    Console.WriteLine($"IdempotencyKeyHandler added key: {api.PaymentKeys.Single() != "(none)"}");

    var services = new ServiceCollection();
    services.AddHttpClient("payments").AddIdempotencyKeys();
    Console.WriteLine($"AddIdempotencyKeys registered: {services.Count > 0}");

    using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/payments");
    string generated = request.SetIdempotencyKey();
    request.SetIdempotencyKey("capture-order-1");
    Console.WriteLine($"SetIdempotencyKey: generated {generated.Length} chars, now {request.Headers.GetValues("Idempotency-Key").Single()}");

    var busy = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { RequestMessage = request };
    busy.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(90));
    var replayed = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { RequestMessage = request };
    replayed.Headers.Add("Idempotency-Replayed", "true");
    Console.WriteLine($"ShouldRetry 429: {await IdempotentRetry.ShouldRetryAsync(request, busy, null)}, 502: {await IdempotentRetry.ShouldRetryAsync(request, new HttpResponseMessage(HttpStatusCode.BadGateway), null)}, replayed: {await IdempotentRetry.ShouldRetryAsync(request, replayed, null)}, no response: {await IdempotentRetry.ShouldRetryAsync(request, null, new HttpRequestException("reset"))}");
    Console.WriteLine($"GetRetryAfter(90 s, cap 30 s): {IdempotentRetry.GetRetryAfter(busy, TimeSpan.FromSeconds(30))}");
}

Section("11. SSRF guard");
{
    var services = new ServiceCollection();
    services.AddHttpClient("webhooks").AddSsrfGuard(options =>
    {
        options.MaxResponseContentLength = 1_048_576;
        options.Timeout = TimeSpan.FromSeconds(10);
        options.BlockedHosts.Add("*.internal.example.com");
    });
    services.AddHttpClient("strict").AddSsrfGuard(options => options.RequestPolicy = new OnlyHostPolicy("hooks.example.com"));
    services.AddHttpClient("partners").AddSsrfGuard(options =>
    {
        options.AllowedPorts.Add(8443);                                  // replaces the default 443
        options.AllowedHosts.Add("*.partner.example.com");               // skip the address check for these names
        options.AllowedNetworks.Add(IPNetwork.Parse("10.20.0.0/16"));    // a private range you trust
        options.BlockedNetworks.Add(IPNetwork.Parse("8.8.8.0/24"));      // block lists win
        options.AddressPolicy = new NoGooglePublicDnsPolicy();
    });
    using ServiceProvider provider = services.BuildServiceProvider();
    IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
    HttpClient webhooks = factory.CreateClient("webhooks");

    // Every target below is refused before a socket opens or DNS is asked.
    foreach (string target in new[] { "https://127.0.0.1/", "https://169.254.169.254/latest/meta-data", "https://[::ffff:10.0.0.1]/", "http://example.com/", "https://db.internal.example.com/" })
    {
        Result result = await webhooks.SendAsResultAsync(new HttpRequestMessage(HttpMethod.Get, target));
        Console.WriteLine($"{target,-42} {result.FirstError.Code} ({result.FirstError.Metadata?["reason"]})");
    }

    Result strict = await factory.CreateClient("strict").SendAsResultAsync(new HttpRequestMessage(HttpMethod.Get, "https://other.example.com/"));
    Console.WriteLine($"{"custom request policy",-42} {strict.FirstError.Code} ({strict.FirstError.Metadata?["reason"]})");

    HttpClient partners = factory.CreateClient("partners");
    foreach (string target in new[] { "https://8.8.8.8:8443/", "https://8.8.4.4:8443/", "https://1.1.1.1/" })
    {
        Result result = await partners.SendAsResultAsync(new HttpRequestMessage(HttpMethod.Get, target));
        Console.WriteLine($"{"partners " + target,-42} {result.FirstError.Code} ({result.FirstError.Metadata?["reason"]})");
    }

    try
    {
        await webhooks.GetAsync("https://[::1]/");
    }
    catch (SsrfBlockedException ex)
    {
        Console.WriteLine($"SsrfBlockedException: {ex.Reason}, address {ex.BlockedAddress}, uri {ex.RequestUri}");
    }

    Console.WriteLine($"default address policy: 10.0.0.5 {DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse("10.0.0.5"), new Uri("https://x/"))}, 8.8.8.8 {DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse("8.8.8.8"), new Uri("https://x/"))}");
    var httpAllowed = new SsrfGuardOptions { AllowHttp = true };
    Console.WriteLine($"default request policy: :8443 {new DefaultOutboundRequestPolicy(new SsrfGuardOptions()).IsAllowed(new Uri("https://example.com:8443/"))}, http with AllowHttp {new DefaultOutboundRequestPolicy(httpAllowed).IsAllowed(new Uri("http://example.com/"))}");
}

Console.WriteLine();
Console.WriteLine("Done.");

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}

public sealed record User(int Id, string Name);

public sealed record NewUser(string Name);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(NewUser))]
internal sealed partial class AppJson : JsonSerializerContext;

[StringEnum]
public enum OrderStatus { Draft, PendingApproval }

[Flags, StringEnum]
public enum Permissions { None = 0, Read = 1, Write = 2 }

public sealed class OnlyHostPolicy(string host) : IOutboundRequestPolicy
{
    public bool IsAllowed(Uri requestUri) => requestUri.Scheme == Uri.UriSchemeHttps && requestUri.Host == host;
}

public sealed class NoGooglePublicDnsPolicy : IOutboundAddressPolicy
{
    public bool IsAllowed(IPAddress address, Uri requestUri) =>
        !address.Equals(IPAddress.Parse("8.8.4.4")) && DefaultOutboundAddressPolicy.Instance.IsAllowed(address, requestUri);
}

public sealed class FakeApi : HttpMessageHandler
{
    public int ResetFailures { get; set; }
    public int FlakyFailures { get; set; }
    public int PaymentFailures { get; set; }
    public List<string> PaymentKeys { get; } = [];
    public string? LastUri { get; private set; }
    public string? LastBody { get; private set; }
    public string? LastContentType { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastUri = request.RequestUri!.AbsoluteUri;
        LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        LastContentType = request.Content?.Headers.ContentType?.MediaType;
        HttpResponseMessage response = await RespondAsync(request, cancellationToken);
        response.RequestMessage = request;
        return response;
    }

    private async Task<HttpResponseMessage> RespondAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string path = request.RequestUri!.AbsolutePath;
        switch (path)
        {
            case "/slow":
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                return Json("{}");
            case "/null":
                return Json("null");
            case "/broken-json":
                return Json("{ not json");
            case "/no-content":
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            case "/down":
                throw new HttpRequestException("Connection refused.");
            case "/old-user":
                return Redirect(HttpStatusCode.MovedPermanently, "/users/1");
            case "/loop":
                return Redirect(HttpStatusCode.Found, "/loop");
            case "/elsewhere":
                return Redirect(HttpStatusCode.TemporaryRedirect, "https://other.example.com/users/1");
            case "/flaky":
                return FlakyFailures-- > 0 ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) : Json("""{"id":1,"name":"Ada"}""");
            case "/reset":
                if (ResetFailures-- > 0)
                    throw new HttpRequestException("Connection reset.");
                return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{}") };
            case "/payments":
                PaymentKeys.Add(request.Headers.TryGetValues("Idempotency-Key", out IEnumerable<string>? keys) ? keys.Single() : "(none)");
                return PaymentFailures-- > 0 ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) : new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{}") };
            case "/users" when request.Method == HttpMethod.Post:
                return Json("""{"id":2,"name":"Linus"}""", HttpStatusCode.Created);
            case "/users/1" when request.Method == HttpMethod.Delete:
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            case "/audit" or "/imports":
                return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        if (path.StartsWith("/status/", StringComparison.Ordinal))
            return new HttpResponseMessage((HttpStatusCode)int.Parse(path["/status/".Length..]));
        if (path.EndsWith("/users/1", StringComparison.Ordinal))
            return Json(request.Method == HttpMethod.Put ? """{"id":1,"name":"Ada L."}""" : """{"id":1,"name":"Ada"}""");
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static HttpResponseMessage Redirect(HttpStatusCode status, string location)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
        return response;
    }
}
