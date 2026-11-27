using CSharpEssentials.EntityFrameworkCore.Locking;
using CSharpEssentials.Locking;
using CSharpEssentials.Locking.Testing;
using CSharpEssentials.Mediator;
using FluentAssertions;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

public sealed class RelationalResourceLockCancellationTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"cse-lock-{Guid.NewGuid():N}.db");
    private readonly CancelAfterAcquireInterceptor _interceptor = new();
    private readonly ServiceProvider _provider;

    public RelationalResourceLockCancellationTests()
    {
        ServiceCollection services = new();
        services.AddDbContext<LockDbContext>(options => options
            .UseSqlite($"Data Source={_path};Pooling=False")
            .AddInterceptors(_interceptor));
        services.AddRelationalResourceLock<LockDbContext>(RelationalResourceLockSqliteContractTests.SqliteTableDialect);
        _provider = services.BuildServiceProvider();
        using IServiceScope scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<LockDbContext>().Database.EnsureCreated();
    }

    public void Dispose()
    {
        _provider.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }

    private sealed class CancelAfterAcquireInterceptor : DbCommandInterceptor
    {
        public bool Armed { get; set; }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (!Armed)
                return result;

            Armed = false;
            await result.DisposeAsync();
            throw new OperationCanceledException();
        }
    }

    [Fact]
    public async Task TryAcquireAsync_Should_ReleaseTheLock_When_CancelledAfterTheAcquireStatementRan()
    {
        IResourceLock resourceLock = _provider.GetRequiredService<IResourceLock>();
        _interceptor.Armed = true;
        Func<Task> cancelled = async () => await resourceLock.TryAcquireAsync("order:1");
        await cancelled.Should().ThrowAsync<OperationCanceledException>();

        CSharpEssentials.Maybe.Maybe<IAsyncDisposable> next = await resourceLock.TryAcquireAsync("order:1");

        next.HasValue.Should().BeTrue();
        await next.Value.DisposeAsync();
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
