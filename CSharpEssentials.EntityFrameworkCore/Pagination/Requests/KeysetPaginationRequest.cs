namespace CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

/// <inheritdoc cref="IKeysetPaginationRequest"/>
public sealed record KeysetPaginationRequest : IKeysetPaginationRequest
{
    /// <summary>Default <see cref="Limit"/>: 10.</summary>
    public const int DefaultLimit = 10;

    /// <inheritdoc/>
    public int Limit { get; init; } = DefaultLimit;

    /// <inheritdoc/>
    public string? After { get; init; }

    /// <inheritdoc/>
    public string? Before { get; init; }
}
