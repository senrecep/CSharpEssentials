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
        public TestDbContext(DbContextOptions<TestDbContext<T>> options, Action<ModelBuilder>? configure = null) : base(options) => _configure = configure;
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
        DbContextOptions<TestDbContext<GuidIdEntity>> options = CreateOptions<GuidIdEntity>();
        using var context = new TestDbContext<GuidIdEntity>(options, mb => mb.Entity<GuidIdEntity>().EntityBaseGuidIdMap());

        IEntityType? entityType = context.Model.FindEntityType(typeof(GuidIdEntity));
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
        DbContextOptions<TestDbContext<SoftGuidIdEntity>> options = CreateOptions<SoftGuidIdEntity>();
        using var context = new TestDbContext<SoftGuidIdEntity>(options, mb => mb.Entity<SoftGuidIdEntity>().SoftDeletableEntityBaseGuidIdMap());

        IEntityType entityType = context.Model.FindEntityType(typeof(SoftGuidIdEntity))!;
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
        DbContextOptions<TestDbContext<AuditEntity>> options = CreateOptions<AuditEntity>();
        using var context = new TestDbContext<AuditEntity>(options, mb =>
        {
            mb.Entity<AuditEntity>().HasKey(x => x.Id);
            mb.Entity<AuditEntity>().EntityBaseMap();
        });

        IEntityType entityType = context.Model.FindEntityType(typeof(AuditEntity))!;
        GetProperty(entityType, nameof(AuditEntity.CreatedAt)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(AuditEntity.CreatedBy)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(AuditEntity.CreatedBy)).GetMaxLength().Should().Be(40);
        GetProperty(entityType, nameof(AuditEntity.UpdatedAt)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(AuditEntity.UpdatedBy)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(AuditEntity.UpdatedBy)).GetMaxLength().Should().Be(40);
        _ = new AuditEntity { Id = 1 };
    }

    [Fact]
    public void SoftDeletableEntityBaseMap_ShouldConfigureSoftDeleteAndAuditProperties()
    {
        DbContextOptions<TestDbContext<SoftAuditEntity>> options = CreateOptions<SoftAuditEntity>();
        using var context = new TestDbContext<SoftAuditEntity>(options, mb =>
        {
            mb.Entity<SoftAuditEntity>().HasKey(x => x.Id);
            mb.Entity<SoftAuditEntity>().SoftDeletableEntityBaseMap();
        });

        IEntityType entityType = context.Model.FindEntityType(typeof(SoftAuditEntity))!;
        GetProperty(entityType, nameof(SoftAuditEntity.DeletedAt)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(SoftAuditEntity.DeletedBy)).IsNullable.Should().BeTrue();
        GetProperty(entityType, nameof(SoftAuditEntity.DeletedBy)).GetMaxLength().Should().Be(40);
        GetProperty(entityType, nameof(SoftAuditEntity.IsDeleted)).IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(SoftAuditEntity.IsHardDeleted)).Should().BeNull();
        GetProperty(entityType, nameof(SoftAuditEntity.CreatedAt)).IsNullable.Should().BeFalse();
        _ = new SoftAuditEntity { Id = 1 };
    }

    [Fact]
    public void EntityBaseMap_Generic_ShouldConfigureKeyAndAuditProperties()
    {
        DbContextOptions<TestDbContext<IntEntity>> options = CreateOptions<IntEntity>();
        using var context = new TestDbContext<IntEntity>(options, mb => mb.Entity<IntEntity>().EntityBaseMap<IntEntity, int>());

        IEntityType entityType = context.Model.FindEntityType(typeof(IntEntity))!;
        entityType.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(IntEntity.Id));
        GetProperty(entityType, nameof(IntEntity.Id)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(IntEntity.CreatedAt)).IsNullable.Should().BeFalse();
    }

    [Fact]
    public void SoftDeletableEntityBaseMap_Generic_ShouldConfigureKeyAndSoftDeleteProperties()
    {
        DbContextOptions<TestDbContext<SoftIntEntity>> options = CreateOptions<SoftIntEntity>();
        using var context = new TestDbContext<SoftIntEntity>(options, mb => mb.Entity<SoftIntEntity>().SoftDeletableEntityBaseMap<SoftIntEntity, int>());

        IEntityType entityType = context.Model.FindEntityType(typeof(SoftIntEntity))!;
        entityType.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == nameof(SoftIntEntity.Id));
        GetProperty(entityType, nameof(SoftIntEntity.Id)).IsNullable.Should().BeFalse();
        GetProperty(entityType, nameof(SoftIntEntity.IsDeleted)).IsNullable.Should().BeFalse();
        entityType.FindProperty(nameof(SoftIntEntity.IsHardDeleted)).Should().BeNull();
    }

    [Fact]
    public void OptimisticConcurrencyVersionMap_ShouldConfigureRowVersion()
    {
        DbContextOptions<TestDbContext<VersionedEntity>> options = CreateOptions<VersionedEntity>();
        using var context = new TestDbContext<VersionedEntity>(options, mb =>
        {
            mb.Entity<VersionedEntity>().HasKey(x => x.Id);
            mb.Entity<VersionedEntity>().OptimisticConcurrencyVersionMap();
        });

        IEntityType entityType = context.Model.FindEntityType(typeof(VersionedEntity))!;
        IProperty property = entityType.FindProperty("RowVersion")!;
        property.IsConcurrencyToken.Should().BeTrue();
        property.ValueGenerated.Should().Be(ValueGenerated.OnAddOrUpdate);
        property.ClrType.Should().Be<byte[]>();
        _ = new VersionedEntity { Id = 1 };
    }

    [Fact]
    public void AddQueryFilter_ShouldApplyFilter()
    {
        DbContextOptions<TestDbContext<FilteredMarker>> options = CreateOptions<FilteredMarker>();
        using var context = new TestDbContext<FilteredMarker>(options, mb =>
        {
            mb.Entity<FilteredEntity>().HasKey(x => x.Id);
            mb.Entity<FilteredEntity>().AddQueryFilter<FilteredEntity>(e => e.IsActive);
        });

        IEntityType entityType = context.Model.FindEntityType(typeof(FilteredEntity))!;
        LambdaExpression? filter = entityType.GetQueryFilter();
        filter.Should().NotBeNull();
        var compiled = (Expression<Func<FilteredEntity, bool>>)filter!;
        compiled.Compile()(new FilteredEntity { Id = 1, IsActive = true }).Should().BeTrue();
        compiled.Compile()(new FilteredEntity { Id = 1, IsActive = false }).Should().BeFalse();
    }

    [Fact]
    public void AddQueryFilter_ShouldCombineFilters()
    {
        DbContextOptions<TestDbContext<FilteredCombineMarker>> options = CreateOptions<FilteredCombineMarker>();
        using var context = new TestDbContext<FilteredCombineMarker>(options, mb =>
        {
            mb.Entity<FilteredEntity>().HasKey(x => x.Id);
            mb.Entity<FilteredEntity>().AddQueryFilter<FilteredEntity>(e => e.IsActive);
            mb.Entity<FilteredEntity>().AddQueryFilter<FilteredEntity>(e => e.Name != null);
        });

        IEntityType entityType = context.Model.FindEntityType(typeof(FilteredEntity))!;
        LambdaExpression? filter = entityType.GetQueryFilter();
        filter.Should().NotBeNull();
        var compiled = (Expression<Func<FilteredEntity, bool>>)filter!;
        compiled.Compile()(new FilteredEntity { Id = 1, IsActive = true, Name = "A" }).Should().BeTrue();
        compiled.Compile()(new FilteredEntity { Id = 1, IsActive = false, Name = "A" }).Should().BeFalse();
        compiled.Compile()(new FilteredEntity { Id = 1, IsActive = true, Name = null }).Should().BeFalse();
    }

    [Fact]
    public void ApplySoftDeleteQueryFilter_ShouldApplyToSoftDeletableEntitiesOnly()
    {
        DbContextOptions<TestDbContext<SoftDeleteMarker>> options = CreateOptions<SoftDeleteMarker>();
        using var context = new TestDbContext<SoftDeleteMarker>(options, mb =>
        {
            mb.Entity<DirectSoftEntity>().HasKey(x => x.Id);
            mb.Entity<NonSoftEntity>().HasKey(x => x.Id);
            mb.ApplySoftDeleteQueryFilter();
        });

        IEntityType softType = context.Model.FindEntityType(typeof(DirectSoftEntity))!;
        IEntityType nonSoftType = context.Model.FindEntityType(typeof(NonSoftEntity))!;
        softType.GetQueryFilter().Should().NotBeNull();
        nonSoftType.GetQueryFilter().Should().BeNull();
        _ = new DirectSoftEntity { Id = Guid.NewGuid() };
        _ = new NonSoftEntity { Id = Guid.NewGuid() };
    }

    private static IProperty GetProperty(IEntityType entityType, string name)
    {
        return entityType.FindProperty(name)!;
    }
}
