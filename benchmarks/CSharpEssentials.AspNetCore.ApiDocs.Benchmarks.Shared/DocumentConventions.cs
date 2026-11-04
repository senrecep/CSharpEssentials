namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>What a row adds to the plain document, so the self-check knows what to expect.</summary>
[Flags]
internal enum DocumentConventions
{
    None = 0,
    Enums = 1,
    Idempotency = 2,
    OptionalRoutes = 4,
    All = Enums | Idempotency | OptionalRoutes,
}
