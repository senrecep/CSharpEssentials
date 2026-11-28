using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Comparers;
using CSharpEssentials.ResultPattern.Interfaces;

Section("Creating results");
{
    Result ok = Result.Success();
    Result<int> answer = 42;
    Result notFound = Error.NotFound("User.NotFound", "User not found.");
    Result<int> twoErrors = new[] { Error.Validation("A", "First."), Error.Validation("B", "Second.") };
    Result fromBool = false;
    Console.WriteLine($"ok={ok.IsSuccess} answer={answer.Value} notFound={notFound.FirstError.Code}");
    Console.WriteLine($"twoErrors={twoErrors.Errors.Length} fromBool={fromBool.FirstError.Code}");

    (bool isSuccess, int value, Error[] errors) = answer;
    Console.WriteLine($"deconstruct: {isSuccess} {value} {errors.Length}");
    Console.WriteLine($"ToString: {answer}");
}

Section("Chaining: Then, Map, Bind, Tap, BindIf");
{
    Result<string> chained = ParseNumber("5")
        .Then(n => n * 2)
        .Bind(n => n > 0 ? Result.Success(n + 10) : Result.Failure<int>(Error.Validation("Range", "Negative.")))
        .Tap(n => Console.WriteLine($"  tap: {n}"))
        .BindIf(n => n > 15, n => Result.Success(n * 100))
        .Map(n => $"value {n}");
    Console.WriteLine(chained.Value);

    Result<string> skipped = ParseNumber("abc").Then(n => n * 2).Map(n => $"value {n}");
    Console.WriteLine($"failure skips the chain: {skipped.FirstError.Code}");
}

Section("Ensuring: Ensure, FailIf, ThenEnsure");
{
    Result<int> age = Result.Success(15)
        .Ensure(a => a >= 0, Error.Validation("Age.Negative", "Negative."))
        .Ensure(a => a >= 18, a => Error.Validation("Age.Minor", $"{a} is under 18."));
    Console.WriteLine(age.FirstError.Description);

    Result<int> quantity = Result.Success(500).FailIf(q => q > 100, Error.Validation("Quantity.TooHigh", "At most 100."));
    Console.WriteLine(quantity.FirstError.Code);

    Result<int> even = Result.Success(8)
        .ThenEnsure(v => v % 2 == 0 ? Result.Success() : Error.Validation("Odd", "Must be even."));
    Console.WriteLine($"ThenEnsure: {even.Value}");
}

Section("Matching: Match, MatchFirst, Switch");
{
    Result<int> failed = new[] { Error.Validation("A", "First."), Error.Validation("B", "Second.") };
    Console.WriteLine(failed.Match(v => $"value {v}", errors => $"{errors.Length} errors"));
    Console.WriteLine(failed.MatchFirst(v => "ok", e => $"first {e.Code}"));
    Console.WriteLine(failed.MatchLast(v => "ok", e => $"last {e.Code}"));
    Result.Success(3).Switch(v => Console.WriteLine($"switch value {v}"), errors => Console.WriteLine("failed"));
}

Section("Handling failures: Else, Compensate, Recover, TapError, ElseDo, MapError");
{
    Result<int> missing = Error.NotFound("Count.Missing", "No count.");
    Console.WriteLine($"Else(0): {missing.Else(0).Value}");
    Console.WriteLine($"Else(error): {missing.Else(Error.Conflict("New", "Replaced.")).FirstError.Code}");
    Console.WriteLine($"Compensate: {missing.Compensate(errors => Result.Success(10)).Value}");
    Console.WriteLine($"CompensateFirst: {missing.CompensateFirst(e => Result.Success(e.Code.Length)).Value}");
    Console.WriteLine($"Recover(NotFound): {missing.Recover(ErrorType.NotFound, e => -1).Value}");
    Console.WriteLine($"Recover(Conflict) still fails: {missing.Recover(ErrorType.Conflict, e => -1).IsFailure}");
    Console.WriteLine($"RecoverFirst: {missing.RecoverFirst(ErrorType.NotFound, e => -2).Value}");
    missing
        .TapError(errors => Console.WriteLine($"  TapError: {errors.Length} error(s)"))
        .TapErrorFirst(e => Console.WriteLine($"  TapErrorFirst: {e.Code}"))
        .TapErrorIf(true, errors => Console.WriteLine("  TapErrorIf ran"))
        .ElseDo(errors => Console.WriteLine("  ElseDo ran"))
        .ElseDoFirst(e => Console.WriteLine("  ElseDoFirst ran"));
    Console.WriteLine($"MapError: {missing.MapError(e => Error.Failure($"Svc.{e.Code}", e.Description)).FirstError.Code}");
}

Section("Safe execution: Try, TryAsync, TryCatch");
{
    Result<int> parsed = Result.Try(() => int.Parse("x"), ex => Error.Exception(ex));
    Console.WriteLine($"Try: {parsed.FirstError.Code} ({parsed.FirstError.Type})");

    Result<int> loaded = await Result.TryAsync(async () => { await Task.Delay(1); return 99; }, ex => Error.Exception(ex));
    Console.WriteLine($"TryAsync: {loaded.Value}");

    Result<int> divided = Result.Success(0).TryCatch(n => Result.Success(10 / n));
    Console.WriteLine($"TryCatch: {divided.FirstError.Code}");
}

Section("Getting the value out");
{
    Result<int> failed = Error.NotFound("X", "Missing.");
    Console.WriteLine($"GetValueOrDefault(5): {failed.GetValueOrDefault(5)}");
    Console.WriteLine($"UnwrapOrDefault(7): {failed.UnwrapOrDefault(7)}");
    Console.WriteLine($"Finally: {failed.Finally(r => r.IsSuccess ? "ok" : "failed")}");
    try { failed.Unwrap(); }
    catch (ResultUnwrapException ex) { Console.WriteLine($"Unwrap threw with {ex.Errors.Length} error(s)"); }
    try { failed.GetValueOrThrow("no value"); }
    catch (InvalidOperationException ex) { Console.WriteLine($"GetValueOrThrow: {ex.Message}"); }
}

Section("Combining: And, Or, CombineAll, Combine, SuccessIf, LINQ");
{
    Result ok = Result.Success();
    Result a = Error.NotFound("A", "A missing.");
    Result b = Error.Conflict("B", "B taken.");
    Console.WriteLine($"And stops at first failure: {Result.And(ok, a, b).Errors.Length}");
    Console.WriteLine($"CombineAll collects every error: {new[] { ok, a, b }.CombineAll().Errors.Length}");
    Console.WriteLine($"Or: {Result.Or(a, ok).IsSuccess}");
    Console.WriteLine($"Combine: {Result<int>.Combine(Result.Success(1), Result.Success("x")).Value}");
    Console.WriteLine($"SuccessIf: {Result.SuccessIf(false, Error.Validation("Cond", "False.")).FirstError.Code}");
    Console.WriteLine($"FailureIf: {Result.FailureIf(true, Error.Validation("Cond", "True.")).FirstError.Code}");
    Result<int> sum = from x in Result.Success(1) from y in Result.Success(2) select x + y;
    Console.WriteLine($"LINQ: {sum.Value}");
}

Section("Collections: Sequence, Traverse, Partition, FirstFailureOrSuccesses, TraverseAsync, SequenceAsync");
{
    Result<int>[] results = [1, Error.Validation("Bad.2", "Two."), 3];
    Console.WriteLine($"Sequence errors: {results.Sequence().Errors.Length}");
    Console.WriteLine($"Traverse: {string.Join(",", new[] { "1", "2" }.Traverse(ParseNumber).Value)}");
    (int[] successes, Error[] errors) = results.Partition();
    Console.WriteLine($"Partition: {successes.Length} ok, {errors.Length} failed");
    Console.WriteLine($"FirstFailureOrSuccesses: {results.FirstFailureOrSuccesses().FirstError.Code}");

    Result<string[]> names = await new[] { 1, 2 }.TraverseAsync(
        async (id, ct) => { await Task.Delay(1, ct); return Result.Success($"user-{id}"); }, CancellationToken.None);
    Console.WriteLine($"TraverseAsync: {string.Join(",", names.Value)}");

    Result<int[]> all = await new[] { Task.FromResult(Result.Success(1)), Task.FromResult(Result.Success(2)) }.SequenceAsync();
    Console.WriteLine($"SequenceAsync: {all.Value.Sum()}");
}

Section("Async chains");
{
    Result<string> order = await LoadOrderAsync(1)
        .EnsureAsync(o => o.Lines > 0, Error.Validation("Order.Empty", "No lines."))
        .TapAsync(async o => { await Task.Yield(); Console.WriteLine($"  audit order {o.Id}"); })
        .MapAsync(o => $"order {o.Id}, {o.Lines} lines");
    Console.WriteLine(order.Value);

    string text = await LoadOrderAsync(9)
        .BindAsync(o => Task.FromResult(Result.Success(o.Id)))
        .MatchAsync(id => $"found {id}", errors => $"failed: {errors[0].Code}");
    Console.WriteLine(text);
}

Section("More members: inspection, extensions, comparers, strings");
{
    Result<int> failed = new[] { Error.Validation("A", "First."), Error.Validation("B", "Second.") };
    Console.WriteLine($"LastError: {failed.LastError.Code}, ErrorsOrEmptyArray on success: {Result.Success(1).ErrorsOrEmptyArray.Length}");
    Console.WriteLine($"TryGet: {Result.Success(5).TryGet(out int five, out _)} {five}");
    Console.WriteLine($"ValueOrDefault: {failed.ValueOrDefault}, | picks the success: {(failed | Result.Success(9)).Value}");
    Result fromErrors = Result.From(Error.Conflict("C", "Conflict."));
    Result<int> viaExtension = Error.NotFound("N", "Missing.").ToResult<int>();
    Console.WriteLine($"Result.From: {fromErrors.FirstError.Code}, ToResult<T>: {viaExtension.FirstError.Code}, value ToResult: {7.ToResult().Value}");

    failed.SwitchFirst(v => Console.WriteLine(v), e => Console.WriteLine($"SwitchFirst: {e.Code}"));
    failed.SwitchLast(v => Console.WriteLine(v), e => Console.WriteLine($"SwitchLast: {e.Code}"));
    Result.Success(4).ThenDo(v => Console.WriteLine($"ThenDo: {v}")).TapIf(v => v > 3, v => Console.WriteLine($"TapIf: {v}"));
    Result<int> selected = Result.Success(2).Select(v => v * 3).SelectMany(v => Result.Success(v + 1));
    Console.WriteLine($"Select/SelectMany: {selected.Value}");

    Result<string?> nullFromJson = JsonSerializer.Deserialize<Result<string?>>("""{"IsSuccess":true,"Value":null}""");
    Console.WriteLine($"EnsureNotNull on a null read from JSON: {nullFromJson.EnsureNotNull(Error.Validation("Null", "Missing.")).FirstError.Code}");

    IResultBase boxed = failed;
    Console.WriteLine($"IResultBase: {boxed.IsFailure}, ResultComparer<int>: {new ResultComparer<int>().Equals(Result.Success(1), Result.Success(1))}");
    Console.WriteLine($"TrimStart: {"v1.2".TrimStart("v").Value}, TrimEnd: {"file.txt".TrimEnd(".txt").Value}, empty: {"".TrimStart("v").FirstError.Code}");
}

Section("More async members");
{
    static Task<Result<int>> Load(int value) => Task.FromResult(value > 0 ? Result.Success(value) : Result.Failure<int>(Error.NotFound("Missing", "Missing.")));

    Console.WriteLine($"ElseAsync: {(await Load(0).ElseAsync(-1)).Value}");
    Console.WriteLine($"CompensateAsync: {(await Load(0).CompensateAsync(errors => Load(3))).Value}");
    Console.WriteLine($"FailIfAsync: {(await Load(500).FailIfAsync(v => v > 100, Error.Validation("Big", "Too big."))).FirstError.Code}");
    Console.WriteLine($"ThenAsync: {(await Load(2).ThenAsync(v => v * 10)).Value}");
    Console.WriteLine($"ThenEnsureAsync: {(await Load(2).ThenEnsureAsync(v => Task.FromResult(Result.Success()))).Value}");
    Console.WriteLine($"MapErrorAsync: {(await Load(0).MapErrorAsync(e => Error.Failure($"Svc.{e.Code}", e.Description))).FirstError.Code}");
    Console.WriteLine($"MatchFirstAsync: {await Load(0).MatchFirstAsync(v => "ok", e => e.Code)}");
    Console.WriteLine($"GetValueOrDefaultAsync: {await Load(0).GetValueOrDefaultAsync(42)}");
    await Load(0).TapErrorAsync(errors => { Console.WriteLine($"TapErrorAsync: {errors[0].Code}"); return Task.CompletedTask; });
    await Load(5).SwitchAsync(v => Console.WriteLine($"SwitchAsync: {v}"), errors => Console.WriteLine("failed"));
    string final = await Load(1).FinallyAsync(r => r.IsSuccess ? "finally ok" : "finally failed");
    Console.WriteLine(final);
}

Section("Typed results: Result<T, E1, E2>");
{
    foreach (int id in new[] { 1, 2, 3, 4 })
    {
        string response = Users.Get(id) switch
        {
            UserDto user => $"200 {user.Name}",
            NotFoundError { Code: UserErrors.Codes.Deleted } => "410 deleted",
            NotFoundError => "404 not found",
            UserErrors.Locked locked => $"409 user {locked.Id} is locked",
        };
        Console.WriteLine(response);
    }

    int status = Users.Get(4).Match(onSuccess: _ => 200, onError1: _ => 404, onError2: _ => 409);
    Console.WriteLine($"Match: {status}");
    Console.WriteLine($"ToResult: {Users.Get(4).ToResult().FirstError.Type}");

    Result<UserDto, NotFoundError, ConflictError> widened = Users.Find(1).BindWiden(Users.Lock);
    Result<UserDto, ConflictError> recovered = Users.Find(9).BindWiden(Users.Lock).Recover((NotFoundError _) => new UserDto(0, "guest"));
    Console.WriteLine($"BindWiden: {widened.IsSuccess}, Recover: {recovered.GetValueOrThrow().Name}");

    Result<int, NotFoundError, FormatException> parsed = Users.Find(1).TryCatch<int, FormatException>(u => int.Parse(u.Name));
    parsed.TapError((FormatException ex) => Console.WriteLine($"  TryCatch caught {ex.GetType().Name}"));

    Result<UserDto[], NotFoundError> traversed = new[] { 1, 9 }.Traverse(Users.Find);
    Console.WriteLine($"Traverse stops at the first error: {traversed.ToResult().FirstError.Code}");

    Result<UserDto, NotFoundError> notFound = Users.Find(9);
    Console.WriteLine($"GetValueOr: {notFound.GetValueOr(e => new UserDto(0, e.Code)).Name}, HasValue: {notFound.HasValue}");
    UserDto viaAsync = await Task.FromResult(notFound).GetValueOrAsync(new UserDto(0, "fallback"));
    Result<UserDto, NotFoundError, ConflictError> bound = await Task.FromResult(Users.Find(1)).BindWidenAsync(u => Task.FromResult(Users.Lock(u)));
    Console.WriteLine($"GetValueOrAsync: {viaAsync.Name}, BindWidenAsync: {bound.IsSuccess}");
}

Section("Typed result JSON");
{
    TypedResultJsonConverterFactory.Register<UserDto, ConflictError>();
    Result<UserDto, NotFoundError> found = new UserDto(3, "Ada");
    Result<UserDto, NotFoundError> missing = UserErrors.NotFound;
    Console.WriteLine(JsonSerializer.Serialize(found, AppJsonContext.Default.Options));
    string json = JsonSerializer.Serialize(missing, AppJsonContext.Default.Options);
    Console.WriteLine(json);
    var back = JsonSerializer.Deserialize<Result<UserDto, NotFoundError>>(json, AppJsonContext.Default.Options);
    Console.WriteLine($"round trip: {(back.TryGetValue(out NotFoundError? error) ? error.Code : "?")}");
}

static void Section(string title) => Console.WriteLine($"{Environment.NewLine}--- {title} ---");

static Result<int> ParseNumber(string input)
{
    if (int.TryParse(input, out int number))
        return number;
    return Error.Validation("Parse", $"'{input}' is not a number.");
}

static async Task<Result<Order>> LoadOrderAsync(int id)
{
    await Task.Yield();
    if (id != 1)
        return Error.NotFound("Order.NotFound", $"Order {id} not found.");
    return new Order(id, 3);
}

public sealed record Order(int Id, int Lines);

public sealed record UserDto(int Id, string Name);

public static class UserErrors
{
    public static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

    public sealed record Locked(int Id) : ConflictError(Codes.Locked, "User is locked.");
}

public static class Users
{
    public static Result<UserDto, NotFoundError, UserErrors.Locked> Get(int id) => id switch
    {
        1 => new UserDto(1, "Ada"),
        2 => UserErrors.Deleted,
        3 => UserErrors.NotFound,
        _ => new UserErrors.Locked(id),
    };

    public static Result<UserDto, NotFoundError> Find(int id) =>
        id == 1 ? new UserDto(1, "Ada") : UserErrors.NotFound;

    public static Result<UserDto, ConflictError> Lock(UserDto user) => user;
}

[JsonSerializable(typeof(Result<UserDto, NotFoundError>))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(NotFoundError))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
