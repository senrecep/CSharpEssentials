using Microsoft.CodeAnalysis;

namespace CSharpEssentials.EntityFrameworkCore.Generators;

internal static class QueryFilterDiagnostics
{
    public const string Category = "CSharpEssentials.EntityFrameworkCore";

    private const string HelpLink = "https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.EntityFrameworkCore/Readme.MD#named-query-filters-ef-core-10";

    // Info by default: apps that still use the anonymous ApplySoftDeleteQueryFilter() have no named filter to pass, and a warning would
    // break their TreatWarningsAsErrors builds in a minor release. Raise it with dotnet_diagnostic.CSE3001.severity = warning.
    public static readonly DiagnosticDescriptor UnnamedIgnoreQueryFilters = new(
        "CSE3001",
        "IgnoreQueryFilters() ignores every query filter",
        "'IgnoreQueryFilters()' turns off every query filter of '{0}', including filters such as the tenant filter; pass the names of the filters to ignore in a cached array, for example 'IgnoreQueryFilters(SoftDeleteOnly)' with 'static readonly string[] SoftDeleteOnly = [QueryFilterNames.SoftDelete]'",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "EF Core 10 named query filters can be ignored one by one with IgnoreQueryFilters(filterKeys). The parameterless overload drops all of them, so a query that only meant to include soft-deleted rows can also leak rows of other tenants. Pass the keys as a static readonly array or 'new[] { ... }': EF Core 10.0.x compiles the query again on every execution when the keys come from a collection expression or a List. Named keys only ignore named filters; an anonymous filter such as ApplySoftDeleteQueryFilter() still applies.",
        helpLinkUri: HelpLink);
}
