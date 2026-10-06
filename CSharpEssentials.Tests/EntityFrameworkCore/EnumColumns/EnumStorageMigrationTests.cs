using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>The migration a 4.x project gets when it moves to the 5.0 enum conventions.</summary>
public sealed class EnumStorageMigrationTests
{
    [Fact]
    public void ExistingStorageInteger_Should_ProduceEmptyMigration_When_UpgradingFrom4x()
    {
        StoredOrderModel before = new($"{nameof(EnumStorageMigrationTests)}.4x", null, StoredOrderContext.ConfigureOrders);
        StoredOrderModel after = new(
            $"{nameof(EnumStorageMigrationTests)}.existing-integer",
            builder => builder.ConfigureEnumConventions(EnumConventions.Default, existingStorage: EnumStoredAs.Integer),
            StoredOrderContext.ConfigureOrders);

        IReadOnlyList<MigrationOperation> operations = Diff(before, after);

        operations.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConventions_Should_ChangeStringEnumColumns_When_ExistingStorageIsNotSet()
    {
        StoredOrderModel before = new($"{nameof(EnumStorageMigrationTests)}.4x-baseline", null, StoredOrderContext.ConfigureOrders);
        StoredOrderModel after = new(
            $"{nameof(EnumStorageMigrationTests)}.default",
            builder => builder.ConfigureEnumConventions(),
            StoredOrderContext.ConfigureOrders);

        IReadOnlyList<MigrationOperation> operations = Diff(before, after);

        operations.OfType<AlterColumnOperation>().Should().Contain(operation => operation.Name == nameof(StoredOrder.Status) && operation.ClrType == typeof(string));
        operations.OfType<AddCheckConstraintOperation>().Should().Contain(operation => operation.Name == "ck_orders_Status_enum");
    }

    [Fact]
    public void DefaultConventions_Should_ProduceEmptyMigration_When_ModelHasOnlyPlainEnums()
    {
        StoredOrderModel before = new($"{nameof(EnumStorageMigrationTests)}.plain-4x", null, modelBuilder => ConfigureVersioned<PlainOrder>(modelBuilder));
        StoredOrderModel after = new(
            $"{nameof(EnumStorageMigrationTests)}.plain-default",
            builder => builder.ConfigureEnumConventions(),
            modelBuilder => ConfigureVersioned<PlainOrder>(modelBuilder));

        IReadOnlyList<MigrationOperation> operations = Diff(before, after);

        operations.Should().BeEmpty();
    }

    [Fact]
    public void AddingEnumMember_Should_OnlyReplaceCheckConstraint()
    {
        StoredOrderModel before = new(
            $"{nameof(EnumStorageMigrationTests)}.v1",
            builder => builder.ConfigureEnumConventions(),
            modelBuilder => ConfigureVersioned<StoredOrderV1>(modelBuilder));
        StoredOrderModel after = new(
            $"{nameof(EnumStorageMigrationTests)}.v2",
            builder => builder.ConfigureEnumConventions(),
            modelBuilder => ConfigureVersioned<StoredOrderV2>(modelBuilder));

        IReadOnlyList<MigrationOperation> operations = Diff(before, after);

        operations.Select(operation => operation.GetType()).Should().BeEquivalentTo([typeof(DropCheckConstraintOperation), typeof(AddCheckConstraintOperation)]);
        operations.OfType<AddCheckConstraintOperation>().Single().Sql.Should().Be("\"Status\" IN ('pending', 'pending_approval', 'shipped', 'returned')");
    }

    private static void ConfigureVersioned<TOrder>(ModelBuilder modelBuilder)
        where TOrder : class
    {
        modelBuilder.Ignore<StoredOrder>();
        modelBuilder.Entity<TOrder>().ToTable("orders");
    }

    private static IReadOnlyList<MigrationOperation> Diff(StoredOrderModel before, StoredOrderModel after)
    {
        using SqliteConnection connection = new("DataSource=:memory:");
        using StoredOrderContext source = StoredOrderContext.Sqlite(connection, before);
        using StoredOrderContext target = StoredOrderContext.Sqlite(connection, after);

        IRelationalModel sourceModel = source.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        IRelationalModel targetModel = target.GetService<IDesignTimeModel>().Model.GetRelationalModel();

        return target.GetService<IMigrationsModelDiffer>().GetDifferences(sourceModel, targetModel);
    }
}
