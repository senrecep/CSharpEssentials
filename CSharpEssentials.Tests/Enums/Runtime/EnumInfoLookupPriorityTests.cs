using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums.Runtime;

public class EnumInfoLookupPriorityTests
{
    private static readonly EnumConventions Defaults = EnumConventions.Default;
    private static readonly EnumConventions CaseSensitive = Defaults with { CaseInsensitive = false };
    private static readonly EnumConventions NoMembers = Defaults with { AcceptMemberNames = false };
    private static readonly EnumConventions CaseSensitiveNoMembers = Defaults with { CaseInsensitive = false, AcceptMemberNames = false };

    private static readonly EnumInfo<LookupProbe> Info = new(
        [
            new EnumMemberInfo<LookupProbe>(LookupProbe.X, 0, "Foo", "a") { Aliases = ["shared"] },
            new EnumMemberInfo<LookupProbe>(LookupProbe.Y, 1, "FOO", "A"),
            new EnumMemberInfo<LookupProbe>(LookupProbe.Z, 2, "Zed", "beta"),
            new EnumMemberInfo<LookupProbe>(LookupProbe.W, 3, "Beta", "w") { Aliases = ["BETA2", "Shared"] },
        ],
        static v => unchecked((ulong)(long)v),
        static r => unchecked((LookupProbe)(long)r),
        isFlags: false,
        EnumStorage.String);

    public static readonly TheoryData<string, LookupProbe?> DataMode = new()
    {
        { "a", LookupProbe.X },
        { "A", LookupProbe.Y },
        { "beta", LookupProbe.Z },
        { "Beta", LookupProbe.Z },
        { "BETA", LookupProbe.Z },
        { "Foo", LookupProbe.X },
        { "FOO", LookupProbe.X },
        { "foo", LookupProbe.X },
        { "zed", LookupProbe.Z },
        { "shared", LookupProbe.X },
        { "Shared", LookupProbe.X },
        { "SHARED", LookupProbe.X },
        { "beta2", LookupProbe.W },
        { "W", LookupProbe.W },
        { "2", LookupProbe.Z },
        { "4", null },
        { "nope", null },
    };

    public static readonly TheoryData<string, LookupProbe?> InputCaseSensitive = new()
    {
        { "a", LookupProbe.X },
        { "A", LookupProbe.Y },
        { "beta", LookupProbe.Z },
        { "Beta", LookupProbe.W },
        { "BETA", null },
        { "FOO", LookupProbe.Y },
        { "Foo", LookupProbe.X },
        { "foo", null },
        { "Shared", LookupProbe.W },
        { "shared", LookupProbe.X },
        { "BETA2", LookupProbe.W },
        { "beta2", null },
        { "W", null },
    };

    public static readonly TheoryData<string, LookupProbe?> InputNoMembers = new()
    {
        { "Beta", LookupProbe.Z },
        { "Foo", null },
        { "zed", null },
        { "SHARED", LookupProbe.X },
        { "beta2", LookupProbe.W },
        { "A", LookupProbe.Y },
        { "a", LookupProbe.X },
    };

    public static readonly TheoryData<string, LookupProbe?> InputCaseSensitiveNoMembers = new()
    {
        { "Beta", null },
        { "Shared", LookupProbe.W },
        { "Foo", null },
        { "BETA2", LookupProbe.W },
        { "A", LookupProbe.Y },
    };

    [Theory]
    [MemberData(nameof(DataMode))]
    public void Data_Mode_Should_Prefer_Exact_Wire_Then_Wire_Then_Member_Then_Alias(string text, LookupProbe? expected)
    {
        AssertParse(text, EnumReadMode.Data, Defaults, expected);
        AssertParse(text, EnumReadMode.Data, CaseSensitiveNoMembers, expected);
        AssertParse(text, EnumReadMode.Input, Defaults, expected);
    }

    [Theory]
    [MemberData(nameof(InputCaseSensitive))]
    public void Case_Sensitive_Input_Should_Use_Exact_Tables_In_Priority_Order(string text, LookupProbe? expected) =>
        AssertParse(text, EnumReadMode.Input, CaseSensitive, expected);

    [Theory]
    [MemberData(nameof(InputNoMembers))]
    public void Input_Without_Member_Names_Should_Still_Read_Wire_Names_And_Aliases(string text, LookupProbe? expected) =>
        AssertParse(text, EnumReadMode.Input, NoMembers, expected);

    [Theory]
    [MemberData(nameof(InputCaseSensitiveNoMembers))]
    public void Case_Sensitive_Input_Without_Member_Names_Should_Read_Exact_Wire_Names_And_Aliases(string text, LookupProbe? expected) =>
        AssertParse(text, EnumReadMode.Input, CaseSensitiveNoMembers, expected);

    private static void AssertParse(string text, EnumReadMode mode, EnumConventions conventions, LookupProbe? expected)
    {
        bool ok = Info.TryParse(text, mode, conventions, out LookupProbe value, out EnumValueError? error);

        ok.Should().Be(expected.HasValue, "'{0}' in {1}", text, mode);
        if (expected.HasValue)
        {
            value.Should().Be(expected.Value);
            error.Should().BeNull();
        }
        else
        {
            error!.Value.Should().Be(text);
        }
    }
}
