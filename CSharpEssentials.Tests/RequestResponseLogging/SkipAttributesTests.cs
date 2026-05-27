using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public sealed class SkipAttributesTests
{
    [Fact]
    public void SkipResponseLoggingAttribute_Should_BeInstantiable()
    {
        var attribute = new SkipResponseLoggingAttribute();

        attribute.Should().NotBeNull();
        attribute.Should().BeAssignableTo<Attribute>();
    }

    [Fact]
    public void SkipResponseLoggingAttribute_Should_HaveCorrectAttributeUsage()
    {
        var usage = typeof(SkipResponseLoggingAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        usage.AllowMultiple.Should().BeFalse();
        usage.Inherited.Should().BeFalse();
    }

    [Fact]
    public void SkipRequestResponseLoggingAttribute_Should_BeInstantiable()
    {
        var attribute = new SkipRequestResponseLoggingAttribute();

        attribute.Should().NotBeNull();
        attribute.Should().BeAssignableTo<Attribute>();
    }

    [Fact]
    public void SkipRequestResponseLoggingAttribute_Should_HaveCorrectAttributeUsage()
    {
        var usage = typeof(SkipRequestResponseLoggingAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usage.ValidOn.Should().HaveFlag(AttributeTargets.Method);
        usage.AllowMultiple.Should().BeFalse();
        usage.Inherited.Should().BeFalse();
    }

    [Fact]
    public void SkipResponseLoggingAttribute_Should_BeApplicableToMethod()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.SkippedResponseAction))!;
        var attribute = methodInfo.GetCustomAttributes(typeof(SkipResponseLoggingAttribute), false);

        attribute.Should().HaveCount(1);
    }

    [Fact]
    public void SkipRequestResponseLoggingAttribute_Should_BeApplicableToClass()
    {
        var attribute = typeof(FullySkippedController)
            .GetCustomAttributes(typeof(SkipRequestResponseLoggingAttribute), false);

        attribute.Should().HaveCount(1);
    }

    [Fact]
    public void SkipResponseLoggingAttribute_Should_BeApplicableToClass()
    {
        var attribute = typeof(ResponseSkippedController)
            .GetCustomAttributes(typeof(SkipResponseLoggingAttribute), false);

        attribute.Should().HaveCount(1);
    }

    [Fact]
    public void SkipRequestResponseLoggingAttribute_Should_BeApplicableToMethod()
    {
        var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.FullySkippedAction))!;
        var attribute = methodInfo.GetCustomAttributes(typeof(SkipRequestResponseLoggingAttribute), false);

        attribute.Should().HaveCount(1);
    }

    private sealed class FakeController
    {
        [SkipResponseLogging]
        public void SkippedResponseAction() { /* Test fixture — attribute detection target. */ }

        [SkipRequestResponseLogging]
        public void FullySkippedAction() { /* Test fixture — attribute detection target. */ }
    }

    [SkipRequestResponseLogging]
    private sealed class FullySkippedController { }

    [SkipResponseLogging]
    private sealed class ResponseSkippedController { }
}
