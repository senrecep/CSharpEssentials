using System.Text;
using AotProbe;
using CSharpEssentials.Clone;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.Idempotency;
using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;
using CSharpEssentials.These;
using CSharpEssentials.Time;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

int failures = 0;

void Check(string package, bool ok)
{
    Console.WriteLine($"{(ok ? "OK" : "FAIL")} {package}");
    if (!ok)
        failures++;
}

Check("Time", new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc).NextDayOfWeek(DayOfWeek.Monday) == new DateTime(2026, 11, 9, 0, 0, 0, DateTimeKind.Utc));

These<Error, int> these = TheseExtensions.FromResult(Result<int>.Success(41)).Map(x => x + 1);
Check("These", these.IsRight && these.RightOrDefault == 42);

Result rules = RuleEngine.And<int>([x => x > 0 ? Result.Success() : Error.Validation("Positive"), x => x < 10 ? Result.Success() : Error.Validation("Small")], 5);
Check("Rules", rules.IsSuccess);

Order order = new();
order.SetCreatedInfo(DateTimeOffset.UnixEpoch.AddDays(1), "probe");
Check("Entity", order.CreatedBy == "probe");

List<CloneableItem> items = [new(1), new(2)];
List<CloneableItem> clones = items.Clone().ToList();
Check("Clone", clones.Count == 2 && !ReferenceEquals(clones[0], items[0]) && clones[1].Value == 2);

int attempts = 0;
Func<CancellationToken, Task<Result<int>>> flaky = _ => Task.FromResult(++attempts < 2 ? Result<int>.Failure(Error.Unexpected("Flaky")) : Result<int>.Success(7));
Result<int> retried = await flaky.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));
Check("Resilience", retried.IsSuccess && retried.Value == 7 && attempts == 2);

using HttpClient client = new(new EchoHandler()) { BaseAddress = new Uri("http://probe.local/") };
Result<ProbeDto> fetched = await client.GetFromJsonAsResultAsync(new Uri("items", UriKind.Relative), ProbeJsonContext.Default.ProbeDto);
Result<ProbeDto> posted = await HttpRequestBuilder.Post("items")
    .WithJsonContent(new ProbeDto("sent", 3), ProbeJsonContext.Default.ProbeDto)
    .AsResultAsync(client, ProbeJsonContext.Default.ProbeDto);
Check("Http", fetched.IsSuccess && fetched.Value.Name == "get" && posted.IsSuccess && posted.Value is { Name: "echo", Count: 3 });

ServiceCollection services = new();
services.AddIdempotencyExecutor();
await using (ServiceProvider provider = services.BuildServiceProvider())
{
    await using AsyncServiceScope scope = provider.CreateAsyncScope();
    IIdempotencyExecutor executor = scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>();
    IIdempotencyResultCodec<string> codec = IdempotencyResultCodec.Create<string>(
        value => Encoding.UTF8.GetBytes(value),
        payload => Encoding.UTF8.GetString(payload.Span));
    IdempotencyRecordKey key = new("probe", IdempotencyScope.User("u1"), IdempotencyKeys.For("op", "1"));
    int runs = 0;
    IdempotencyExecution<string> first = await executor.ExecuteAsync(key, "fp", null, _ => new ValueTask<string>($"run{++runs}"), codec);
    IdempotencyExecution<string> second = await executor.ExecuteAsync(key, "fp", null, _ => new ValueTask<string>($"run{++runs}"), codec);
    Check("Idempotency", first.Status == IdempotencyExecutionStatus.Executed && second.Status == IdempotencyExecutionStatus.Replayed && second.Value == "run1" && runs == 1);
}

ServiceCollection webServices = new();
webServices.AddLogging();
await using (ServiceProvider webProvider = webServices.BuildServiceProvider())
{
    string? loggedResponse = null;
    ApplicationBuilder app = new(webProvider);
    app.AddRequestResponseLogging(options => options.UseHandler(context =>
    {
        loggedResponse = context.ResponseBody;
        return Task.CompletedTask;
    }));
    app.Run(context => context.Response.WriteAsync("pong"));
    RequestDelegate pipeline = app.Build();
    DefaultHttpContext http = new() { RequestServices = webProvider };
    http.Request.Method = "GET";
    http.Request.Path = "/ping";
    http.Response.Body = new MemoryStream();
    await pipeline(http);
    Check("RequestResponseLogging", loggedResponse == "pong");
}

return failures == 0 ? 0 : 1;
