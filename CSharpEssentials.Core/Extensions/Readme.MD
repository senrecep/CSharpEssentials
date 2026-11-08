# CSharpEssentials.Core extension methods

All extensions live in namespace `CSharpEssentials.Core`, except the C# 14 extension members noted at the end.

## Case conversion

Every conversion takes an optional `CultureInfo` (invariant culture when omitted), for example `"istanbul".ToPascalCase(CultureInfo.GetCultureInfo("tr-TR"))`.

```csharp
"helloWorld".ToPascalCase();          // "HelloWorld"
"hello_world".ToCamelCase();          // "helloWorld"
"HelloWorld".ToKebabCase();           // "hello-world"
"helloWorld".ToSnakeCase();           // "hello_world"
"hello world".ToMacroCase();          // "HELLO_WORLD"
"hello-world".ToTrainCase();          // "Hello-World"
"hello world".ToTitleCase();          // "Hello World"
"helloWorld".ToUnderscoreCamelCase(); // "_helloWorld"
```

Each call allocates only the result string. The work buffer is `stackalloc` up to 512 characters and rented from `ArrayPool<char>` above that, so large inputs are safe. The word-splitting rules are in the [package README](../README.MD#string-case-conversions).

## Random items

`GetRandomItem` and `GetRandomItems` exist for `T[]`, `List<T>` and `Span<T>` and use `RandomNumberGenerator`, not `System.Random`.

```csharp
List<int> numbers = [1, 2, 3, 4, 5];
int one = numbers.GetRandomItem();
int[] three = numbers.GetRandomItems(3);

Span<string> words = ["apple", "banana", "cherry"];
string word = words.GetRandomItem();
```

`GetRandomItems` picks items at distinct positions. A `count` at or above the length returns all items shuffled, and a negative `count` throws `ArgumentOutOfRangeException`. `GetRandomItem` on an empty source throws `ArgumentException`.

## Null, empty and boolean checks

```csharp
string? text = null;
text.IsEmpty();      // true for null, "" and whitespace (string? only)
text.IsNotEmpty();
text.IsNull();       // any T
text.IsNotNull();

bool condition = true;
condition.IsTrue();
condition.IsFalse();
condition.IfTrue(() => Console.WriteLine("true"));
condition.IfFalse(() => Console.WriteLine("false"));
```

All four checks carry `[NotNullWhen]`, so the compiler treats the value as non-null after a successful `IsNotEmpty()` or `IsNotNull()`. `IfTrue` and `IfFalse` return the condition.

`IfNotNull` and `IfNull` run an action, with an optional else action:

```csharp
text.IfNotNull(t => Console.WriteLine(t));
text.IfNotNull(() => Console.WriteLine("has a value"));   // ignores the value
text.IfNull(() => Console.WriteLine("null"));

text.IfNotNull(t => Console.WriteLine(t), () => Console.WriteLine("null"));
text.IfNull(() => Console.WriteLine("null"), t => Console.WriteLine(t));
```

Each overload returns a `bool`: `true` when the value was not null for `IfNotNull`, `true` when it was null for `IfNull`.

## Tasks

```csharp
Task<string> task = "test".AsTask();              // Task.FromResult
ValueTask<string> valueTask = "test".AsValueTask();

await task.WithCancellation(cancellationToken);
await valueTask.WithCancellation(cancellationToken);
```

`WithCancellation` exists for `Task`, `Task<T>`, `ValueTask` and `ValueTask<T>`. It throws `OperationCanceledException` as soon as the token is cancelled (on .NET 9 and later the `TaskCanceledException` subclass from `Task.WaitAsync`), but it does not cancel the wrapped operation. A completed task, or a token that cannot be cancelled, returns the original task unchanged.

## Collections

```csharp
list.IfAdd(condition: true, 1);
list.IfAddRange(condition: true, [1, 2, 3]);

var query = items.WhereIf(condition, x => x.IsActive);   // IQueryable<T> and IEnumerable<T>
var nonNull = items.WithoutNulls();
var withProperty = items.WithoutNulls(x => x.Property);  // drops items whose property is null

bool same = list1.HasSameElements(list2);   // set comparison: order and duplicates ignored
bool allTrue = flags.AllTrue();
bool allFalse = flags.AllFalse();

items.ForEach(item => Console.WriteLine(item))
     .Where(x => x.IsValid)
     .ToList();   // the actions run here
```

`ForEach` is lazy and yields each item after running the action. When `items` is a `List<T>`, the instance method `List<T>.ForEach` (returns `void`) wins overload resolution; call `items.AsEnumerable().ForEach(...)` to get this one. `WithoutNulls`, `GetInnerExceptions` and `GetInnerExceptionsMessages` are lazy too.

## Range checks

```csharp
5.IsBetween(1, 10);            // true  (inclusive)
10.IsBetweenExclusive(1, 10);  // false (exclusive)
```

Both work for any `IComparable<T>`.

## Other extensions

```csharp
int number = ((object)42).ExplicitCast<int>();   // (int)boxed

DateTime when = 1700000000000L.MsToDateTime();   // Unix milliseconds, Kind is Utc

string shortId = Guider.NewGuid().ToStringFromGuid();   // see ../Guids/Readme.MD
Guid parsed = shortId.ToGuidFromString();

HttpGroup group = HttpStatus.BadRequest.GetTypeGroup<HttpGroup, HttpStatus>();   // HttpGroup.ClientError

public enum HttpStatus { Ok = 200, BadRequest = 400 }
public enum HttpGroup { Success = 200, ClientError = 400 }
```

`GetTypeGroup` rounds the enum value down to a multiple of `group` (default 100) and converts it to the target enum.

`GetInnerExceptions()` returns the exception itself followed by each inner exception; `GetInnerExceptionsMessages()` returns their `Message` values as `IEnumerable<string?>`.

`HttpCodes` holds `int` constants for common status codes (`HttpCodes.Ok`, `HttpCodes.NotFound`, `HttpCodes.UnprocessableEntity`, `HttpCodes.TooManyRequests`, ...).

On .NET 10 and later, `using CSharpEssentials;` brings the C# 14 extension properties `IEnumerable<T>.IsEmpty` (`!source.Any()`) and `string.IsPalindrome`.
