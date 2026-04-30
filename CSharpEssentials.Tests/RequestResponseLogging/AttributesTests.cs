using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public class AttributesTests
{
    [Fact]
    public void SkipRequestLoggingAttribute_ShouldBeApplicableToClassAndMethod()
    {
        Type attributeType = typeof(SkipRequestLoggingAttribute);
        var usageAttribute = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        usageAttribute.Should().NotBeNull();
        usageAttribute!.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usageAttribute.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        usageAttribute.AllowMultiple.Should().BeFalse();
        usageAttribute.Inherited.Should().BeFalse();
    }

    [SkipRequestLogging]
    private sealed class TestController
    {
    }

    [Fact]
    public void SkipRequestLoggingAttribute_ShouldBeApplicable()
    {
        Type controllerType = typeof(TestController);
        object? attribute = controllerType.GetCustomAttributes(typeof(SkipRequestLoggingAttribute), false)
            .FirstOrDefault();

        attribute.Should().NotBeNull();
    }
}
