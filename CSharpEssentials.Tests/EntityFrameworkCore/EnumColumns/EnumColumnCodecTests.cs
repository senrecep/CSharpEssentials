using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

public sealed class EnumColumnCodecTests
{
    [Theory]
    [InlineData(EnumStoredAs.LegacySnakeCase, "pending_approval")]
    [InlineData(EnumStoredAs.LegacySnakeCase, "PENDING_APPROVAL")]
    [InlineData(EnumStoredAs.LegacySnakeCase, "Pending_Approval")]
    [InlineData(EnumStoredAs.MemberName, "PendingApproval")]
    [InlineData(EnumStoredAs.MemberName, "pendingapproval")]
    [InlineData(EnumStoredAs.CamelCase, "pendingApproval")]
    [InlineData(EnumStoredAs.CamelCase, "PENDINGAPPROVAL")]
    public void FromText_Should_ReadLegacyText_When_CasingMatchesOrDiffers(EnumStoredAs format, string text)
    {
        EnumColumnCodec<StoredOrderStatus> codec = new(EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, format);

        codec.FromText(text).Should().Be(StoredOrderStatus.PendingApproval);
    }

    [Fact]
    public void FromText_Should_RoundTripEveryMember_When_LegacySnakeCase()
    {
        EnumColumnCodec<StoredOrderStatus> codec = new(EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, EnumStoredAs.LegacySnakeCase);

        foreach (StoredOrderStatus member in Enum.GetValues<StoredOrderStatus>())
            codec.FromText(codec.ToText(member)).Should().Be(member);
    }

    [Fact]
    public void FromText_Should_FallBackToWireName_When_TextIsNotLegacy()
    {
        EnumColumnCodec<StoredOrderStatus> codec = new(EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, EnumStoredAs.CamelCase);

        codec.FromText("pending_approval").Should().Be(StoredOrderStatus.PendingApproval);
    }

    [Fact]
    public void FromText_Should_Throw_When_TextIsUnknown()
    {
        EnumColumnCodec<StoredOrderStatus> codec = new(EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, EnumStoredAs.LegacySnakeCase, "status");

        Action read = () => codec.FromText("lost");

        read.Should().Throw<InvalidOperationException>().WithMessage("Cannot read column status*");
    }
}
