using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumInfoParserTests
{
    private static readonly EnumConventions Defaults = EnumConventions.Default;

    private static EnumInfo<T> Info<T>() where T : struct, Enum =>
        (EnumInfo<T>)EnumMetadata.GetOrCreateWithReflection(typeof(T));

    public static readonly TheoryData<string, ParserStatus> AcceptedInBothModes = new()
    {
        { "pending", ParserStatus.Pending },
        { "PENDING", ParserStatus.Pending },
        { "in_progress", ParserStatus.InProgress },
        { "InProgress", ParserStatus.InProgress },
        { "inprogress", ParserStatus.InProgress },
        { "Started", ParserStatus.InProgress },
        { "running", ParserStatus.InProgress },
        { "done", ParserStatus.Completed },
        { "Completed", ParserStatus.Completed },
        { "cancelled_by_user", ParserStatus.Cancelled },
        { "http_status", ParserStatus.HTTPStatus },
        { "httpstatus", ParserStatus.HTTPStatus },
        { "HTTPStatus", ParserStatus.HTTPStatus },
        { "aborted", ParserStatus.Aborted },
        { "1", ParserStatus.InProgress },
        { "0", ParserStatus.Pending },
    };

    [Theory]
    [MemberData(nameof(AcceptedInBothModes))]
    public void Known_Spellings_Should_Be_Accepted_In_Both_Modes(string text, ParserStatus expected)
    {
        foreach (EnumReadMode mode in new[] { EnumReadMode.Input, EnumReadMode.Data })
        {
            Info<ParserStatus>().TryParse(text, mode, Defaults, out ParserStatus value, out EnumValueError? error).Should().BeTrue();
            error.Should().BeNull();
            value.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" pending")]
    [InlineData("pending ")]
    [InlineData("1.0")]
    [InlineData("0x1")]
    [InlineData("+1")]
    [InlineData("-0")]
    [InlineData("99999999999")]
    [InlineData(".5")]
    public void Malformed_Values_Should_Be_Rejected_Even_In_Data_Mode(string text)
    {
        foreach (EnumReadMode mode in new[] { EnumReadMode.Input, EnumReadMode.Data })
        {
            Info<ParserStatus>().TryParse(text, mode, Defaults, out _, out EnumValueError? error).Should().BeFalse();
            error!.EnumType.Should().Be<ParserStatus>();
            error.Value.Should().Be(text);
        }
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData("99")]
    [InlineData("unknown")]
    [InlineData("100")]
    public void Unknown_Values_Should_Be_Rejected_By_Input(string text) =>
        Info<ParserStatus>().TryParse(text, EnumReadMode.Input, Defaults, out _, out _).Should().BeFalse();

    [Theory]
    [InlineData("bogus")]
    [InlineData("99")]
    [InlineData("unknown")]
    [InlineData("100")]
    public void Unknown_Values_Should_Map_To_Fallback_In_Data_Mode(string text)
    {
        Info<ParserStatus>().TryParse(text, EnumReadMode.Data, Defaults, out ParserStatus value, out _).Should().BeTrue();
        value.Should().Be(ParserStatus.Unknown);
    }

    [Fact]
    public void Data_Mode_With_Reject_Should_Not_Use_Fallback()
    {
        EnumConventions reject = Defaults with { UnknownValue = UnknownEnumValueHandling.Reject };
        Info<ParserStatus>().TryParse("bogus", EnumReadMode.Data, reject, out _, out _).Should().BeFalse();
        Info<ParserStatus>().TryParseNumber(99L, EnumReadMode.Data, reject, out _, out _).Should().BeFalse();
        Info<ParserStatus>().TryParse("unknown", EnumReadMode.Data, reject, out ParserStatus value, out _).Should().BeTrue();
        value.Should().Be(ParserStatus.Unknown);
    }

    [Fact]
    public void Data_Mode_Without_Fallback_Member_Should_Reject_Unknown_Values()
    {
        Info<StrictStatus>().TryParse("bogus", EnumReadMode.Data, Defaults, out _, out EnumValueError? error).Should().BeFalse();
        error!.Message.Should().Be("'bogus' is not a valid StrictStatus. Allowed values: open, closed.");
    }

    [Fact]
    public void Input_Switches_Should_Restrict_Input_Only()
    {
        EnumConventions strict = Defaults with { CaseInsensitive = false, AcceptMemberNames = false, AcceptNumbers = false };
        EnumInfo<ParserStatus> info = Info<ParserStatus>();

        info.TryParse("PENDING", EnumReadMode.Input, strict, out _, out _).Should().BeFalse();
        info.TryParse("InProgress", EnumReadMode.Input, strict, out _, out _).Should().BeFalse();
        info.TryParse("1", EnumReadMode.Input, strict, out _, out _).Should().BeFalse();
        info.TryParseNumber(1L, EnumReadMode.Input, strict, out _, out _).Should().BeFalse();
        info.TryParse("Started", EnumReadMode.Input, strict, out ParserStatus alias, out _).Should().BeTrue();
        alias.Should().Be(ParserStatus.InProgress);
        info.TryParse("started", EnumReadMode.Input, strict, out _, out _).Should().BeFalse();

        info.TryParse("PENDING", EnumReadMode.Data, strict, out _, out _).Should().BeTrue();
        info.TryParse("InProgress", EnumReadMode.Data, strict, out _, out _).Should().BeTrue();
        info.TryParse("1", EnumReadMode.Data, strict, out _, out _).Should().BeTrue();
    }

    [Fact]
    public void Input_Error_Should_List_Wire_Names_Without_Fallback()
    {
        Info<ParserStatus>().TryParse("bogus", EnumReadMode.Input, Defaults, out _, out EnumValueError? error).Should().BeFalse();
        error!.AllowedValues.Should().Equal("pending", "in_progress", "done", "cancelled_by_user", "http_status", "aborted");
        error.Message.Should().Be(
            "'bogus' is not a valid ParserStatus. Allowed values: pending, in_progress, done, cancelled_by_user, http_status, aborted.");
    }

    [Fact]
    public void Data_Error_Should_List_Every_Wire_Name()
    {
        EnumConventions reject = Defaults with { UnknownValue = UnknownEnumValueHandling.Reject };
        Info<ParserStatus>().TryParse("bogus", EnumReadMode.Data, reject, out _, out EnumValueError? error).Should().BeFalse();
        error!.AllowedValues.Should().Contain("unknown");
    }

    [Fact]
    public void Numbers_Should_Follow_The_Read_Mode()
    {
        EnumInfo<ParserStatus> info = Info<ParserStatus>();
        info.TryParseNumber(2L, EnumReadMode.Input, Defaults, out ParserStatus value, out _).Should().BeTrue();
        value.Should().Be(ParserStatus.Completed);
        info.TryParseNumber(2UL, EnumReadMode.Input, Defaults, out value, out _).Should().BeTrue();
        value.Should().Be(ParserStatus.Completed);
        info.TryParseNumber(99L, EnumReadMode.Input, Defaults, out _, out _).Should().BeFalse();
        info.TryParseNumber(100L, EnumReadMode.Input, Defaults, out _, out _).Should().BeFalse();
        info.TryParseNumber(99L, EnumReadMode.Data, Defaults, out value, out _).Should().BeTrue();
        value.Should().Be(ParserStatus.Unknown);
        info.TryParseNumber(long.MaxValue, EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void Flags_Should_Accept_Combinations_Of_Defined_Bits_Only()
    {
        EnumInfo<ParserPermissions> info = Info<ParserPermissions>();
        info.TryParseNumber(7L, EnumReadMode.Input, Defaults, out ParserPermissions value, out _).Should().BeTrue();
        value.Should().Be(ParserPermissions.Read | ParserPermissions.Write | ParserPermissions.Delete);
        info.TryParseNumber(8L, EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
        info.TryParse("read_write", EnumReadMode.Input, Defaults, out value, out _).Should().BeTrue();
        value.Should().Be(ParserPermissions.ReadWrite);
        info.TryParse("256", EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
        info.IsDefined((ParserPermissions)5).Should().BeTrue();
        info.IsDefined((ParserPermissions)8).Should().BeFalse();
    }

    [Fact]
    public void Ranges_Should_Follow_The_Underlying_Type()
    {
        Info<HugeValue>().TryParse("18446744073709551615", EnumReadMode.Input, Defaults, out HugeValue huge, out _).Should().BeTrue();
        huge.Should().Be(HugeValue.Max);
        Info<HugeValue>().TryParse("18446744073709551616", EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
        Info<HugeValue>().TryParse("-1", EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
        Info<HugeValue>().TryParseNumber(-1L, EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();

        Info<SignedValue>().TryParse("-5", EnumReadMode.Input, Defaults, out SignedValue signed, out _).Should().BeTrue();
        signed.Should().Be(SignedValue.Negative);
        Info<SignedValue>().TryParse("-9223372036854775808", EnumReadMode.Input, Defaults, out signed, out _).Should().BeTrue();
        signed.Should().Be(SignedValue.Min);
        Info<SignedValue>().TryParseNumber(ulong.MaxValue, EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();

        Info<TinyValue>().TryParse("-128", EnumReadMode.Input, Defaults, out TinyValue tiny, out _).Should().BeTrue();
        tiny.Should().Be(TinyValue.Low);
        Info<TinyValue>().TryParse("-129", EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
        Info<TinyValue>().TryParseNumber(200L, EnumReadMode.Data, Defaults, out _, out _).Should().BeFalse();
    }

    private static readonly string[] AgreementInputs =
    [
        "", " ", " pending", "pending ", "pending", "PENDING", "in_progress", "InProgress", "inprogress", "Started", "running",
        "done", "finished", "Completed", "cancelled_by_user", "http_status", "httpstatus", "HTTPStatus", "aborted", "unknown",
        "Unknown", "bogus", "0", "1", "5", "99", "100", "+1", "-0", "-1", "1.0", "0x1", ".5", "00000000000000000000001",
        "18446744073709551615", "18446744073709551616", "-9223372036854775808", "-5", "7", "read", "write", "read_write",
        "ReadWrite", "3", "7", "8", "255", "256", "read,write", new string('x', 80),
    ];

    private static readonly EnumConventions[] AgreementConventions =
    [
        Defaults,
        Defaults with { UnknownValue = UnknownEnumValueHandling.Reject },
        Defaults with { CaseInsensitive = false },
        Defaults with { AcceptMemberNames = false },
        Defaults with { AcceptNumbers = false },
        Defaults with { CaseInsensitive = false, AcceptMemberNames = false, AcceptNumbers = false },
    ];

    [Fact]
    public void TryParse_Without_Error_Should_Agree_With_The_Error_Overload()
    {
        AssertAgreement(Info<ParserStatus>());
        AssertAgreement(Info<StrictStatus>());
        AssertAgreement(Info<ParserPermissions>());
        AssertAgreement(Info<ParserAccess>());
        AssertAgreement(Info<HugeValue>());
        AssertAgreement(Info<SignedValue>());
        AssertAgreement(Info<TinyValue>());
    }

    [Fact]
    public void TryParseWire_Should_Agree_With_Data_Mode_That_Rejects_Unknown_Values()
    {
        AssertWireAgreement(Info<ParserStatus>());
        AssertWireAgreement(Info<StrictStatus>());
        AssertWireAgreement(Info<ParserPermissions>());
        AssertWireAgreement(Info<ParserAccess>());
        AssertWireAgreement(Info<HugeValue>());
        AssertWireAgreement(Info<SignedValue>());
        AssertWireAgreement(Info<TinyValue>());
    }

    [Fact]
    public void TryParseWire_Should_Never_Use_The_Fallback_Member()
    {
        EnumInfo<ParserStatus> info = Info<ParserStatus>();

        info.TryParseWire("bogus", out ParserStatus value).Should().BeFalse();
        value.Should().Be(default(ParserStatus));
        info.TryParseWire("unknown", out value).Should().BeTrue();
        value.Should().Be(ParserStatus.Unknown);
        info.TryParseWire("99", out _).Should().BeFalse();
    }

    [Fact]
    public void Input_Mode_Should_Report_The_Fallback_Value_On_Rejection_In_Both_Overloads()
    {
        EnumInfo<ParserStatus> info = Info<ParserStatus>();

        info.TryParse("unknown", EnumReadMode.Input, Defaults, out ParserStatus withoutError).Should().BeFalse();
        info.TryParse("unknown", EnumReadMode.Input, Defaults, out ParserStatus withError, out EnumValueError? error).Should().BeFalse();

        withoutError.Should().Be(ParserStatus.Unknown);
        withError.Should().Be(ParserStatus.Unknown);
        error!.Value.Should().Be("unknown");
    }

    [Fact]
    public void Error_Value_Should_Be_Truncated_To_64_Chars()
    {
        string text = new('x', 80);

        Info<ParserStatus>().TryParse(text, EnumReadMode.Input, Defaults, out _, out EnumValueError? error).Should().BeFalse();

        error!.Value.Should().Be(new string('x', 64) + "…");
    }

    [Fact]
    public void EnumValueParser_Without_Error_Should_Parse_Generated_Enums()
    {
        EnumValueParser.TryParse("in_progress", EnumReadMode.Input, Defaults, out Status value).Should().BeTrue();
        value.Should().Be(Status.InProgress);
        EnumValueParser.TryParse("bogus", EnumReadMode.Input, Defaults, out Status _).Should().BeFalse();
    }

    [Fact]
    public void TryParse_Without_Error_Should_Not_Allocate_On_Hit_Or_Miss()
    {
        EnumInfo<ParserStatus> info = Info<ParserStatus>();
        EnumConventions reject = Defaults with { UnknownValue = UnknownEnumValueHandling.Reject };
        string[] inputs = ["in_progress", "IN_PROGRESS", "Started", "bogus", "99", "1"];
        foreach (string text in inputs)
            info.TryParse(text, EnumReadMode.Data, reject, out _);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            foreach (string text in inputs)
                info.TryParse(text, EnumReadMode.Data, reject, out _);
        }

        (GC.GetAllocatedBytesForCurrentThread() - before).Should().Be(0);
    }

    private static void AssertWireAgreement<T>(EnumInfo<T> info) where T : struct, Enum
    {
        EnumConventions reject = Defaults with { UnknownValue = UnknownEnumValueHandling.Reject };
        foreach (string text in AgreementInputs)
        {
            bool expected = info.TryParse(text, EnumReadMode.Data, reject, out T expectedValue);

            info.TryParseWire(text, out T value).Should().Be(expected, "'{0}'", text);
            value.Should().Be(expectedValue, "'{0}'", text);
        }
    }

    private static void AssertAgreement<T>(EnumInfo<T> info) where T : struct, Enum
    {
        foreach (EnumConventions conventions in AgreementConventions)
        {
            foreach (EnumReadMode mode in new[] { EnumReadMode.Input, EnumReadMode.Data })
            {
                foreach (string text in AgreementInputs)
                {
                    bool withError = info.TryParse(text, mode, conventions, out T errorValue, out EnumValueError? error);
                    bool withoutError = info.TryParse(text, mode, conventions, out T value);

                    withoutError.Should().Be(withError, "'{0}' in {1}", text, mode);
                    value.Should().Be(errorValue, "'{0}' in {1}", text, mode);
                    (error is null).Should().Be(withError);
                }
            }
        }
    }
}
