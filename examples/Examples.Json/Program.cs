using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

Section("Option presets");
var person = new Person("Ada", 36, null);
Console.WriteLine($"DefaultOptions                   {JsonSerializer.Serialize(person, EnhancedJsonSerializerOptions.DefaultOptions)}");
Console.WriteLine($"DefaultOptionsWithoutConverters  {JsonSerializer.Serialize(person, EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters)}");
JsonSerializerOptions indented = EnhancedJsonSerializerOptions.DefaultOptions.Create(o => o.WriteIndented = true);
Console.WriteLine($"Create(WriteIndented)            {JsonSerializer.Serialize(person, indented).Split('\n').Length} lines");
JsonSerializerOptions withConverter = EnhancedJsonSerializerOptions.CreateOptionsWithConverters(new JsonStringEnumConverter());
Console.WriteLine($"CreateOptionsWithConverters      {JsonSerializer.Serialize(Level.High, withConverter)}");
var aspNetOptions = new JsonSerializerOptions();
EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(aspNetOptions);
Console.WriteLine($"ApplyTo                          {JsonSerializer.Serialize(person, aspNetOptions)}");
var other = new JsonSerializerOptions();
other.ApplyFrom(EnhancedJsonSerializerOptions.DefaultOptionsWithDateTimeConverter);
Console.WriteLine($"ApplyFrom                        naming policy copied: {other.PropertyNamingPolicy is not null}");
try
{
    JsonSerializer.Deserialize<Person>("""{"name":"Ada","age":36,"extra":1}""", EnhancedJsonSerializerOptions.StrictOptions);
}
catch (JsonException)
{
    Console.WriteLine("StrictOptions                    unknown property rejected");
}

Section("Extensions");
string json = person.ConvertToJson();
Person? back = json.ConvertFromJson<Person>();
object? untyped = json.ConvertFromJson(typeof(Person));
using JsonDocument fromValue = person.ConvertToJsonDocument();
using JsonDocument? fromText = "not json".ConvertToJsonDocument();
Console.WriteLine($"ConvertToJson / ConvertFromJson  {json} -> {back}");
Console.WriteLine($"ConvertFromJson(Type)            {untyped?.GetType().Name}");
Console.WriteLine($"ConvertToJsonDocument<T>         name = {fromValue.RootElement.GetProperty("name").GetString()}");
Console.WriteLine($"string.ConvertToJsonDocument()   {fromText?.RootElement.ValueKind}");
try
{
    "{ broken".ConvertFromJson<Person>();
}
catch (JsonException)
{
    Console.WriteLine("Malformed JSON                   JsonException");
}

Section("JsonElement.ToClrObject");
using (JsonDocument doc = JsonDocument.Parse("""{"id":1,"big":5000000000,"price":9.99,"tags":["a","b"],"active":true,"note":null}"""))
{
    var values = (Dictionary<string, object?>)doc.RootElement.ToClrObject()!;
    Console.WriteLine(string.Join(", ", values.Select(kv => $"{kv.Key}:{kv.Value?.GetType().Name ?? "null"}")));
}

Section("Required value-type constructor parameters");
JsonSerializerOptions requiring = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
    .RequireValueTypeConstructorParameters();
Console.WriteLine($"with quantity    {JsonSerializer.Deserialize<CreateOrder>("""{"quantity":2}""", requiring)}");
try
{
    JsonSerializer.Deserialize<CreateOrder>("""{"note":"gift"}""", requiring);
}
catch (JsonException)
{
    Console.WriteLine("without quantity JsonException instead of Quantity = 0");
}

Section("Enum conventions");
JsonSerializerOptions data = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default);
JsonSerializerOptions input = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);
JsonSerializerOptions numbers = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default, writeAs: EnumWireFormat.Number);
var account = new Account(UserStatus.PendingReview, Level.High, new Dictionary<UserStatus, int> { [UserStatus.Active] = 3 });
Console.WriteLine($"Data write     {JsonSerializer.Serialize(account, data)}");
Console.WriteLine($"Number write   {JsonSerializer.Serialize(account, numbers)}");
Console.WriteLine($"Data read      {JsonSerializer.Deserialize<UserStatus>("\"banned\"", data)} (fallback)");
try
{
    JsonSerializer.Deserialize<UserStatus>("\"banned\"", input);
}
catch (EnumValueJsonException ex)
{
    Console.WriteLine($"Input read     {ex.Error.Message}");
}

Section("Polymorphism with [JsonTypeDiscriminator]");
JsonSerializerOptions events = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
    .AddPolymorphism();
IEvent[] stream = [new OrderPlaced(1, 10m), new UserCreated(2, "Ada")];
string eventJson = JsonSerializer.Serialize(stream, events);
Console.WriteLine(eventJson);
IEvent[] readEvents = JsonSerializer.Deserialize<IEvent[]>("""[{"id":5,"total":3,"$event":"order.placed.v1"}]""", events)!;
Console.WriteLine($"discriminator last: {readEvents[0]}");
var form = new Form { Value = new TextValue { Text = "a" }, Other = new NumberValue { Number = 1 } };
Console.WriteLine($"[JsonPolymorphicProperty] {JsonSerializer.Serialize(form, events)}");
try
{
    JsonSerializer.Deserialize<IEvent>("""{"$event":"order.cancelled.v9"}""", events);
}
catch (JsonException)
{
    Console.WriteLine("unknown discriminator: JsonException");
}

Section("Source-generated context (Native AOT path)");
JsonSerializerOptions aot = new JsonSerializerOptions(AppJsonContext.Default.Options).AddPolymorphism();
var eventInfo = (JsonTypeInfo<IEvent>)aot.GetTypeInfo(typeof(IEvent));
Console.WriteLine(JsonSerializer.Serialize<IEvent>(new UserCreated(3, "Grace"), eventInfo));

Section("[JsonAcceptClrTypeNames] reads old CLR names");
IShape? oldShape = JsonSerializer.Deserialize<IShape>("""{"$type":"Circle","radius":2}""", EnhancedJsonSerializerOptions.DefaultOptions);
Console.WriteLine($"read {oldShape}, written as {JsonSerializer.Serialize(oldShape, EnhancedJsonSerializerOptions.DefaultOptions)}");

Section("Bases without attributes ($type)");
Animal[] zoo = [new Dog { Name = "Rex", Breed = "Labrador" }, new Cat { Name = "Tom", Indoor = true }];
string zooJson = zoo.ConvertToJson();
Console.WriteLine(zooJson);
Console.WriteLine(string.Join(", ", zooJson.ConvertFromJson<Animal[]>()!.Select(a => a.GetType().Name)));
try
{
    """{"name":"x"}""".ConvertFromJson<Animal>();
}
catch (JsonException)
{
    Console.WriteLine("missing $type: JsonException");
}

Section("Multi-format DateTime");
foreach (string text in new[] { "\"2024-03-14T15:30:45Z\"", "\"2024-03-14T15:30:45+03:00\"", "\"14.03.2024\"", "\"March 14, 2024\"", "\"03/04/2024\"", "\"20240314\"", "\"1710430245\"" })
    Console.WriteLine($"{text,-30} {text.ConvertFromJson<DateTime>():yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"null into DateTime?            {"null".ConvertFromJson<DateTime?>()?.ToString() ?? "null"}");
var custom = new JsonSerializerOptions();
custom.Converters.Add(new MultiFormatDateTimeConverterFactory("dd.MM.yyyy HH-mm"));
Console.WriteLine($"custom format                  {JsonSerializer.Deserialize<DateTime>("\"14.03.2024 10-20\"", custom):yyyy-MM-dd HH:mm}");
try
{
    "\"someday\"".ConvertFromJson<DateTime>();
}
catch (JsonException)
{
    Console.WriteLine("unknown format                 JsonException");
}

static void Section(string title) => Console.WriteLine($"\n--- {title} ---");

public sealed record Person(string Name, int Age, string? Email);

public sealed record CreateOrder(int Quantity, string? Note);

public enum Level { Low, High }

[StringEnum]
public enum UserStatus { Active, PendingReview, [EnumFallback] Unknown }

public sealed record Account(UserStatus Status, Level Level, Dictionary<UserStatus, int> Counts);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
public interface IEvent;

[JsonTypeDiscriminator("order.placed.v1")]
public sealed record OrderPlaced(int Id, decimal Total) : IEvent;

[JsonTypeDiscriminator("user.created.v1")]
public sealed record UserCreated(int Id, string Name) : IEvent;

[JsonPolymorphic]
public interface IValue;

[JsonTypeDiscriminator("text")]
public sealed class TextValue : IValue { public string Text { get; set; } = ""; }

[JsonTypeDiscriminator("number")]
public sealed class NumberValue : IValue { public decimal Number { get; set; } }

public sealed class Form
{
    [JsonPolymorphicProperty("kind")]
    public IValue? Value { get; set; }

    public IValue? Other { get; set; }
}

[JsonSerializable(typeof(IEvent))]
[JsonSerializable(typeof(OrderPlaced))]
[JsonSerializable(typeof(UserCreated))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, AllowOutOfOrderMetadataProperties = true)]
public partial class AppJsonContext : JsonSerializerContext;

[JsonPolymorphic]
[JsonAcceptClrTypeNames]
public interface IShape;

[JsonTypeDiscriminator("circle")]
public sealed record Circle(double Radius) : IShape;

public abstract class Animal { public string Name { get; set; } = ""; }

public sealed class Dog : Animal { public string Breed { get; set; } = ""; }

public sealed class Cat : Animal { public bool Indoor { get; set; } }
