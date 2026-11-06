using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public sealed class GeneralExtensionsBranchTests
{
    [Fact]
    public void IfNotNull_ParameterlessAction_WithValue_Should_Run_Action()
    {
        int calls = 0;

        bool result = "value".IfNotNull(() => calls++);

        result.Should().BeTrue();
        calls.Should().Be(1);
    }

    [Fact]
    public void IfNotNull_ParameterlessAction_WithNull_Should_Skip_Action()
    {
        string? value = null;
        int calls = 0;

        bool result = value.IfNotNull(() => calls++);

        result.Should().BeFalse();
        calls.Should().Be(0);
    }

    [Fact]
    public void IfNull_WithTypedElseAction_WhenNotNull_Should_Pass_Value_To_Else()
    {
        bool ifExecuted = false;
        string? received = null;

        bool result = "value".IfNull(() => ifExecuted = true, v => received = v);

        result.Should().BeFalse();
        ifExecuted.Should().BeFalse();
        received.Should().Be("value");
    }
}
