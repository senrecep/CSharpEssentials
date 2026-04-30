using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

public class StringEnumAttributeTests
{
    [StringEnum]
    private enum TestStringValue
    {
        Value1,
        Value2,
        Value3
    }

    private enum TestValueWithoutAttribute
    {
        Value1,
        Value2
    }

    [Fact]
    public void StringEnumAttribute_ShouldBeApplicableToEnum()
    {
        Type enumType = typeof(TestStringValue);
        var attribute = enumType.GetCustomAttributes(typeof(StringEnumAttribute), false)
            .FirstOrDefault() as StringEnumAttribute;

        attribute.Should().NotBeNull();
    }

    [Fact]
    public void StringEnumAttribute_ShouldNotBePresent_WhenNotApplied()
    {
        Type enumType = typeof(TestValueWithoutAttribute);
        var attribute = enumType.GetCustomAttributes(typeof(StringEnumAttribute), false)
            .FirstOrDefault() as StringEnumAttribute;

        attribute.Should().BeNull();
    }

    [Fact]
    public void StringEnumAttribute_ShouldHaveCorrectAttributeUsage()
    {
        Type attributeType = typeof(StringEnumAttribute);
        var usageAttribute = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        usageAttribute.Should().NotBeNull();
        usageAttribute!.ValidOn.Should().Be(AttributeTargets.Enum);
        usageAttribute.AllowMultiple.Should().BeFalse();
        usageAttribute.Inherited.Should().BeFalse();
    }
}
