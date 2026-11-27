using CSharpEssentials.EntityFrameworkCore.Locking;
using CSharpEssentials.Locking;
using CSharpEssentials.Locking.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Locking;

public abstract class ResourceLockContractSuite
{
    public static TheoryData<string> Scenarios => new(ResourceLockContract.ScenarioNames);

    protected abstract IResourceLock CreateLock();

    [Theory]
    [MemberData(nameof(Scenarios))]
    public Task Scenario(string name) => new ResourceLockContract(CreateLock()).RunAsync(name);
}

public sealed class InProcessResourceLockContractTests : ResourceLockContractSuite
{
    protected override IResourceLock CreateLock() => new InProcessResourceLock();
}

public sealed class RecordingResourceLockContractTests : ResourceLockContractSuite
{
    protected override IResourceLock CreateLock() => new RecordingResourceLock();
}

internal sealed class LockRow
{
    public string Key { get; set; } = string.Empty;
}

internal sealed class LockDbContext(DbContextOptions<LockDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LockRow>(entity =>
        {
            entity.ToTable("locks");
            entity.HasKey(row => row.Key);
        });
}

public sealed class RelationalResourceLockSqliteContractTests : ResourceLockContractSuite, IDisposable
{
    internal static readonly ResourceLockSqlDialect SqliteTableDialect = new(
        "INSERT INTO \"locks\" (\"Key\") VALUES ({0}) ON CONFLICT DO NOTHING RETURNING 1 AS \"Value\"",
        "DELETE FROM \"locks\" WHERE \"Key\" = {0}");

    private readonly string _path = Path.Combine(Path.GetTempPath(), $"cse-lock-{Guid.NewGuid():N}.db");
    private readonly ServiceProvider _provider;

    public RelationalResourceLockSqliteContractTests()
    {
        ServiceCollection services = new();
        services.AddDbContext<LockDbContext>(options => options.UseSqlite($"Data Source={_path};Pooling=False"));
        services.AddRelationalResourceLock<LockDbContext>(SqliteTableDialect, options => options.PollInterval = TimeSpan.FromMilliseconds(20));
        _provider = services.BuildServiceProvider();
        using IServiceScope scope = _provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<LockDbContext>().Database.EnsureCreated();
    }

    protected override IResourceLock CreateLock() => _provider.GetRequiredService<IResourceLock>();

    public void Dispose()
    {
        _provider.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_path);
    }
}
