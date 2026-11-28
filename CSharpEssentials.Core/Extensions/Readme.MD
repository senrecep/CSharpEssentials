# CSharpEssentials.Core extension methods

Every public extension in the package, grouped by class, with the behavior that is easy to miss. All classes are in namespace `CSharpEssentials.Core`, except the two C# 14 classes at the end. Worked examples are in the [package README](../README.MD).

## StringExtensions

| Method | `"helloWorld"` gives |
|---|---|
| `ToPascalCase(culture = null)` | `HelloWorld` |
| `ToCamelCase(culture = null)` | `helloWorld` |
| `ToSnakeCase(culture = null)` | `hello_world` |
| `ToKebabCase(culture = null)` | `hello-world` |
| `ToMacroCase(culture = null)` | `HELLO_WORLD` |
| `ToTrainCase(culture = null)` | `Hello-World` |
| `ToTitleCase(culture = null)` | `Hello World` |
| `ToUnderscoreCamelCase(culture = null)` | `_helloWorld` |

- The input may use any separator: `"hello world"`, `"hello-world"` and `"hello_world"` give the same results.
- `culture` decides upper and lower casing; `null` uses fast invariant rules. `"istanbul".ToPascalCase(CultureInfo.GetCultureInfo("tr-TR"))` is `"İstanbul"`.
- A run of capitals is one word: `"HTTPStatus".ToSnakeCase()` is `"httpstatus"`. The word rules are in the [package README](../README.MD#string-case-conversions).
- An empty string returns an empty string. A `null` string throws `NullReferenceException`.

## GeneralExtensions

| Method | Behavior |
|---|---|
| `string?.IsEmpty()` / `IsNotEmpty()` | `true` / `false` for `null`, `""` and whitespace |
| `T?.IsNull()` / `IsNotNull()` | Null checks for any type |
| `bool.IsTrue()` / `IsFalse()` | Return the value / its negation |
| `bool.IfTrue(Action)` / `IfFalse(Action)` | Run the action when the condition matches; return the condition |
| `T?.IfNotNull(Action<T>)`, `IfNotNull(Action)` | Run when not null; return whether it was not null |
| `T?.IfNotNull(Action<T>, Action else)`, `IfNotNull(Action, Action else)` | As above, else action when null |
| `T?.IfNull(Action)` | Run when null; return whether it was null |
| `T?.IfNull(Action, Action<T> else)`, `IfNull(Action, Action else)` | As above, else action when not null |
| `object.ExplicitCast<T>()` | `(T)obj`; throws `InvalidCastException` on a wrong type |
| `Guid.ToStringFromGuid()` | Same as `Guider.ToStringFromGuid` |
| `string.ToGuidFromString()`, `ReadOnlySpan<char>.ToGuidFromString()` | Same as `Guider.ToGuidFromString`; throws `FormatException` |
| `long.MsToDateTime()` | Unix milliseconds to a `DateTime` with `Kind = Utc` |
| `TType.GetTypeGroup<TGroup, TType>(group = 100)` | Rounds an enum value down to a multiple of `group` and converts it to `TGroup` |
| `T.AsTask()` / `AsValueTask()` | `Task.FromResult` / a completed `ValueTask<T>` |
| `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>` `.WithCancellation(token)` | Stop waiting when `token` is cancelled |

Notes:

- `IsEmpty`, `IsNotEmpty`, `IsNull` and `IsNotNull` carry `[NotNullWhen]`, so the compiler treats the value as not null after a passing check.
- `WithCancellation` does not cancel the wrapped work. It returns the original task when it is already complete or when the token cannot be cancelled. A `null` task gives a faulted task with `ArgumentNullException`. The .NET 9 and later builds throw `TaskCanceledException`, the `netstandard` builds `OperationCanceledException`. A fault of the abandoned task is observed, so it does not surface as an unobserved task exception.

```csharp
string? input = null;
input.IfNotNull(v => Console.WriteLine(v), () => Console.WriteLine("missing")); // prints "missing", returns false
```

## CollectionExtensions

| Method | Behavior |
|---|---|
| `ICollection<T>.IfAdd(condition, item)` | Adds `item` when `condition` is true; returns the collection |
| `ICollection<T>.IfAddRange(condition, params IEnumerable<T> items)` | Adds all `items` when `condition` is true; returns the collection |
| `IEnumerable<T>.ForEach(action)` | Lazy: runs `action` per item as the result is enumerated and yields the item |
| `IEnumerable<T>.WhereIf(condition, Func<T, bool>)` | `Where` only when `condition` is true |
| `IEnumerable<T>.WhereIf(condition, Func<T, int, bool>)` | Same, with the index |
| `IQueryable<T>.WhereIf(condition, Expression<Func<T, bool>>)` | Same, translated by the query provider |
| `IEnumerable<T?>.WithoutNulls()` | Drops `null` items; typed as non-nullable |
| `IEnumerable<T?>.WithoutNulls(Expression<Func<T, TProperty?>> selector)` | Also drops items whose selected value is `null` (the expression is compiled on each call) |
| `IEnumerable<T>.HasSameElements(other)` | Set equality: order and duplicates are ignored |
| `IEnumerable<bool>.AllTrue()` / `AllFalse()` | `true` for an empty sequence |

- `IfAddRange` takes a `params` collection, so `list.IfAddRange(true, "a", "b")` needs C# 13. Pass a collection expression (`["a", "b"]`) on older versions.
- On a variable typed `List<T>`, `list.ForEach(...)` binds to `List<T>.ForEach`, which runs at once and returns `void`. Use `list.AsEnumerable().ForEach(...)` for the lazy version.

## RandomItemsExtensions

| Method | Behavior |
|---|---|
| `T[]`, `List<T>`, `Span<T>` `.GetRandomItem()` | One random item |
| `T[]`, `List<T>`, `Span<T>` `.GetRandomItems(count)` | `count` items at distinct positions |

Both use `RandomNumberGenerator`, not `System.Random`. A `count` at or above the length returns every item, shuffled. A negative `count` throws `ArgumentOutOfRangeException`. `GetRandomItem` on an empty source throws `ArgumentException`.

```csharp
List<int> numbers = [1, 2, 3, 4, 5];
int[] three = numbers.GetRandomItems(3);
```

## ComparableExtensions

| Method | Behavior |
|---|---|
| `T.IsBetween(min, max)` | `min <= value <= max` for any `IComparable<T>` |
| `T.IsBetweenExclusive(min, max)` | `min < value < max` |

## ExceptionExtensions

| Method | Behavior |
|---|---|
| `Exception.GetInnerExceptions()` | Lazy: the exception itself, then each `InnerException` |
| `Exception.GetInnerExceptionsMessages()` | Lazy: the `Message` of each of those |

An `AggregateException` is followed through `InnerException` only, so its other inner exceptions are not listed.

## C# 14 extension members (namespace `CSharpEssentials`, .NET 10 and later)

| Member | Behavior |
|---|---|
| `IEnumerable<T>.IsEmpty` | Property: `!source.Any()` |
| `string.IsPalindrome` | Property: reads the same in both directions, ordinal; `true` for `""` |

A file that imports both `CSharpEssentials` and `CSharpEssentials.Core` gets CS9339 for `text.IsEmpty()` and `text.IsEmpty` on a `string`. Import one of the two namespaces per file, or call `GeneralExtensions.IsEmpty(text)`.
