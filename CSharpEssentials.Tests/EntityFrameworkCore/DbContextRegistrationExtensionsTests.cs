using CSharpEssentials.EntityFrameworkCore.Extensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class DbContextRegistrationExtensionsTests
{
    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options);
    private sealed class WriteDbContext(DbContextOptions<WriteDbContext> options) : DbContext(options);
    private sealed class ReadDbContext(DbContextOptions<ReadDbContext> options) : DbContext(options);

    private static Action<IServiceProvider, DbContextOptionsBuilder> InMemoryProvider(string? dbName = null) =>
        (_, b) => b.UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString());

    [Fact]
    public void AddPooledDbContext_Should_RegisterDbContext_When_ConfigureCallbackProvided()
    {
        var services = new ServiceCollection();

        services.AddPooledDbContext<TestDbContext>(InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetService<TestDbContext>();

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void AddPooledDbContext_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddPooledDbContext<TestDbContext>(InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPooledDbContext_Should_ThrowArgumentNullException_When_ConfigureIsNull()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddPooledDbContext<TestDbContext>(
            (Action<IServiceProvider, DbContextOptionsBuilder>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPooledDbContext_WithOptions_Should_RegisterDbContext_When_OptionsConfigured()
    {
        var services = new ServiceCollection();

        services.AddPooledDbContext<TestDbContext>(
            opts => { },
            InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetService<TestDbContext>();

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void AddPooledDbContext_WithOptions_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddPooledDbContext<TestDbContext>(
            (Action<DbContextRegistrationOptions>)(_ => { }),
            InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPooledDbContext_WithOptions_Should_ThrowArgumentNullException_When_ConfigureOptionsIsNull()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddPooledDbContext<TestDbContext>(
            (Action<DbContextRegistrationOptions>)null!,
            InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterDbContextFactory_Should_RegisterFactory_When_ConfigureCallbackProvided()
    {
        var services = new ServiceCollection();

        services.RegisterDbContextFactory<TestDbContext>(InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var factory = provider.GetService<IDbContextFactory<TestDbContext>>();

        factory.Should().NotBeNull();
    }

    [Fact]
    public void RegisterDbContextFactory_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.RegisterDbContextFactory<TestDbContext>(InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterDbContextFactory_Should_ThrowArgumentNullException_When_ConfigureIsNull()
    {
        var services = new ServiceCollection();

        Action act = () => services.RegisterDbContextFactory<TestDbContext>(
            (Action<IServiceProvider, DbContextOptionsBuilder>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterDbContextFactory_WithOptions_Should_RegisterFactory_When_OptionsConfigured()
    {
        var services = new ServiceCollection();

        services.RegisterDbContextFactory<TestDbContext>(
            opts => { },
            InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var factory = provider.GetService<IDbContextFactory<TestDbContext>>();

        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddWriteDbContext_Should_RegisterWriteContext_When_ConfigureCallbackProvided()
    {
        var services = new ServiceCollection();

        services.AddWriteDbContext<WriteDbContext>(InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetService<WriteDbContext>();

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void AddWriteDbContext_Should_UseTrackAllBehavior_When_Resolved()
    {
        var services = new ServiceCollection();

        services.AddWriteDbContext<WriteDbContext>(InMemoryProvider("write-track"));

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetRequiredService<WriteDbContext>();

        ctx.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.TrackAll);
    }

    [Fact]
    public void AddWriteDbContext_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddWriteDbContext<WriteDbContext>(InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddWriteDbContext_WithOptions_Should_RegisterWriteContext_When_OptionsConfigured()
    {
        var services = new ServiceCollection();

        services.AddWriteDbContext<WriteDbContext>(
            opts => { },
            InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetService<WriteDbContext>();

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void AddReadDbContext_Should_RegisterReadContext_When_ConfigureCallbackProvided()
    {
        var services = new ServiceCollection();

        services.AddReadDbContext<ReadDbContext>(InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetService<ReadDbContext>();

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void AddReadDbContext_Should_UseNoTrackingBehavior_When_Resolved()
    {
        var services = new ServiceCollection();

        services.AddReadDbContext<ReadDbContext>(InMemoryProvider("read-notrack"));

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetRequiredService<ReadDbContext>();

        ctx.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTracking);
    }

    [Fact]
    public void AddReadDbContext_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddReadDbContext<ReadDbContext>(InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddReadDbContext_WithOptions_Should_RegisterReadContext_When_OptionsConfigured()
    {
        var services = new ServiceCollection();

        services.AddReadDbContext<ReadDbContext>(
            opts => { },
            InMemoryProvider());

        var provider = services.BuildServiceProvider();
        var ctx = provider.GetService<ReadDbContext>();

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void AddCqrsDbContexts_Should_RegisterBothContexts_When_ConfigureCallbackProvided()
    {
        var services = new ServiceCollection();

        services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(InMemoryProvider());

        var provider = services.BuildServiceProvider();

        provider.GetService<WriteDbContext>().Should().NotBeNull();
        provider.GetService<ReadDbContext>().Should().NotBeNull();
    }

    [Fact]
    public void AddCqrsDbContexts_Should_WriteContextUseTrackAll_And_ReadContextUseNoTracking_When_Resolved()
    {
        var services = new ServiceCollection();

        services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
            (_, b) => b.UseInMemoryDatabase("cqrs-shared"));

        var provider = services.BuildServiceProvider();

        var writeCtx = provider.GetRequiredService<WriteDbContext>();
        var readCtx = provider.GetRequiredService<ReadDbContext>();

        writeCtx.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.TrackAll);
        readCtx.ChangeTracker.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTracking);
    }

    [Fact]
    public void AddCqrsDbContexts_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(InMemoryProvider());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCqrsDbContexts_WithOptions_Should_RegisterBothContexts_When_OptionsConfigured()
    {
        var services = new ServiceCollection();

        services.AddCqrsDbContexts<WriteDbContext, ReadDbContext>(
            opts => { },
            InMemoryProvider());

        var provider = services.BuildServiceProvider();

        provider.GetService<WriteDbContext>().Should().NotBeNull();
        provider.GetService<ReadDbContext>().Should().NotBeNull();
    }
}
