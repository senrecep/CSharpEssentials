using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

public sealed class EnumStorageConventionTests
{
    [Fact]
    public void ConfigureEnumConventions_Should_StoreStringEnumAsWireName_When_Default()
    {
        IProperty property = Property(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_StoreStringEnumAsWireName_When_Default)), nameof(StoredOrder.Status));

        property.GetValueConverter().Should().BeOfType<EnumWireNameConverter<StoredOrderStatus>>();
        property.GetValueConverter()!.ConvertToProvider(StoredOrderStatus.PendingApproval).Should().Be("pending_approval");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_AddStringCheckConstraint_When_Default()
    {
        IEntityType orders = Orders(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_AddStringCheckConstraint_When_Default)));

        orders.FindCheckConstraint("ck_orders_Status_enum")!.Sql.Should().Be("\"Status\" IN ('pending', 'pending_approval', 'shipped')");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_LeavePlainEnumUntouched_When_ItHasNoMetadata()
    {
        IEntityType orders = Orders(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_LeavePlainEnumUntouched_When_ItHasNoMetadata)));

        orders.FindProperty(nameof(StoredOrder.Color))!.GetValueConverter().Should().BeNull();
        orders.GetCheckConstraints().Should().NotContain(constraint => constraint.Name!.Contains("Color", StringComparison.Ordinal));
    }

    [Fact]
    public void ConfigureEnumConventions_Should_SkipProperty_When_UserConfiguredConversion()
    {
        IEntityType orders = Orders(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_SkipProperty_When_UserConfiguredConversion)));

        orders.FindProperty(nameof(StoredOrder.ManualStatus))!.GetValueConverter().Should().BeNull();
        orders.FindCheckConstraint("ck_orders_ManualStatus_enum").Should().BeNull();
    }

    [Fact]
    public void ConfigureEnumConventions_Should_UseAttributeStorage_When_EnumDeclaresInteger()
    {
        IEntityType orders = Orders(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_UseAttributeStorage_When_EnumDeclaresInteger)));

        orders.FindProperty(nameof(StoredOrder.Priority))!.GetValueConverter().Should().BeOfType<EnumIntegerConverter<StoredPriority, int>>();
        orders.FindCheckConstraint("ck_orders_Priority_enum")!.Sql.Should().Be("\"Priority\" IN (0, 1, 2)");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_AddMaskConstraint_When_FlagsStoredAsInteger()
    {
        IEntityType orders = Orders(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_AddMaskConstraint_When_FlagsStoredAsInteger)));

        orders.FindCheckConstraint("ck_orders_Permissions_enum")!.Sql.Should().Be("(\"Permissions\" & ~7) = 0");
    }

    [Fact]
    public void HasEnumStorage_Should_WinOverAttribute()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(HasEnumStorage_Should_WinOverAttribute),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(o => o.Priority).HasEnumStorage(EnumStorage.String));

        IProperty property = Property(model, nameof(StoredOrder.Priority));

        property.GetValueConverter().Should().BeOfType<EnumWireNameConverter<StoredPriority>>();
    }

    [Fact]
    public void EnumConventions_Should_SetStorage_When_EnumDeclaresNone()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(EnumConventions_Should_SetStorage_When_EnumDeclaresNone),
            conventions: EnumConventions.Default with { Storage = EnumStorage.Integer });

        IProperty property = Property(model, nameof(StoredOrder.Status));

        property.GetValueConverter().Should().BeOfType<EnumIntegerConverter<StoredOrderStatus, int>>();
    }

    [Fact]
    public void HasEnumCheckConstraint_Should_SkipConstraint_When_Disabled()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(HasEnumCheckConstraint_Should_SkipConstraint_When_Disabled),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(o => o.Status).HasEnumCheckConstraint(false));

        IEntityType orders = Orders(model);

        orders.FindCheckConstraint("ck_orders_Status_enum").Should().BeNull();
        orders.FindProperty(nameof(StoredOrder.Status))!.GetValueConverter().Should().BeOfType<EnumWireNameConverter<StoredOrderStatus>>();
    }

    [Fact]
    public void EnumConventions_Should_SkipAllConstraints_When_CheckConstraintsDisabled()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(EnumConventions_Should_SkipAllConstraints_When_CheckConstraintsDisabled),
            conventions: EnumConventions.Default with { CheckConstraints = false });

        IEntityType orders = Orders(model);

        orders.GetCheckConstraints().Should().BeEmpty();
    }

    [Fact]
    public void ConfigureEnumConventions_Should_RemoveItsAnnotations_When_ModelIsFinalized()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(ConfigureEnumConventions_Should_RemoveItsAnnotations_When_ModelIsFinalized),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(o => o.Status).HasEnumCheckConstraint(false));

        IEntityType orders = Orders(model);

        orders.GetProperties()
            .SelectMany(property => property.GetAnnotations())
            .Should().NotContain(annotation => annotation.Name.StartsWith("CSharpEssentials:", StringComparison.Ordinal));
    }

    [Fact]
    public void ExistingStorage_Should_KeepIntegers_And_AddNoConstraint()
    {
        StoredOrderModel model = new(
            nameof(ExistingStorage_Should_KeepIntegers_And_AddNoConstraint),
            builder => builder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer),
            StoredOrderContext.ConfigureOrders);

        IEntityType orders = Orders(model);

        orders.FindProperty(nameof(StoredOrder.Status))!.GetValueConverter().Should().BeOfType<EnumIntegerConverter<StoredOrderStatus, int>>();
        orders.GetCheckConstraints().Should().BeEmpty();
    }

    [Fact]
    public void HasEnumStorage_Should_OptIn_When_ExistingStorageIsSet()
    {
        StoredOrderModel model = new(
            nameof(HasEnumStorage_Should_OptIn_When_ExistingStorageIsSet),
            builder => builder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer),
            modelBuilder =>
            {
                StoredOrderContext.ConfigureOrders(modelBuilder);
                modelBuilder.Entity<StoredOrder>().Property(o => o.Status).HasEnumStorage(EnumStorage.String);
            });

        IEntityType orders = Orders(model);

        orders.FindProperty(nameof(StoredOrder.Status))!.GetValueConverter().Should().BeOfType<EnumWireNameConverter<StoredOrderStatus>>();
        orders.GetCheckConstraints().Select(constraint => constraint.Name).Should().Equal("ck_orders_Status_enum");
    }

    [Fact]
    public void HasLegacyEnumStorage_Should_WriteOldFormat_And_ReadEverySpelling()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(HasLegacyEnumStorage_Should_WriteOldFormat_And_ReadEverySpelling),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(o => o.Status).HasLegacyEnumStorage(EnumStoredAs.MemberName));

        IEntityType orders = Orders(model);
        Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter converter = orders.FindProperty(nameof(StoredOrder.Status))!.GetValueConverter()!;

        converter.ConvertToProvider(StoredOrderStatus.PendingApproval).Should().Be("PendingApproval");
        converter.ConvertFromProvider("pending_approval").Should().Be(StoredOrderStatus.PendingApproval);
        orders.FindCheckConstraint("ck_orders_Status_enum").Should().BeNull();
    }

    [Fact]
    public void HasLegacyEnumStorage_Should_Throw_When_FormatIsText()
    {
        Action configure = () => new ModelBuilder().Entity<StoredOrder>().Property(o => o.Status).HasLegacyEnumStorage(EnumStoredAs.Text);

        configure.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void HasEnumStorage_Should_Throw_When_EnumHasNoMetadata()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(HasEnumStorage_Should_Throw_When_EnumHasNoMetadata),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(o => o.Color).HasEnumStorage(EnumStorage.String));

        Action build = () => Orders(model);

        build.Should().Throw<InvalidOperationException>().WithMessage("*PlainColor*no generated metadata*ConfigureEnumConventionsWithReflection*");
    }

    [Fact]
    public void ConfigureEnumConventionsWithReflection_Should_StorePlainEnum_When_Configured()
    {
        StoredOrderModel model = new(
            nameof(ConfigureEnumConventionsWithReflection_Should_StorePlainEnum_When_Configured),
            builder => builder.ConfigureEnumConventionsWithReflection(),
            modelBuilder =>
            {
                StoredOrderContext.ConfigureOrders(modelBuilder);
                modelBuilder.Entity<StoredOrder>().Property(o => o.Color).HasEnumStorage(EnumStorage.String);
            });

        IProperty property = Property(model, nameof(StoredOrder.Color));

        property.GetValueConverter()!.ConvertToProvider(PlainColor.Green).Should().Be("green");
    }

    [Fact]
    public void HasEnumStorage_Should_Throw_When_PropertyIsNotAnEnum()
    {
        StoredOrderModel model = StoredOrderContext.Default(
            nameof(HasEnumStorage_Should_Throw_When_PropertyIsNotAnEnum),
            modelBuilder => modelBuilder.Entity<StoredOrder>().Property(o => o.Id).HasEnumStorage(EnumStorage.String));

        Action build = () => Orders(model);

        build.Should().Throw<InvalidOperationException>().WithMessage("*StoredOrder.Id*is not an enum*");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_Throw_When_ExistingStorageIsText()
    {
        StoredOrderModel model = new(
            nameof(ConfigureEnumConventions_Should_Throw_When_ExistingStorageIsText),
            builder => builder.ConfigureEnumConventions(existingStorage: EnumStoredAs.Text),
            StoredOrderContext.ConfigureOrders);

        Action configure = () => Orders(model);

        configure.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("existingStorage");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_ConfigureJsonColumnElements()
    {
        IEntityType orders = Orders(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_ConfigureJsonColumnElements)));

        IProperty status = orders.FindNavigation(nameof(StoredOrder.Details))!.TargetEntityType.FindProperty(nameof(StoredOrderDetails.Status))!;

        status.GetTypeMapping().JsonValueReaderWriter.Should().BeOfType<EnumJsonValueReaderWriter<StoredOrderStatus>>();
    }

    [Fact]
    public void ConfigureEnumConventions_Should_WorkWithoutRelationalProvider()
    {
        using StoredOrderContext context = StoredOrderContext.InMemory(StoredOrderContext.Default(nameof(ConfigureEnumConventions_Should_WorkWithoutRelationalProvider)));

        IProperty property = context.Model.FindEntityType(typeof(StoredOrder))!.FindProperty(nameof(StoredOrder.Status))!;

        property.GetValueConverter().Should().BeOfType<EnumWireNameConverter<StoredOrderStatus>>();
    }

    private static IEntityType Orders(StoredOrderModel model)
    {
        using SqliteConnection connection = new("DataSource=:memory:");
        using StoredOrderContext context = StoredOrderContext.Sqlite(connection, model);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(StoredOrder))!;
    }

    private static IProperty Property(StoredOrderModel model, string name) => Orders(model).FindProperty(name)!;
}
