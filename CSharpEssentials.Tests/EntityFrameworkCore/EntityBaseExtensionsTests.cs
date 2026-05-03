using System.Linq.Expressions;
using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class EntityBaseExtensionsTests
{
    private static DbContextOptions<TestDbContext<TMarker>> CreateOptions<TMarker>() where TMarker : class
    {
        return new DbContextOptionsBuilder<TestDbContext<TMarker>>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private sealed class TestDbContext<T> : DbContext where T : class
    {
        private readonly Action<ModelBuilder>? _configure;
        public TestDbContext(DbContextOptions<TestDbContext<T>> options, Action<ModelBuilder>? configure = null) : base(options)
        {
            _configure = configure;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _configure?.Invoke(modelBuilder);
        }
    }

    #region Entities

    private sealed class GuidIdEntity : EntityBase, IEntityBase<Guid>
    {
        public Guid Id { get; set; }
    }

    private sealed class SoftGuidIdEntity : SoftDeletableEntityBase, ISoftDeletableEntityBase<Guid>
    {
        public Guid Id { get; set; }
    }

    private sealed class AuditEntity : EntityBase { public int Id { get; set; } }

    private sealed class SoftAuditEntity : SoftDeletableEntityBase { public int Id { get; set; } }

    private sealed class IntEntity : EntityBase, IEntityBase<int>
    {
        public int Id { get; set; }
    }

    private sealed class SoftIntEntity : SoftDeletableEntityBase, ISoftDeletableEntityBase<int>
    {
        public int Id { get; set; }
    }

    private sealed class VersionedEntity
    {
        public int Id { get; set; }
    }

    private sealed class FilteredEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string? Name { get; set; }
    }

    private sealed class DirectSoftEntity : ISoftDeletableBase
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    private sealed class NonSoftEntity
    {
        public Guid Id { get; set; }
    }

    private sealed class FilteredMarker { }
    private sealed class FilteredCombineMarker { }
    private sealed class SoftDeleteMarker { }

    #endregion

    [Fact]
    public void EntityBaseGuidIdMap_ShouldConfigureKeyAndProperties()
    {
        var options = CreateOptions<GuidIdEntity>();
        using var context = new TestDbContext<GuidIdEntity>(options, mb =>
        {
            mb.Entity<GuidIdEntity>().EntityBaseGuidIdMap();
        });

        var entityType = context.Model.FindEntityType(typeof(GuidIdEntity));
        entityType.Should().NotBeNull();
        entityType!.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(GuidIdEntity.Id));
        GetProperty(entityType, nameof(GuidIdEntity.Id)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(GuidIdEntity.CreatedAt)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(GuidIdEntity.CreatedBy)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(GuidIdEntity.CreatedBy)).GetMaxLength().Should().Be(40);
        GetProperty(entityType, nameof(GuidIdEntity.UpdatedAt)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(GuidIdEntity.UpdatedBy)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(GuidIdEntity.UpdatedBy)).GetMaxLength().Should().Be(40);
    }

    [Fact]
    public void SoftDeletableEntityBaseGuidIdMap_ShouldConfigureSoftDeleteProperties()
    {
        var options = CreateOptions<SoftGuidIdEntity>();
        using var context = new TestDbContext<SoftGuidIdEntity>(options, mb =>
        {
            mb.Entity<SoftGuidIdEntity>().SoftDeletableEntityBaseGuidIdMap();
        });

        var entityType = context.Model.FindEntityType(typeof(SoftGuidIdEntity))!;
        entityType.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(SoftGuidIdEntity.Id));
        GetProperty(entityType, nameof(SoftGuidIdEntity.DeletedAt)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(SoftGuidIdEntity.DeletedBy)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(SoftGuidIdEntity.DeletedBy)).GetMaxLength().Should().Be(40);
        GetProperty(entityType, nameof(SoftGuidIdEntity.IsDeleted)).IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(SoftGuidIdEntity.IsHardDeleted)).Should().BeNull();
    }

    [Fact]
    public void EntityBaseMap_ShouldConfigureAuditProperties()
    {
        var options = CreateOptions<AuditEntity>();
        using var context = new TestDbContext<AuditEntity>(options, mb =>
        {
            mb.Entity<AuditEntity>().HasKey(x => x.Id);
            mb.Entity<AuditEntity>().EntityBaseMap();
        });

        var entityType = context.Model.FindEntityType(typeof(AuditEntity))!;
        GetProperty(entityType, nameof(AuditEntity.CreatedAt)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(AuditEntity.CreatedBy)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(AuditEntity.CreatedBy)).GetMaxLength().Should().Be(40);
        GetProperty(entityType, nameof(AuditEntity.UpdatedAt)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(AuditEntity.UpdatedBy)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(AuditEntity.UpdatedBy)).GetMaxLength().Should().Be(40);
    }

    [Fact]
    public void SoftDeletableEntityBaseMap_ShouldConfigureSoftDeleteAndAuditProperties()
    {
        var options = CreateOptions<SoftAuditEntity>();
        using var context = new TestDbContext<SoftAuditEntity>(options, mb =>
        {
            mb.Entity<SoftAuditEntity>().HasKey(x => x.Id);
            mb.Entity<SoftAuditEntity>().SoftDeletableEntityBaseMap();
        });

        var entityType = context.Model.FindEntityType(typeof(SoftAuditEntity))!;
        GetProperty(entityType, nameof(SoftAuditEntity.DeletedAt)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(SoftAuditEntity.DeletedBy)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(SoftAuditEntity.DeletedBy)).GetMaxLength().Should().Be(40);
        GetProperty(entityType, nameof(SoftAuditEntity.IsDeleted)).IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(SoftAuditEntity.IsHardDeleted)).Should().BeNull();
        GetProperty(entityType, nameof(SoftAuditEntity.CreatedAt)).IsNullable.Should().BeFalse();
    }

    [Fact]
    public void EntityBaseMap_Generic_ShouldConfigureKeyAndAuditProperties()
    {
        var options = CreateOptions<IntEntity>();
        using var context = new TestDbContext<IntEntity>(options, mb =>
        {
            mb.Entity<IntEntity>().EntityBaseMap<IntEntity, int>();
        });

        var entityType = context.Model.FindEntityType(typeof(IntEntity))!;
        entityType.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(IntEntity.Id));
        GetProperty(entityType, nameof(IntEntity.Id)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(IntEntity.CreatedAt)).IsNullable.Should().BeFalse();
    }

    [Fact]
    public void SoftDeletableEntityBaseMap_Generic_ShouldConfigureKeyAndSoftDeleteProperties()
    {
        var options = CreateOptions<SoftIntEntity>();
        using var context = new TestDbContext<SoftIntEntity>(options, mb =>
        {
            mb.Entity<SoftIntEntity>().SoftDeletableEntityBaseMap<SoftIntEntity, int>();
        });

        var entityType = context.Model.FindEntityType(typeof(SoftIntEntity))!;
        entityType.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(SoftIntEntity.Id));
        GetProperty(entityType, nameof(SoftIntEntity.Id)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(SoftIntEntity.IsDeleted)).IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(SoftIntEntity.IsHardDeleted)).Should().BeNull();
    }

    [Fact]
    public void OptimisticConcurrencyVersionMap_ShouldConfigureRowVersion()
    {
        var options = CreateOptions<VersionedEntity>();
        using var context = new TestDbContext<VersionedEntity>(options, mb =>
        {
            mb.Entity<VersionedEntity>().HasKey(x => x.Id);
            mb.Entity<VersionedEntity>().OptimisticConcurrencyVersionMap();
        });

        var entityType = context.Model.FindEntityType(typeof(VersionedEntity))!;
        var property = entityType.FindProperty("RowVersion")!;
        property.IsConcurrencyToken.Should().BeTrue();
        property.ValueGenerated.Should().Be(ValueGenerated.OnAddOrUpdate);
        property.ClrType.Should().Be<byte[]>();
    }

    [Fact]
    public void AddQueryFilter_ShouldApplyFilter()
    {
        var options = CreateOptions<FilteredMarker>();
        using var context = new TestDbContext<FilteredMarker>(options, mb =>
        {
            mb.Entity<FilteredEntity>().HasKey(x => x.Id);
            mb.Entity<FilteredEntity>().AddQueryFilter<FilteredEntity>(e => e.IsActive);
        });

        var entityType = context.Model.FindEntityType(typeof(FilteredEntity))!;
        var filter = entityType.GetQueryFilter();
        filter.Should().NotBeNull();
        var compiled = (Expression<Func<FilteredEntity, bool>>)filter!;
        compiled.Compile()(new FilteredEntity { IsActive = true }).Should().BeTrue();
        compiled.Compile()(new FilteredEntity { IsActive = false }).Should().BeFalse();
    }

    [Fact]
    public void AddQueryFilter_ShouldCombineFilters()
    {
        var options = CreateOptions<FilteredCombineMarker>();
        using var context = new TestDbContext<FilteredCombineMarker>(options, mb =>
        {
            mb.Entity<FilteredEntity>().HasKey(x => x.Id);
            mb.Entity<FilteredEntity>().AddQueryFilter<FilteredEntity>(e => e.IsActive);
            mb.Entity<FilteredEntity>().AddQueryFilter<FilteredEntity>(e => e.Name != null);
        });

        var entityType = context.Model.FindEntityType(typeof(FilteredEntity))!;
        var filter = entityType.GetQueryFilter();
        filter.Should().NotBeNull();
        var compiled = (Expression<Func<FilteredEntity, bool>>)filter!;
        compiled.Compile()(new FilteredEntity { IsActive = true, Name = "A" }).Should().BeTrue();
        compiled.Compile()(new FilteredEntity { IsActive = false, Name = "A" }).Should().BeFalse();
        compiled.Compile()(new FilteredEntity { IsActive = true, Name = null }).Should().BeFalse();
    }

    [Fact]
    public void ApplySoftDeleteQueryFilter_ShouldApplyToSoftDeletableEntitiesOnly()
    {
        var options = CreateOptions<SoftDeleteMarker>();
        using var context = new TestDbContext<SoftDeleteMarker>(options, mb =>
        {
            mb.Entity<DirectSoftEntity>().HasKey(x => x.Id);
            mb.Entity<NonSoftEntity>().HasKey(x => x.Id);
            mb.ApplySoftDeleteQueryFilter();
        });

        var softType = context.Model.FindEntityType(typeof(DirectSoftEntity))!;
        var nonSoftType = context.Model.FindEntityType(typeof(NonSoftEntity))!;
        softType.GetQueryFilter().Should().NotBeNull();
        nonSoftType.GetQueryFilter().Should().BeNull();
    }

    private static IProperty GetProperty(IEntityType entityType, string name)
    {
        return entityType.FindProperty(name)!;
    }
}
