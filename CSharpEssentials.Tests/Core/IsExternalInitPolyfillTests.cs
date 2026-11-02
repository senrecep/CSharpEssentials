using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public class IsExternalInitPolyfillTests
{
    private const string IsExternalInitName = "System.Runtime.CompilerServices.IsExternalInit";

    [Fact]
    public void Core_ShouldForwardIsExternalInit_InsteadOfDefiningIt()
    {
        System.Reflection.Assembly core = typeof(Guider).Assembly;

        core.GetTypes().Should().NotContain(t => t.FullName == IsExternalInitName);
        core.GetForwardedTypes().Should().Contain(t => t.FullName == IsExternalInitName);
    }
}
