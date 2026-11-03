using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

public sealed class EnumComplexPropertyBuilderTests
{
    private static ComplexTypePropertyBuilder<StoredOrderStatus> StatusBuilder() =>
        new ModelBuilder().Entity<StoredComplexOrder>().ComplexProperty(order => order.Address).Property(address => address.Status);

    [Fact]
    public void HasEnumStorage_Should_AnnotateComplexTypeProperty()
    {
        ComplexTypePropertyBuilder<StoredOrderStatus> builder = StatusBuilder();

        ComplexTypePropertyBuilder<StoredOrderStatus> returned = builder.HasEnumStorage(EnumStorage.Integer);

        returned.Should().BeSameAs(builder);
        builder.Metadata.FindAnnotation(EnumPropertyBuilderExtensions.StorageAnnotation)!.Value.Should().Be("Integer");
    }

    [Fact]
    public void HasLegacyEnumStorage_Should_AnnotateComplexTypeProperty()
    {
        ComplexTypePropertyBuilder<StoredOrderStatus> builder = StatusBuilder();

        builder.HasLegacyEnumStorage(EnumStoredAs.MemberName);

        builder.Metadata.FindAnnotation(EnumPropertyBuilderExtensions.LegacyStorageAnnotation)!.Value.Should().Be("MemberName");
    }

    [Fact]
    public void HasLegacyEnumStorage_Should_Throw_When_ComplexTypeFormatIsText()
    {
        Action act = () => StatusBuilder().HasLegacyEnumStorage(EnumStoredAs.Text);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("format");
    }

    [Fact]
    public void HasLegacyEnumStorage_Should_Throw_When_FormatIsUndefined()
    {
        Action act = () => StatusBuilder().HasLegacyEnumStorage((EnumStoredAs)99);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("format");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HasEnumCheckConstraint_Should_AnnotateComplexTypeProperty(bool enabled)
    {
        ComplexTypePropertyBuilder<StoredOrderStatus> builder = StatusBuilder();

        builder.HasEnumCheckConstraint(enabled);

        builder.Metadata.FindAnnotation(EnumPropertyBuilderExtensions.CheckConstraintAnnotation)!.Value.Should().Be(enabled);
    }

    [Fact]
    public void HasEnumCheckConstraint_Should_DefaultToEnabled_When_ComplexTypeArgumentOmitted()
    {
        ComplexTypePropertyBuilder<StoredOrderStatus> builder = StatusBuilder();

        builder.HasEnumCheckConstraint();

        builder.Metadata.FindAnnotation(EnumPropertyBuilderExtensions.CheckConstraintAnnotation)!.Value.Should().Be(true);
    }

    [Fact]
    public void ComplexTypeOverloads_Should_Throw_When_BuilderIsNull()
    {
        ComplexTypePropertyBuilder<StoredOrderStatus> builder = null!;

        Action storage = () => builder.HasEnumStorage(EnumStorage.String);
        Action legacy = () => builder.HasLegacyEnumStorage(EnumStoredAs.MemberName);
        Action check = () => builder.HasEnumCheckConstraint();

        storage.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        legacy.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        check.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }
}
