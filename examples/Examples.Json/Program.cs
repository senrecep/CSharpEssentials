using CSharpEssentials;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Json - Advanced Example");
Console.WriteLine("========================================\n");

// ============================================================================
// 1. DEFAULT JSON OPTIONS & EXTENSION METHODS
// ============================================================================
Console.WriteLine("--- 1. Default Json Options & Extensions ---");

JsonSerializerOptions defaultOpts = EnhancedJsonSerializerOptions.DefaultOptions;
Console.WriteLine($"DefaultOptions Converters: {defaultOpts.Converters.Count}");

Person person = new() { Name = "Alice", Age = 30, City = "New York" };
string personJson = person.ConvertToJson();
Console.WriteLine($"ConvertToJson: {personJson}");

Person? personBack = personJson.ConvertFromJson<Person>();
Console.WriteLine($"ConvertFromJson: {personBack?.Name}, {personBack?.Age}");

using JsonDocument personDoc = person.ConvertToJsonDocument();
Console.WriteLine($"ConvertToJsonDocument: name={personDoc.RootElement.GetProperty("name").GetString()}");
Console.WriteLine();

// ============================================================================
// 2. JSON OPTIONS CHAINING (Create / ApplyTo / ApplyFrom)
// ============================================================================
Console.WriteLine("--- 2. Json Options Chaining ---");

JsonSerializerOptions customOpts = EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters.Create(opts =>
{
    opts.WriteIndented = true;
    opts.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

string snakeJson = person.ConvertToJson(customOpts);
Console.WriteLine($"SnakeCase + Indented:\n{snakeJson}");

JsonSerializerOptions targetOpts = new();
customOpts.ApplyTo(targetOpts);
Console.WriteLine($"ApplyTo: WriteIndented={targetOpts.WriteIndented}, Policy={targetOpts.PropertyNamingPolicy}");

JsonSerializerOptions sourceOpts = new() { WriteIndented = false };
sourceOpts.ApplyFrom(customOpts);
Console.WriteLine($"ApplyFrom: WriteIndented={sourceOpts.WriteIndented}");
Console.WriteLine();

// ============================================================================
// 3. SIMPLE POLYMORPHIC SERIALIZATION
// ============================================================================
Console.WriteLine("--- 3. Simple Polymorphic Serialization ---");

JsonSerializerOptions polyOptions = EnhancedJsonSerializerOptions.DefaultOptions;

Animal cat = new Cat { Name = "Whiskers", LivesLeft = 9 };
Animal dog = new Dog { Name = "Rex", Breed = "German Shepherd" };

string catJson = JsonSerializer.Serialize(cat, polyOptions);
string dogJson = JsonSerializer.Serialize(dog, polyOptions);
Console.WriteLine($"Cat JSON: {catJson}");
Console.WriteLine($"Dog JSON: {dogJson}");

Animal deserializedCat = JsonSerializer.Deserialize<Animal>(catJson, polyOptions)!;
Animal deserializedDog = JsonSerializer.Deserialize<Animal>(dogJson, polyOptions)!;
Console.WriteLine($"Cat: {deserializedCat.GetType().Name}, Name={deserializedCat.Name}");
Console.WriteLine($"Dog: {deserializedDog.GetType().Name}, Name={deserializedDog.Name}");
Console.WriteLine();

// ============================================================================
// 4. DEEP INHERITANCE HIERARCHY
// ============================================================================
Console.WriteLine("--- 4. Deep Inheritance Hierarchy ---");

Animal mammal = new Mammal { Name = "Generic Mammal", HasFur = true };
Animal houseCat = new HouseCat { Name = "Garfield", LivesLeft = 7, HasFur = true, Indoor = true };

string mammalJson = JsonSerializer.Serialize(mammal, polyOptions);
string houseCatJson = JsonSerializer.Serialize(houseCat, polyOptions);
Console.WriteLine($"Mammal JSON: {mammalJson}");
Console.WriteLine($"HouseCat JSON: {houseCatJson}");

Animal deserializedMammal = JsonSerializer.Deserialize<Animal>(mammalJson, polyOptions)!;
Animal deserializedHouseCat = JsonSerializer.Deserialize<Animal>(houseCatJson, polyOptions)!;
Console.WriteLine($"Mammal: {deserializedMammal.GetType().Name}, HasFur={((Mammal)deserializedMammal).HasFur}");
Console.WriteLine($"HouseCat: {deserializedHouseCat.GetType().Name}, Indoor={((HouseCat)deserializedHouseCat).Indoor}");
Console.WriteLine();

// ============================================================================
// 5. POLYMORPHIC COLLECTIONS
// ============================================================================
Console.WriteLine("--- 5. Polymorphic Collections ---");

List<Animal> zoo = new()
{
    new Cat { Name = "Whiskers", LivesLeft = 9 },
    new Dog { Name = "Rex", Breed = "German Shepherd" },
    new HouseCat { Name = "Garfield", LivesLeft = 7, HasFur = true, Indoor = true }
};

string zooJson = JsonSerializer.Serialize(zoo, polyOptions);
Console.WriteLine($"Zoo JSON: {zooJson}");

List<Animal>? deserializedZoo = JsonSerializer.Deserialize<List<Animal>>(zooJson, polyOptions);
Console.WriteLine($"Deserialized zoo count: {deserializedZoo?.Count}");
foreach (Animal animal in deserializedZoo!)
{
    Console.WriteLine($"  - {animal.GetType().Name}: {animal.Name}");
}
Console.WriteLine();

// ============================================================================
// 6. NESTED POLYMORPHIC OBJECTS
// ============================================================================
Console.WriteLine("--- 6. Nested Polymorphic Objects ---");

PetOwner owner = new()
{
    OwnerName = "Alice",
    FavoritePet = new HouseCat { Name = "Luna", LivesLeft = 9, HasFur = true, Indoor = false }
};

string ownerJson = JsonSerializer.Serialize(owner, polyOptions);
Console.WriteLine($"Owner JSON: {ownerJson}");

PetOwner? deserializedOwner = JsonSerializer.Deserialize<PetOwner>(ownerJson, polyOptions);
Console.WriteLine($"Owner: {deserializedOwner?.OwnerName}, Pet: {deserializedOwner?.FavoritePet?.GetType().Name} ({deserializedOwner?.FavoritePet?.Name})");
Console.WriteLine();

// ============================================================================
// 7. POLYMORPHIC ERROR HANDLING
// ============================================================================
Console.WriteLine("--- 7. Polymorphic Error Handling ---");

// Missing $type
string missingTypeJson = "{\"name\":\"Ghost\"}";
try
{
    JsonSerializer.Deserialize<Animal>(missingTypeJson, polyOptions);
}
catch (JsonException ex)
{
    Console.WriteLine($"Missing $type error (expected): {ex.Message}");
}

// Unknown $type
string unknownTypeJson = "{\"$type\":\"UnknownType\",\"name\":\"Ghost\"}";
try
{
    JsonSerializer.Deserialize<Animal>(unknownTypeJson, polyOptions);
}
catch (JsonException ex)
{
    Console.WriteLine($"Unknown $type error (expected): {ex.Message}");
}

// Null value
string nullJson = "null";
Animal? nullAnimal = JsonSerializer.Deserialize<Animal>(nullJson, polyOptions);
Console.WriteLine($"Null deserialized: {nullAnimal == null}");
Console.WriteLine();

// ============================================================================
// 8. MULTI-FORMAT DATE TIME CONVERTER - EXHAUSTIVE
// ============================================================================
Console.WriteLine("--- 8. MultiFormatDateTimeConverter Exhaustive ---");

JsonSerializerOptions dateOpts = EnhancedJsonSerializerOptions.DefaultOptionsWithDateTimeConverter;

// ISO 8601
TestDateFormat("{\"date\":\"2025-01-15T10:00:00\"}", "ISO 8601");

// US format
TestDateFormat("{\"date\":\"01/15/2025 10:00:00\"}", "US format");

// European format
TestDateFormat("{\"date\":\"15/03/2025 14:30:00\"}", "European format");

// Unix timestamp
TestDateFormat("{\"date\":\"1705312800\"}", "Unix timestamp");

// Compact
TestDateFormat("{\"date\":\"20250115\"}", "Compact yyyyMMdd");

// Human readable
TestDateFormat("{\"date\":\"15 March 2025\"}", "Human readable");

// With timezone offset
TestDateFormat("{\"date\":\"2025-01-15T10:00:00+03:00\"}", "With timezone offset");

// Null DateTime (nullable)
string nullDateJson = "{\"optionalDate\":null}";
NullableDateDto? nullDateDto = nullDateJson.ConvertFromJson<NullableDateDto>(dateOpts);
Console.WriteLine($"Null nullable DateTime: {nullDateDto?.OptionalDate.HasValue == false}");
Console.WriteLine();

// ============================================================================
// 9. CONDITIONAL STRING ENUM CONVERTER - COMPLEX SCENARIOS
// ============================================================================
Console.WriteLine("--- 9. ConditionalStringEnumConverter Complex ---");

// Mixed enums in same object
MixedEnumDto mixed = new()
{
    UserStatus = UserStatus.Suspended,
    OrderPriority = OrderPriority.Critical,
    PlainStatus = PlainStatus.Disabled
};
string mixedJson = mixed.ConvertToJson();
Console.WriteLine($"Mixed enums: {mixedJson}");

// Enum array
string enumArrayJson = JsonSerializer.Serialize(new[] { UserStatus.Active, UserStatus.Inactive }, defaultOpts);
Console.WriteLine($"Enum array: {enumArrayJson}");

// Enum with duplicate value behavior
string priorityJson = JsonSerializer.Serialize(OrderPriority.High, defaultOpts);
Console.WriteLine($"Priority as int (no [StringEnum]): {priorityJson}");

string statusJson = JsonSerializer.Serialize(UserStatus.Active, defaultOpts);
Console.WriteLine($"Status as string ([StringEnum]): {statusJson}");
Console.WriteLine();

// ============================================================================
// 10. JSON DOCUMENT NAVIGATION - COMPLEX
// ============================================================================
Console.WriteLine("--- 10. Json Document Navigation Complex ---");

string complexJson = """
{
    "company": {
        "departments": [
            {
                "name": "Engineering",
                "employees": [
                    { "id": 1, "name": "Alice", "skills": ["C#", "TypeScript"] },
                    { "id": 2, "name": "Bob", "skills": ["Python", "Go"] }
                ]
            }
        ],
        "metadata": {
            "founded": 2010,
            "active": true,
            "rating": 4.8
        }
    }
}
""";

using JsonDocument complexDoc = JsonDocument.Parse(complexJson);

// TryGetNestedProperty deep object path
var companyResult = complexDoc.TryGetNestedProperty("company", "metadata", "founded");
companyResult.Switch(
    onSuccess: v => Console.WriteLine($"Deep nested founded year: {v!.Value.GetInt32()}"),
    onError: e => Console.WriteLine($"Error: {e[0].Description}")
);

// TryGetNestedProperty on JsonElement via JsonDocument
var metaResult = complexDoc.TryGetNestedProperty("company", "metadata", "founded");
metaResult.Switch(
    onSuccess: v => Console.WriteLine($"Metadata founded: {v!.Value.GetInt32()}"),
    onError: e => Console.WriteLine($"Error: {e[0].Description}")
);

// Missing nested path
var missingDeep = complexDoc.TryGetNestedProperty("company", "metadata", "missing_property");
missingDeep.Switch(
    onSuccess: v => Console.WriteLine($"Found: {v}"),
    onError: e => Console.WriteLine($"Missing deep path (expected): {e[0].Description}")
);

// Null document
JsonDocument? nullDoc = null;
var nullDocResult = nullDoc!.TryGetNestedProperty("test");
nullDocResult.Switch(
    onSuccess: v => Console.WriteLine($"Found: {v}"),
    onError: e => Console.WriteLine($"Null document error (expected): {e[0].Description}")
);

// Empty property names
var emptyProps = complexDoc.TryGetNestedProperty(Array.Empty<string>());
emptyProps.Switch(
    onSuccess: v => Console.WriteLine($"Found: {v}"),
    onError: e => Console.WriteLine($"Empty props error (expected): {e[0].Description}")
);
Console.WriteLine();

// ============================================================================
// 11. MALFORMED JSON HANDLING
// ============================================================================
Console.WriteLine("--- 11. Malformed JSON Handling ---");

string malformed = "{\"name\":\"Alice\",\"age\":}";
try
{
    malformed.ConvertFromJson<Person>();
}
catch (JsonException ex)
{
    Console.WriteLine($"Malformed JSON caught (expected): {ex.Message[..50]}...");
}

string incomplete = "{\"name\":\"Alice\"";
try
{
    incomplete.ConvertFromJson<Person>();
}
catch (JsonException ex)
{
    Console.WriteLine($"Incomplete JSON caught (expected): {ex.Message[..50]}...");
}
Console.WriteLine();

// ============================================================================
// 12. LARGE PAYLOAD PERFORMANCE
// ============================================================================
Console.WriteLine("--- 12. Large Payload ---");

List<Person> people = new();
for (int i = 0; i < 1000; i++)
{
    people.Add(new Person { Name = $"Person{i}", Age = i % 100, City = $"City{i % 50}" });
}

string bigJson = people.ConvertToJson();
Console.WriteLine($"Serialized {people.Count} people, JSON length: {bigJson.Length} chars");

List<Person>? bigBack = bigJson.ConvertFromJson<List<Person>>();
Console.WriteLine($"Deserialized back: {bigBack?.Count} people");
Console.WriteLine();

// ============================================================================
// 13. JSON ELEMENT EXTRACTION
// ============================================================================
Console.WriteLine("--- 13. JsonElement Extraction ---");

string dataJson = """{"id":42,"active":true,"score":98.6,"tags":["a","b","c"]}""";
using JsonDocument dataDoc = JsonDocument.Parse(dataJson);
JsonElement root = dataDoc.RootElement;

Console.WriteLine($"id (int): {root.GetProperty("id").GetInt32()}");
Console.WriteLine($"active (bool): {root.GetProperty("active").GetBoolean()}");
Console.WriteLine($"score (double): {root.GetProperty("score").GetDouble()}");

JsonElement tags = root.GetProperty("tags");
Console.WriteLine($"tags array length: {tags.GetArrayLength()}");
foreach (JsonElement tag in tags.EnumerateArray())
{
    Console.WriteLine($"  tag: {tag.GetString()}");
}
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("All advanced scenarios completed.");
Console.WriteLine("========================================");

// ============================================================================
// HELPERS
// ============================================================================

static void TestDateFormat(string json, string description)
{
    try
    {
        DateWrapper? result = json.ConvertFromJson<DateWrapper>(EnhancedJsonSerializerOptions.DefaultOptionsWithDateTimeConverter);
        Console.WriteLine($"  {description}: {result?.Date:O}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {description}: FAILED - {ex.Message[..60]}...");
    }
}

// ============================================================================
// MODELS
// ============================================================================

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string City { get; set; } = string.Empty;
}

public abstract class Animal
{
    public string Name { get; set; } = string.Empty;
}

public class Mammal : Animal
{
    public bool HasFur { get; set; }
}

public class Cat : Mammal
{
    public int LivesLeft { get; set; }
}

public class HouseCat : Cat
{
    public bool Indoor { get; set; }
}

public class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}

public class PetOwner
{
    public string OwnerName { get; set; } = string.Empty;
    public Animal FavoritePet { get; set; } = null!;
}

public class EventDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class DateWrapper
{
    public DateTime Date { get; set; }
}

public class NullableDateDto
{
    public DateTime? OptionalDate { get; set; }
}

[StringEnum]
public enum UserStatus
{
    Active,
    Inactive,
    Suspended
}

public enum OrderPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum PlainStatus
{
    Enabled,
    Disabled
}

public class MixedEnumDto
{
    public UserStatus UserStatus { get; set; }
    public OrderPriority OrderPriority { get; set; }
    public PlainStatus PlainStatus { get; set; }
}

public class UserDto
{
    public string Name { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public OrderPriority Priority { get; set; }
}
