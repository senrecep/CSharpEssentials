using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Enums.Net8.Tests;

public class Net8FlagsFormatTests
{
    [Fact]
    public void ToOptimizedString_Should_Match_Enum_ToString_For_Every_Combination()
    {
        for (int raw = 0; raw < 1024; raw++)
        {
            var value = (Net8Permissions)raw;
            string expected = value.ToString();

            value.ToOptimizedString().Should().Be(expected);
            value.ToLowerCase().Should().Be(expected.ToLowerInvariant());
            value.ToUpperCase().Should().Be(expected.ToUpperInvariant());
        }

        ((Net8Permissions)(-1)).ToOptimizedString().Should().Be(((Net8Permissions)(-1)).ToString());
        (Net8Permissions.Write | Net8Permissions.Execute | Net8Permissions.Delete).ToOptimizedString().Should().Be("Write, Execute, Delete");
        (Net8Permissions.Read | Net8Permissions.Write | Net8Permissions.Delete).ToOptimizedString().Should().Be("ReadWrite, Delete");
    }
}

[StringEnum, Flags]
internal enum Net8Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    Delete = 8,
    Share = 16,
    Archive = 32,
    Audit = 64,
    Admin = 128,
    ReadWrite = Read | Write,
}
