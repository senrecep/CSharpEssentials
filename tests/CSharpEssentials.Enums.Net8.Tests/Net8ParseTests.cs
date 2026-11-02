using System.Runtime.Serialization;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Enums.Net8.Tests;

public class Net8ParseTests
{
    private static readonly EnumConventions Reject = EnumConventions.Default with { UnknownValue = UnknownEnumValueHandling.Reject };

    [Theory]
    [InlineData("pending_approval", (int)Net8Status.PendingApproval)]
    [InlineData("PENDING_APPROVAL", (int)Net8Status.PendingApproval)]
    [InlineData("PendingApproval", (int)Net8Status.PendingApproval)]
    [InlineData("pendingapproval", (int)Net8Status.PendingApproval)]
    [InlineData("Approval", (int)Net8Status.PendingApproval)]
    [InlineData("approval", (int)Net8Status.PendingApproval)]
    [InlineData("shipped_out", (int)Net8Status.Shipped)]
    [InlineData("2", (int)Net8Status.Shipped)]
    public void Data_Reads_Should_Accept_Every_Known_Spelling(string text, int expectedRaw)
    {
        Net8Status expected = (Net8Status)expectedRaw;
        Net8StatusExtensions.TryParseWire(text, out Net8Status value).Should().BeTrue();
        value.Should().Be(expected);
        EnumValueParser.TryParse(text.AsSpan(), EnumReadMode.Data, Reject, out Net8Status parsed, out EnumValueError? error).Should().BeTrue();
        parsed.Should().Be(expected);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("PendingApproval", false)]
    [InlineData("pending_approval", true)]
    [InlineData("Pending_Approval", false)]
    [InlineData("Approval", true)]
    [InlineData("approval", false)]
    public void Strict_Input_Should_Use_Exact_Wire_Names_And_Aliases(string text, bool accepted)
    {
        EnumConventions strict = EnumConventions.Default with { CaseInsensitive = false, AcceptMemberNames = false };

        EnumValueParser.TryParse(text.AsSpan(), EnumReadMode.Input, strict, out Net8Status _).Should().Be(accepted);
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData("99")]
    [InlineData("")]
    [InlineData(" pending_approval")]
    public void Unknown_Values_Should_Be_Rejected_With_An_Error(string text)
    {
        EnumValueParser.TryParse(text.AsSpan(), EnumReadMode.Data, Reject, out Net8Status _, out EnumValueError? error).Should().BeFalse();
        error!.Value.Should().Be(text);
        Net8StatusExtensions.TryParseWire(text, out _).Should().BeFalse();
    }

    [Fact]
    public void Parsing_Should_Not_Allocate_On_Hit_Or_Miss()
    {
        string[] inputs = ["pending_approval", "PENDING_APPROVAL", "PendingApproval", "approval", "bogus", "1"];
        foreach (string text in inputs)
            EnumValueParser.TryParse(text.AsSpan(), EnumReadMode.Data, Reject, out Net8Status _);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
        {
            foreach (string text in inputs)
                EnumValueParser.TryParse(text.AsSpan(), EnumReadMode.Data, Reject, out Net8Status _);
        }

        (GC.GetAllocatedBytesForCurrentThread() - before).Should().Be(0);
    }

    [Fact]
    public void Generated_Helpers_Should_Match_The_Runtime_Metadata()
    {
        EnumInfo<Net8Status> info = EnumMetadata.Get<Net8Status>();
        for (int raw = -2; raw <= 5; raw++)
        {
            Net8Status value = (Net8Status)raw;
            value.IsDefined().Should().Be(info.IsDefined(value));
            value.ToDescription().Should().Be(info.TryGetMember(value, out EnumMemberInfo<Net8Status>? member) ? member.Description : null);
            value.ToOptimizedString().Should().Be(value.ToString());
        }
    }
}

[StringEnum]
internal enum Net8Status
{
    Draft,

    [System.ComponentModel.Description("Waiting for a reviewer")]
    [EnumAlias("Approval")]
    PendingApproval,

    [EnumMember(Value = "shipped_out")]
    Shipped,
}
