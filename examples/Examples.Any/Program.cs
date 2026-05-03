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
// ANY ACTION RESULT
// ============================================================================
Console.WriteLine("--- Any Action Result ---");

AnyActionResult<string> success = new(AnyActionStatus.Executed, "Operation completed");
Console.WriteLine($"Success: Status={success.Status}, Result={success.Result}");

AnyActionResult<string> notExecuted = AnyActionStatus.NotExecuted;
Console.WriteLine($"NotExecuted: Status={notExecuted.Status}, Result={notExecuted.Result}");

AnyActionResult<string> fromResult = "Implicit result";
Console.WriteLine($"From implicit: Status={fromResult.Status}, Result={fromResult.Result}");
Console.WriteLine();

// ============================================================================
// MULTIPLE TYPES (AnyT3, AnyT4, AnyT5)
// ============================================================================
Console.WriteLine("--- Multiple Typed Values ---");

Any<string, int, bool> triple = Any<string, int, bool>.First("Age");
Console.WriteLine($"Triple: Value={triple.Value}, IsFirst={triple.IsFirst}, IsSecond={triple.IsSecond}, IsThird={triple.IsThird}");

Any<string, int, bool> tripleSecond = Any<string, int, bool>.Second(95);
Console.WriteLine($"Triple Second: Value={tripleSecond.Value}, IsSecond={tripleSecond.IsSecond}");

Any<string, int, bool> tripleThird = Any<string, int, bool>.Third(true);
Console.WriteLine($"Triple Third: Value={tripleThird.Value}, IsThird={tripleThird.IsThird}");

tripleThird.Switch(
    first: v => Console.WriteLine($"  Triple Switch first: {v}"),
    second: v => Console.WriteLine($"  Triple Switch second: {v}"),
    third: v => Console.WriteLine($"  Triple Switch third: {v}")
);

AnyActionResult<string> tripleMatch = tripleSecond.Match(
    first: v => $"Triple match string: {v}",
    second: v => $"Triple match int: {v}",
    third: v => $"Triple match bool: {v}"
);
Console.WriteLine($"Triple match: Status={tripleMatch.Status}, Result={tripleMatch.Result}");
Console.WriteLine();

// ============================================================================
// ANY T4 - T8
// ============================================================================
Console.WriteLine("--- AnyT4 and AnyT5 ---");

Any<int, string, bool, double> quad = Any<int, string, bool, double>.Fourth(3.14);
Console.WriteLine($"AnyT4: IsFourth={quad.IsFourth}, Value={quad.GetFourth()}");

Any<int, string, bool, double, DateTime> quint = Any<int, string, bool, double, DateTime>.Fifth(DateTime.UtcNow);
Console.WriteLine($"AnyT5: IsFifth={quint.IsFifth}, Value={quint.GetFifth()}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
