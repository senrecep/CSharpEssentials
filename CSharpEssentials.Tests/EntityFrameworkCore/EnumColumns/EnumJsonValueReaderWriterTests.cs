using System.Linq.Expressions;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Enums;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

public sealed class EnumJsonValueReaderWriterTests
{
    [Fact]
    public void ParameterlessConstructor_Should_WriteWireNames_With_TheGeneratedMetadata()
    {
        EnumJsonValueReaderWriter<StoredOrderStatus> readerWriter = new();

        readerWriter.ToJsonString(StoredOrderStatus.PendingApproval).Should().Be("\"pending_approval\"");
        readerWriter.Info.Should().BeSameAs(EnumMetadata.Get<StoredOrderStatus>());
        readerWriter.Conventions.Should().BeSameAs(EnumConventions.Default);
        readerWriter.Storage.Should().Be(EnumStorage.String);
        readerWriter.LegacyFormat.Should().BeNull();
    }

    [Fact]
    public void ConstructorExpression_Should_BuildAnEquivalentReaderWriter()
    {
        EnumJsonValueReaderWriter<StoredPermissions> original = new(
            EnumMetadata.Get<StoredPermissions>(), EnumConventions.Default, EnumStorage.Integer, EnumStoredAs.FlagsText);

        Func<object> factory = Expression.Lambda<Func<object>>(original.ConstructorExpression).Compile();
        var rebuilt = (EnumJsonValueReaderWriter<StoredPermissions>)factory();

        rebuilt.Should().NotBeSameAs(original);
        rebuilt.Info.Should().BeSameAs(original.Info);
        rebuilt.Conventions.Should().BeSameAs(original.Conventions);
        rebuilt.Storage.Should().Be(EnumStorage.Integer);
        rebuilt.LegacyFormat.Should().Be(EnumStoredAs.FlagsText);
        rebuilt.ToJsonString(StoredPermissions.Read | StoredPermissions.Write)
            .Should().Be(original.ToJsonString(StoredPermissions.Read | StoredPermissions.Write));
    }

    [Fact]
    public void ConstructorExpression_Should_KeepANullLegacyFormat()
    {
        EnumJsonValueReaderWriter<StoredOrderStatus> original = new();

        Func<object> factory = Expression.Lambda<Func<object>>(original.ConstructorExpression).Compile();

        ((EnumJsonValueReaderWriter<StoredOrderStatus>)factory()).LegacyFormat.Should().BeNull();
    }

    [Fact]
    public void IntegerStorage_Should_WriteAnUnsignedNumber_When_TheValueDoesNotFitInALong()
    {
        EnumJsonValueReaderWriter<StoredWideMask> readerWriter = new(EnumMetadata.Get<StoredWideMask>(), EnumConventions.Default, EnumStorage.Integer);
        const StoredWideMask value = StoredWideMask.High | StoredWideMask.Low;

        string json = readerWriter.ToJsonString(value);

        json.Should().Be("9223372036854775809");
        readerWriter.FromJsonString(json).Should().Be(value);
    }

    [Fact]
    public void IntegerStorage_Should_WriteAnUnsignedNumber_When_TheUnderlyingTypeIsUint()
    {
        EnumJsonValueReaderWriter<StoredUIntLevel> readerWriter = new(EnumMetadata.Get<StoredUIntLevel>(), EnumConventions.Default, EnumStorage.Integer);

        string json = readerWriter.ToJsonString(StoredUIntLevel.High);

        json.Should().Be("4000000000");
        readerWriter.FromJsonString(json).Should().Be(StoredUIntLevel.High);
    }

    [Fact]
    public void IntegerStorage_Should_WriteANegativeNumber_When_TheUnderlyingTypeIsSigned()
    {
        EnumJsonValueReaderWriter<StoredSByteLevel> readerWriter = new(EnumMetadata.Get<StoredSByteLevel>(), EnumConventions.Default, EnumStorage.Integer);

        string json = readerWriter.ToJsonString(StoredSByteLevel.Below);

        json.Should().Be("-1");
        readerWriter.FromJsonString(json).Should().Be(StoredSByteLevel.Below);
    }

    [Fact]
    public void LegacyIntegerFormat_Should_WriteNumbers_When_StorageIsString()
    {
        EnumJsonValueReaderWriter<StoredOrderStatus> readerWriter = new(
            EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, EnumStorage.String, EnumStoredAs.Integer);

        readerWriter.ToJsonString(StoredOrderStatus.Shipped).Should().Be("2");
    }

    [Fact]
    public void ToJsonTyped_Should_Throw_When_WriterIsNull()
    {
        EnumJsonValueReaderWriter<StoredOrderStatus> readerWriter = new();

        Action act = () => readerWriter.ToJsonTyped(null!, StoredOrderStatus.Pending);

        act.Should().Throw<ArgumentNullException>().WithParameterName("writer");
    }

    [Fact]
    public void Constructor_Should_Throw_When_LegacyFormatIsText()
    {
        Action create = () => _ = new EnumJsonValueReaderWriter<StoredOrderStatus>(
            EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, EnumStorage.String, EnumStoredAs.Text);

        create.Should().Throw<ArgumentException>().WithParameterName("legacyFormat");
    }

    [Fact]
    public void LegacyMemberNameFormat_Should_WriteMemberNames_And_ReadWireNames()
    {
        EnumJsonValueReaderWriter<StoredOrderStatus> readerWriter = new(
            EnumMetadata.Get<StoredOrderStatus>(), EnumConventions.Default, EnumStorage.String, EnumStoredAs.MemberName);

        readerWriter.ToJsonString(StoredOrderStatus.PendingApproval).Should().Be("\"PendingApproval\"");
        readerWriter.FromJsonString("\"pending_approval\"").Should().Be(StoredOrderStatus.PendingApproval);
    }
}
