using System.Globalization;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>
/// The parity checks every row passes before it is measured: a valid, stable document for the same 50 endpoints, and
/// exactly the additions its conventions promise.
/// </summary>
internal static class ApiDocsChecks
{
    public const string CseEnum = "string|alpha,beta,gamma,delta,epsilon,zeta|Alpha,Beta,Gamma,Delta,Epsilon,Zeta|0,1,2,3,4,5";
    public const string CseKindDefault = "\"beta\"";

    public static readonly IReadOnlyList<string> FullForms = Sorted(Resources().SelectMany(static k => new[]
    {
        $"GET /resources{k}",
        $"GET /resources{k}/{{id}}",
        $"GET /resources{k}/archive/{{year}}/{{month}}",
        $"POST /resources{k}",
        $"DELETE /resources{k}/{{id}}",
    }));

    public static readonly IReadOnlyList<string> SplitForms = Sorted(FullForms.Concat(Resources().SelectMany(static k => new[]
    {
        $"GET /resources{k}/archive",
        $"GET /resources{k}/archive/{{year}}",
    })));

    public static readonly IReadOnlyList<string> IdempotentPosts = Sorted(Resources().Select(static k => $"POST /resources{k}"));

    /// <summary>Reads document 1 and document 5 of a host, checks they are equal unless told otherwise, and returns document 1.</summary>
    public static DocumentFacts Read(string row, Func<byte[]> probe, bool deterministic = true)
    {
        var first = DocumentFacts.Read(probe());
        for (int request = 2; request < 5; request++)
            probe();
        var fifth = DocumentFacts.Read(probe());
        if (deterministic)
            Parity.Same(row + " determinism", ("document 1", first.Hash), ("document 5", fifth.Hash));
        return first;
    }

    public static void Verify(string row, DocumentFacts facts, string versionPrefix, DocumentConventions conventions, PlainShapes? plain)
    {
        Parity.True(row, facts.Length > 10_000, "document has only " + facts.Length.ToString(CultureInfo.InvariantCulture) + " bytes");
        Parity.True(row, facts.Version.StartsWith(versionPrefix, StringComparison.Ordinal), "openapi is " + facts.Version);

        bool split = conventions.HasFlag(DocumentConventions.OptionalRoutes);
        Parity.Same(row + " operations", ("expected", Join(split ? SplitForms : FullForms)), (row, Join(facts.Operations)));
        if (split)
            Parity.True(row, facts.PathParametersRequired, "a path parameter is not required");
        Parity.Same(row + " resource schemas", ("expected", "10"), (row, facts.ResourceSchemas.ToString(CultureInfo.InvariantCulture)));

        if (conventions.HasFlag(DocumentConventions.Enums))
        {
            Parity.Same(row + " enum components", ("expected", Join(Enumerable.Range(1, SampleApi.EnumCount).Select(static n => "Enum" + n.ToString("00", CultureInfo.InvariantCulture)))), (row, Join(facts.EnumComponents.Keys)));
            foreach ((string name, string summary) in facts.EnumComponents)
                Parity.Same(row + " " + name, ("expected", CseEnum), (row, summary));
            foreach (string value in facts.KindDefaults)
                Parity.Same(row + " Kind default", ("expected", CseKindDefault), (row, value));
        }
        else
        {
            Parity.Same(row + " x-enum-varnames", ("expected", "0"), (row, facts.EnumVarnameMarkers.ToString(CultureInfo.InvariantCulture)));
            Parity.Same(row + " Enum01", ("expected", plain?.Enum01 ?? "<no shape given>"), (row, facts.Enum01Shape));
            foreach (string value in facts.KindDefaults)
                Parity.Same(row + " Kind default", ("expected", plain?.KindDefault ?? "<no shape given>"), (row, value));
            Parity.Same(row + " Stage", ("expected", plain?.Stage ?? "<no shape given>"), (row, facts.StageShape));
        }

        if (conventions.HasFlag(DocumentConventions.Idempotency))
        {
            Parity.Same(row + " idempotent operations", ("expected", Join(IdempotentPosts)), (row, Join(facts.IdempotentOperations)));
            Parity.Same(row + " operations with idempotency parts", ("expected", Join(IdempotentPosts)), (row, Join(facts.PartlyIdempotentOperations)));
        }
        else
        {
            Parity.Same(row + " Idempotency-Key", ("expected", "0"), (row, facts.IdempotencyKeyMarkers.ToString(CultureInfo.InvariantCulture)));
            Parity.Same(row + " operations with idempotency parts", ("expected", string.Empty), (row, Join(facts.PartlyIdempotentOperations)));
        }
    }

    private static IEnumerable<string> Resources() =>
        Enumerable.Range(1, SampleApi.ResourceCount).Select(static k => k.ToString("00", CultureInfo.InvariantCulture));

    private static string[] Sorted(IEnumerable<string> values) => [.. values.Order(StringComparer.Ordinal)];

    private static string Join(IEnumerable<string> values) => string.Join(", ", values);
}
