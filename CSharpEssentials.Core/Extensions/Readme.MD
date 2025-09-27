# CSharpEssentials Extension Methods

This library provides a comprehensive set of extension methods to enhance C#'s functional programming capabilities and improve code readability.

## String Extensions

High-performance extension methods for string manipulation:

### Case Conversion Methods

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

## Random Item Extensions

Extension methods for cryptographically secure random item selection:

```csharp
// Single random item selection
List<int> numbers = [1, 2, 3, 4, 5];
int randomNumber = numbers.GetRandomItem();
// Selects a random number from the list

// Multiple random item selection
int[] threeRandomNumbers = numbers.GetRandomItems(3);
// Selects 3 unique random numbers from the list

// Works with arrays and spans
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
bool isNull = text.IsNull();        // true if null
bool isNotNull = text.IsNotNull();  // true if not null

// Null analysis support with NotNullWhen attribute
public static bool IsNotEmpty([NotNullWhen(true)] this string? str)
```

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

// Generic and non-generic versions
text.IfNotNull<string>(action);  // Generic version
text.IfNotNull(action);          // Non-generic version
```

### Task Extensions and Asynchronous Programming

```csharp
// Converting synchronous values to Tasks
object obj = "test";
Task<string> task = obj.AsTask();         // Converts to Task<T>
ValueTask<string> valueTask = obj.AsValueTask(); // Converts to ValueTask<T>

// Asynchronous operations with cancellation support
await task.WithCancellation(cancellationToken);
// Makes Task cancellable with CancellationToken

await valueTask.WithCancellation(cancellationToken);
// Makes ValueTask cancellable with CancellationToken
```

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
// Returns elements where specified property is not null

// Collection comparison
bool areEqual = list1.HasSameElements(list2);
// Checks if two collections contain the same elements

// Boolean collection operations
bool allTrue = boolList.AllTrue();   // True if all elements are true
bool allFalse = boolList.AllFalse(); // True if all elements are false
```

### Enumerable Extensions and Fluent Interface

```csharp
// Fluent interface usage with ForEach
items.ForEach(item => Console.WriteLine(item))  // Executes action for each item
     .Where(x => x.IsValid)                     // Applies filter
     .ToList();                                 // Converts to list
```

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
    // Returns exception and all inner exceptions using yield return

    // Get all exception messages
    var allMessages = ex.GetInnerExceptionsMessages();
    // Returns messages from all exceptions as an array
}
```

## Performance Optimizations

1. String Case Conversions:

   - Minimizes heap allocations using `Span<T>`
   - Optimizes performance with stack allocation
   - Prevents unnecessary string creation

2. Random Selection Operations:

   - Uses cryptographically secure `RandomNumberGenerator`
   - Implements optimal algorithm for unique element selection

3. Collection Operations:

   - Avoids unnecessary enumeration
   - Implements LINQ optimizations for performance
   - Provides efficiency in chain operations with fluent interface

4. Task and Asynchronous Operations:
   - Prevents memory leaks with proper resource disposal
   - Implements efficient cancellation handling
   - Optimizes performance with ValueTask support

## Best Practices and Usage Guidelines

1. String Operations:

   - Choose appropriate case conversion method
   - Use `Span<T>` for large strings

2. Null Checks:

   - Use extension methods instead of null checks
   - Leverage NotNullWhen attribute for null analysis

3. Collection Operations:

   - Utilize fluent interface
   - Use WhereIf for conditional operations

4. Exception Handling:

   - Properly handle inner exceptions
   - Collect exception messages appropriately

5. Asynchronous Programming:
   - Use proper cancellation token handling
   - Implement using pattern for resource cleanup
