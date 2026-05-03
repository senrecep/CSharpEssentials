using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Results Example");
Console.WriteLine("========================================\n");

// ============================================================================
// BASIC RESULT CREATION
// ============================================================================
Console.WriteLine("--- Basic Result Creation ---");

Result success = Result.Success();
Console.WriteLine($"Success: IsSuccess={success.IsSuccess}, IsFailure={success.IsFailure}");

Result failure = Result.Failure(Error.Validation("Code", "Something went wrong"));
Console.WriteLine($"Failure: IsSuccess={failure.IsSuccess}, Errors={failure.Errors.Length}");

Result<int> intSuccess = Result.Success(42);
Console.WriteLine($"Int Success: Value={intSuccess.Value}");

Result<int> intFailure = Result.Failure<int>(Error.NotFound("Item not found"));
Console.WriteLine($"Int Failure: IsFailure={intFailure.IsFailure}");
Console.WriteLine();

// ============================================================================
// RESULT CHAINING WITH THEN
// ============================================================================
Console.WriteLine("--- Result Chaining (Then) ---");

Result<int> ParseNumber(string input)
{
    if (int.TryParse(input, out int number))
        return Result.Success(number);
    return Result.Failure<int>(Error.Validation("Parse", $"'{input}' is not a valid number"));
}

Result<int> Double(int value) => Result.Success(value * 2);
Result<int> AddTen(int value) => Result.Success(value + 10);

Result<int> chained = ParseNumber("5")
    .Then(Double)
    .Then(AddTen);

string chainedOutput = chained.Match(
    onSuccess: value => $"Chained result: {value}",
    onError: errors => $"Chained error: {errors[0].Description}"
);
Console.WriteLine(chainedOutput);

Result<int> chainedFail = ParseNumber("abc")
    .Then(Double)
    .Then(AddTen);

string chainedFailOutput = chainedFail.Match(
    onSuccess: value => $"Chained result: {value}",
    onError: errors => $"Chained error (short-circuited): {errors[0].Description}"
);
Console.WriteLine(chainedFailOutput);
Console.WriteLine();

// ============================================================================
// MAP (TRANSFORM VALUE)
// ============================================================================
Console.WriteLine("--- Map ---");

Result<int> parsed = ParseNumber("20");
Result<string> mapped = parsed.Map(v => $"Number is {v}");
mapped.Switch(
    onSuccess: s => Console.WriteLine($"Mapped: {s}"),
    onError: e => Console.WriteLine($"Map error: {e[0].Description}")
);

Result<int> emptyMap = ParseNumber("xyz");
Result<string> emptyMapped = emptyMap.Map(v => $"Number is {v}");
emptyMapped.Switch(
    onSuccess: s => Console.WriteLine($"Mapped: {s}"),
    onError: e => Console.WriteLine($"Map error (short-circuited): {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// MATCH AND SWITCH
// ============================================================================
Console.WriteLine("--- Match and Switch ---");

Result<string> GetUserName(int id)
{
    if (id <= 0)
        return Result.Failure<string>(Error.Validation("Id", "Id must be positive"));
    return Result.Success($"User_{id}");
}

string result = GetUserName(10).Match(
    onSuccess: name => $"Found: {name}",
    onError: errors => $"Error: {errors[0].Description}"
);
Console.WriteLine(result);

string failResult = GetUserName(-1).Match(
    onSuccess: name => $"Found: {name}",
    onError: errors => $"Error: {errors[0].Description}"
);
Console.WriteLine(failResult);

// Switch (for side effects)
GetUserName(10).Switch(
    onSuccess: name => Console.WriteLine($"Switch: user found - {name}"),
    onError: errors => Console.WriteLine($"Switch: {errors[0].Description}")
);

// MatchFirst / MatchLast
Result<int> multiError = Result.Failure<int>(Error.Validation("A", "First error"), Error.Validation("B", "Second error"));
string firstErrorMsg = multiError.MatchFirst(
    onSuccess: v => $"Success: {v}",
    onFirstError: e => $"First error only: {e.Description}"
);
Console.WriteLine(firstErrorMsg);
Console.WriteLine();

// ============================================================================
// ELSE (FALLBACK)
// ============================================================================
Console.WriteLine("--- Else (Fallback) ---");

Result<int> fallback = ParseNumber("xyz")
    .Else(errors => 0);

Console.WriteLine($"Fallback result: {fallback.Value}");

Result<int> fallbackWithError = ParseNumber("xyz")
    .Else(errors => Error.Validation("Fallback", "Using fallback"));
fallbackWithError.Switch(
    onSuccess: v => Console.WriteLine($"Else success: {v}"),
    onError: e => Console.WriteLine($"Else still failed: {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// BIND (FLATTEN NESTED RESULTS)
// ============================================================================
Console.WriteLine("--- Bind ---");

Result<int> parsedForBind = ParseNumber("100");
Result<int> bound = parsedForBind.Bind<int>(v => Divide(v, 4));

string bindOutput = bound.Match(
    onSuccess: v => $"100 / 4 = {v}",
    onError: e => $"Error: {e[0].Description}"
);
Console.WriteLine(bindOutput);

Result<int> badBind = ParseNumber("100").Bind<int>(v => Divide(v, 0));
string badBindOutput = badBind.Match(
    onSuccess: v => $"100 / 0 = {v}",
    onError: e => $"Divide error: {e[0].Description}"
);
Console.WriteLine(badBindOutput);
Console.WriteLine();

// ============================================================================
// TAP (SIDE EFFECTS)
// ============================================================================
Console.WriteLine("--- Tap ---");

Result<int> tapValue = Result.Success(42);
tapValue.Tap(() => Console.WriteLine("  Tap executed on success"));
tapValue.Tap(v => Console.WriteLine($"  Tap with value: {v}"));

Result<int> tapFailure = Result.Failure<int>(Error.NotFound("X", "Not found"));
tapFailure.Tap(() => Console.WriteLine("  This tap won't execute"));
tapFailure.Tap(v => Console.WriteLine($"  This tap also won't execute: {v}"));
Console.WriteLine();

// ============================================================================
// TRY CATCH
// ============================================================================
Console.WriteLine("--- TryCatch ---");

Result<int> tryCatchSuccess = Result.Success(10).TryCatch(v => Result.Success(v / 2));
tryCatchSuccess.Switch(
    onSuccess: v => Console.WriteLine($"TryCatch success: {v}"),
    onError: e => Console.WriteLine($"TryCatch error: {e[0].Description}")
);

Result tryCatchFail = Result.Success().TryCatch(() => throw new InvalidOperationException("Boom!"));
tryCatchFail.Switch(
    onSuccess: () => Console.WriteLine("TryCatch success"),
    onFailure: e => Console.WriteLine($"TryCatch caught: {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// FINALLY
// ============================================================================
Console.WriteLine("--- Finally ---");

string finallyResult = Result.Success(99).Finally(r => r.IsSuccess ? $"Finally: {r.Value}" : $"Finally error: {r.Errors[0].Description}");
Console.WriteLine(finallyResult);

string finallyFail = Result.Failure<int>(Error.NotFound("X", "Missing")).Finally(r => r.IsSuccess ? $"Finally: {r.Value}" : $"Finally error: {r.Errors[0].Description}");
Console.WriteLine(finallyFail);
Console.WriteLine();

// ============================================================================
// FAIL IF
// ============================================================================
Console.WriteLine("--- FailIf ---");

Result<int> failIfResult = Result.Success(150).FailIf(v => v > 100, Error.Validation("Range", "Value must be <= 100"));
failIfResult.Switch(
    onSuccess: v => Console.WriteLine($"FailIf success: {v}"),
    onError: e => Console.WriteLine($"FailIf triggered: {e[0].Description}")
);

Result<int> failIfOk = Result.Success(50).FailIf(v => v > 100, Error.Validation("Range", "Value must be <= 100"));
failIfOk.Switch(
    onSuccess: v => Console.WriteLine($"FailIf passed: {v}"),
    onError: e => Console.WriteLine($"FailIf error: {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// AND / OR
// ============================================================================
Console.WriteLine("--- And / Or ---");

Result r1 = Result.Success();
Result r2 = Result.Success();
Result r3 = Result.Failure(Error.Validation("X", "Error"));

Result andResult = Result.And(r1, r2);
andResult.Switch(
    onSuccess: () => Console.WriteLine("And: all success"),
    onFailure: e => Console.WriteLine($"And failed: {e[0].Description}")
);

Result andFail = Result.And(r1, r3);
andFail.Switch(
    onSuccess: () => Console.WriteLine("And: all success"),
    onFailure: e => Console.WriteLine($"And failed: {e.Length} error(s)")
);

Result orResult = Result.Or(r1, r3);
orResult.Switch(
    onSuccess: () => Console.WriteLine("Or: at least one success"),
    onFailure: e => Console.WriteLine($"Or failed: {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// GET VALUE / FROM
// ============================================================================
Console.WriteLine("--- GetValue / From ---");

Result<int> fromResult = Result<int>.From(Error.Validation("X", "Bad"));
fromResult.Switch(
    onSuccess: v => Console.WriteLine($"From success: {v}"),
    onError: e => Console.WriteLine($"From failure: {e[0].Description}")
);

Result<int> getValue = Result.Success(77);
Console.WriteLine($"GetValueOrDefault: {getValue.GetValueOrDefault()}");
Console.WriteLine($"GetValueOrDefault(0): {getValue.GetValueOrDefault(0)}");

try
{
    Result<int> noValue = Result.Failure<int>(Error.NotFound("X", "Missing"));
    _ = noValue.GetValueOrThrow("Value is required!");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"GetValueOrThrow: {ex.Message}");
}
Console.WriteLine();

// ============================================================================
// SELECT (LINQ)
// ============================================================================
Console.WriteLine("--- Select (LINQ) ---");

Result<int> linqValue = Result.Success(5);
Result<int> linqResult = from v in linqValue
                         select v * 3;
linqResult.Switch(
    onSuccess: v => Console.WriteLine($"Select result: {v}"),
    onError: e => Console.WriteLine($"Select error: {e[0].Description}")
);
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

static Result<int> Divide(int a, int b)
{
    if (b == 0)
        return Result.Failure<int>(Error.Validation("Divide", "Cannot divide by zero"));
    return Result.Success(a / b);
}
