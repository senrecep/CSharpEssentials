using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Dictionary<int, User> users = new()
{
    [1] = new User(1, "Ada", "ada@example.com", IsActive: true),
    [2] = new User(2, "Bo", null, IsActive: false),
};

Maybe<User> FindUser(int id) => users.TryGetValue(id, out User? user) ? user : Maybe.None;

// 1. Creating
Console.WriteLine("--- Creating ---");
Maybe<int> some = 42;                                   // implicit from T
Maybe<string> fromNull = (string?)null;                 // null becomes None
Maybe<string> none = Maybe.None;                        // non-generic None converts to any Maybe<T>
Maybe<string> viaFrom = Maybe.From("Ann");
Maybe<int> viaExtension = 5.AsMaybe();
Maybe<int> parsed = Maybe.FromTry(() => int.Parse("not a number")); // exception becomes None
Maybe<string> lazy = Maybe.From(() => Environment.GetEnvironmentVariable("CSE_EXAMPLE_UNSET"));
Console.WriteLine($"{some} {fromNull} {none} {viaFrom} {viaExtension} {parsed} {lazy}");

// 2. Reading the value
Console.WriteLine("--- Reading ---");
Console.WriteLine($"HasValue={some.HasValue}, HasNoValue={none.HasNoValue}, Value={some.Value}");
Console.WriteLine($"GetValueOrDefault(0)={parsed.GetValueOrDefault(0)}");
Console.WriteLine($"GetValueOrDefault(() => -1)={parsed.GetValueOrDefault(() => -1)}");
Console.WriteLine($"GetValueOrDefault(v => v * 2, 0)={some.GetValueOrDefault(v => v * 2, 0)}");
Console.WriteLine($"GetValueOrElse={parsed.GetValueOrElse(() => 7)}");
Console.WriteLine($"TryGetValue={some.TryGetValue(out int got)} {got}");
var (hasValue, inner) = some;
Console.WriteLine($"Deconstruct=({hasValue}, {inner}), AsNullable={none.AsNullable() ?? "null"}, ToList.Count={some.ToList().Count}");
try
{
    none.GetValueOrThrow("name is required");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"GetValueOrThrow: {ex.Message}");
}

// 3. Chaining
Console.WriteLine("--- Map / Bind / Where / Or ---");
string email1 = FindUser(1).Bind(u => Maybe.From(u.Email)).Map(e => e.ToUpperInvariant()).GetValueOrDefault("no email");
string email2 = FindUser(2).Bind(u => Maybe.From(u.Email)).GetValueOrDefault("no email");
string active = FindUser(2).Where(u => u.IsActive).Map(u => u.Name).GetValueOrDefault("inactive");
Console.WriteLine($"{email1} | {email2} | {active}");
Console.WriteLine($"Or(7)={Maybe<int>.None.Or(7)}, Or(() => 8)={Maybe<int>.None.Or(() => 8)}, OrElse={Maybe<int>.None.OrElse(() => 9)}");
Console.WriteLine($"MapIf={some.MapIf(v => v > 40, v => v + 1)}, BindIf={some.BindIf(false, v => Maybe<int>.None)}");

// 4. LINQ query syntax
Console.WriteLine("--- LINQ ---");
Maybe<string> greeting =
    from user in FindUser(1)
    from email in Maybe.From(user.Email)
    where user.IsActive
    select $"{user.Name} <{email}>";
Console.WriteLine(greeting);

// 5. Match
Console.WriteLine("--- Match ---");
foreach (int id in new[] { 1, 3 })
{
    string label = FindUser(id).Match(some: u => $"found {u.Name}", none: () => $"user {id} missing");
    Console.WriteLine(label);
}
FindUser(3).Match(some: u => Console.WriteLine(u.Name), none: () => Console.WriteLine("Match(Action, Action): none"));

// 6. Side effects
Console.WriteLine("--- Tap / TapIf / TapNone / Execute ---");
some.Tap(v => Console.WriteLine($"Tap: {v}"))
    .TapIf(v => v > 40, v => Console.WriteLine($"TapIf: {v} > 40"))
    .TapNone(() => Console.WriteLine("not printed"));
none.TapNone(() => Console.WriteLine("TapNone: missing"));
some.Execute(v => Console.WriteLine($"Execute: {v}"));
none.ExecuteNoValue(() => Console.WriteLine("ExecuteNoValue: missing"));

// 7. Result bridge
Console.WriteLine("--- Result bridge ---");
Result<User> found = FindUser(1).ToResult(Error.NotFound("User.NotFound", "User not found."));
Result<User> missing = FindUser(3).ToResult(Error.NotFound("User.NotFound", "User not found."));
Result<User> defaultError = FindUser(3).ToMaybeResult();
Result checkedOnly = FindUser(1).ToUnitResult();
Console.WriteLine($"found={found.IsSuccess}, missing={missing.FirstError.Code}, default={defaultError.FirstError.Code}, unit={checkedOnly.IsSuccess}");

Result<int> failedResult = Error.Validation("X", "bad");
Console.WriteLine($"Result.AsMaybe: {((Result<int>)5).AsMaybe()} / {failedResult.AsMaybe()}");

Result<User, UserMissing> typed = FindUser(3).ToResult(new UserMissing(3));
Console.WriteLine($"Typed result AsMaybe: {typed.AsMaybe()}");

// 8. Collections
Console.WriteLine("--- Collections ---");
int[] numbers = [3, 8, 11];
Console.WriteLine($"TryFirst(even)={numbers.TryFirst(n => n % 2 == 0)}, TryLast()={numbers.TryLast()}, TryFirst on empty={Array.Empty<int>().TryFirst()}");
IReadOnlyDictionary<int, User> lookup = users;
Console.WriteLine($"TryFind(1)={lookup.TryFind(1).Map(u => u.Name)}, TryFind(9)={lookup.TryFind(9)}");

Maybe<int>[] maybes = [1, Maybe.None, 3];
Console.WriteLine($"Choose=[{string.Join(", ", maybes.Choose())}], Choose(x10)=[{string.Join(", ", maybes.Choose(v => v * 10))}]");
var (values, noneCount) = maybes.Partition();
Console.WriteLine($"Partition=[{string.Join(", ", values)}], NoneCount={noneCount}");
Console.WriteLine($"Sequence with a None={maybes.Sequence()}, Traverse(1,2)={new[] { 1, 2 }.Traverse(id => FindUser(id)).Map(u => u.Length)}");
Console.WriteLine($"Flatten={Maybe<Maybe<int>>.From(Maybe<int>.From(5)).Flatten()}");

// 9. Async
Console.WriteLine("--- Async ---");
Maybe<User> loaded = await Maybe.FromAsync(LoadUserAsync(1));
Maybe<string> name = await loaded.MapAsync(u => Task.FromResult(u.Name));
Maybe<User> fallback = await LoadMaybeAsync(9).OrAsync(() => new User(0, "Guest", null, true));
string described = await loaded.MatchAsync(
    some: (u, ct) => Task.FromResult($"async found {u.Name}"),
    none: ct => Task.FromResult("async missing"));
Result<User> asResult = await LoadMaybeAsync(9).ToResultAsync(Error.NotFound("User.NotFound", "User not found."));
Console.WriteLine($"{name} | {fallback.Value.Name} | {described} | {asResult.FirstError.Code}");

Maybe<string> bound = await loaded.BindAsync(u => Task.FromResult(Maybe.From(u.Email)));
Maybe<User> activeOnly = await loaded.WhereAsync(u => Task.FromResult(u.IsActive));
await loaded.TapAsync(u => Task.CompletedTask);
await loaded.ExecuteAsync(u => { Console.WriteLine($"ExecuteAsync: {u.Name}"); return Task.CompletedTask; });
string lengthText = await loaded.GetValueOrDefaultAsync(u => Task.FromResult(u.Name.Length.ToString()), "0");
Maybe<int> fromResultTask = await Task.FromResult((Result<int>)3).AsMaybeAsync();
Console.WriteLine($"{bound} | {activeOnly.HasValue} | {lengthText} | {fromResultTask}");

await foreach (int value in new[] { Task.FromResult<Maybe<int>>(1), Task.FromResult(Maybe<int>.None) }.ChooseAsync())
    Console.WriteLine($"ChooseAsync: {value}");

// 10. Equality and JSON
Console.WriteLine("--- Equality / JSON ---");
Console.WriteLine($"From(0) == 0: {Maybe<int>.From(0) == 0}, None == 0: {Maybe<int>.None == 0}, None == null (string): {Maybe<string>.None == null}");
string json = JsonSerializer.Serialize(some);
Console.WriteLine($"{json} -> {JsonSerializer.Deserialize<Maybe<int>>(json)}");

async Task<User?> LoadUserAsync(int id)
{
    await Task.Yield();
    return users.GetValueOrDefault(id);
}

async Task<Maybe<User>> LoadMaybeAsync(int id) => await LoadUserAsync(id);

internal sealed record User(int Id, string Name, string? Email, bool IsActive);
internal sealed record UserMissing(int Id);
