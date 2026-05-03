using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Maybe Example");
Console.WriteLine("========================================\n");

// ============================================================================
// MAYBE CREATION
// ============================================================================
Console.WriteLine("--- Maybe Creation ---");

Maybe<string> some = Maybe<string>.From("hello");
Maybe<string> none = Maybe<string>.None;

Console.WriteLine($"Some: HasValue={some.HasValue}, Value={some.Value}");
Console.WriteLine($"None: HasValue={none.HasValue}, HasNoValue={none.HasNoValue}");
Console.WriteLine();

// ============================================================================
// TAP / TAP IF
// ============================================================================
Console.WriteLine("--- Tap / TapIf ---");

Maybe<int> number = 10.AsMaybe();
number.Tap(v => Console.WriteLine($"  Tap with value: {v}"));
number.Tap(() => Console.WriteLine("  Tap without value"));

Maybe<int> empty = Maybe<int>.None;
empty.Tap(v => Console.WriteLine($"  This won't print: {v}"));

number.TapIf(true, v => Console.WriteLine($"  TapIf (true): {v}"));
number.TapIf(false, v => Console.WriteLine($"  This won't print"));
number.TapIf(v => v > 5, v => Console.WriteLine($"  TapIf (predicate): {v}"));
Console.WriteLine();

// ============================================================================
// BIND IF
// ============================================================================
Console.WriteLine("--- BindIf ---");

Maybe<int> boundIf = number.BindIf(true, v => Maybe<int>.From(v * 2));
Console.WriteLine($"BindIf (true): {boundIf.Value}");

Maybe<int> boundIfFalse = number.BindIf(false, v => Maybe<int>.From(v * 2));
Console.WriteLine($"BindIf (false): HasValue={boundIfFalse.HasValue}, Value={boundIfFalse.Value}");

Maybe<int> boundIfPred = number.BindIf(v => v > 5, v => Maybe<int>.From(v * 3));
Console.WriteLine($"BindIf (predicate): {boundIfPred.Value}");

Maybe<int> boundIfPredFail = number.BindIf(v => v > 20, v => Maybe<int>.From(v * 3));
Console.WriteLine($"BindIf (predicate false): HasValue={boundIfPredFail.HasValue}");
Console.WriteLine();

// ============================================================================
// MAP IF
// ============================================================================
Console.WriteLine("--- MapIf ---");

Maybe<int> mappedIf = number.MapIf(true, v => v + 100);
Console.WriteLine($"MapIf (true): {mappedIf.Value}");

Maybe<int> mappedIfFalse = number.MapIf(false, v => v + 100);
Console.WriteLine($"MapIf (false): {mappedIfFalse.Value}");

Maybe<int> mappedIfPred = number.MapIf(v => v > 5, v => v + 1000);
Console.WriteLine($"MapIf (predicate): {mappedIfPred.Value}");

Maybe<int> mappedIfPredFail = number.MapIf(v => v > 20, v => v + 1000);
Console.WriteLine($"MapIf (predicate false): {mappedIfPredFail.Value}");
Console.WriteLine();

// ============================================================================
// TO RESULT / TO UNIT RESULT
// ============================================================================
Console.WriteLine("--- ToResult / ToUnitResult ---");

Result<int> toResult = number.ToMaybeResult();
Console.WriteLine($"ToResult (Some): IsSuccess={toResult.IsSuccess}, Value={toResult.Value}");

Result<int> toResultNone = empty.ToMaybeResult(Error.NotFound("Maybe", "Value missing"));
Console.WriteLine($"ToResult (None): IsFailure={toResultNone.IsFailure}, Error={toResultNone.FirstError.Description}");

Result toUnitResult = number.ToUnitResult();
Console.WriteLine($"ToUnitResult (Some): IsSuccess={toUnitResult.IsSuccess}");

Result toUnitResultNone = empty.ToUnitResult(Error.NotFound("Maybe", "Value missing"));
Console.WriteLine($"ToUnitResult (None): IsFailure={toUnitResultNone.IsFailure}");
Console.WriteLine();

// ============================================================================
// SELECT / SELECTMANY (LINQ)
// ============================================================================
Console.WriteLine("--- Select / SelectMany (LINQ) ---");

Maybe<int> linqNumber = 5.AsMaybe();
Maybe<int> linqResult = from n in linqNumber
                        select n * 4;
Console.WriteLine($"Select: {linqResult.Value}");

Maybe<int> linqBind = from n in linqNumber
                      from m in Maybe<int>.From(n + 3)
                      select m;
Console.WriteLine($"SelectMany: {linqBind.Value}");

Maybe<int> linqMulti = from a in 2.AsMaybe()
                       from b in 3.AsMaybe()
                       select a * b;
Console.WriteLine($"SelectMany multiply: {linqMulti.Value}");
Console.WriteLine();

// ============================================================================
// MAP (TRANSFORM VALUE)
// ============================================================================
Console.WriteLine("--- Map ---");

Maybe<int> doubled = number.Map(x => x * 2);
Console.WriteLine($"10 * 2 = {doubled.Value}");

Maybe<int> emptyDoubled = empty.Map(x => x * 2);
Console.WriteLine($"None * 2 = HasValue={emptyDoubled.HasValue}");
Console.WriteLine();

// ============================================================================
// MATCH (SIDE EFFECTS)
// ============================================================================
Console.WriteLine("--- Match ---");

Maybe<int> score = 95.AsMaybe();
score.Match(
    some: val => Console.WriteLine($"Great score: {val}"),
    none: () => Console.WriteLine("No score available")
);

Maybe<int> noScore = Maybe<int>.None;
noScore.Match(
    some: val => Console.WriteLine($"Score: {val}"),
    none: () => Console.WriteLine("No score available (from None)")
);
Console.WriteLine();

// ============================================================================
// OR (FALLBACK)
// ============================================================================
Console.WriteLine("--- Or (Fallback) ---");

Maybe<string> emptyName = Maybe<string>.None;
Maybe<string> fallbackName = emptyName.Or(() => "Default Name");
Console.WriteLine($"Fallback name: {fallbackName.Value}");

Maybe<string> existingName = "Alice".AsMaybe();
Maybe<string> keptName = existingName.Or(() => "Default Name");
Console.WriteLine($"Kept name: {keptName.Value}");
Console.WriteLine();

// ============================================================================
// CHOOSE
// ============================================================================
Console.WriteLine("--- Choose ---");

List<Maybe<int>> maybes = new() { 1.AsMaybe(), Maybe<int>.None, 3.AsMaybe(), Maybe<int>.None, 5.AsMaybe() };
List<int> chosen = maybes.Choose().ToList();
Console.WriteLine($"Choose: [{string.Join(", ", chosen)}]");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
