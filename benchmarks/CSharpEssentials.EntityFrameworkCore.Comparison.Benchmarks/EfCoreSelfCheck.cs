using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharpEssentials.EntityFrameworkCore.Comparison.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class EfCoreSelfCheck
{
    public static void VerifyAll()
    {
        Console.WriteLine($"EF Core {VersionOf(typeof(DbContext))}, Sqlite provider {VersionOf(typeof(SqliteDbContextOptionsBuilderExtensions))}{Competitors()} on {AppContext.TargetFrameworkName} ({RuntimeInformation.FrameworkDescription})");

        foreach (int count in new[] { 1, 100, 1000 })
        {
            foreach (EntityState state in new[] { EntityState.Added, EntityState.Modified })
            {
                var audit = new SaveChangesAuditComparison { Count = count, State = state };
                audit.Setup();
                audit.Cleanup();
            }

            foreach (int events in new[] { 0, 1 })
            {
                var domainEvents = new SaveChangesDomainEventsComparison { Count = count, EventsPerEntity = events };
                domainEvents.Setup();
                domainEvents.Cleanup();
            }
        }

        foreach (string page in new[] { "First", "After" })
        {
            var keyset = new KeysetPaginationComparison { Page = page };
            keyset.Setup();
            keyset.Cleanup();
        }

        foreach (int pageNumber in new[] { 1, 250 })
        {
            var offset = new OffsetPaginationComparison { PageNumber = pageNumber };
            offset.Setup();
            offset.Cleanup();
        }

        var enums = new EnumConverterComparison();
        enums.Setup();
        Console.WriteLine($"Enum texts: {enums.Texts}");

        foreach (int count in new[] { 1_000, 10_000 })
        {
            var migrate = new MigrateDataComparison { Count = count };
            migrate.Setup();
            migrate.Cleanup();
        }

        var lifetime = new DbContextLifetimeComparison();
        lifetime.Setup();
        lifetime.Cleanup();

        var translation = new DbErrorTranslationComparison();
        translation.Setup();
        translation.Cleanup();

        new DbErrorClassificationComparison().Setup();

        foreach (int events in new[] { 0, 1, 10 })
            new DomainEventsReadComparison { Events = events }.Setup();
    }

    private static string Competitors() =>
#if NET10_0_OR_GREATER
        $", X.PagedList.EF {VersionOf(typeof(X.PagedList.EF.PagedListExtensions))}, MR.EntityFrameworkCore.KeysetPagination {VersionOf(typeof(MR.EntityFrameworkCore.KeysetPagination.KeysetQuery))}, EntityFrameworkCore.Exceptions.Sqlite {VersionOf(typeof(EntityFramework.Exceptions.Sqlite.ExceptionProcessorExtensions))}";
#elif NET9_0_OR_GREATER
        $", X.PagedList.EF {VersionOf(typeof(X.PagedList.EF.PagedListExtensions))}";
#else
        string.Empty;
#endif

    private static string VersionOf(Type type) =>
        type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "?";
}
