using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Fixtures.EnumsContracts;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

/// <summary>
/// The fixture assembly holds only enum types and is referenced here by type only, so its module initializer has not run when a
/// lookup by <see cref="Type"/> reaches it.
/// </summary>
public class EnumRegistrationTests
{
    [Fact]
    public void TryGet_Should_Find_Metadata_Of_A_Type_Only_Assembly()
    {
        bool found = EnumMetadata.TryGet(typeof(ContractStatus), out IEnumInfo? info);

        found.Should().BeTrue();
        info!.EnumType.Should().Be<ContractStatus>();
    }

    [Fact]
    public void IsRegistered_Should_Be_True_For_A_Type_Only_Assembly()
    {
        bool registered = EnumMetadata.IsRegistered(typeof(ContractStatus));

        registered.Should().BeTrue();
    }

    [Fact]
    public void Get_Should_Return_Metadata_Of_A_Type_Only_Assembly()
    {
        EnumInfo<ContractStatus> info = EnumMetadata.Get<ContractStatus>();

        info.Fallback!.Value.Should().Be(ContractStatus.Unknown);
    }

    [Fact]
    public void Wire_Names_Should_Use_The_Naming_Of_The_Declaring_Project()
    {
        bool found = EnumMetadata.TryGet(typeof(ContractStatus), out IEnumInfo? info);

        found.Should().BeTrue();
        info!.WireNames.Should().Equal("pending-approval", "http-shipped", "unknown");
    }

    [Fact]
    public void EnumValueFormatter_Should_Format_A_Value_Of_A_Type_Only_Assembly()
    {
        bool formatted = EnumValueFormatter.TryFormat(ContractStatus.HTTPShipped, EnumConventions.Default, out string? text);

        formatted.Should().BeTrue();
        text.Should().Be("http-shipped");
    }
}
