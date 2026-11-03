using System.Text.Json;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public sealed class JsonConvertOptionsTests
{
    private static readonly JsonSerializerOptions SnakeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public sealed record Person(string FirstName, int YearsOld);

    [Fact]
    public void ConvertToJson_Should_Use_The_Given_Options()
    {
        new Person("Ada", 36).ConvertToJson(SnakeOptions).Should().Be("""{"first_name":"Ada","years_old":36}""");
    }

    [Fact]
    public void ConvertToJsonDocument_Generic_Should_Use_The_Given_Options()
    {
        using JsonDocument document = new Person("Ada", 36).ConvertToJsonDocument(SnakeOptions);

        document.RootElement.GetProperty("first_name").GetString().Should().Be("Ada");
        document.RootElement.GetProperty("years_old").GetInt32().Should().Be(36);
    }

    [Fact]
    public void ConvertFromJson_Generic_Should_Use_The_Given_Options()
    {
        Person? person = """{"first_name":"Ada","years_old":36}""".ConvertFromJson<Person>(SnakeOptions);

        person.Should().Be(new Person("Ada", 36));
    }

    [Fact]
    public void ConvertFromJson_NonGeneric_Should_Use_The_Given_Options()
    {
        Type returnType = new Person("Ada", 36).GetType();

        object? person = """{"first_name":"Ada","years_old":36}""".ConvertFromJson(returnType, SnakeOptions);

        person.Should().Be(new Person("Ada", 36));
    }
}
