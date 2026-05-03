using CSharpEssentials.Any;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Any Example");
Console.WriteLine("========================================\n");

// ============================================================================
// ANY TYPE - CREATION
// ============================================================================
Console.WriteLine("--- Any Type ---");

Any<string, int> anyString = Any.Create<string, int>("hello");
Any<string, int> anyInt = Any.Create<string, int>(42);

Console.WriteLine($"Any from string: {anyString.Value} (Type: {anyString.Value?.GetType().Name})");
Console.WriteLine($"Any from int: {anyInt.Value} (Type: {anyInt.Value?.GetType().Name})");
Console.WriteLine();

// ============================================================================
// DECONSTRUCT (TUPLE SYNTAX)
// ============================================================================
Console.WriteLine("--- Deconstruct ---");

(string? sVal, int iVal) = anyString;
Console.WriteLine($"Deconstruct string: first={sVal}, second={iVal}");

(string? sVal2, int iVal2) = anyInt;
Console.WriteLine($"Deconstruct int: first={sVal2}, second={iVal2}");
Console.WriteLine();

// ============================================================================
// TO TUPLE
// ============================================================================
Console.WriteLine("--- ToTuple ---");

(string? t1, int? t2) = anyString.ToTuple();
Console.WriteLine($"ToTuple string: First={t1}, Second={t2}");

(string? t3, int? t4) = anyInt.ToTuple();
Console.WriteLine($"ToTuple int: First={t3}, Second={t4}");
Console.WriteLine();

// ============================================================================
// IS<T> / AS<T> / TRYAS<T>
// ============================================================================
Console.WriteLine("--- Is<T> / As<T> / TryAs<T> ---");

Console.WriteLine($"anyString.Is<string>(): {anyString.Is<string, string, int>()}");
Console.WriteLine($"anyString.Is<int>(): {anyString.Is<int, string, int>()}");
Console.WriteLine($"anyInt.Is<int>(): {anyInt.Is<int, string, int>()}");

string? asString = anyString.As<string, string, int>();
Console.WriteLine($"anyString.As<string>(): {asString}");

int? asInt = anyInt.As<int, string, int>();
Console.WriteLine($"anyInt.As<int>(): {asInt}");

bool trySuccess = anyString.TryAs<string, string, int>(out string? tryString);
Console.WriteLine($"anyString.TryAs<string>(): success={trySuccess}, value={tryString}");

bool tryFail = anyString.TryAs<int, string, int>(out int tryInt);
Console.WriteLine($"anyString.TryAs<int>(): success={tryFail}, value={tryInt}");
Console.WriteLine();

// ============================================================================
// TYPE CHECKING
// ============================================================================
Console.WriteLine("--- Type Checking ---");

Console.WriteLine($"anyString.IsFirst: {anyString.IsFirst}");
Console.WriteLine($"anyString.IsSecond: {anyString.IsSecond}");
Console.WriteLine($"anyInt.IsFirst: {anyInt.IsFirst}");
Console.WriteLine($"anyInt.IsSecond: {anyInt.IsSecond}");
Console.WriteLine();

// ============================================================================
// GET VALUES
// ============================================================================
Console.WriteLine("--- Get Values ---");

string stringValue = anyString.GetFirst();
Console.WriteLine($"GetFirst: {stringValue}");

int intValue = anyInt.GetSecond();
Console.WriteLine($"GetSecond: {intValue}");
Console.WriteLine();

// ============================================================================
// FIRST / SECOND FACTORY METHODS
// ============================================================================
Console.WriteLine("--- Factory Methods ---");

Any<string, int> first = Any<string, int>.First("factory-first");
Any<string, int> second = Any<string, int>.Second(999);
Console.WriteLine($"First factory: IsFirst={first.IsFirst}, Value={first.GetFirst()}");
Console.WriteLine($"Second factory: IsSecond={second.IsSecond}, Value={second.GetSecond()}");
Console.WriteLine();

// ============================================================================
// SWITCH
// ============================================================================
Console.WriteLine("--- Switch ---");

anyString.Switch(
    first: v => Console.WriteLine($"  Switch first: {v}"),
    second: v => Console.WriteLine($"  Switch second: {v}")
);

anyInt.Switch(
    first: v => Console.WriteLine($"  Switch first: {v}"),
    second: v => Console.WriteLine($"  Switch second: {v}")
);
Console.WriteLine();

// ============================================================================
// MATCH
// ============================================================================
Console.WriteLine("--- Match ---");

AnyActionResult<string> matched = anyString.Match(
    first: v => $"Matched string: {v}",
    second: v => $"Matched int: {v}"
);
Console.WriteLine($"Match result: Status={matched.Status}, Result={matched.Result}");

AnyActionResult<string> matchedInt = anyInt.Match(
    first: v => $"Matched string: {v}",
    second: v => $"Matched int: {v}"
);
Console.WriteLine($"Match result: Status={matchedInt.Status}, Result={matchedInt.Result}");
Console.WriteLine();

// ============================================================================
// MULTIPLE TYPES (AnyT3)
// ============================================================================
Console.WriteLine("--- Multiple Typed Values (AnyT3) ---");

Any<string, int, bool> triple = Any<string, int, bool>.First("Age");
Any<string, int, bool> tripleSecond = Any<string, int, bool>.Second(95);
Any<string, int, bool> tripleThird = Any<string, int, bool>.Third(true);

(string? t3First, int? t3Second, bool? t3Third) = tripleSecond.ToTuple();
Console.WriteLine($"AnyT3 ToTuple: First={t3First}, Second={t3Second}, Third={t3Third}");

Console.WriteLine($"tripleSecond.Is<int>(): {tripleSecond.Is<int, string, int, bool>()}");
int? t3As = tripleSecond.As<int, string, int, bool>();
Console.WriteLine($"tripleSecond.As<int>(): {t3As}");

bool t3Try = tripleThird.TryAs<bool, string, int, bool>(out bool t3Bool);
Console.WriteLine($"tripleThird.TryAs<bool>(): success={t3Try}, value={t3Bool}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
