using CSharpEssentials.EntityFrameworkCore.Locking;
using CSharpEssentials.Locking;
using CSharpEssentials.Locking.Testing;
using CSharpEssentials.Mediator;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Locking;

public sealed class RelationalResourceLockTests
{
    [Fact]
    public void PostgreSql_Should_UseSessionAdvisoryLocksOnAStableHash()
    {
        ResourceLockSqlDialect dialect = ResourceLockSqlDialect.PostgreSql;

        dialect.TryAcquireSql.Should().Be("SELECT CASE WHEN pg_try_advisory_lock(hashtextextended({0}, 0)) THEN 1 ELSE 0 END AS \"Value\"");
        dialect.ReleaseSql.Should().Be("SELECT pg_advisory_unlock(hashtextextended({0}, 0))");
    }

    [Fact]
    public void SqlServer_Should_UseSessionAppLocksWithoutWaiting()
    {
        ResourceLockSqlDialect dialect = ResourceLockSqlDialect.SqlServer;

        dialect.TryAcquireSql.Should().Contain("sp_getapplock @Resource = {0}").And.Contain("@LockOwner = 'Session'")
            .And.Contain("@LockTimeout = 0").And.EndWith("AS [Value];");
        dialect.ReleaseSql.Should().Be("EXEC sp_releaseapplock @Resource = {0}, @LockOwner = 'Session';");
    }

    [Fact]
    public void Dialect_Should_Throw_When_StatementIsEmpty()
    {
        Action act = () => _ = new ResourceLockSqlDialect(" ", "x");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRelationalResourceLock_Should_ReplaceTheInProcessLock()
    {
        ServiceCollection services = new();
        services.AddDbContext<LockDbContext>(options => options.UseSqlite("DataSource=:memory:"));
        services.AddMediatorLockBehavior();

        services.AddRelationalResourceLock<LockDbContext>(ResourceLockSqlDialect.PostgreSql);

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IResourceLock>().Should().BeOfType<RelationalResourceLock<LockDbContext>>();
        services.Count(d => d.ServiceType == typeof(IResourceLock)).Should().Be(1);
    }

    [Fact]
    public void AddRelationalResourceLock_Should_Throw_When_PollIntervalIsNotPositive()
    {
        Action act = () => new ServiceCollection().AddRelationalResourceLock<LockDbContext>(
            ResourceLockSqlDialect.PostgreSql, options => options.PollInterval = TimeSpan.Zero);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_Should_Throw_When_DialectIsMissing()
    {
        IServiceScopeFactory scopes = new ServiceCollection().BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();

        Action act = () => _ = new RelationalResourceLock<LockDbContext>(scopes, new RelationalResourceLockOptions());

        act.Should().Throw<ArgumentException>();
    }
}

public sealed class RecordingResourceLockTests
{
    [Fact]
    public async Task Should_RecordAcquisitionsReleasesAndHeldKeys()
    {
        RecordingResourceLock resourceLock = new();

        IAsyncDisposable handle = await resourceLock.AcquireAsync("order:1", TimeSpan.FromSeconds(5));
        CSharpEssentials.Maybe.Maybe<IAsyncDisposable> refused = await resourceLock.TryAcquireAsync("order:1");
        string[] heldWhileOpen = [.. resourceLock.HeldKeys];
        await handle.DisposeAsync();
        await handle.DisposeAsync();

        refused.HasValue.Should().BeFalse();
        heldWhileOpen.Should().Equal("order:1");
        resourceLock.Acquisitions.Should().Equal(
            new LockAcquisition("order:1", TimeSpan.FromSeconds(5), TryOnly: false),
            new LockAcquisition("order:1", null, TryOnly: true));
        resourceLock.Releases.Should().Equal("order:1");
        resourceLock.HeldKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_RecordAndThrow_When_FailWithIsSet()
    {
        RecordingResourceLock resourceLock = new() { FailWith = new TimeoutException("busy") };

        Func<Task> act = async () => await resourceLock.AcquireAsync("k", null);

        await act.Should().ThrowAsync<TimeoutException>().WithMessage("busy");
        resourceLock.Acquisitions.Should().ContainSingle();
        resourceLock.HeldKeys.Should().BeEmpty();
    }
}
