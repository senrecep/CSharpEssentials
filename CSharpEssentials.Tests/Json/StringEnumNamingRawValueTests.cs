using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public sealed class StringEnumNamingRawValueTests
{
    [StringEnum]
    internal enum RawStatus
    {
        Active,
        Inactive,
        PendingApproval
    }

    public static TheoryData<object> RawNumbers() => new()
    {
        (sbyte)2,
        (short)2,
        2,
        2L,
        (byte)2,
        (ushort)2,
        2u,
        2ul
    };

    [Theory]
    [MemberData(nameof(RawNumbers))]
    public void GetName_NonGeneric_Should_Map_Every_Integral_Number_Type_To_The_Wire_Name(object number)
    {
        StringEnumNaming.GetName(typeof(RawStatus), number).Should().Be("pending_approval");
    }

    [Fact]
    public void GetName_NonGeneric_Should_Write_Undefined_Numbers_As_Digits()
    {
        StringEnumNaming.GetName(typeof(RawStatus), 42).Should().Be("42");
    }

    [Fact]
    public void GetName_NonGeneric_Should_Reject_Non_Integral_Values()
    {
        Action act = () => StringEnumNaming.GetName(typeof(RawStatus), "active");

        act.Should().Throw<ArgumentException>().WithMessage("*Unsupported enum value type*System.String*");
    }

    [Fact]
    public void GetName_NonGeneric_Should_Reject_Null_Value()
    {
        Action act = () => StringEnumNaming.GetName(typeof(RawStatus), null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Fact]
    public void Null_Enum_Type_Should_Throw_ArgumentException()
    {
        Action getNames = () => StringEnumNaming.GetNames(null!);
        Action tryParse = () => StringEnumNaming.TryParse(null!, "active", out object? _);

        getNames.Should().Throw<ArgumentException>().WithParameterName("enumType");
        tryParse.Should().Throw<ArgumentException>().WithParameterName("enumType");
    }
}
