using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumInfoFormatterTests
{
    private static EnumInfo<T> Info<T>() where T : struct, Enum =>
        (EnumInfo<T>)EnumMetadata.GetOrCreateWithReflection(typeof(T));

    [Theory]
    [InlineData(ParserStatus.Pending, "pending", "0")]
    [InlineData(ParserStatus.Completed, "done", "2")]
    [InlineData(ParserStatus.Cancelled, "cancelled_by_user", "3")]
    [InlineData(ParserStatus.HTTPStatus, "http_status", "4")]
    [InlineData(ParserStatus.Unknown, "unknown", "100")]
    public void Format_Should_Write_Wire_Name_Or_Number(ParserStatus value, string wireName, string number)
    {
        Info<ParserStatus>().Format(value, EnumWireFormat.String).Should().Be(wireName);
        Info<ParserStatus>().Format(value, EnumWireFormat.Number).Should().Be(number);
    }

    [Fact]
    public void Format_Should_Throw_For_Undefined_Values()
    {
        Action act = () => Info<ParserStatus>().Format((ParserStatus)42, EnumWireFormat.String);
        act.Should().Throw<EnumValueException>().Which.Error.Value.Should().Be("42");
    }

    [Fact]
    public void Flags_Should_Be_Written_As_Single_Flags()
    {
        EnumInfo<ParserPermissions> info = Info<ParserPermissions>();
        info.Format(ParserPermissions.ReadWrite, EnumWireFormat.String).Should().Be("read,write");
        info.Format(ParserPermissions.None, EnumWireFormat.String).Should().Be("none");
        info.Format(ParserPermissions.ReadWrite | ParserPermissions.Delete, EnumWireFormat.Number).Should().Be("7");

        List<string> names = [];
        info.FormatFlags(ParserPermissions.ReadWrite | ParserPermissions.Delete, names);
        names.Should().Equal("read", "write", "delete");

        names.Clear();
        info.FormatFlags(ParserPermissions.None, names);
        names.Should().BeEmpty();

        Action act = () => info.FormatFlags((ParserPermissions)8, names);
        act.Should().Throw<EnumValueException>();
    }

    [Fact]
    public void Flags_Zero_Without_A_Zero_Member_Should_Be_Written_As_A_Number_That_Reads_Back()
    {
        EnumInfo<ParserAccess> info = Info<ParserAccess>();

        string text = info.Format(default, EnumWireFormat.String);

        text.Should().Be("0");
        info.TryParse(text, EnumReadMode.Data, EnumConventions.Default, out ParserAccess value, out _).Should().BeTrue();
        value.Should().Be(default(ParserAccess));
    }

    [Fact]
    public void Numeric_Text_Should_Use_The_Underlying_Sign()
    {
        Info<HugeValue>().Format(HugeValue.Max, EnumWireFormat.Number).Should().Be("18446744073709551615");
        Info<SignedValue>().Format(SignedValue.Negative, EnumWireFormat.Number).Should().Be("-5");
        Info<SignedValue>().Format(SignedValue.Min, EnumWireFormat.Number).Should().Be("-9223372036854775808");
        Info<TinyValue>().Format(TinyValue.Low, EnumWireFormat.Number).Should().Be("-128");
    }

    [Fact]
    public void Format_Should_Throw_For_Flags_With_Bits_Outside_Every_Member()
    {
        Action act = () => Info<ParserPermissions>().Format(ParserPermissions.Read | (ParserPermissions)8, EnumWireFormat.String);

        act.Should().Throw<EnumValueException>().Which.Error.Value.Should().Be("9");
    }

    [Fact]
    public void Format_Should_Write_The_Composite_For_Bits_Without_A_Single_Flag()
    {
        Info<ParserCompositeAccess>().Format(ParserCompositeAccess.Read | ParserCompositeAccess.WriteDelete, EnumWireFormat.String)
            .Should().Be("read,write_delete");
    }

    [Fact]
    public void Format_Should_Throw_When_Only_Part_Of_A_Composite_Is_Set()
    {
        Action act = () => Info<ParserCompositeAccess>().Format((ParserCompositeAccess)5, EnumWireFormat.String);

        act.Should().Throw<EnumValueException>().Which.Error.Value.Should().Be("5");
    }
}
