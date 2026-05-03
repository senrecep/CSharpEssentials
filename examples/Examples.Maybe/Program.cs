using CSharpEssentials.Maybe;

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

string? nullable = "world";
Maybe<string> fromNullable = nullable.AsMaybe();
Console.WriteLine($"From nullable: {fromNullable.Value}");

string? nullValue = null;
Maybe<string> fromNull = nullValue.AsMaybe();
Console.WriteLine($"From null: HasValue={fromNull.HasValue}");

// From func
Maybe<int> fromFunc = Maybe<int>.From(() => 42);
Console.WriteLine($"From func: {fromFunc.Value}");

// TryGetValue
if (some.TryGetValue(out string? val))
{
    Console.WriteLine($"TryGetValue: {val}");
}
Console.WriteLine();

// ============================================================================
// MAP (TRANSFORM VALUE)
// ============================================================================
Console.WriteLine("--- Map ---");

Maybe<int> number = 10.AsMaybe();
Maybe<int> doubled = number.Map(x => x * 2);
Console.WriteLine($"10 * 2 = {doubled.Value}");

Maybe<int> empty = Maybe<int>.None;
Maybe<int> emptyDoubled = empty.Map(x => x * 2);
Console.WriteLine($"None * 2 = HasValue={emptyDoubled.HasValue}");

// MapAsync with explicit Task return type
Maybe<int> asyncMapped = number.MapAsync<int>(x => Task.FromResult(x * 3)).Result;
Console.WriteLine($"MapAsync: {asyncMapped.Value}");
Console.WriteLine();

// ============================================================================
// BIND (FLATTEN MAYBE)
// ============================================================================
Console.WriteLine("--- Bind ---");

Maybe<string> input = "123".AsMaybe();
Maybe<int> parsed = input.Bind(str => int.TryParse(str, out int val) ? val.AsMaybe() : Maybe<int>.None);
Console.WriteLine($"Parsed '123': {parsed.Value}");

Maybe<string> badInput = "abc".AsMaybe();
Maybe<int> badParsed = badInput.Bind(str => int.TryParse(str, out int val) ? val.AsMaybe() : Maybe<int>.None);
Console.WriteLine($"Parsed 'abc': HasValue={badParsed.HasValue}");
Console.WriteLine();

// ============================================================================
// WHERE (FILTER)
// ============================================================================
Console.WriteLine("--- Where ---");

Maybe<int> age = 25.AsMaybe();
Maybe<int> adult = age.Where(x => x >= 18);
Console.WriteLine($"Adult age: HasValue={adult.HasValue}, Value={adult.Value}");

Maybe<int> child = 12.AsMaybe();
Maybe<int> notAdult = child.Where(x => x >= 18);
Console.WriteLine($"Child age filtered: HasValue={notAdult.HasValue}");
Console.WriteLine();

// ============================================================================
// GET VALUE OR DEFAULT / OR THROW
// ============================================================================
Console.WriteLine("--- GetValueOrDefault / GetValueOrThrow ---");

Maybe<string> name = "Alice".AsMaybe();
Console.WriteLine($"Name: {name.GetValueOrDefault("Unknown")}");

Maybe<string> missing = Maybe<string>.None;
Console.WriteLine($"Missing: {missing.GetValueOrDefault("Unknown")}");
Console.WriteLine($"Missing (no param): '{missing.GetValueOrDefault()}'");

try
{
    _ = missing.GetValueOrThrow("Value is required!");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Thrown: {ex.Message}");
}
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
// SELECT / SELECTMANY (LINQ)
// ============================================================================
Console.WriteLine("--- Select / SelectMany ---");

Maybe<int> linqNumber = 5.AsMaybe();
Maybe<int> linqResult = from n in linqNumber
                        select n * 4;
Console.WriteLine($"Select: {linqResult.Value}");

Maybe<int> linqBind = from n in linqNumber
                      from m in Maybe<int>.From(n + 3)
                      select m;
Console.WriteLine($"SelectMany: {linqBind.Value}");
Console.WriteLine();

// ============================================================================
// EXECUTE (SIDE EFFECTS)
// ============================================================================
Console.WriteLine("--- Execute ---");

Maybe<int> executeValue = 100.AsMaybe();
executeValue.Execute(val => Console.WriteLine($"Execute with value: {val}"));

Maybe<int> executeEmpty = Maybe<int>.None;
executeEmpty.Execute(val => Console.WriteLine($"This won't print: {val}"));
Console.WriteLine();

// ============================================================================
// FLATTEN
// ============================================================================
Console.WriteLine("--- Flatten ---");

Maybe<Maybe<int>> nested = Maybe<Maybe<int>>.From(7.AsMaybe());
Maybe<int> flattened = nested.Flatten();
Console.WriteLine($"Flattened: {flattened.Value}");
Console.WriteLine();

// ============================================================================
// TO LIST
// ============================================================================
Console.WriteLine("--- ToList ---");

Maybe<int> listValue = 42.AsMaybe();
List<int> values = listValue.ToList();
Console.WriteLine($"ToList from Some: [{string.Join(", ", values)}]");

Maybe<int> emptyList = Maybe<int>.None;
List<int> emptyValues = emptyList.ToList();
Console.WriteLine($"ToList from None: [{string.Join(", ", emptyValues)}]");
Console.WriteLine();

// ============================================================================
// TRY FIRST / TRY LAST
// ============================================================================
Console.WriteLine("--- TryFirst / TryLast ---");

List<int> numbers = new() { 1, 2, 3, 4, 5 };
Maybe<int> first = numbers.TryFirst();
Maybe<int> last = numbers.TryLast();
Maybe<int> firstEven = numbers.TryFirst(x => x % 2 == 0);
Console.WriteLine($"TryFirst: {first.Value}");
Console.WriteLine($"TryLast: {last.Value}");
Console.WriteLine($"TryFirst even: {firstEven.Value}");

List<int> emptyNumbers = new();
Maybe<int> firstEmpty = emptyNumbers.TryFirst();
Console.WriteLine($"TryFirst empty: HasValue={firstEmpty.HasValue}");
Console.WriteLine();

// ============================================================================
// TRY FIND
// ============================================================================
Console.WriteLine("--- TryFind ---");

Dictionary<string, int> dict = new() { { "one", 1 }, { "two", 2 }, { "three", 3 } };
Maybe<int> found = dict.TryFind("two");
Maybe<int> notFound = dict.TryFind("four");
Console.WriteLine($"TryFind 'two': {found.Value}");
Console.WriteLine($"TryFind 'four': HasValue={notFound.HasValue}");
Console.WriteLine();

// ============================================================================
// CHOOSE
// ============================================================================
Console.WriteLine("--- Choose ---");

List<Maybe<int>> maybes = new() { 1.AsMaybe(), Maybe<int>.None, 3.AsMaybe(), Maybe<int>.None, 5.AsMaybe() };
List<int> chosen = maybes.Choose().ToList();
Console.WriteLine($"Choose: [{string.Join(", ", chosen)}]");
Console.WriteLine();

// ============================================================================
// AS NULLABLE
// ============================================================================
Console.WriteLine("--- AsNullable ---");

Maybe<int> intMaybe = 99.AsMaybe();
int? nullableInt = intMaybe.AsNullable();
Console.WriteLine($"AsNullable Some: {nullableInt}");

Maybe<int> emptyIntMaybe = Maybe<int>.None;
int? nullNullable = emptyIntMaybe.AsNullable();
Console.WriteLine($"AsNullable None: {nullNullable.HasValue}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
