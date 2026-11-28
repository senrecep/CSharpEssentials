using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.These;

// 1. Creating values: Left (error only), Right (value only), Both (value with a warning)
These<string, int> left = These<string, int>.Left("not a number");
These<string, int> right = These<string, int>.Right(42);
These<string, int> both = These<string, int>.Both("rounded down", 7);

Console.WriteLine("--- State ---");
foreach (These<string, int> item in new[] { left, right, both })
    Console.WriteLine($"IsLeft={item.IsLeft} IsRight={item.IsRight} IsBoth={item.IsBoth} HasLeft={item.HasLeft} HasRight={item.HasRight}");

// 2. Match: one handler per state
Console.WriteLine("--- Match ---");
foreach (These<string, int> item in new[] { left, right, both })
    Console.WriteLine(Describe(item));

// 3. Map and MapLeft
Console.WriteLine("--- Map / MapLeft ---");
Console.WriteLine(Describe(right.Map(n => n * 2)));                 // Right(84)
Console.WriteLine(Describe(both.Map(n => n * 2)));                  // Both keeps its warning
These<int, int> errorLength = left.MapLeft(e => e.Length);
Console.WriteLine($"MapLeft: {errorLength.LeftOrDefault}");         // 12

// 4. FlatMap: Both keeps its own error, the mapper's error in a Both is dropped
Console.WriteLine("--- FlatMap ---");
These<string, int> Half(int n) => n % 2 == 0
    ? These<string, int>.Right(n / 2)
    : These<string, int>.Both("odd value halved", n / 2);

Console.WriteLine(Describe(right.FlatMap(Half)));                   // Right(21)
Console.WriteLine(Describe(both.FlatMap(Half)));                    // Both(rounded down, 3)
Console.WriteLine(Describe(left.FlatMap(Half)));                    // Left passes through

// 5. Tap and TapLeft: side effects that return the same value
Console.WriteLine("--- Tap / TapLeft ---");
both
    .Tap(v => Console.WriteLine($"value: {v}"))
    .TapLeft(e => Console.WriteLine($"warning: {e}"));

// 6. GetRight / GetLeft return Maybe<T>
Console.WriteLine("--- GetRight / GetLeft ---");
Maybe<int> value = both.GetRight();
Maybe<string> warning = right.GetLeft();
Console.WriteLine($"both.GetRight() = {value}, right.GetLeft() = {warning}");

// 7. Bridge to and from Result<T>
Console.WriteLine("--- Result bridge ---");
Result<int> ok = 5;
Result<int> failed = Error.Validation("Qty.Invalid", "Quantity is invalid.");
These<Error, int> fromOk = TheseExtensions.FromResult(ok);
These<Error, int> fromFailed = TheseExtensions.FromResult(failed);
Console.WriteLine($"FromResult(ok).IsRight={fromOk.IsRight}, FromResult(failed).IsLeft={fromFailed.IsLeft}");

These<Error, int> partial = These<Error, int>.Both(Error.Validation("Qty.Capped", "Quantity capped at 10."), 10);
Result<int> strict = partial.ToResult();
Result<int> lenient = partial.ToResultLenient();
Console.WriteLine($"ToResult: IsFailure={strict.IsFailure} ({strict.FirstError.Code})");
Console.WriteLine($"ToResultLenient: IsSuccess={lenient.IsSuccess} ({lenient.Value})");

// 8. Partition: split a batch in one pass
Console.WriteLine("--- Partition ---");
string[] input = ["10", "abc", "250", "-3"];
These<string, int>[] parsed = [.. input.Select(ParseQuantity)];
var (errors, clean, warned) = parsed.Partition();
Console.WriteLine($"errors: {string.Join(", ", errors)}");
Console.WriteLine($"clean: {string.Join(", ", clean)}");
Console.WriteLine($"warned: {string.Join(", ", warned.Select(w => $"{w.Item2} ({w.Item1})"))}");

// 9. JSON round trip
Console.WriteLine("--- JSON ---");
string json = JsonSerializer.Serialize(both);
These<string, int> back = JsonSerializer.Deserialize<These<string, int>>(json);
Console.WriteLine($"{json} -> IsBoth={back.IsBoth}");

static string Describe<TError, TValue>(These<TError, TValue> these) => these.Match(
    onLeft: e => $"Left({e})",
    onRight: v => $"Right({v})",
    onBoth: (e, v) => $"Both({e}, {v})");

static These<string, int> ParseQuantity(string text)
{
    if (!int.TryParse(text, out int n))
        return These<string, int>.Left($"'{text}' is not a number");
    if (n < 0)
        return These<string, int>.Left($"{n} is negative");
    if (n > 100)
        return These<string, int>.Both($"{n} capped at 100", 100);
    return These<string, int>.Right(n);
}
