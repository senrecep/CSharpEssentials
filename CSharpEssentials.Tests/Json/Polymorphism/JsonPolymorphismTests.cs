using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Json;
using CSharpEssentials.Tests.Fixtures.JsonCore;
using CSharpEssentials.Tests.Fixtures.JsonEvents;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json.Polymorphism;

public class JsonPolymorphismTests
{
    private static readonly Guid Id = Guid.Parse("8d4f1c7e-35f0-4a51-9a35-6f0a3c6b1d22");

    private static readonly JsonSerializerOptions Generated =
        new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }.AddPolymorphism();

    private static readonly JsonSerializerOptions Scanned =
        new JsonSerializerOptions(JsonSerializerDefaults.Web).AddPolymorphismWithReflection(typeof(JsonPolymorphismTests).Assembly, typeof(FixtureOrderPlaced).Assembly);

    private static readonly JsonSerializerOptions Context =
        new JsonSerializerOptions(PolymorphismTestContext.Default.Options).AddPolymorphism();

    [Fact]
    public void Generated_Should_Write_The_Custom_Property_Name_And_Value()
    {
        string json = JsonSerializer.Serialize<ITestEvent>(new UserCreated(Id), Generated);

        json.Should().Be($$"""{"$event":"user.created.v1","id":"{{Id}}"}""");
        JsonSerializer.Deserialize<ITestEvent>(json, Generated).Should().Be(new UserCreated(Id));
    }

    [Fact]
    public void Generated_Should_Round_Trip_Integer_Discriminators()
    {
        string json = JsonSerializer.Serialize<ITestEvent>(new UserDeleted(Id), Generated);

        json.Should().StartWith("""{"$event":7,""");
        JsonSerializer.Deserialize<ITestEvent>(json, Generated).Should().Be(new UserDeleted(Id));
    }

    [Fact]
    public void Generated_Should_Leave_Out_Derived_Types_Without_A_Value()
    {
        Action act = () => JsonSerializer.Serialize<ITestEvent>(new UserRenamed(Id, "a"), Generated);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Generated_Should_Read_A_Discriminator_That_Is_Not_First()
    {
        string json = $$"""{"id":"{{Id}}","total":12.5,"$event":"order.placed.v1"}""";

        JsonSerializer.Deserialize<IFixtureEvent>(json, Generated).Should().Be(new FixtureOrderPlaced(Id, 12.5m));
    }

    [Fact]
    public void Generated_Should_Register_Events_From_Another_Assembly_Through_The_Chain()
    {
        string json = JsonSerializer.Serialize<IFixtureEvent>(new FixtureOrderCancelled(Id, "late"), Generated);

        json.Should().Be($$"""{"$event":"order.cancelled.v1","id":"{{Id}}","reason":"late"}""");
        JsonSerializer.Deserialize<IFixtureEvent>(json, Generated).Should().Be(new FixtureOrderCancelled(Id, "late"));
    }

    [Fact]
    public void Generated_Should_Throw_JsonException_For_An_Unknown_Discriminator()
    {
        Action act = () => JsonSerializer.Deserialize<IFixtureEvent>("""{"$event":"order.unknown.v1"}""", Generated);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Generated_Should_Throw_Naming_Both_Types_For_A_Duplicate_Across_Assemblies()
    {
        Action act = () => JsonSerializer.Serialize<IFixtureDuplicateEvent>(new TestDuplicateTwo(), Generated);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'duplicate'*FixtureDuplicateOne, CSharpEssentials.Tests.Fixtures.JsonEvents*TestDuplicateTwo, CSharpEssentials.Tests*");
    }

    [Fact]
    public void Generated_Should_Add_Opt_In_Types_To_A_Closed_Base_And_Keep_The_Base_Value()
    {
        string triangle = JsonSerializer.Serialize<ClosedShape>(new ClosedTriangle { Height = 2 }, Generated);
        string square = JsonSerializer.Serialize<ClosedShape>(new ClosedSquare { Side = 3 }, Generated);

        triangle.Should().Be("""{"t":"tri","height":2}""");
        square.Should().Be("""{"t":"square","side":3}""");
        JsonSerializer.Deserialize<ClosedShape>(triangle, Generated).Should().BeOfType<ClosedTriangle>().Which.Height.Should().Be(2);
        JsonSerializer.Deserialize<ClosedShape>("""{"t":"circle","radius":1}""", Generated).Should().BeOfType<ClosedCircle>();
    }

    [Fact]
    public void Generated_Should_Use_The_Default_Property_For_JsonDerivedType_Without_JsonPolymorphic()
    {
        string json = JsonSerializer.Serialize<Pet>(new Dog { Name = "Rex" }, Generated);

        json.Should().Be("""{"$type":"dog","name":"Rex"}""");
        JsonSerializer.Deserialize<Pet>(json, Generated).Should().BeOfType<Dog>().Which.Name.Should().Be("Rex");
    }

    [Fact]
    public void Generated_Should_Support_A_Concrete_Base()
    {
        string json = JsonSerializer.Serialize<Animal>(new Bird { Name = "Tweety", CanFly = true }, Generated);

        json.Should().Be("""{"$type":"bird","canFly":true,"name":"Tweety"}""");
        JsonSerializer.Deserialize<Animal>(json, Generated).Should().BeOfType<Bird>().Which.CanFly.Should().BeTrue();
        JsonSerializer.Serialize(new Animal { Name = "x" }, Generated).Should().Be("""{"name":"x"}""");
    }

    [Fact]
    public void Generated_Should_Throw_For_A_Base_Without_Derived_Types()
    {
        Action act = () => JsonSerializer.Serialize<IOrphan?>(null, Generated);

        act.Should().Throw<InvalidOperationException>().WithMessage("*IOrphan' has no derived types*");
    }

    [Fact]
    public void Generated_Should_Use_The_Property_Name_On_Properties_Lists_Arrays_And_Dictionaries()
    {
        ValueHolder holder = new()
        {
            Value = new TextValue { Text = "a" },
            Items = [new NumberValue { Number = 2 }],
            Array = [new TextValue { Text = "b" }, null],
            Map = new() { ["k"] = new NumberValue { Number = 3 } },
            Other = new NumberValue { Number = 1 },
        };

        string json = JsonSerializer.Serialize(holder, Generated);

        json.Should().Be("""{"value":{"values":"text","text":"a"},"items":[{"values":"number","number":2}],"array":[{"kind":"text","text":"b"},null],"map":{"k":{"kind":"number","number":3}},"other":{"$type":"number","number":1},"child":null}""");
        ValueHolder back = JsonSerializer.Deserialize<ValueHolder>(json, Generated)!;
        back.Value.Should().BeOfType<TextValue>().Which.Text.Should().Be("a");
        back.Items.Should().ContainSingle().Which.Should().BeOfType<NumberValue>();
        back.Array[0].Should().BeOfType<TextValue>();
        back.Array[1].Should().BeNull();
        back.Map["k"].Should().BeOfType<NumberValue>().Which.Number.Should().Be(3);
        back.Other.Should().BeOfType<NumberValue>();
    }

    [Fact]
    public void Generated_Should_Apply_The_Property_Name_To_Inherited_And_Nested_Properties()
    {
        SpecialValueHolder holder = new()
        {
            Extra = "e",
            Value = new TextValue { Text = "a" },
            Child = new ValueHolder { Value = new NumberValue { Number = 5 } },
        };

        string json = JsonSerializer.Serialize<ValueHolder>(holder, Generated);
        string child = JsonSerializer.Serialize(holder.Child, Generated);

        json.Should().Contain("""{"values":"text","text":"a"}""");
        json.Should().Contain("""{"values":"number","number":5}""");
        child.Should().Contain("""{"values":"number","number":5}""");
        JsonSerializer.Deserialize<SpecialValueHolder>(JsonSerializer.Serialize(holder, Generated), Generated)!
            .Child!.Value.Should().BeOfType<NumberValue>().Which.Number.Should().Be(5);
    }

    [Fact]
    public void AddPolymorphism_Should_Throw_Without_A_Resolver()
    {
        Action act = () => new JsonSerializerOptions().AddPolymorphism();

        act.Should().Throw<InvalidOperationException>().WithMessage("*TypeInfoResolver*");
    }

    [Fact]
    public void Context_Should_Round_Trip_With_Generated_Registrations()
    {
        string json = JsonSerializer.Serialize<IFixtureEvent>(new FixtureOrderPlaced(Id, 3), Context);

        json.Should().Be($$"""{"$event":"order.placed.v1","id":"{{Id}}","total":3}""");
        JsonSerializer.Deserialize<IFixtureEvent>($$"""{"total":3,"id":"{{Id}}","$event":"order.placed.v1"}""", Context)
            .Should().Be(new FixtureOrderPlaced(Id, 3));
    }

    [Fact]
    public void Context_Should_Leave_Out_A_Derived_Type_It_Does_Not_List()
    {
        JsonSerializer.Serialize<ITestEvent>(new UserCreated(Id), Context).Should().StartWith("""{"$event":"user.created.v1",""");

        Action act = () => JsonSerializer.Serialize<ITestEvent>(new UserDeleted(Id), Context);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Context_Should_Apply_Property_Names()
    {
        ValueHolder holder = new() { Value = new TextValue { Text = "a" }, Items = [new NumberValue { Number = 1 }] };

        string json = JsonSerializer.Serialize(holder, Context);

        json.Should().Contain("""{"values":"text","text":"a"}""").And.Contain("""[{"values":"number","number":1}]""");
        JsonSerializer.Deserialize<ValueHolder>(json, Context)!.Items.Should().ContainSingle().Which.Should().BeOfType<NumberValue>();
    }

    [Fact]
    public void Scan_Should_Give_Derived_Types_Without_A_Value_Their_Full_Name()
    {
        string json = JsonSerializer.Serialize<ITestEvent>(new UserRenamed(Id, "a"), Scanned);

        json.Should().StartWith("""{"$event":"CSharpEssentials.Tests.Json.Polymorphism.UserRenamed",""");
        JsonSerializer.Deserialize<ITestEvent>(json, Scanned).Should().Be(new UserRenamed(Id, "a"));
        JsonSerializer.Serialize<ITestEvent>(new UserCreated(Id), Scanned).Should().StartWith("""{"$event":"user.created.v1",""");
    }

    [Fact]
    public void Scan_Should_Only_Look_In_The_Trusted_Assemblies()
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddPolymorphismWithReflection(typeof(FixtureOrderPlaced).Assembly);

        Action act = () => JsonSerializer.Serialize<ITestEvent>(new UserRenamed(Id, "a"), options);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Scan_Should_Read_The_Property_Name_Of_Types_Generated_Code_Cannot_Reach()
    {
        HiddenHolder holder = new() { Value = new TextValue { Text = "a" } };

        string json = JsonSerializer.Serialize(holder, Scanned);

        json.Should().Be("""{"value":{"k":"text","text":"a"}}""");
        JsonSerializer.Deserialize<HiddenHolder>(json, Scanned)!.Value.Should().BeOfType<TextValue>();
    }

    [Fact]
    public void AcceptClrTypeNames_Should_Read_Full_Names_And_Write_The_Current_Value()
    {
        LegacyCreated value = new() { Name = "a" };

        string json = JsonSerializer.Serialize<ILegacyEvent>(value, Scanned);
        ILegacyEvent? legacy = JsonSerializer.Deserialize<ILegacyEvent>(
            """{"name":"b","$type":"CSharpEssentials.Tests.Json.Polymorphism.LegacyCreated"}""", Scanned);
        ILegacyEvent? current = JsonSerializer.Deserialize<ILegacyEvent>(json, Scanned);

        json.Should().Be("""{"$type":"legacy.created","name":"a"}""");
        legacy.Should().BeOfType<LegacyCreated>().Which.Name.Should().Be("b");
        current.Should().BeOfType<LegacyCreated>().Which.Name.Should().Be("a");
    }

    [Fact]
    public void AcceptClrTypeNames_Should_Work_With_StrictOptions()
    {
        ILegacyEvent? legacy = JsonSerializer.Deserialize<ILegacyEvent>(
            """{"$type":"CSharpEssentials.Tests.Json.Polymorphism.LegacyCreated","name":"b"}""", EnhancedJsonSerializerOptions.StrictOptions);

        legacy.Should().BeOfType<LegacyCreated>().Which.Name.Should().Be("b");
    }

    [Fact]
    public void Without_AcceptClrTypeNames_A_Full_Name_Should_Not_Read()
    {
        Action act = () => JsonSerializer.Deserialize<ITestEvent>(
            $$"""{"$event":"CSharpEssentials.Tests.Json.Polymorphism.UserCreated","id":"{{Id}}"}""", Generated);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void DefaultOptions_Should_Use_The_New_Path_For_Attributed_Bases()
    {
        JsonSerializerOptions options = EnhancedJsonSerializerOptions.DefaultOptions;

        string json = JsonSerializer.Serialize<IFixtureEvent>(new FixtureOrderPlaced(Id, 1), options);

        json.Should().Be($$"""{"$event":"order.placed.v1","id":"{{Id}}","total":1}""");
        JsonSerializer.Deserialize<IFixtureEvent>($$"""{"id":"{{Id}}","$event":"order.placed.v1","total":1}""", options)
            .Should().Be(new FixtureOrderPlaced(Id, 1));
        options.AllowOutOfOrderMetadataProperties.Should().BeTrue();
    }

    [Fact]
    public void DefaultOptions_Should_Keep_The_Legacy_Output_For_Unattributed_Bases()
    {
        BaseShape shape = new Circle { Radius = 2, Type = "c" };

        string json = JsonSerializer.Serialize(shape, EnhancedJsonSerializerOptions.DefaultOptions);

        json.Should().Be("""{"$type":"CSharpEssentials.Tests.Json.Circle","radius":2,"type":"c"}""");
    }

    [Fact]
    public void DefaultOptions_Should_Not_Add_A_Discriminator_To_Values_Typed_As_Object()
    {
        Dictionary<string, object> map = new() { ["user"] = new PlainRecord("a") };

        string json = JsonSerializer.Serialize(map, EnhancedJsonSerializerOptions.DefaultOptions);
        string root = JsonSerializer.Serialize<object>(new PlainRecord("b"), EnhancedJsonSerializerOptions.DefaultOptions);

        json.Should().Be("""{"user":{"name":"a"}}""");
        root.Should().Be("""{"name":"b"}""");
    }

    [Fact]
    public void StrictOptions_Should_Round_Trip_An_Attributed_Hierarchy()
    {
        JsonSerializerOptions options = EnhancedJsonSerializerOptions.StrictOptions;

        string json = JsonSerializer.Serialize<IFixtureEvent>(new FixtureOrderCancelled(Id, "x"), options);

        JsonSerializer.Deserialize<IFixtureEvent>(json, options).Should().Be(new FixtureOrderCancelled(Id, "x"));
    }

    [Fact]
    public void Registry_Should_Ignore_A_Repeated_Registration()
    {
        Action act = () => JsonPolymorphism.Register(
        [
            PolymorphicRegistration.DerivedType(typeof(ITestEvent), typeof(UserCreated), "user.created.v1"),
        ]);

        act.Should().NotThrow();
        JsonSerializer.Serialize<ITestEvent>(new UserCreated(Id), new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }.AddPolymorphism())
            .Should().StartWith("""{"$event":"user.created.v1",""");
    }

    [Fact]
    public void Scan_Should_Skip_Abstract_Generic_And_Converter_Types()
    {
        Assembly assembly = typeof(JsonPolymorphismTests).Assembly;
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddPolymorphismWithReflection(assembly);

        string json = JsonSerializer.Serialize<IScanned>(new ScannedLeaf(), options);

        json.Should().Be("""{"$type":"CSharpEssentials.Tests.Json.Polymorphism.ScannedLeaf"}""");
        JsonSerializer.Deserialize<IScanned>(json, options).Should().BeOfType<ScannedLeaf>();
    }

    private sealed class HiddenHolder
    {
        [JsonPolymorphicProperty("k")]
        public IValue? Value { get; set; }
    }
}

public sealed record PlainRecord(string Name);

[JsonPolymorphic]
public interface IScanned;

public sealed class ScannedLeaf : IScanned;

public abstract class ScannedAbstract : IScanned;

public sealed class ScannedGeneric<T> : IScanned;

[JsonConverter(typeof(ScannedConverter))]
public sealed class ScannedWithConverter : IScanned;

public sealed class ScannedConverter : JsonConverter<ScannedWithConverter>
{
    public override ScannedWithConverter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new();

    public override void Write(Utf8JsonWriter writer, ScannedWithConverter value, JsonSerializerOptions options) => writer.WriteNullValue();
}
