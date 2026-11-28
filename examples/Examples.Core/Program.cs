using System.Globalization;
using CSharpEssentials.Core;
using CSharpEssentials.Transactions;

Section("String case conversion");
foreach (string input in new[] { "helloWorld", "hello_world", "Hello World", "HTTPStatus" })
{
    Console.WriteLine($"{input,-12} snake={input.ToSnakeCase()} kebab={input.ToKebabCase()} pascal={input.ToPascalCase()} camel={input.ToCamelCase()}");
    Console.WriteLine($"{"",-12} macro={input.ToMacroCase()} train={input.ToTrainCase()} title={input.ToTitleCase()} field={input.ToUnderscoreCamelCase()}");
}
Console.WriteLine($"tr-TR: {"istanbul".ToPascalCase(CultureInfo.GetCultureInfo("tr-TR"))}");

Section("GUIDs");
Guid v7 = Guider.NewGuid();
Guid stamped = Guider.NewGuid(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
Guid fromClock = Guider.NewGuid(TimeProvider.System);
Guid first = Guider.NewMonotonicGuid();
Guid second = Guider.NewMonotonicGuid();
Guid sqlKey = Guider.NewSqlServerGuid();
Guid named = Guider.NewGuidV5(GuidNamespaces.Dns, "www.example.com");
Console.WriteLine($"NewGuid            {v7} (version {v7.Version})");
Console.WriteLine($"NewGuid(time)      {stamped} (starts with the 2025-01-01 timestamp)");
Console.WriteLine($"NewGuid(clock)     {fromClock}");
Console.WriteLine($"NewMonotonicGuid   second > first: {second.CompareTo(first) > 0}");
Console.WriteLine($"NewSqlServerGuid   {sqlKey} (version {sqlKey.Version})");
Console.WriteLine($"NewGuidV5          {named} (always the same)");

string shortId = v7.ToStringFromGuid();
Console.WriteLine($"ToStringFromGuid   {shortId} ({shortId.Length} chars)");
Console.WriteLine($"ToGuidFromString   round trip: {shortId.ToGuidFromString() == v7}");
try
{
    Guider.ToGuidFromString("too-short");
}
catch (FormatException ex)
{
    Console.WriteLine($"Invalid input      FormatException: {ex.Message}");
}

Section("Collections");
List<string> items = ["a"];
items.IfAdd(true, "b");
items.IfAdd(false, "skipped");
items.IfAddRange(true, ["c", "d"]);
Console.WriteLine($"IfAdd/IfAddRange   {string.Join(",", items)}");

int[] numbers = [1, 2, 3, 4, 5];
Console.WriteLine($"WhereIf(true)      {string.Join(",", numbers.WhereIf(true, n => n > 2))}");
Console.WriteLine($"WhereIf(false)     {string.Join(",", numbers.WhereIf(false, n => n > 2))}");
Console.WriteLine($"WhereIf(index)     {string.Join(",", numbers.WhereIf(true, (n, i) => i % 2 == 0))}");
Console.WriteLine($"WhereIf(query)     {string.Join(",", numbers.AsQueryable().WhereIf(true, n => n < 3))}");

string?[] raw = ["x", null, "y"];
Console.WriteLine($"WithoutNulls       {string.Join(",", raw.WithoutNulls())}");
Contact?[] contacts = [new("Ada", "ada@example.com"), new("Bob", null), null];
Console.WriteLine($"WithoutNulls(sel)  {string.Join(",", contacts.WithoutNulls(c => c.Email).Select(c => c.Name))}");

Console.WriteLine($"HasSameElements    {new[] { 1, 2, 2 }.HasSameElements([2, 1])}");
Console.WriteLine($"AllTrue/AllFalse   {new[] { true, true }.AllTrue()} {new[] { false, false }.AllFalse()}");

int visited = 0;
IEnumerable<int> lazy = numbers.ForEach(_ => visited++);
Console.WriteLine($"ForEach (lazy)     before enumeration: {visited}");
_ = lazy.ToList();
Console.WriteLine($"ForEach (lazy)     after ToList: {visited}");

Console.WriteLine($"GetRandomItem      {numbers.GetRandomItem()}");
Console.WriteLine($"GetRandomItems(3)  {string.Join(",", numbers.GetRandomItems(3))}");
Console.WriteLine($"GetRandomItems(9)  {numbers.GetRandomItems(9).Length} items (all, shuffled)");

Section("Null, empty and boolean checks");
string? missing = null;
string blank = "   ";
Console.WriteLine($"IsEmpty            null={missing.IsEmpty()} blank={blank.IsEmpty()} text={"x".IsEmpty()}");
Console.WriteLine($"IsNotEmpty         {"x".IsNotEmpty()}");
Console.WriteLine($"IsNull/IsNotNull   {missing.IsNull()} {"x".IsNotNull()}");
bool ran = missing.IfNotNull(v => Console.WriteLine(v), () => Console.WriteLine("IfNotNull          else action ran"));
Console.WriteLine($"IfNotNull returned {ran}");
missing.IfNull(() => Console.WriteLine("IfNull             action ran"));
"x".IfNull(() => Console.WriteLine("not printed"), v => Console.WriteLine($"IfNull else        got {v}"));
true.IfTrue(() => Console.WriteLine("IfTrue             action ran"));
false.IfFalse(() => Console.WriteLine("IfFalse            action ran"));
Console.WriteLine($"IsTrue/IsFalse     {true.IsTrue()} {true.IsFalse()}");

Section("Tasks");
Console.WriteLine($"AsTask/AsValueTask {await 1.AsTask()} {await "two".AsValueTask()}");
using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)))
{
    Task slow = Task.Delay(TimeSpan.FromSeconds(10));
    try
    {
        await slow.WithCancellation(cts.Token);
    }
    catch (OperationCanceledException ex)
    {
        Console.WriteLine($"WithCancellation   {ex.GetType().Name}; the task still runs: {!slow.IsCompleted}");
    }
}

Section("Other helpers");
Console.WriteLine($"IsBetween          {10.IsBetween(1, 10)} exclusive: {10.IsBetweenExclusive(1, 10)}");
object boxed = 42;
Console.WriteLine($"ExplicitCast       {boxed.ExplicitCast<int>()}");
DateTime when = 1700000000000L.MsToDateTime();
Console.WriteLine($"MsToDateTime       {when:O} ({when.Kind})");
Console.WriteLine($"GetTypeGroup       {HttpStatus.NotFound} -> {HttpStatus.NotFound.GetTypeGroup<HttpStatusGroup, HttpStatus>()}");
var error = new InvalidOperationException("outer", new ArgumentException("inner"));
Console.WriteLine($"GetInnerExceptions {string.Join(" > ", error.GetInnerExceptions().Select(e => e.GetType().Name))}");
Console.WriteLine($"...Messages        {string.Join(" > ", error.GetInnerExceptionsMessages())}");
Console.WriteLine($"HttpCodes          Ok={HttpCodes.Ok} NotFound={HttpCodes.NotFound} TooManyRequests={HttpCodes.TooManyRequests}");
Console.WriteLine($"IsEmpty (C# 14)    {Members.IsEmptySequence([])} palindrome: {Members.IsPalindrome("level")}");

Section("Transactions");
var runner = new InMemoryTransactionRunner();
await runner.ExecuteAsync(
    async ct =>
    {
        await runner.OnCommittedAsync(_ => { Console.WriteLine("OnCommitted        callback ran after commit"); return ValueTask.CompletedTask; }, ct);
        return true;
    },
    shouldCommit: ok => ok);
await runner.ExecuteAsync(
    async ct =>
    {
        await runner.OnCommittedAsync(_ => { Console.WriteLine("not printed: rolled back"); return ValueTask.CompletedTask; }, ct);
        return false;
    },
    shouldCommit: ok => ok);
Console.WriteLine("Rollback           callback dropped");

static void Section(string title) => Console.WriteLine($"\n--- {title} ---");

record Contact(string Name, string? Email);

enum HttpStatus { Ok = 200, BadRequest = 400, NotFound = 404, InternalServerError = 500 }

enum HttpStatusGroup { Success = 200, ClientError = 400, ServerError = 500 }

sealed class InMemoryTransactionRunner : ITransactionRunner, ITransactionCallbacks
{
    private List<Func<CancellationToken, ValueTask>>? _pending;

    public async ValueTask<T> ExecuteAsync<T>(Func<CancellationToken, ValueTask<T>> work, Func<T, bool> shouldCommit, CancellationToken cancellationToken = default)
    {
        _pending = [];
        try
        {
            T result = await work(cancellationToken);
            if (shouldCommit(result))
                foreach (var callback in _pending)
                    await callback(CancellationToken.None);
            return result;
        }
        finally
        {
            _pending = null;
        }
    }

    public ValueTask OnCommittedAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default)
    {
        if (_pending is null)
            return callback(cancellationToken);
        _pending.Add(callback);
        return ValueTask.CompletedTask;
    }
}
