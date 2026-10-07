using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Keyset;

public sealed class KeysetOrderingTests
{
    public sealed class AuditInfo
    {
        public DateTime CreatedAt { get; set; }
    }

    public sealed class AuditedRow
    {
        public int Id { get; set; }
        public AuditInfo Audit { get; set; } = new();
        public AuditInfo? LastReview { get; set; }
    }

    public sealed class OtherRow
    {
        public int Id { get; set; }
    }

    [Fact]
    public void Ascending_Should_Throw_When_KeyPathHasNullableNavigation()
    {
        Action act = () => new KeysetOrdering<AuditedRow>().Ascending(x => x.LastReview!.CreatedAt);

        act.Should().Throw<ArgumentException>().WithMessage("*LastReview*");
    }

    [Fact]
    public void Ascending_Should_AcceptKey_When_KeyPathHasRequiredNavigation()
    {
        KeysetOrdering<AuditedRow> ordering = new KeysetOrdering<AuditedRow>().Ascending(x => x.Audit.CreatedAt).Ascending(x => x.Id);

        ordering.Count.Should().Be(2);
    }

    [Fact]
    public void TryDecode_Should_ReturnKeyMismatch_When_CursorComesFromAnotherEntityWithSameKeyShape()
    {
        KeysetOrdering<KeysetRow> source = new KeysetOrdering<KeysetRow>().Ascending(x => x.Id);
        KeysetOrdering<OtherRow> target = new KeysetOrdering<OtherRow>().Ascending(x => x.Id);
        string cursor = KeysetCursorCodec.Encode(
            KeysetCursorDirection.After, source.Fingerprint, source.KeyTypes, [5], NoOpCursorProtector.Instance);

        bool decoded = KeysetCursorCodec.TryDecode(
            cursor, KeysetCursorDirection.After, target.Fingerprint, target.KeyTypes, NoOpCursorProtector.Instance, out _, out Error error);

        decoded.Should().BeFalse();
        error.Code.Should().Be(KeysetCursorErrors.KeyMismatchCode);
    }

    [Fact]
    public void Ascending_Should_Throw_When_KeyIsNullableValueType()
    {
        Action act = () => new KeysetOrdering<KeysetRow>().Ascending(x => x.Rank);

        act.Should().Throw<ArgumentException>().WithMessage("*nullable*");
    }

    [Fact]
    public void Ascending_Should_Throw_When_KeyIsNullableReferenceType()
    {
        Action act = () => new KeysetOrdering<KeysetRow>().Ascending(x => x.Note);

        act.Should().Throw<ArgumentException>().WithMessage("*nullable*");
    }

    [Fact]
    public void Ascending_Should_Throw_When_KeyTypeIsNotSupported()
    {
        Action act = () => new KeysetOrdering<KeysetRow>().Ascending(x => x.Active);

        act.Should().Throw<ArgumentException>().WithMessage("*Boolean*");
    }

    [Fact]
    public void Ascending_Should_Throw_When_KeyIsNotMemberAccess()
    {
        Action act = () => new KeysetOrdering<KeysetRow>().Ascending(x => x.Id + 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ascending_Should_Throw_When_KeyDoesNotUseParameter()
    {
        var other = new KeysetRow();

        Action act = () => new KeysetOrdering<KeysetRow>().Ascending(_ => other.Id);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Descending_Should_Throw_When_KeyIsAlreadyAdded()
    {
        KeysetOrdering<KeysetRow> ordering = new KeysetOrdering<KeysetRow>().Ascending(x => x.Id);

        Action act = () => ordering.Descending(x => x.Id);

        act.Should().Throw<ArgumentException>().WithMessage("*already*");
    }

    [Fact]
    public void Ascending_Should_ReturnNewInstance_When_KeyIsAdded()
    {
        var empty = new KeysetOrdering<KeysetRow>();

        KeysetOrdering<KeysetRow> ordering = empty.Ascending(x => x.CreatedAt).Ascending(x => x.Id);

        empty.Count.Should().Be(0);
        ordering.Count.Should().Be(2);
    }

    [Fact]
    public void Fingerprint_Should_Differ_When_DirectionOrKeysDiffer()
    {
        string ascending = new KeysetOrdering<KeysetRow>().Ascending(x => x.CreatedAt).Ascending(x => x.Id).Fingerprint;
        string descending = new KeysetOrdering<KeysetRow>().Descending(x => x.CreatedAt).Ascending(x => x.Id).Fingerprint;
        string reordered = new KeysetOrdering<KeysetRow>().Ascending(x => x.Id).Ascending(x => x.CreatedAt).Fingerprint;
        string same = new KeysetOrdering<KeysetRow>().Ascending(x => x.CreatedAt).Ascending(x => x.Id).Fingerprint;

        new[] { ascending, descending, reordered }.Should().OnlyHaveUniqueItems();
        same.Should().Be(ascending);
    }

    [Fact]
    public void KeysetPaginationOptions_Should_Throw_When_MaxLimitIsLessThanOne()
    {
        Action act = () => _ = new KeysetPaginationOptions { MaxLimit = 0 };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    public void KeysetPaginationOptions_Should_Throw_When_MaxLimitWouldOverflow(int maxLimit)
    {
        Action act = () => _ = new KeysetPaginationOptions { MaxLimit = maxLimit };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void KeysetPaginationOptions_Should_AcceptMaxLimit_When_ItIsLargestNonOverflowingValue()
    {
        var options = new KeysetPaginationOptions { MaxLimit = int.MaxValue - 1 };

        options.MaxLimit.Should().Be(int.MaxValue - 1);
    }

    [Fact]
    public void KeysetPaginationOptions_Should_Throw_When_MaxCursorLengthIsLessThanOne()
    {
        Action act = () => _ = new KeysetPaginationOptions { MaxCursorLength = 0 };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void KeysetPaginationOptions_Should_UseDefaults_When_NotConfigured()
    {
        KeysetPaginationOptions options = KeysetPaginationOptions.Default;

        options.MaxLimit.Should().Be(KeysetPaginationOptions.DefaultMaxLimit);
        options.MaxCursorLength.Should().Be(KeysetPaginationOptions.DefaultMaxCursorLength);
        options.Protector.Should().BeSameAs(NoOpCursorProtector.Instance);
        options.PredicateBuilder.Should().BeSameAs(KeysetPredicateBuilder.Instance);
    }
}
