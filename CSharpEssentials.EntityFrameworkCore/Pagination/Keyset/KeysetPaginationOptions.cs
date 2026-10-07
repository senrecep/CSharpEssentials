namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>Options of keyset pagination. Instances are immutable and can be shared, for example as a DI singleton.</summary>
public sealed record KeysetPaginationOptions
{
    /// <summary>Default <see cref="MaxLimit"/>: 100.</summary>
    public const int DefaultMaxLimit = 100;

    /// <summary>Default <see cref="MaxCursorLength"/>: 2048 characters.</summary>
    public const int DefaultMaxCursorLength = 2048;

    /// <summary>Options with every default: <see cref="NoOpCursorProtector"/>, <see cref="KeysetPredicateBuilder"/> and the default limits.</summary>
    public static KeysetPaginationOptions Default { get; } = new();

    /// <summary>Upper bound of the page size; a larger requested limit is lowered to it. Must be between 1 and
    /// <c>int.MaxValue - 1</c>, because <c>limit + 1</c> rows are read. Defaults to <see cref="DefaultMaxLimit"/>.</summary>
    public int MaxLimit
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, int.MaxValue - 1);
            field = value;
        }
    } = DefaultMaxLimit;

    /// <summary>
    /// Longest accepted cursor, in characters; a longer <c>After</c> or <c>Before</c> is rejected with an invalid-cursor
    /// error before it is decoded. Defaults to <see cref="DefaultMaxCursorLength"/>. Raise it for long string keys or a
    /// protector that adds a lot of overhead.
    /// </summary>
    public int MaxCursorLength
    {
        get;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = DefaultMaxCursorLength;

    /// <summary>Protects the cursors. Defaults to <see cref="NoOpCursorProtector"/> (base64url only, no tamper protection).</summary>
    public ICursorProtector Protector { get; init; } = NoOpCursorProtector.Instance;

    /// <summary>Builds the keyset predicate. Defaults to <see cref="KeysetPredicateBuilder"/>.</summary>
    public IKeysetPredicateBuilder PredicateBuilder { get; init; } = KeysetPredicateBuilder.Instance;
}
