using System.Collections.ObjectModel;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Fixtures.EnumsContracts;
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
    public void TryFormat_Should_Apply_The_Default_CanHandle_Of_Derived_Conventions()
    {
        EnumConventions derived = Conventions with { AcceptNumbers = false };

        EnumValueFormatter.TryFormat(FormatterInt.SecondValue, derived, out string? text).Should().BeTrue();
        text.Should().Be("second_value");
        EnumValueFormatter.TryFormat(DayOfWeek.Monday, derived, out _).Should().BeFalse();
    }

    [Fact]
    public void TryFormat_Should_Throw_When_The_Default_CanHandle_Accepts_A_StringEnum_Without_Metadata()
    {
        List<string> values = [];

        Action format = () => EnumValueFormatter.TryFormat(UnreachableHolder<int>.Status.SecondValue, Conventions, out _);
        Action formatMany = () => EnumValueFormatter.TryFormatMany(UnreachableHolder<int>.Status.SecondValue, Conventions, values);

        format.Should().Throw<InvalidOperationException>().WithMessage("*no generated metadata*");
        formatMany.Should().Throw<InvalidOperationException>().WithMessage("*no generated metadata*");
        values.Should().BeEmpty();
    }

    [Fact]
    public void TryFormat_Should_Format_Through_The_Visitor_For_A_Foreign_EnumInfo()
    {
        EnumInfo<FormatterForeignInfo> typed = new(
            [
                new EnumMemberInfo<FormatterForeignInfo>(FormatterForeignInfo.First, 0, "First", "first"),
                new EnumMemberInfo<FormatterForeignInfo>(FormatterForeignInfo.Second, 1, "Second", "second"),
            ],
            static v => unchecked((ulong)(long)v),
            static r => unchecked((FormatterForeignInfo)(long)r),
            isFlags: false,
            EnumStorage.Default);
        EnumMetadata.RegisterRange([new(typeof(FormatterForeignInfo), () => new ForwardingEnumInfo(typed))]);
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormat(FormatterForeignInfo.Second, Conventions, out string? text);
        bool formattedMany = EnumValueFormatter.TryFormatMany(
            new[] { FormatterForeignInfo.First, FormatterForeignInfo.Second }, Conventions, values);

        EnumMetadata.TryGet(typeof(FormatterForeignInfo), out IEnumInfo? info).Should().BeTrue();
        info.Should().BeOfType<ForwardingEnumInfo>();
        formatted.Should().BeTrue();
        text.Should().Be("second");
        formattedMany.Should().BeTrue();
        values.Should().Equal("first", "second");
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
    public void TryFormatMany_Should_Format_Each_Item_With_Its_Own_Enum_Type()
    {
        List<string> values = [];

        bool formatted = EnumValueFormatter.TryFormatMany(
            new object[] { FormatterInt.SecondValue, FormatterInt.MinValue, FormatterByte.MaxValue, FormatterInt.SecondValue }, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().Equal("second_value", "min_value", "max_value", "second_value");
    }

    [Fact]
    public void TryFormatMany_Should_Ask_Custom_Conventions_About_Every_New_Item_Type()
    {
        List<string> values = ["existing"];
        EnumConventions conventions = Conventions with { CanHandle = static type => type != typeof(FormatterByte) };

        bool formatted = EnumValueFormatter.TryFormatMany(
            new object[] { FormatterInt.SecondValue, FormatterInt.MinValue, FormatterByte.MaxValue }, conventions, values);

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

    [Fact]
    public void TryFormatMany_Should_Leave_Values_Unchanged_When_An_Item_Throws()
    {
        List<string> values = ["existing"];

        Action act = () => EnumValueFormatter.TryFormatMany(
            new[] { FormatterInt.MinValue, FormatterInt.SecondValue, (FormatterInt)42 }, Conventions, values);

        act.Should().Throw<EnumValueException>();
        values.Should().Equal("existing");
    }

    [Fact]
    public void TryFormatMany_Should_Leave_Values_Unchanged_When_A_Later_Item_Is_Not_Handled()
    {
        List<string> values = ["existing"];

        bool formatted = EnumValueFormatter.TryFormatMany(
            new object[] { FormatterInt.MinValue, FormatterInt.SecondValue, DayOfWeek.Monday }, Conventions, values);

        formatted.Should().BeFalse();
        values.Should().Equal("existing");
    }

    [Fact]
    public void TryFormatMany_Should_Not_Touch_A_Caller_Collection_That_Is_Not_A_List_When_An_Item_Is_Not_Handled()
    {
        ObservableCollection<string> values = ["existing"];
        int changes = 0;
        values.CollectionChanged += (_, _) => changes++;

        bool formatted = EnumValueFormatter.TryFormatMany(
            new object[] { FormatterInt.MinValue, FormatterInt.SecondValue, DayOfWeek.Monday }, Conventions, values);

        formatted.Should().BeFalse();
        values.Should().Equal("existing");
        changes.Should().Be(0);
    }

    [Fact]
    public void TryFormatMany_Should_Add_Collection_Items_To_A_Caller_Collection_That_Is_Not_A_List()
    {
        ObservableCollection<string> values = ["existing"];

        bool formatted = EnumValueFormatter.TryFormatMany(new[] { FormatterInt.MinValue, FormatterInt.SecondValue }, Conventions, values);

        formatted.Should().BeTrue();
        values.Should().Equal("existing", "min_value", "second_value");
    }

    private sealed class ForwardingEnumInfo(IEnumInfo inner) : IEnumInfo
    {
        public Type EnumType => inner.EnumType;

        public Type UnderlyingType => inner.UnderlyingType;

        public bool IsFlags => inner.IsFlags;

        public ulong DefinedMask => inner.DefinedMask;

        public EnumStorage Storage => inner.Storage;

        public IReadOnlyList<IEnumMemberInfo> Members => inner.Members;

        public IEnumMemberInfo? Fallback => inner.Fallback;

        public IReadOnlyList<string> WireNames => inner.WireNames;

        public TResult Accept<TResult>(IEnumInfoVisitor<TResult> visitor) => inner.Accept(visitor);
    }
}

public enum FormatterForeignInfo
{
    First,
    Second,
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
