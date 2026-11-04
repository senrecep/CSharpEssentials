using System.Text;
using AotProbe;
using CSharpEssentials.Clone;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.Idempotency;
using CSharpEssentials.RequestResponseLogging;
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
string both = These<Error, int>.Both(Error.Validation("Warn"), 1).Match(_ => "left", _ => "right", (_, v) => $"both{v}");
Check("These", these.IsRight && these.RightOrDefault == 42 && both == "both1" && these.ToResult().Value == 42);

Result rules = RuleEngine.And<int>([x => x > 0 ? Result.Success() : Error.Validation("Positive"), x => x < 10 ? Result.Success() : Error.Validation("Small")], 5);
Func<int, Result>[] alternatives = [x => x < 0 ? Result.Success() : Error.Validation("Negative"), x => x == 5 ? Result.Success() : Error.Validation("Five")];
Result anyOf = await RuleEngine.EvaluateAsync(alternatives.Or(), 5);
Check("Rules", rules.IsSuccess && anyOf.IsSuccess);

Order order = new();
order.SetCreatedInfo(DateTimeOffset.UnixEpoch.AddDays(1), "probe");
order.SetUpdatedInfo(DateTimeOffset.UnixEpoch.AddDays(2), "editor");
order.Raise(new OrderPlaced());
order.MarkAsDeleted(DateTimeOffset.UnixEpoch.AddDays(3), "admin");
bool deleted = order.IsDeleted;
order.Restore();
Check("Entity", order.CreatedBy == "probe" && order.UpdatedBy == "editor" && order.DomainEvents.Count == 1 && deleted && !order.IsDeleted);

List<CloneableItem> items = [new(1), new(2)];
List<CloneableItem> clones = items.Clone().ToList();
Check("Clone", clones.Count == 2 && !ReferenceEquals(clones[0], items[0]) && clones[1].Value == 2);

using EchoHandler echo = new();
using HttpClient client = new(echo) { BaseAddress = new UriBuilder(Uri.UriSchemeHttps, "probe.local").Uri };
var info = ProbeJsonContext.Default.ProbeDto;
Uri itemsUri = new("items", UriKind.Relative);
ProbeDto sent = new("sent", 3);
Result<ProbeDto> fetched = await client.GetFromJsonAsResultAsync(itemsUri, info);
Result<ProbeDto> postedDirect = await client.PostAsJsonAsResultAsync(itemsUri, sent, info, info);
Result<ProbeDto> put = await client.PutAsJsonAsResultAsync(itemsUri, sent, info, info);
Result<ProbeDto> patched = await client.PatchAsJsonAsResultAsync(itemsUri, sent, info, info);
using HttpRequestMessage sendRequest = new(HttpMethod.Get, itemsUri);
Result<ProbeDto> sentResult = await client.SendAsResultAsync(sendRequest, info);
using HttpRequestMessage redirectRequest = new(HttpMethod.Get, new Uri(client.BaseAddress, "redirect"));
Result<ProbeDto> redirected = await client.SendWithRedirectsAsResultAsync(redirectRequest, info);
Result<ProbeDto> posted = await HttpRequestBuilder.Post("items")
    .WithJsonTypeInfoContent(sent, info)
    .AsResultAsync(client, info);
Result<ProbeDto> builderRedirected = await HttpRequestBuilder.Get(new Uri(client.BaseAddress, "redirect"))
    .FollowRedirects()
    .AsResultAsync(client, info);
using HttpContent body = System.Net.Http.Json.JsonContent.Create(sent, info);
Result<ProbeDto> read = await body.ReadFromJsonAsResultAsync(info);
Check("Http", fetched.Value is { Name: "get" }
    && postedDirect.Value is { Name: "echo POST", Count: 3 }
    && put.Value is { Name: "echo PUT", Count: 3 }
    && patched.Value is { Name: "echo PATCH", Count: 3 }
    && sentResult.Value is { Name: "get" }
    && redirected.Value is { Name: "get" }
    && posted.Value is { Name: "echo POST", Count: 3 }
    && builderRedirected.Value is { Name: "get" }
    && read.Value is { Name: "sent", Count: 3 });

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
