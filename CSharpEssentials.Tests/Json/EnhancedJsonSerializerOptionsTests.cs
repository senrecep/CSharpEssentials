using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public class EnhancedJsonSerializerOptionsTests
{
    [Fact]
    public void DefaultOptionsWithoutConverters_ShouldHaveCorrectSettings()
    {
        JsonSerializerOptions options = EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters;

        options.PropertyNameCaseInsensitive.Should().BeTrue();
        options.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
        options.WriteIndented.Should().BeFalse();
        options.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.WhenWritingNull);
    }

    [Fact]
    public void DefaultOptions_ShouldContainConverters()
    {
        JsonSerializerOptions options = EnhancedJsonSerializerOptions.DefaultOptions;

        options.Converters.Should().Contain(c => c is ConditionalStringEnumConverter);
        options.Converters.Should().Contain(c => c is MultiFormatDateTimeConverterFactory);
        options.Converters.Should().Contain(c => c is PolymorphicJsonConverterFactory);
    }

    [Fact]
    public void DefaultOptionsWithDateTimeConverter_ShouldContainDateTimeFactory()
    {
        JsonSerializerOptions options = EnhancedJsonSerializerOptions.DefaultOptionsWithDateTimeConverter;

        options.Converters.Should().Contain(c => c is MultiFormatDateTimeConverterFactory);
        options.Converters.Should().NotContain(c => c is ConditionalStringEnumConverter);
    }

    [Fact]
    public void CreateOptionsWithConverters_ShouldAddSpecifiedConverters()
    {
        JsonSerializerOptions options = EnhancedJsonSerializerOptions.CreateOptionsWithConverters(new ConditionalStringEnumConverter());

        options.Converters.Should().ContainSingle();
        options.Converters[0].Should().BeOfType<ConditionalStringEnumConverter>();
    }

    [Fact]
    public void Create_ShouldCloneAndApplyConfiguration()
    {
        JsonSerializerOptions baseOptions = EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters;

        JsonSerializerOptions newOptions = baseOptions.Create(opts => opts.WriteIndented = true);

        newOptions.WriteIndented.Should().BeTrue();
        newOptions.PropertyNameCaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public void ApplyTo_ShouldCopySettingsAndConverters()
    {
        JsonSerializerOptions source = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = false
        };
        source.Converters.Add(new ConditionalStringEnumConverter());

        JsonSerializerOptions target = new();
        source.ApplyTo(target);

        target.WriteIndented.Should().BeTrue();
        target.PropertyNameCaseInsensitive.Should().BeFalse();
        target.Converters.Should().Contain(c => c is ConditionalStringEnumConverter);
    }

    [Fact]
    public void ApplyFrom_ShouldCopySettingsAndConverters()
    {
        JsonSerializerOptions source = new()
        {
            WriteIndented = true
        };
        source.Converters.Add(new ConditionalStringEnumConverter());

        JsonSerializerOptions target = new();
        target.ApplyFrom(source);

        target.WriteIndented.Should().BeTrue();
        target.Converters.Should().Contain(c => c is ConditionalStringEnumConverter);
    }
}
