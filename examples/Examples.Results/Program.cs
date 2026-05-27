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
// ENSURE / ENSURE NOT NULL
// ============================================================================
Console.WriteLine("--- Ensure / EnsureNotNull ---");

Result<int> ensured = Result.Success(50)
    .Ensure(v => v > 0, Error.Validation("Range", "Must be positive"))
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100"));
Console.WriteLine($"Ensure passed: {ensured.Value}");

Result<int> ensureFailed = Result.Success(150)
    .Ensure(v => v < 100, Error.Validation("Range", "Must be less than 100"));
Console.WriteLine($"Ensure failed: {ensureFailed.FirstError.Description}");

Result<string> ensureNotNull = Result.Success("hello")
    .EnsureNotNull(Error.Validation("Null", "Value cannot be null"));
Console.WriteLine($"EnsureNotNull: {ensureNotNull.Value}");
Console.WriteLine();

// ============================================================================
// MAP ERROR
// ============================================================================
Console.WriteLine("--- MapError ---");

Result<int> originalError = Result.Failure<int>(Error.Validation("Old", "Original error"));
Result<int> mappedError = originalError.MapError(e => Error.NotFound("New", $"Mapped: {e.Description}"));
Console.WriteLine($"MapError: {mappedError.FirstError.Code} - {mappedError.FirstError.Description}");

Result unitMapError = Result.Failure(Error.Validation("X", "Bad input"))
    .MapError(errors => errors.Select(e => Error.Conflict(e.Code, $"Upgraded: {e.Description}")).ToArray());
Console.WriteLine($"MapError array: {unitMapError.FirstError.Code}");
Console.WriteLine();

// ============================================================================
// TRY / TRY ASYNC
// ============================================================================
Console.WriteLine("--- Try / TryAsync ---");

Result<int> trySuccess = Result.Try(() => 10 * 5, ex => Error.Exception(ex));
Console.WriteLine($"Try success: {trySuccess.Value}");

Result<int> tryFail = Result.Try<int>(() => throw new InvalidOperationException("Boom!"), ex => Error.Exception(ex));
Console.WriteLine($"Try failure: {tryFail.FirstError.Description}");

Result<int> tryAsyncSuccess = Result.TryAsync(async () =>
{
    await Task.Delay(1);
    return 99;
}, ex => Error.Exception(ex)).Result;
Console.WriteLine($"TryAsync success: {tryAsyncSuccess.Value}");
Console.WriteLine();

// ============================================================================
// ELSE DO
// ============================================================================
Console.WriteLine("--- ElseDo ---");

Result.Failure(Error.NotFound("User", "User not found"))
    .ElseDo((Error[] errors) => Console.WriteLine($"  ElseDo (all): {errors.Length} error(s)"));

Result.Failure(Error.Validation("Input", "Invalid"))
    .ElseDoFirst(first => Console.WriteLine($"  ElseDo (first): {first.Description}"));

Result.Success()
    .ElseDo((Error[] _) => Console.WriteLine("  This won't print"));
Console.WriteLine();

// ============================================================================
// TAP ERROR / TAP ERROR IF
// ============================================================================
Console.WriteLine("--- TapError / TapErrorIf ---");

Result<int> tapErr = Result.Failure<int>(Error.NotFound("X", "Missing"))
    .TapErrorFirst(first => Console.WriteLine($"  TapError: {first.Description}"));

Result<int> tapErrIf = Result.Failure<int>(Error.Validation("Y", "Bad"))
    .TapErrorIf(true, (Error[] errors) => Console.WriteLine($"  TapErrorIf: {errors[0].Description}"));

Result.Success(42)
    .TapErrorFirst(_ => Console.WriteLine("  This won't print"));
Console.WriteLine();

// ============================================================================
// DECONSTRUCT
// ============================================================================
Console.WriteLine("--- Deconstruct ---");

Result<int> deconstructSuccess = Result.Success(123);
(bool isSuccess, int value, Error[] errors) = deconstructSuccess;
Console.WriteLine($"Deconstruct success: isSuccess={isSuccess}, value={value}, errors={errors.Length}");

Result<int> deconstructFail = Result.Failure<int>(Error.NotFound("X", "Missing"));
(bool isFail, int failValue, Error[] failErrors) = deconstructFail;
Console.WriteLine($"Deconstruct failure: isSuccess={isFail}, value={failValue}, errors={failErrors.Length}");
Console.WriteLine();

// ============================================================================
// SUCCESS IF / FAILURE IF
// ============================================================================
Console.WriteLine("--- SuccessIf / FailureIf ---");

Result successIf = Result.SuccessIf(true, Error.Validation("X", "Should not happen"));
Console.WriteLine($"SuccessIf(true): {successIf.IsSuccess}");

Result successIfFail = Result.SuccessIf(false, Error.Validation("X", "Condition was false"));
Console.WriteLine($"SuccessIf(false): {successIfFail.FirstError.Description}");

Result failureIf = Result.FailureIf(true, Error.Validation("X", "Condition was true"));
Console.WriteLine($"FailureIf(true): {failureIf.FirstError.Description}");

Result failureIfOk = Result.FailureIf(false, Error.Validation("X", "Should not happen"));
Console.WriteLine($"FailureIf(false): {failureIfOk.IsSuccess}");
Console.WriteLine();

// ============================================================================
// COMPENSATE
// ============================================================================
Console.WriteLine("--- Compensate ---");

Result<int> compensated = Result.Failure<int>(Error.NotFound("X", "Not found"))
    .Compensate((Error[] errors) => Result.Success(42));
Console.WriteLine($"Compensated to success: {compensated.Value}");

Result<int> stillFailed = Result.Failure<int>(Error.Validation("X", "Bad"))
    .CompensateFirst(firstError => Result.Failure<int>(Error.Conflict("Y", $"Compensated: {firstError.Description}")));
Console.WriteLine($"Compensated to failure: {stillFailed.FirstError.Description}");

Result<int> noCompensate = Result.Success(10)
    .Compensate((Error[] _) => Result.Success(99));
Console.WriteLine($"No compensate (was success): {noCompensate.Value}");
Console.WriteLine();

// ============================================================================
// THEN ENSURE
// ============================================================================
Console.WriteLine("--- ThenEnsure ---");

Result<int> thenEnsureOk = Result.Success(5)
    .ThenEnsure(v => v > 0 ? Result.Success(v) : Result.Failure<int>(Error.Validation("X", "Must be positive")));
Console.WriteLine($"ThenEnsure ok: {thenEnsureOk.Value}");

Result<int> thenEnsureFail = Result.Success(-3)
    .ThenEnsure(v => v > 0 ? Result.Success(v) : Result.Failure<int>(Error.Validation("X", "Must be positive")));
Console.WriteLine($"ThenEnsure fail: {thenEnsureFail.FirstError.Description}");

Result<int> thenEnsureUnit = Result.Success(8)
    .ThenEnsure(v => v % 2 == 0 ? Result.Success() : Result.Failure(Error.Validation("X", "Must be even")));
Console.WriteLine($"ThenEnsure unit: {thenEnsureUnit.Value}");
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
Console.WriteLine();

// ============================================================================
// MAP (TRANSFORM VALUE)
// ============================================================================
Console.WriteLine("--- Map ---");

Result<int> doubled = Result.Success(21).Map(v => v * 2);
Console.WriteLine($"Map: {doubled.Value}");

Result<string> asString = Result.Success(42).Map(v => $"Value is {v}");
Console.WriteLine($"Map to string: {asString.Value}");

Result<string> mapFailed = Result.Failure<int>(Error.Validation("X", "Bad")).Map(v => $"Value is {v}");
Console.WriteLine($"Map on failure propagates: {mapFailed.FirstError.Code}");
Console.WriteLine();

// ============================================================================
// BIND (CHAIN WITH DIFFERENT RETURN TYPE)
// ============================================================================
Console.WriteLine("--- Bind ---");

Result<int> parsed = Result.Success("123")
    .Bind(s => int.TryParse(s, out int n) ? Result.Success(n) : Result.Failure<int>(Error.Validation("Parse", "Not a number")));
Console.WriteLine($"Bind parse: {parsed.Value}");

Result bindUnit = Result.Success("alice")
    .Bind(s => s.Length > 0 ? Result.Success() : Result.Failure(Error.Validation("Name", "Empty")));
Console.WriteLine($"Bind to unit: {bindUnit.IsSuccess}");
Console.WriteLine();

// ============================================================================
// TAP (SIDE EFFECT ON SUCCESS)
// ============================================================================
Console.WriteLine("--- Tap ---");

Result.Success(10)
    .Tap(v => Console.WriteLine($"  Tap value: {v}"))
    .Tap(() => Console.WriteLine("  Tap no-arg"))
    .Tap(true, v => Console.WriteLine($"  Tap conditional true: {v}"))
    .Tap(false, v => Console.WriteLine("  This won't print"))
    .Tap(() => 5 > 3, v => Console.WriteLine($"  Tap predicate: {v}"));

Result.Failure<int>(Error.NotFound("X", "Missing"))
    .Tap(v => Console.WriteLine("  This won't print (failure)"));
Console.WriteLine();

// ============================================================================
// ELSE (FALLBACK ON FAILURE)
// ============================================================================
Console.WriteLine("--- Else ---");

Result<int> elseValue = Result.Failure<int>(Error.NotFound("X", "Missing")).Else(42);
Console.WriteLine($"Else with value: {elseValue.Value}");

Result<int> elseFromErrors = Result.Failure<int>(Error.Validation("X", "Bad")).Else(errors => -1);
Console.WriteLine($"Else from errors: {elseFromErrors.Value}");

Result<int> elseReplaceError = Result.Failure<int>(Error.Validation("Old", "Old error"))
    .Else(Error.Conflict("New", "Replaced error"));
Console.WriteLine($"Else replace error: {elseReplaceError.FirstError.Code}");

Result<int> elseNoChange = Result.Success(99).Else(0);
Console.WriteLine($"Else on success (no change): {elseNoChange.Value}");
Console.WriteLine();

// ============================================================================
// FINALLY (ALWAYS EXECUTES)
// ============================================================================
Console.WriteLine("--- Finally ---");

string finallySuccess = Result.Success(7).Finally(r => r.IsSuccess ? $"Ok: {r.Value}" : "Fail");
Console.WriteLine($"Finally success: {finallySuccess}");

string finallyFail = Result.Failure<int>(Error.NotFound("X", "Missing"))
    .Finally(r => r.IsSuccess ? "Ok" : $"Fail: {r.FirstError.Code}");
Console.WriteLine($"Finally failure: {finallyFail}");
Console.WriteLine();

// ============================================================================
// GET VALUE OR DEFAULT / GET VALUE OR THROW
// ============================================================================
Console.WriteLine("--- GetValueOrDefault / GetValueOrThrow ---");

int defaulted = Result.Failure<int>(Error.NotFound("X", "Missing")).GetValueOrDefault(-1);
Console.WriteLine($"GetValueOrDefault: {defaulted}");

int fromFactory = Result.Failure<int>(Error.NotFound("X", "Missing")).GetValueOrDefault(() => 99);
Console.WriteLine($"GetValueOrDefault factory: {fromFactory}");

int found = Result.Success(42).GetValueOrDefault(-1);
Console.WriteLine($"GetValueOrDefault success: {found}");

try
{
    Result.Failure<int>(Error.NotFound("X", "Missing")).GetValueOrThrow("Custom message");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"GetValueOrThrow threw: {ex.Message}");
}
Console.WriteLine();

// ============================================================================
// UNWRAP / UNWRAP OR DEFAULT
// ============================================================================
Console.WriteLine("--- Unwrap / UnwrapOrDefault ---");

int unwrapped = Result.Success(55).Unwrap();
Console.WriteLine($"Unwrap success: {unwrapped}");

int unwrapOr = Result.Failure<int>(Error.NotFound("X", "Missing")).UnwrapOrDefault(-999);
Console.WriteLine($"UnwrapOrDefault: {unwrapOr}");

try
{
    Result.Failure<int>(Error.Validation("X", "Bad")).Unwrap();
}
catch (ResultUnwrapException ex)
{
    Console.WriteLine($"Unwrap threw ResultUnwrapException: {ex.Errors.Length} error(s)");
}
Console.WriteLine();

// ============================================================================
// RECOVER / RECOVER FIRST
// ============================================================================
Console.WriteLine("--- Recover / RecoverFirst ---");

Result<int> recovered = Result.Failure<int>(Error.NotFound("Cache.Miss", "Not in cache"))
    .Recover(ErrorType.NotFound, _ => Result.Success(0));
Console.WriteLine($"Recover NotFound: {recovered.Value}");

Result<int> notRecovered = Result.Failure<int>(Error.Validation("X", "Bad"))
    .Recover(ErrorType.NotFound, _ => Result.Success(0));
Console.WriteLine($"Recover (wrong type, unchanged): {notRecovered.IsFailure}");

Result<int> recoveredFirst = Result.Failure<int>(Error.Unauthorized("Auth", "No token"))
    .RecoverFirst(ErrorType.Unauthorized, _ => Result.Success(-1));
Console.WriteLine($"RecoverFirst Unauthorized: {recoveredFirst.Value}");
Console.WriteLine();

// ============================================================================
// BINDIF (CONDITIONAL BIND)
// ============================================================================
Console.WriteLine("--- BindIf ---");

bool isAdmin = true;
Result<string> role = Result.Success("alice")
    .BindIf(isAdmin, name => Result.Success($"{name}:admin"))
    .BindIf(!isAdmin, name => Result.Success($"{name}:guest"));
Console.WriteLine($"BindIf (bool): {role.Value}");

Result<int> bindIfPredicate = Result.Success(10)
    .BindIf(v => v > 5, v => Result.Success(v * 2));
Console.WriteLine($"BindIf predicate: {bindIfPredicate.Value}");
Console.WriteLine();

// ============================================================================
// FAILIF (FAIL WHEN CONDITION IS MET)
// ============================================================================
Console.WriteLine("--- FailIf ---");

Result<int> failIfNeg = Result.Success(-5)
    .FailIf(v => v < 0, Error.Validation("Range", "Must be non-negative"));
Console.WriteLine($"FailIf (met): {failIfNeg.FirstError.Code}");

Result<int> failIfOk = Result.Success(5)
    .FailIf(v => v < 0, Error.Validation("Range", "Must be non-negative"));
Console.WriteLine($"FailIf (not met): {failIfOk.Value}");
Console.WriteLine();

// ============================================================================
// TRYCATCH (EXCEPTION-SAFE BIND)
// ============================================================================
Console.WriteLine("--- TryCatch ---");

Result<int> tryCatchOk = Result.Success("42")
    .TryCatch(s => Result.Success(int.Parse(s)));
Console.WriteLine($"TryCatch success: {tryCatchOk.Value}");

Result<int> tryCatchFail = Result.Success("not-a-number")
    .TryCatch(s => Result.Success(int.Parse(s)));
Console.WriteLine($"TryCatch caught exception: IsFailure={tryCatchFail.IsFailure}");
Console.WriteLine();

// ============================================================================
// COLLECTION: SEQUENCE / TRAVERSE / PARTITION / FIRSTFAILUREORSUCCESSES
// ============================================================================
Console.WriteLine("--- Collection Extensions ---");

List<Result<int>> allOkResults = new()
{
    Result.Success(1), Result.Success(2), Result.Success(3)
};
Result<int[]> sequenced = allOkResults.Sequence();
Console.WriteLine($"Sequence (all ok): [{string.Join(", ", sequenced.Value)}]");

List<Result<int>> withFailure = new()
{
    Result.Success(1),
    Result.Failure<int>(Error.Validation("X", "Bad")),
    Result.Success(3)
};
Result<int[]> sequencedFail = withFailure.Sequence();
Console.WriteLine($"Sequence (has failure): IsFailure={sequencedFail.IsFailure}, errors={sequencedFail.Errors.Length}");

Result<int[]> traversed = new[] { "1", "2", "3" }
    .Traverse(s => int.TryParse(s, out int n) ? Result.Success(n) : Result.Failure<int>(Error.Validation("Parse", "Bad")));
Console.WriteLine($"Traverse: [{string.Join(", ", traversed.Value)}]");

(int[] successes, Error[] errs) = withFailure.Partition();
Console.WriteLine($"Partition: {successes.Length} successes, {errs.Length} errors");

List<Result> mixedResults = new()
{
    Result.Success(), Result.Failure(Error.NotFound("X", "Missing")), Result.Success()
};
Result firstFail = mixedResults.FirstFailureOrSuccesses();
Console.WriteLine($"FirstFailureOrSuccesses: {firstFail.FirstError.Code}");

Result allOk = new List<Result> { Result.Success(), Result.Success() }.FirstFailureOrSuccesses();
Console.WriteLine($"FirstFailureOrSuccesses (all ok): {allOk.IsSuccess}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
