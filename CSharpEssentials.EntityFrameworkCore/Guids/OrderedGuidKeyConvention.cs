using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace CSharpEssentials.EntityFrameworkCore;

internal sealed class OrderedGuidKeyConvention(OrderedGuidLayout? layout, string? providerName) : IModelFinalizingConvention
{
    internal const string SqlServerProvider = "Microsoft.EntityFrameworkCore.SqlServer";

    private static readonly string[] _version7Providers =
    [
        EnumStorageConvention.PostgresProvider,
        EnumStorageConvention.SqliteProvider,
    ];

    internal static OrderedGuidLayout? Resolve(OrderedGuidLayout? layout, string? providerName) =>
        layout ?? providerName switch
        {
            SqlServerProvider => OrderedGuidLayout.SqlServer,
            _ when Array.IndexOf(_version7Providers, providerName) >= 0 => OrderedGuidLayout.Version7,
            _ => null,
        };

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        Type? factory = Resolve(layout, providerName) switch
        {
            OrderedGuidLayout.Version7 => typeof(Version7GuidValueGeneratorFactory),
            OrderedGuidLayout.SqlServer => typeof(SqlServerGuidValueGeneratorFactory),
            _ => null,
        };
        if (factory is null)
            return;

        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
            foreach (IConventionProperty property in entityType.GetDeclaredProperties())
                if (IsGeneratedGuidKey(property))
                    _ = property.Builder.HasValueGeneratorFactory(factory);
    }

    private static bool IsGeneratedGuidKey(IConventionProperty property) =>
        property.ClrType == typeof(Guid)
        && property.IsPrimaryKey()
        && !property.IsForeignKey()
        && property.ValueGenerated == ValueGenerated.OnAdd
        && property.GetValueGeneratorFactoryConfigurationSource() is null
        && property.GetDefaultValueSql() is null
        && property.GetComputedColumnSql() is null
        && property.GetDefaultValueConfigurationSource() is null
        && property.GetValueConverter() is null
        && property.GetProviderClrType() is null;
}
