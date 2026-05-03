using CSharpEssentials.Entity;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class AuditInterceptorTests
{
    private sealed class AuditEntity : EntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SoftDeleteAuditEntity : SoftDeletableEntityBase<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<AuditEntity> Entities => Set<AuditEntity>();
        public DbSet<SoftDeleteAuditEntity> SoftDeleteEntities => Set<SoftDeleteAuditEntity>();
    }

    private static (TestDbContext Db, ServiceProvider Provider) CreateContext(string userId = "test-user")
    {
        ServiceCollection services = new();
        services.AddAuditInterceptor(() => userId);

        ServiceProvider provider = services.BuildServiceProvider();

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(provider.GetRequiredService<AuditInterceptor>())
            .Options;

        return (new TestDbContext(options), provider);
    }

    [Fact]
    public void SavingChanges_WhenEntityAdded_ShouldSetCreatedInfo()
    {
        (TestDbContext db, ServiceProvider provider) = CreateContext("creator-user");

        AuditEntity entity = new() { Name = "Test" };
        db.Entities.Add(entity);
        db.SaveChanges();

        entity.CreatedBy.Should().Be("creator-user");
        entity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_WhenEntityModified_ShouldSetUpdatedInfo()
    {
        (TestDbContext db, ServiceProvider provider) = CreateContext("modifier-user");

        AuditEntity entity = new() { Name = "Original" };
        db.Entities.Add(entity);
        db.SaveChanges();

        entity.Name = "Modified";
        db.SaveChanges();

        entity.UpdatedBy.Should().Be("modifier-user");
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void SavingChanges_WhenSoftDeletableEntityDeleted_ShouldMarkAsDeleted()
    {
        (TestDbContext db, ServiceProvider provider) = CreateContext("deleter-user");

        SoftDeleteAuditEntity entity = new() { Name = "ToDelete" };
        db.SoftDeleteEntities.Add(entity);
        db.SaveChanges();

        entity.MarkAsDeleted(DateTimeOffset.UtcNow, "deleter-user");
        db.Entry(entity).State = EntityState.Modified;
        db.SaveChanges();

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedBy.Should().Be("deleter-user");
        entity.DeletedAt.Should().NotBeNull();

        db.Dispose();
        provider.Dispose();
    }

    [Fact]
    public async Task SavingChangesAsync_WhenEntityAdded_ShouldSetCreatedInfo()
    {
        (TestDbContext db, ServiceProvider provider) = CreateContext("async-creator");

        AuditEntity entity = new() { Name = "AsyncTest" };
        db.Entities.Add(entity);
        await db.SaveChangesAsync();

        entity.CreatedBy.Should().Be("async-creator");
        entity.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));

        await db.DisposeAsync();
        await provider.DisposeAsync();
    }

    // ── DI Extension Tests ────────────────────────────────────────────

    [Fact]
    public void AddAuditUserIdProvider_WithStringFactory_ShouldResolve()
    {
        ServiceCollection services = new();
        services.AddAuditUserIdProvider(() => "factory-user");
        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope = provider.CreateScope();
        IAuditUserIdProvider userProvider = scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>();

        userProvider.GetCurrentUserId().Should().Be("factory-user");

        provider.Dispose();
    }

    [Fact]
    public void AddAuditUserIdProvider_WithServiceProviderFactory_ShouldResolve()
    {
        ServiceCollection services = new();
        services.AddSingleton("sp-user");
        services.AddAuditUserIdProvider(sp => sp.GetRequiredService<string>());
        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope = provider.CreateScope();
        IAuditUserIdProvider userProvider = scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>();

        userProvider.GetCurrentUserId().Should().Be("sp-user");

        provider.Dispose();
    }

    [Fact]
    public void AddAuditUserIdProvider_GenericWithGuid_ShouldResolveAsString()
    {
        Guid userId = Guid.NewGuid();
        ServiceCollection services = new();
        services.AddAuditUserIdProvider(() => userId);
        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope = provider.CreateScope();
        IAuditUserIdProvider userProvider = scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>();

        userProvider.GetCurrentUserId().Should().Be(userId.ToString());

        provider.Dispose();
    }

    [Fact]
    public void AddAuditInterceptor_ShouldRegisterAllRequiredServices()
    {
        ServiceCollection services = new();
        services.AddAuditInterceptor(() => "full-user");
        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<IAuditUserIdProvider>().Should().NotBeNull();
        provider.GetService<TimeProvider>().Should().NotBeNull();
        provider.GetService<AuditInterceptor>().Should().NotBeNull();

        provider.Dispose();
    }
}
