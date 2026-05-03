using CSharpEssentials.Entity;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class DbContextExtensionMethodsTests
{
    private static DbContextOptions<T> CreateOptions<T>() where T : DbContext
    {
        return new DbContextOptionsBuilder<T>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    #region Entities

    private sealed class TrackEntity : EntityBase, IEntityBase<Guid>
    {
        public Guid Id { get; set; }
    }

    private sealed class HardDeleteEntity : SoftDeletableEntityBase, ISoftDeletableEntityBase<Guid>
    {
        public Guid Id { get; set; }
    }

    private sealed class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SimpleSeed
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SoftMigrateEntity : SoftDeletableEntityBase, IEntityBase<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SoftMigrateSeed
    {
        public int Key { get; init; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region DbContexts

    private sealed class TrackDbContext : DbContext
    {
        public DbSet<TrackEntity> TrackEntities { get; set; } = null!;
        public TrackDbContext(DbContextOptions<TrackDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackEntity>().HasKey(x => x.Id);
        }
    }

    private sealed class HardDeleteDbContext : DbContext
    {
        public DbSet<HardDeleteEntity> HardDeleteEntities { get; set; } = null!;
        public HardDeleteDbContext(DbContextOptions<HardDeleteDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HardDeleteEntity>().HasKey(x => x.Id);
        }
    }

    private sealed class MigrateDbContext : DbContext
    {
        public DbSet<SimpleEntity> SimpleEntities { get; set; } = null!;
        public MigrateDbContext(DbContextOptions<MigrateDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleEntity>().HasKey(x => x.Id);
        }
    }

    private sealed class ChangeTrackerDbContext : DbContext
    {
        public DbSet<SoftMigrateEntity> SoftMigrates { get; set; } = null!;
        public List<EntityEntry> EntriesBeforeSave { get; } = new();
        public ChangeTrackerDbContext(DbContextOptions<ChangeTrackerDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SoftMigrateEntity>().HasKey(x => x.Id);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EntriesBeforeSave.AddRange(ChangeTracker.Entries().ToList());
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region AsNoTracking

    [Fact]
    public void AsNoTracking_WhenDisabled_ShouldTrackEntities()
    {
        using var context = new TrackDbContext(CreateOptions<TrackDbContext>());
        context.TrackEntities.Add(new TrackEntity { Id = Guid.NewGuid() });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        _ = context.TrackEntities.AsNoTracking(isDisabled: true).ToList();
        context.ChangeTracker.Entries().Should().HaveCount(1);
    }

    [Fact]
    public void AsNoTracking_WhenEnabled_ShouldNotTrackEntities()
    {
        using var context = new TrackDbContext(CreateOptions<TrackDbContext>());
        context.TrackEntities.Add(new TrackEntity { Id = Guid.NewGuid() });
        context.SaveChanges();
        context.ChangeTracker.Clear();

        _ = context.TrackEntities.AsNoTracking(isDisabled: false).ToList();
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    #endregion

    #region HardDelete

    [Fact]
    public void HardDelete_ContextEntity_Null_ShouldThrowArgumentNullException()
    {
        using var context = new HardDeleteDbContext(CreateOptions<HardDeleteDbContext>());
        Action act = () => context.HardDelete((HardDeleteEntity?)null!);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void HardDelete_ContextEntity_ShouldMarkHardDeletedAndRemove()
    {
        using var context = new HardDeleteDbContext(CreateOptions<HardDeleteDbContext>());
        var entity = new HardDeleteEntity { Id = Guid.NewGuid() };
        context.HardDeleteEntities.Add(entity);
        context.SaveChanges();

        context.HardDelete(entity);

        entity.IsHardDeleted.Should().BeTrue();
        context.Entry(entity).State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public void HardDelete_ContextEntities_ShouldMarkHardDeletedAndRemoveRange()
    {
        using var context = new HardDeleteDbContext(CreateOptions<HardDeleteDbContext>());
        var e1 = new HardDeleteEntity { Id = Guid.NewGuid() };
        var e2 = new HardDeleteEntity { Id = Guid.NewGuid() };
        context.HardDeleteEntities.AddRange(e1, e2);
        context.SaveChanges();

        context.HardDelete<HardDeleteEntity>(new[] { e1, e2 });

        e1.IsHardDeleted.Should().BeTrue();
        e2.IsHardDeleted.Should().BeTrue();
        context.Entry(e1).State.Should().Be(EntityState.Deleted);
        context.Entry(e2).State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public void HardDelete_DbSetEntity_Null_ShouldThrowArgumentNullException()
    {
        using var context = new HardDeleteDbContext(CreateOptions<HardDeleteDbContext>());
        Action act = () => context.HardDeleteEntities.HardDelete(null!);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void HardDelete_DbSetEntity_ShouldMarkHardDeletedAndRemove()
    {
        using var context = new HardDeleteDbContext(CreateOptions<HardDeleteDbContext>());
        var entity = new HardDeleteEntity { Id = Guid.NewGuid() };
        context.HardDeleteEntities.Add(entity);
        context.SaveChanges();

        context.HardDeleteEntities.HardDelete(entity);

        entity.IsHardDeleted.Should().BeTrue();
        context.Entry(entity).State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public void Delete_DbSetEntities_ShouldMarkHardDeletedAndRemoveRange()
    {
        using var context = new HardDeleteDbContext(CreateOptions<HardDeleteDbContext>());
        var e1 = new HardDeleteEntity { Id = Guid.NewGuid() };
        var e2 = new HardDeleteEntity { Id = Guid.NewGuid() };
        context.HardDeleteEntities.AddRange(e1, e2);
        context.SaveChanges();

        context.HardDeleteEntities.Delete(new[] { e1, e2 });

        e1.IsHardDeleted.Should().BeTrue();
        e2.IsHardDeleted.Should().BeTrue();
        context.Entry(e1).State.Should().Be(EntityState.Deleted);
        context.Entry(e2).State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region MigrateDataAsync

    [Fact]
    public async Task MigrateDataAsync_Simple_PreConditionTrue_ShouldNotAdd()
    {
        using var context = new MigrateDbContext(CreateOptions<MigrateDbContext>());
        var seeds = new[] { new SimpleSeed { Key = 1, Name = "A" } };
        await context.MigrateDataAsync<SimpleEntity, SimpleSeed>(
            seeds,
            (dbSet, list) => true,
            s => new SimpleEntity { Id = s.Key, Name = s.Name });

        context.SimpleEntities.Should().BeEmpty();
    }

    [Fact]
    public async Task MigrateDataAsync_Simple_PreConditionFalse_ShouldAddAll()
    {
        using var context = new MigrateDbContext(CreateOptions<MigrateDbContext>());
        var seeds = new[] { new SimpleSeed { Key = 1, Name = "A" }, new SimpleSeed { Key = 2, Name = "B" } };
        await context.MigrateDataAsync<SimpleEntity, SimpleSeed>(
            seeds,
            (dbSet, list) => false,
            s => new SimpleEntity { Id = s.Key, Name = s.Name });

        context.SimpleEntities.Should().HaveCount(2);
    }

    [Fact]
    public async Task MigrateDataAsync_WithPreCondition_PreConditionTrue_ShouldNotModify()
    {
        using var context = new MigrateDbContext(CreateOptions<MigrateDbContext>());
        context.SimpleEntities.Add(new SimpleEntity { Id = 1, Name = "Existing" });
        await context.SaveChangesAsync();

        var seeds = new[] { new SimpleSeed { Key = 2, Name = "B" } };
        await context.MigrateDataAsync(
            seeds,
            new MigrateDataOptions<SimpleEntity, SimpleSeed, int>
            {
                Query = q => q,
                PreConditionFunc = (dbSet, list) => true,
                EntityKeyProperty = e => e.Id,
                DataKeyProperty = s => s.Key,
                IsUpdatedFunc = (e, s) => false,
                UpdateFunc = (e, s) => e,
                Converter = s => new SimpleEntity { Id = s.Key, Name = s.Name }
            });

        context.SimpleEntities.Should().HaveCount(1);
    }

    [Fact]
    public async Task MigrateDataAsync_Core_ShouldAddNewEntities()
    {
        using var context = new MigrateDbContext(CreateOptions<MigrateDbContext>());
        var seeds = new[] { new SimpleSeed { Key = 1, Name = "A" }, new SimpleSeed { Key = 2, Name = "B" } };
        await context.MigrateDataAsync(
            seeds,
            new MigrateDataOptions<SimpleEntity, SimpleSeed, int>
            {
                Query = q => q,
                EntityKeyProperty = e => e.Id,
                DataKeyProperty = s => s.Key,
                IsUpdatedFunc = (e, s) => false,
                UpdateFunc = (e, s) => e,
                Converter = s => new SimpleEntity { Id = s.Key, Name = s.Name }
            });

        context.SimpleEntities.Should().HaveCount(2);
    }

    [Fact]
    public async Task MigrateDataAsync_Core_ShouldUpdateExistingEntities()
    {
        using var context = new MigrateDbContext(CreateOptions<MigrateDbContext>());
        context.SimpleEntities.Add(new SimpleEntity { Id = 1, Name = "Old" });
        await context.SaveChangesAsync();

        var seeds = new[] { new SimpleSeed { Key = 1, Name = "New" } };
        await context.MigrateDataAsync(
            seeds,
            new MigrateDataOptions<SimpleEntity, SimpleSeed, int>
            {
                Query = q => q,
                EntityKeyProperty = e => e.Id,
                DataKeyProperty = s => s.Key,
                IsUpdatedFunc = (e, s) => e.Name != s.Name,
                UpdateFunc = (e, s) => { e.Name = s.Name; return e; },
                Converter = s => new SimpleEntity { Id = s.Key, Name = s.Name }
            });

        var entity = await context.SimpleEntities.FindAsync(1);
        entity!.Name.Should().Be("New");
    }

    [Fact]
    public async Task MigrateDataAsync_Core_ShouldDeleteMissingEntities()
    {
        using var context = new MigrateDbContext(CreateOptions<MigrateDbContext>());
        context.SimpleEntities.Add(new SimpleEntity { Id = 1, Name = "A" });
        context.SimpleEntities.Add(new SimpleEntity { Id = 2, Name = "B" });
        await context.SaveChangesAsync();

        var seeds = new[] { new SimpleSeed { Key = 1, Name = "A" } };
        await context.MigrateDataAsync(
            seeds,
            new MigrateDataOptions<SimpleEntity, SimpleSeed, int>
            {
                Query = q => q,
                EntityKeyProperty = e => e.Id,
                DataKeyProperty = s => s.Key,
                IsUpdatedFunc = (e, s) => false,
                UpdateFunc = (e, s) => e,
                Converter = s => new SimpleEntity { Id = s.Key, Name = s.Name }
            });

        context.SimpleEntities.Should().HaveCount(1);
        (await context.SimpleEntities.FindAsync(2)).Should().BeNull();
    }

    [Fact]
    public async Task MigrateDataAsync_Core_HardDeleteMode_ShouldRemoveEntity()
    {
        using var context = new ChangeTrackerDbContext(CreateOptions<ChangeTrackerDbContext>());
        context.SoftMigrates.Add(new SoftMigrateEntity { Id = 1, Name = "A" });
        await context.SaveChangesAsync();

        var seeds = Array.Empty<SoftMigrateSeed>();
        await context.MigrateDataAsync(
            seeds,
            new MigrateDataOptions<SoftMigrateEntity, SoftMigrateSeed, int>
            {
                Query = q => q,
                EntityKeyProperty = e => e.Id,
                DataKeyProperty = s => s.Key,
                IsUpdatedFunc = (e, s) => false,
                UpdateFunc = (e, s) => e,
                Converter = s => new SoftMigrateEntity(),
                HardDeleteMode = true
            });

        context.SoftMigrates.Should().BeEmpty();
    }

    #endregion
}
