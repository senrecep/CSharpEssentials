using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;
using Examples.Main;
using System.Text.Json;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials (Main Package) Example");
Console.WriteLine("========================================\n");

// ============================================================================
// CROSS-PACKET INTEGRATION DEMO
// ============================================================================
Console.WriteLine("--- Cross-Package Integration ---");

string input = "  Hello World  ";
string sanitized = input.Trim().ToPascalCase();
Console.WriteLine($"Sanitized input: '{sanitized}'");

Maybe<User> user = Services.FindUserByEmail("alice@example.com");

Result<Order> result = user.Match(
    some: u => Services.PlaceOrder(u, 99.99m),
    none: () => Result.Failure<Order>(Error.NotFound("User.NotFound", "User not found"))
);

result.Switch(
    onSuccess: order => Console.WriteLine($"Order placed: #{order.OrderNumber} for ${order.Total}"),
    onError: errors => Console.WriteLine($"Failed: {errors[0].Description}")
);
Console.WriteLine();

// ============================================================================
// DATE TIME PROVIDER INTEGRATION
// ============================================================================
Console.WriteLine("--- DateTimeProvider Integration ---");

IDateTimeProvider time = new DateTimeProvider(TimeProvider.System);
DateTime now = time.UtcNowDateTime;
Console.WriteLine($"Current UTC time: {now:O}");
Console.WriteLine();

// ============================================================================
// GUID GENERATION
// ============================================================================
Console.WriteLine("--- Guid Generation ---");

Guid id = Guider.NewGuid();
Console.WriteLine($"New Guid: {id}");
Console.WriteLine($"URL-safe: {Guider.ToStringFromGuid(id)}");
Console.WriteLine();

// ============================================================================
// META PACKAGE - STRING EXTENSIONS (TrimStart / TrimEnd)
// ============================================================================
Console.WriteLine("--- Meta String Extensions ---");

string prefixText = "HelloWorld";
Result<string> trimmedStart = prefixText.TrimStart("Hello");
trimmedStart.Switch(
    onSuccess: s => Console.WriteLine($"TrimStart 'Hello' from '{prefixText}': '{s}'"),
    onError: e => Console.WriteLine($"TrimStart error: {e[0].Description}")
);

string suffixText = "HelloWorld";
Result<string> trimmedEnd = suffixText.TrimEnd("World");
trimmedEnd.Switch(
    onSuccess: s => Console.WriteLine($"TrimEnd 'World' from '{suffixText}': '{s}'"),
    onError: e => Console.WriteLine($"TrimEnd error: {e[0].Description}")
);

Result<string> trimFail = "Hello".TrimStart("World");
trimFail.Switch(
    onSuccess: s => Console.WriteLine($"TrimStart result: '{s}'"),
    onError: e => Console.WriteLine($"TrimStart failed (expected): {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// META PACKAGE - JSON EXTENSIONS (TryGetProperty / TryGetNestedProperty)
// ============================================================================
Console.WriteLine("--- Meta Json Extensions ---");

string jsonString = """
{
    "user": {
        "name": "Alice",
        "email": "alice@example.com"
    }
}
""";

using JsonDocument jsonDoc = JsonDocument.Parse(jsonString);

var nameResult = jsonDoc.RootElement.TryGetProperty("user", "name");
nameResult.Switch(
    onSuccess: el => Console.WriteLine($"TryGetProperty 'user.name': {el}"),
    onError: e => Console.WriteLine($"TryGetProperty error: {e[0].Description}")
);

var nestedResult = jsonDoc.TryGetNestedProperty("user", "name");
nestedResult.Switch(
    onSuccess: el => Console.WriteLine($"TryGetNestedProperty 'user.name': {el}"),
    onError: e => Console.WriteLine($"TryGetNestedProperty error: {e[0].Description}")
);

var missingProp = jsonDoc.RootElement.TryGetProperty("missing");
missingProp.Switch(
    onSuccess: el => Console.WriteLine($"Found: {el}"),
    onError: e => Console.WriteLine($"Missing property (expected): {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// META PACKAGE - TIME EXTENSIONS (MsToDateTime)
// ============================================================================
Console.WriteLine("--- Meta Time Extensions ---");

long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
Maybe<DateTime> maybeDate = timestamp.MsToDateTime();
maybeDate.Match(
    some: dt => Console.WriteLine($"MsToDateTime: {dt:O}"),
    none: () => Console.WriteLine("MsToDateTime returned None")
);

long? nullTimestamp = null;
Maybe<DateTime> nullDate = nullTimestamp.MsToDateTime();
nullDate.Match(
    some: dt => Console.WriteLine($"Null MsToDateTime: {dt:O}"),
    none: () => Console.WriteLine("Null timestamp returned None (expected)")
);
Console.WriteLine();

// ============================================================================
// META PACKAGE - MAYBE TO RESULT (ToMaybeResult)
// ============================================================================
Console.WriteLine("--- Meta Maybe to Result ---");

Maybe<User> existingUser = Services.FindUserByEmail("alice@example.com");
Result<User> userResult = existingUser.ToMaybeResult(Error.NotFound("User.NotFound", "User not found"));
userResult.Switch(
    onSuccess: u => Console.WriteLine($"ToMaybeResult success: {u.Name}"),
    onError: e => Console.WriteLine($"ToMaybeResult error: {e[0].Description}")
);

Maybe<User> missingUser = Services.FindUserByEmail("unknown@example.com");
Result<User> missingResult = missingUser.ToMaybeResult(Error.NotFound("User.NotFound", "User not found"));
missingResult.Switch(
    onSuccess: u => Console.WriteLine($"ToMaybeResult success: {u.Name}"),
    onError: e => Console.WriteLine($"ToMaybeResult error (expected): {e[0].Description}")
);

// ToMaybeUnitResult
Result unitResult = existingUser.ToMaybeUnitResult(Error.NotFound("User.NotFound", "User not found"));
unitResult.Switch(
    onSuccess: () => Console.WriteLine("ToMaybeUnitResult: success (no value)"),
    onFailure: e => Console.WriteLine($"ToMaybeUnitResult error: {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// ERROR HANDLING PIPELINE
// ============================================================================
Console.WriteLine("--- Error Handling Pipeline ---");

Result<int> ParseAndValidate(string value)
{
    if (!int.TryParse(value, out int number))
        return Result.Failure<int>(Error.Validation("Input.Numeric", "Input must be numeric"));

    if (number <= 0)
        return Result.Failure<int>(Error.Validation("Input.Positive", "Input must be positive"));

    return Result.Success(number);
}

ParseAndValidate("42").Switch(
    onSuccess: n => Console.WriteLine($"Valid number: {n}"),
    onError: e => Console.WriteLine($"Invalid: {e[0].Description}")
);

ParseAndValidate("abc").Switch(
    onSuccess: n => Console.WriteLine($"Valid number: {n}"),
    onError: e => Console.WriteLine($"Invalid: {e[0].Description}")
);
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
