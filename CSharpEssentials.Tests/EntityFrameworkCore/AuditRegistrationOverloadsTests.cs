using CSharpEssentials.Entity;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class AuditRegistrationOverloadsTests
{
    private sealed class AuditedEntity : EntityBase<Guid>
    {
        public AuditedEntity() => Id = Guid.NewGuid();
    }

    private sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
    {
        public DbSet<AuditedEntity> Entities => Set<AuditedEntity>();
    }

    private sealed record CurrentUser(string Name, Guid Id);

    private static string CreatedByAfterSave(ServiceProvider provider)
    {
        using AuditDbContext db = new(new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(provider.GetRequiredService<AuditInterceptor>())
            .Options);
        AuditedEntity entity = new();
        db.Entities.Add(entity);
        db.SaveChanges();
        return entity.CreatedBy;
    }

    private static string ResolveUserId(ServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>().GetCurrentUserId();
    }

    [Fact]
    public void AddAuditUserIdProvider_GenericWithServiceProviderFactory_Should_ResolveUserIdFromServices()
    {
        Guid userId = Guid.NewGuid();
        ServiceCollection services = new();
        services.AddSingleton(new CurrentUser("ignored", userId));
        services.AddAuditUserIdProvider(sp => sp.GetRequiredService<CurrentUser>().Id);
        using ServiceProvider provider = services.BuildServiceProvider();

        ResolveUserId(provider).Should().Be(userId.ToString());
    }

    [Fact]
    public void AddAuditUserIdProvider_Should_EvaluateFactoryOncePerScope()
    {
        int calls = 0;
        ServiceCollection services = new();
        services.AddAuditUserIdProvider(() => $"user-{++calls}");
        using ServiceProvider provider = services.BuildServiceProvider();

        using (IServiceScope scope = provider.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>().GetCurrentUserId().Should().Be("user-1");
            scope.ServiceProvider.GetRequiredService<IAuditUserIdProvider>().GetCurrentUserId().Should().Be("user-1");
        }

        ResolveUserId(provider).Should().Be("user-2");
    }

    [Fact]
    public void AddAuditInterceptor_WithServiceProviderFactory_Should_StampCreatedBy()
    {
        ServiceCollection services = new();
        services.AddSingleton(new CurrentUser("sp-user", Guid.NewGuid()));
        services.AddAuditInterceptor(sp => sp.GetRequiredService<CurrentUser>().Name);
        using ServiceProvider provider = services.BuildServiceProvider();

        CreatedByAfterSave(provider).Should().Be("sp-user");
    }

    [Fact]
    public void AddAuditInterceptor_GenericWithServiceProviderFactory_Should_StampCreatedByAsString()
    {
        Guid userId = Guid.NewGuid();
        ServiceCollection services = new();
        services.AddSingleton(new CurrentUser("ignored", userId));
        services.AddAuditInterceptor(sp => sp.GetRequiredService<CurrentUser>().Id);
        using ServiceProvider provider = services.BuildServiceProvider();

        CreatedByAfterSave(provider).Should().Be(userId.ToString());
    }

    [Fact]
    public void AddAuditInterceptor_GenericWithValueFactory_Should_StampCreatedByAsString()
    {
        ServiceCollection services = new();
        services.AddAuditInterceptor(() => 42);
        using ServiceProvider provider = services.BuildServiceProvider();

        CreatedByAfterSave(provider).Should().Be("42");
        provider.GetService<TimeProvider>().Should().BeSameAs(TimeProvider.System);
    }
}
