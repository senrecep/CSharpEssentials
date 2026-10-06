using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

public class EnumValueFormatterTests
{
    private static readonly EnumConventions Conventions = EnumConventions.Default;

    public static TheoryData<object, string, string> EveryUnderlyingType => new()
    {
        { FormatterByte.MaxValue, "max_value", "255" },
        { FormatterSByte.MinValue, "min_value", "-128" },
        { FormatterShort.MinValue, "min_value", "-32768" },
        { FormatterUShort.MaxValue, "max_value", "65535" },
        { FormatterInt.MinValue, "min_value", "-2147483648" },
        { FormatterUInt.MaxValue, "max_value", "4294967295" },
        { FormatterLong.MinValue, "min_value", "-9223372036854775808" },
        { FormatterULong.MaxValue, "max_value", "18446744073709551615" },
    };

    [Theory]
    [MemberData(nameof(EveryUnderlyingType))]
    public void Format_Should_Write_Wire_Name_And_Number_For_Every_Underlying_Type(object value, string wireName, string number)
    {
        string text = EnumValueFormatter.Format(value, Conventions);
        string numeric = EnumValueFormatter.Format(value, Conventions, EnumWireFormat.Number);

        text.Should().Be(wireName);
        numeric.Should().Be(number);
    }

    [Theory]
    [MemberData(nameof(EveryUnderlyingType))]
    public void TryFormat_Should_Write_Wire_Name_For_Every_Underlying_Type(object value, string wireName, string number)
    {
        bool formatted = EnumValueFormatter.TryFormat(value, Conventions, out string? text);

        formatted.Should().BeTrue();
        text.Should().Be(wireName).And.NotBe(number);
    }

    [Fact]
    public void TryFormat_Should_Use_WriteAs_When_No_Format_Is_Given()
    {
        EnumConventions numbers = Conventions with { WriteAs = EnumWireFormat.Number };

        bool formatted = EnumValueFormatter.TryFormat(FormatterInt.SecondValue, numbers, out string? text);

        formatted.Should().BeTrue();
        text.Should().Be("1");
    }

    [Fact]
    public void TryFormat_Should_Let_The_Format_Argument_Override_WriteAs()
    {
        EnumConventions numbers = Conventions with { WriteAs = EnumWireFormat.Number };

        bool formatted = EnumValueFormatter.TryFormat(FormatterInt.SecondValue, numbers, out string? text, EnumWireFormat.String);

        formatted.Should().BeTrue();
        text.Should().Be("second_value");
    }

    [Fact]
    public void TryFormat_Should_Format_A_Boxed_Nullable_With_A_Value()
    {
        FormatterInt? value = FormatterInt.SecondValue;

        bool formatted = EnumValueFormatter.TryFormat(value, Conventions, out string? text);

        formatted.Should().BeTrue();
        text.Should().Be("second_value");
    }

    [Fact]
    public void TryFormat_Should_Return_False_For_A_Null_Nullable()
    {
        FormatterInt? value = null;

        bool formatted = EnumValueFormatter.TryFormat(value, Conventions, out string? text);

        formatted.Should().BeFalse();
        text.Should().BeNull();
    }

    public static TheoryData<object> NotHandled => new()
    {
        "second_value",
        1,
        1L,
        new object(),
        DayOfWeek.Monday,
        new[] { FormatterInt.SecondValue },
    };

    [Theory]
    [MemberData(nameof(NotHandled))]
    public void TryFormat_Should_Return_False_For_Values_That_Are_Not_Enums_With_Metadata(object value)
    {
        bool formatted = EnumValueFormatter.TryFormat(value, Conventions, out string? text);

        formatted.Should().BeFalse();
        text.Should().BeNull();
    }

    [Fact]
    public void TryFormat_Should_Return_False_When_CanHandle_Rejects_The_Type()
    {
        EnumConventions conventions = Conventions with { CanHandle = static type => type != typeof(FormatterInt) };

        bool formatted = EnumValueFormatter.TryFormat(FormatterInt.SecondValue, conventions, out string? text);

        formatted.Should().BeFalse();
        text.Should().BeNull();
    }

    [Fact]
    public void TryFormat_Should_Throw_For_An_Undefined_Value()
    {
        Action act = () => EnumValueFormatter.TryFormat((FormatterInt)42, Conventions, out _);

        act.Should().Throw<EnumValueException>().Which.Error.Value.Should().Be("42");
    }

    [Fact]
    public void Format_Should_Throw_ArgumentException_For_A_Value_That_Is_Not_Handled()
    {
        Action act = () => EnumValueFormatter.Format(DayOfWeek.Monday, Conventions);

        act.Should().Throw<ArgumentException>().WithParameterName("value");
    }

    [Fact]
    public void TryFormat_Should_Join_Flags_For_A_Single_Value()
    {
        bool formatted = EnumValueFormatter.TryFormat(FormatterAccess.Read | FormatterAccess.Delete, Conventions, out string? text);

        formatted.Should().BeTrue();
        text.Should().Be("read,delete");
    }

    [Fact]
    public void TryFormatMany_Should_Add_One_Entry_Per_Single_Flag()
    {
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormatMany(FormatterAccess.ReadWrite | FormatterAccess.Delete, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().Equal("read", "write", "delete");
    }

    [Fact]
    public void TryFormatMany_Should_Add_The_Bitmask_For_Flags_With_Number_Format()
    {
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormatMany(FormatterAccess.ReadWrite, Conventions, values, EnumWireFormat.Number);

        formatted.Should().BeTrue();
        values.Should().Equal("3");
    }

    [Fact]
    public void TryFormatMany_Should_Add_Nothing_For_Empty_Flags()
    {
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormatMany(FormatterAccess.None, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().BeEmpty();
    }

    [Fact]
    public void TryFormatMany_Should_Add_A_Single_Value()
    {
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormatMany(FormatterLong.MinValue, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().Equal("min_value");
    }

    public static TheoryData<object> EnumCollections => new()
    {
        new[] { FormatterInt.MinValue, FormatterInt.SecondValue },
        new List<FormatterInt> { FormatterInt.MinValue, FormatterInt.SecondValue },
        new HashSet<FormatterInt> { FormatterInt.MinValue, FormatterInt.SecondValue },
        new List<FormatterInt?> { FormatterInt.MinValue, null, FormatterInt.SecondValue },
        new object[] { FormatterInt.MinValue, FormatterInt.SecondValue },
    };

    [Theory]
    [MemberData(nameof(EnumCollections))]
    public void TryFormatMany_Should_Add_Every_Item_Of_An_Enum_Collection(object collection)
    {
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormatMany(collection, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().Equal("min_value", "second_value");
    }

    [Fact]
    public void TryFormatMany_Should_Expand_Flags_Items_Of_A_Collection()
    {
        List<string> values = [];
        FormatterAccess[] collection = [FormatterAccess.Read, FormatterAccess.Write | FormatterAccess.Delete];

        bool formatted = EnumValueFormatter.TryFormatMany(collection, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().Equal("read", "write", "delete");
    }

    [Fact]
    public void TryFormatMany_Should_Use_WriteAs_For_Collections()
    {
        List<string> values = [];
        EnumConventions numbers = Conventions with { WriteAs = EnumWireFormat.Number };

        bool formatted = EnumValueFormatter.TryFormatMany(new[] { FormatterByte.Zero, FormatterByte.MaxValue }, numbers, values);

        formatted.Should().BeTrue();
        values.Should().Equal("0", "255");
    }

    private static readonly int[] TwoInts = [1, 2];

    public static TheoryData<object?> NotHandledCollections => new()
    {
        null,
        "second_value",
        42,
        DayOfWeek.Monday,
        TwoInts,
        Array.Empty<FormatterInt>(),
        new List<FormatterInt?> { null },
        new object[] { FormatterInt.MinValue, "second_value" },
        new object[] { FormatterInt.MinValue, DayOfWeek.Monday },
    };

    [Theory]
    [MemberData(nameof(NotHandledCollections))]
    public void TryFormatMany_Should_Return_False_And_Add_Nothing_For_Values_That_Are_Not_Handled(object? value)
    {
        List<string> values = ["existing"];

        bool formatted = EnumValueFormatter.TryFormatMany(value, Conventions, values);

        formatted.Should().BeFalse();
        values.Should().Equal("existing");
    }

    [Fact]
    public void TryFormatMany_Should_Throw_For_An_Undefined_Item()
    {
        List<string> values = [];

        Action act = () => EnumValueFormatter.TryFormatMany(new[] { FormatterInt.MinValue, (FormatterInt)42 }, Conventions, values);

        act.Should().Throw<EnumValueException>();
    }
}

[StringEnum]
public enum FormatterByte : byte
{
    Zero = 0,
    MaxValue = byte.MaxValue,
}

[StringEnum]
public enum FormatterSByte : sbyte
{
    MinValue = sbyte.MinValue,
}

[StringEnum]
public enum FormatterShort : short
{
    MinValue = short.MinValue,
}

[StringEnum]
public enum FormatterUShort : ushort
{
    MaxValue = ushort.MaxValue,
}

[StringEnum]
public enum FormatterInt
{
    MinValue = int.MinValue,
    SecondValue = 1,
}

[StringEnum]
public enum FormatterUInt : uint
{
    MaxValue = uint.MaxValue,
}

[StringEnum]
public enum FormatterLong : long
{
    MinValue = long.MinValue,
}

[StringEnum]
public enum FormatterULong : ulong
{
    MaxValue = ulong.MaxValue,
}

[StringEnum, Flags]
public enum FormatterAccess
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    ReadWrite = Read | Write,
}
