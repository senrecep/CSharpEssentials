namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Contextual data about a slow query, passed to <see cref="ISlowQueryHandler"/>.
/// </summary>
public sealed class SlowQueryContext
{
    public required string CommandText { get; init; }
    public required string Parameters { get; init; }
    public required TimeSpan ElapsedTime { get; init; }
    public string? Database { get; init; }
    public bool HasTransaction { get; init; }
    public string MethodName { get; init; } = string.Empty;
}
