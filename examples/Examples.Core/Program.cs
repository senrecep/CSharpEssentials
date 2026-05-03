using CSharpEssentials.Core;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Core Example");
Console.WriteLine("========================================\n");

// ============================================================================
// GUID GENERATION
// ============================================================================
Console.WriteLine("--- GUID Generation ---");

Guid standardGuid = Guider.NewGuid();
Console.WriteLine($"Standard Guid: {standardGuid}");

string urlSafe = Guider.ToStringFromGuid(standardGuid);
Console.WriteLine($"URL-safe Guid: {urlSafe}");

Guid backToGuid = Guider.ToGuidFromString(urlSafe);
Console.WriteLine($"Back to Guid: {backToGuid}");

// Guid extension methods
Guid extGuid = Guid.NewGuid();
string extUrlSafe = extGuid.ToStringFromGuid();
Console.WriteLine($"Guid extension ToStringFromGuid: {extUrlSafe}");
Guid extBack = extUrlSafe.ToGuidFromString();
Console.WriteLine($"String extension ToGuidFromString: {extBack}");
Console.WriteLine();

// ============================================================================
// STRING EXTENSIONS (Case Conversions)
// ============================================================================
Console.WriteLine("--- String Case Extensions ---");

string input = "hello world example";
Console.WriteLine($"Original: '{input}'");
Console.WriteLine($"ToPascalCase: '{input.ToPascalCase()}'");
Console.WriteLine($"ToCamelCase: '{input.ToCamelCase()}'");
Console.WriteLine($"ToSnakeCase: '{input.ToSnakeCase()}'");
Console.WriteLine($"ToKebabCase: '{input.ToKebabCase()}'");
Console.WriteLine($"ToTitleCase: '{input.ToTitleCase()}'");
Console.WriteLine($"ToMacroCase: '{input.ToMacroCase()}'");
Console.WriteLine($"ToTrainCase: '{input.ToTrainCase()}'");
Console.WriteLine($"ToUnderscoreCamelCase: '{input.ToUnderscoreCamelCase()}'");

// Additional inputs: different separators
string[] testInputs = { "hello-world", "hello_world", "HelloWorld", "helloWorld", "HELLO_WORLD" };
foreach (var t in testInputs)
{
    Console.WriteLine($"\n  Input: '{t}'");
    Console.WriteLine($"    Pascal: '{t.ToPascalCase()}' | Camel: '{t.ToCamelCase()}' | Snake: '{t.ToSnakeCase()}' | Kebab: '{t.ToKebabCase()}'");
    Console.WriteLine($"    Title: '{t.ToTitleCase()}' | Macro: '{t.ToMacroCase()}' | Train: '{t.ToTrainCase()}' | _Camel: '{t.ToUnderscoreCamelCase()}'");
}

// Turkish characters with culture support
Console.WriteLine("\n--- Turkish Character Tests (with tr-TR culture) ---");
var trCulture = System.Globalization.CultureInfo.GetCultureInfo("tr-TR");
string[] turkishInputs = { "İstanbul Ankara", "istanbul", "İSTANBUL", "çok güzel" };
foreach (var t in turkishInputs)
{
    Console.WriteLine($"\n  Input: '{t}'");
    Console.WriteLine($"    Pascal: '{t.ToPascalCase(trCulture)}' | Camel: '{t.ToCamelCase(trCulture)}' | Snake: '{t.ToSnakeCase(trCulture)}' | Kebab: '{t.ToKebabCase(trCulture)}'");
}

string emptyStr = "";
Console.WriteLine($"\nIsEmpty(''): {emptyStr.IsEmpty()}");
Console.WriteLine($"IsNotEmpty('{input}'): {input.IsNotEmpty()}");
Console.WriteLine();

// ============================================================================
// STRING EXTENSIONS (General)
// ============================================================================
Console.WriteLine("--- String General Extensions ---");

string? nullStr = null;
Console.WriteLine($"null.IsNull(): {nullStr.IsNull()}");
Console.WriteLine($"'test'.IsNotNull(): {"test".IsNotNull()}");

string mixed = "HelloWorld123";
Console.WriteLine($"'{mixed}' IsEmpty: {mixed.IsEmpty()}");
Console.WriteLine();

// ============================================================================
// COLLECTION EXTENSIONS
// ============================================================================
Console.WriteLine("--- Collection Extensions ---");

List<int> numbers = new() { 1, 2, 3, 4, 5 };

// ForEach (returns IEnumerable<T>)
Console.Write("ForEach: ");
numbers.AsEnumerable().ForEach(n => Console.Write($"{n} ")).ToList();
Console.WriteLine();

// WhereIf
IEnumerable<int> filtered = numbers.WhereIf(true, n => n > 2);
Console.WriteLine($"WhereIf(true, n > 2): [{string.Join(", ", filtered)}]");

IEnumerable<int> unfiltered = numbers.WhereIf(false, n => n > 100);
Console.WriteLine($"WhereIf(false, n > 100): [{string.Join(", ", unfiltered)}]");

// WithoutNulls
List<string?> nullableList = new() { "a", null, "b", null, "c" };
List<string> noNulls = nullableList.WithoutNulls().ToList();
Console.WriteLine($"WithoutNulls: [{string.Join(", ", noNulls)}]");

// HasSameElements
List<int> listA = new() { 1, 2, 3 };
List<int> listB = new() { 3, 2, 1 };
Console.WriteLine($"HasSameElements [1,2,3] vs [3,2,1]: {listA.HasSameElements(listB)}");

// AllTrue / AllFalse
List<bool> bools = new() { true, true, true };
Console.WriteLine($"AllTrue [T,T,T]: {bools.AllTrue()}");
Console.WriteLine($"AllFalse [T,T,T]: {bools.AllFalse()}");

// IfAdd / IfAddRange
List<string> items = new();
items.IfAdd(true, "first");
items.IfAdd(false, "skipped");
items.IfAddRange(true, new[] { "second", "third" });
items.IfAddRange(false, new[] { "skipped1", "skipped2" });
Console.WriteLine($"IfAdd/IfAddRange: [{string.Join(", ", items)}]");
Console.WriteLine();

// ============================================================================
// GENERAL EXTENSIONS
// ============================================================================
Console.WriteLine("--- General Extensions ---");

// IsTrue / IsFalse
bool flag = true;
Console.WriteLine($"true.IsTrue(): {flag.IsTrue()}");
Console.WriteLine($"true.IsFalse(): {flag.IsFalse()}");

// IfTrue / IfFalse
flag.IfTrue(() => Console.WriteLine("  IfTrue action executed"));
flag.IfFalse(() => Console.WriteLine("  IfFalse action skipped"));

// IfNotNull
string? maybeValue = "hello";
maybeValue.IfNotNull(v => Console.WriteLine($"  IfNotNull value: {v}"));
string? nullValue = null;
nullValue.IfNotNull(v => Console.WriteLine($"  This won't print"), () => Console.WriteLine("  IfNotNull elseAction executed for null"));

// IfNull
nullValue.IfNull(() => Console.WriteLine("  IfNull action executed"));

// ExplicitCast
object numberObj = 42;
int casted = numberObj.ExplicitCast<int>();
Console.WriteLine($"ExplicitCast<int>(42): {casted}");

// MsToDateTime
long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
DateTime fromMs = timestamp.MsToDateTime();
Console.WriteLine($"MsToDateTime: {fromMs:O}");

// AsTask / AsValueTask
string taskResult = "hello".AsTask().Result;
string vtResult = "world".AsValueTask().Result;
Console.WriteLine($"AsTask: {taskResult}, AsValueTask: {vtResult}");

// WithCancellation
CancellationTokenSource cts = new();
Task<string> taskWithCancellation = "cancellable".AsTask().WithCancellation(cts.Token);
Console.WriteLine($"WithCancellation: {taskWithCancellation.Result}");
cts.Dispose();
Console.WriteLine();

// ============================================================================
// RANDOM ITEMS
// ============================================================================
Console.WriteLine("--- Random Items ---");

List<string> fruits = new() { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
string randomFruit = fruits.GetRandomItem();
Console.WriteLine($"Random fruit: {randomFruit}");

List<string> randomFruits = fruits.GetRandomItems(3).ToList();
Console.WriteLine($"3 random fruits: [{string.Join(", ", randomFruits)}]");
Console.WriteLine();

// ============================================================================
// TYPE GROUP
// ============================================================================
Console.WriteLine("--- Type Group ---");

HttpStatus statusCode = HttpStatus.BadRequest;
HttpStatusGroup group = statusCode.GetTypeGroup<HttpStatusGroup, HttpStatus>(100);
Console.WriteLine($"HttpStatus.BadRequest ({(int)statusCode}) group: {group}");
Console.WriteLine();

// ============================================================================
// EXCEPTION EXTENSIONS
// ============================================================================
Console.WriteLine("--- Exception Extensions ---");

try
{
    throw new InvalidOperationException("Outer exception", new ArgumentException("Inner exception"));
}
catch (Exception ex)
{
    var innerExceptions = ex.GetInnerExceptions();
    Console.WriteLine($"Inner exceptions count: {innerExceptions.Count()}");
    foreach (var inner in innerExceptions)
    {
        Console.WriteLine($"  - {inner.GetType().Name}: {inner.Message}");
    }

    var messages = ex.GetInnerExceptionsMessages();
    Console.WriteLine($"Inner messages: [{string.Join(", ", messages)}]");
}
Console.WriteLine();

// ============================================================================
// HTTP CODES
// ============================================================================
Console.WriteLine("--- HttpCodes ---");

Console.WriteLine($"HttpCodes.Ok: {HttpCodes.Ok}");
Console.WriteLine($"HttpCodes.BadRequest: {HttpCodes.BadRequest}");
Console.WriteLine($"HttpCodes.NotFound: {HttpCodes.NotFound}");
Console.WriteLine($"HttpCodes.InternalServerError: {HttpCodes.InternalServerError}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

public enum HttpStatus
{
    Ok = 200,
    BadRequest = 400,
    Unauthorized = 401,
    NotFound = 404,
    InternalServerError = 500
}

public enum HttpStatusGroup
{
    Success = 200,
    ClientError = 400,
    ServerError = 500
}
