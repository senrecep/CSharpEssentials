# CSharpEssentials Extension Methods

This library provides extension methods that add functional programming helpers to C# and improve code readability.

## String Extensions

Extension methods for string manipulation (namespace `CSharpEssentials.Core`):

### Case Conversion Methods

Every conversion takes an optional `CultureInfo` (invariant culture when omitted), for example `"istanbul".ToPascalCase(CultureInfo.GetCultureInfo("tr-TR"))`.

```csharp
// Case Conversion Examples and Explanations
"helloWorld".ToPascalCase();      // => "HelloWorld"
// First letter of each word capitalized, rest lowercase

"hello_world".ToCamelCase();      // => "helloWorld"
// First word lowercase, other words PascalCase

"HelloWorld".ToKebabCase();       // => "hello-world"
// All lowercase, words separated by hyphens

"helloWorld".ToSnakeCase();       // => "hello_world"
// All lowercase, words separated by underscores

"hello world".ToMacroCase();      // => "HELLO_WORLD"
// All uppercase, words separated by underscores

"hello-world".ToTrainCase();      // => "Hello-World"
// Each word PascalCase, separated by hyphens

"hello world".ToTitleCase();      // => "Hello World"
// Each word PascalCase, separated by spaces

"helloWorld".ToUnderscoreCamelCase(); // => "_helloWorld"
// CamelCase with leading underscore
```

The conversions work in a `stackalloc` buffer for short inputs and rent from `ArrayPool<char>` for longer ones, so they are safe on large strings.

## Random Item Extensions

Extension methods for random item selection with `RandomNumberGenerator` (cryptographically secure). They exist for `T[]`, `List<T>` and `Span<T>`:

```csharp
// Single random item selection
List<int> numbers = [1, 2, 3, 4, 5];
int randomNumber = numbers.GetRandomItem();
// Selects a random number from the list

// Multiple random item selection
int[] threeRandomNumbers = numbers.GetRandomItems(3);
// Selects 3 items at distinct positions; a count >= the list length returns all items shuffled

// Works with arrays and spans (an empty source throws ArgumentException)
Span<string> words = ["apple", "banana", "cherry"];
string randomWord = words.GetRandomItem();
// Selects a random word from the span
```

## General Extensions

### Null and Empty Checks

```csharp
string? text = null;
bool isEmpty = text.IsEmpty();      // true if null, empty, or whitespace
bool isNotEmpty = text.IsNotEmpty(); // true if not null, empty, or whitespace
bool isNull = text.IsNull();        // true if null (works for any T)
bool isNotNull = text.IsNotNull();  // true if not null (works for any T)
```

`IsEmpty` and `IsNotEmpty` are for `string?` only. All four carry `[NotNullWhen]` annotations, so the compiler treats the value as non-null after a successful `IsNotEmpty()` or `IsNotNull()` check.

### Boolean Extensions and Functional Programming

```csharp
bool condition = true;
condition.IsTrue();   // Returns true if condition is true
condition.IsFalse();  // Returns true if condition is false

// Conditional Actions - Functional programming approach
condition.IfTrue(() => Console.WriteLine("Condition true"));
// Executes action if condition is true

condition.IfFalse(() => Console.WriteLine("Condition false"));
// Executes action if condition is false
```

### Null Object Pattern Implementation

```csharp
string? text = "Hello";

// Single action usage
text.IfNotNull(t => Console.WriteLine(t));  // Executes if text is not null
text.IfNull(() => Console.WriteLine("Null"));  // Executes if text is null

// Usage with else action
text.IfNotNull(
    t => Console.WriteLine(t),           // Executes if text is not null
    () => Console.WriteLine("Is null")   // Executes if text is null
);

text.IfNull(
    () => Console.WriteLine("Is null"),  // Executes if text is null
    t => Console.WriteLine(t)            // Executes if text is not null
);

// The action may also ignore the value
text.IfNotNull(() => Console.WriteLine("Has a value"));
```

Every `IfNotNull` / `IfNull` overload returns a `bool`: `true` when the value was not null for `IfNotNull`, `true` when it was null for `IfNull`.

### Task Extensions and Asynchronous Programming

```csharp
// Converting synchronous values to Tasks
string value = "test";
Task<string> task = value.AsTask();         // Task.FromResult(value)
ValueTask<string> valueTask = value.AsValueTask(); // ValueTask.FromResult(value)

// Stop waiting when the token is cancelled
await task.WithCancellation(cancellationToken);
await valueTask.WithCancellation(cancellationToken);
```

`WithCancellation` exists for `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>`. It throws `OperationCanceledException` as soon as the token is cancelled, but it does not cancel the underlying operation. A completed task, or a token that cannot be cancelled, returns the original task unchanged.

## Collection Extensions

### Conditional Collection Operations

```csharp
var list = new List<int>();

// Conditional add operations
list.IfAdd(condition: true, 1);
// Adds item if condition is true

list.IfAddRange(condition: true, [1, 2, 3]);
// Adds all items if condition is true
```

### LINQ Extensions and Functional Queries

```csharp
// Conditional where clause
var query = items.WhereIf(condition, x => x.IsActive);
// Applies where filter if condition is true

// Null element filtering
var withoutNulls = items.WithoutNulls();
// Returns non-null elements from collection

var withoutNullProperties = items.WithoutNulls(x => x.Property);
// Returns non-null elements whose specified property is not null

// Collection comparison
bool areEqual = list1.HasSameElements(list2);
// Compares as sets: order and duplicates are ignored

// Boolean collection operations
bool allTrue = boolList.AllTrue();   // True if all elements are true
bool allFalse = boolList.AllFalse(); // True if all elements are false
```

### Enumerable Extensions and Fluent Interface

```csharp
// Fluent interface usage with ForEach
items.ForEach(item => Console.WriteLine(item))  // Runs the action for each item as it is enumerated
     .Where(x => x.IsValid)                     // Applies filter
     .ToList();                                 // Enumerates, so the actions run here
```

`ForEach` is lazy and returns each item after running the action. Nothing runs until the sequence is enumerated. When `items` is a `List<T>`, the instance method `List<T>.ForEach` (returns `void`) is chosen instead; call `items.AsEnumerable().ForEach(...)` to use this one.

### Range Checks

```csharp
5.IsBetween(1, 10);            // true  (inclusive)
10.IsBetweenExclusive(1, 10);  // false (exclusive)
```

Both work for any `IComparable<T>`.

### Other General Extensions

```csharp
object boxed = 42;
int number = boxed.ExplicitCast<int>();   // (int)boxed

DateTime when = 1700000000000L.MsToDateTime();   // from Unix milliseconds

Guid id = Guider.NewGuid();
string shortId = id.ToStringFromGuid();          // see ../Guids/Readme.MD
Guid parsed = shortId.ToGuidFromString();

HttpGroup group = HttpStatus.BadRequest.GetTypeGroup<HttpGroup, HttpStatus>();   // HttpGroup.ClientError

public enum HttpStatus { Ok = 200, BadRequest = 400 }
public enum HttpGroup { Success = 200, ClientError = 400 }
```

`GetTypeGroup` rounds the enum value down to a multiple of `group` (default 100) and converts it to the target enum.

On .NET 10 and later, `using CSharpEssentials;` also brings the C# 14 extension members `IEnumerable<T>.IsEmpty` (a property equal to `!source.Any()`) and `string.IsPalindrome` (a property).

## Exception Extensions

Helper methods for exception handling:

```csharp
try
{
    // Code that throws exception
}
catch (Exception ex)
{
    // Get all inner exceptions
    var allExceptions = ex.GetInnerExceptions();
    // Lazily returns ex itself, then each inner exception in turn

    // Get all exception messages
    var allMessages = ex.GetInnerExceptionsMessages();
    // Lazily returns the Message of each of those exceptions (IEnumerable<string?>)
}
```

## HTTP Status Codes

`HttpCodes` holds `int` constants for common status codes (`HttpCodes.Ok`, `HttpCodes.NotFound`, `HttpCodes.UnprocessableEntity`, `HttpCodes.TooManyRequests`, ...).

## Notes

1. String case conversions allocate one result string and use a `stackalloc` buffer for short inputs (a pooled array for longer ones), so they stay safe on very large inputs.
2. Random selection uses `RandomNumberGenerator`, not `System.Random`.
3. `ForEach`, `WithoutNulls`, `GetInnerExceptions` and `GetInnerExceptionsMessages` are lazy; enumerate them to run them.
4. `WithCancellation` stops the wait only; the wrapped task keeps running.
