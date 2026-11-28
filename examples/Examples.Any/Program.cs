using System.Text.Json;
using CSharpEssentials.Any;

// 1. Creating unions: implicit conversion, branch factories, Any.Create
Console.WriteLine("--- Creating ---");
Any<string, int> fromText = "hello";
Any<string, int> fromNumber = 42;
Any<string, int> viaFactory = Any<string, int>.Second(7);
Any<string, int> viaCreate = Any.Create<string, int>("created");
Console.WriteLine($"{fromText} (Index {fromText.Index}), {fromNumber} (Index {fromNumber.Index}), {viaFactory}, {viaCreate}");

// 2. Match returns AnyActionResult<TResult>; Switch returns AnyActionStatus
Console.WriteLine("--- Match / Switch ---");
AnyActionResult<string> matched = fromNumber.Match(
    first: s => $"text '{s}'",
    second: n => $"number {n}");
Console.WriteLine($"Match: Status={matched.Status}, Result={matched.Result}");

AnyActionStatus status = fromText.Switch(
    first: s => Console.WriteLine($"Switch ran for text: {s}"),
    second: n => Console.WriteLine($"Switch ran for number: {n}"));
Console.WriteLine($"Switch status: {status}");

// A missing handler is not a compile error: the call reports NotExecuted
AnyActionResult<string> skipped = fromNumber.Match(first: s => s.ToUpperInvariant());
Console.WriteLine($"Match without a handler for the active branch: Status={skipped.Status}, Result={skipped.Result ?? "null"}");

// 3. Branch checks and getters
Console.WriteLine("--- IsFirst / GetFirst ---");
if (fromNumber.IsSecond)
    Console.WriteLine($"GetSecond() = {fromNumber.GetSecond()}");
try
{
    fromNumber.GetFirst();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"GetFirst() on the second branch throws: {ex.Message}");
}

// 4. Type helpers: Is, As, TryAs test the runtime type of Value
Console.WriteLine("--- Is / As / TryAs ---");
Console.WriteLine($"Is<int>: {fromNumber.Is<int, string, int>()}");
Console.WriteLine($"As<string> on a number: {fromNumber.As<string, string, int>() ?? "null"}");
Console.WriteLine($"As<int> on text: {fromText.As<int, string, int>()} (default, not null)");
bool found = fromNumber.TryAs<int, string, int>(out int number);
Console.WriteLine($"TryAs<int>: {found}, {number}");

// 5. Deconstruct and ToTuple: inactive branches are default
Console.WriteLine("--- Deconstruct / ToTuple ---");
var (text, num) = fromText;
Console.WriteLine($"Deconstruct: text={text}, num={num}");
(string? First, int Second) tuple = fromNumber.ToTuple();
Console.WriteLine($"ToTuple: First={tuple.First ?? "null"}, Second={tuple.Second}");

// 6. Larger unions: up to eight branches
Console.WriteLine("--- Any<T0, T1, T2, T3> ---");
Any<string, int, bool, Guid> quad = true;
Console.WriteLine($"IsThird={quad.IsThird}, Index={quad.Index}, GetThird()={quad.GetThird()}");
AnyActionResult<string> quadMatch = quad.Match(
    first: s => "string",
    second: i => "int",
    third: b => "bool",
    fourth: g => "guid");
Console.WriteLine($"Match: {quadMatch.Result}");

// 7. A service that returns one of several outcomes
Console.WriteLine("--- Service outcome ---");
foreach (int quantity in new[] { 3, 0, 99 })
{
    string response = PlaceOrder(quantity).Match(
        first: order => $"created order {order.Id} for {order.Quantity}",
        second: invalid => $"400: {invalid.Message}",
        third: missing => $"404: {missing.Message}").Result!;
    Console.WriteLine(response);
}

// 8. Partition and Traverse: one typed array per branch
Console.WriteLine("--- Partition / Traverse ---");
Any<string, int>[] mixed = ["a", 1, "b", 2, 3];
(string[] texts, int[] numbers) = mixed.Partition();
Console.WriteLine($"Partition: texts=[{string.Join(", ", texts)}], numbers=[{string.Join(", ", numbers)}]");

(string[] bad, int[] good) = new[] { "10", "x", "30" }.Traverse(Parse);
Console.WriteLine($"Traverse: bad=[{string.Join(", ", bad)}], good=[{string.Join(", ", good)}]");

// 9. JSON: {"Index":n,"Value":...}
Console.WriteLine("--- JSON ---");
string json = JsonSerializer.Serialize(fromNumber);
Any<string, int> back = JsonSerializer.Deserialize<Any<string, int>>(json);
Console.WriteLine($"{json} -> IsSecond={back.IsSecond}, Value={back.GetSecond()}");

static Any<string, int> Parse(string s) => int.TryParse(s, out int n) ? n : s;

static Any<Order, InvalidQuantity, NotFound> PlaceOrder(int quantity)
{
    if (quantity <= 0)
        return new InvalidQuantity("Quantity must be positive.");
    if (quantity > 50)
        return new NotFound("Not enough stock.");
    return new Order(Guid.NewGuid().ToString("N")[..8], quantity);
}

internal sealed record Order(string Id, int Quantity);
internal sealed record InvalidQuantity(string Message);
internal sealed record NotFound(string Message);
