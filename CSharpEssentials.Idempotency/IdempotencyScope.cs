namespace CSharpEssentials.Idempotency;

/// <summary>
/// The owner of an idempotency key: a user, tenant or client id, or <see cref="Global"/> when every caller shares the
/// key space. An <see cref="IIdempotencyScopeResolver"/> returns <see langword="null"/> instead of a scope when it cannot
/// tell who the caller is, so "unresolved" and "explicitly global" stay two different answers.
/// <para><see langword="default"/> is <see cref="Global"/>.</para>
/// </summary>
public readonly record struct IdempotencyScope
{
    private IdempotencyScope(string? value) => Value = value;

    /// <summary>The scope shared by every caller. Choose it explicitly; it is never the result of a failed lookup.</summary>
    public static IdempotencyScope Global => default;

    /// <summary>The owner id; <see langword="null"/> for <see cref="Global"/>.</summary>
    public string? Value { get; }

    /// <summary><see langword="true"/> for <see cref="Global"/>.</summary>
    public bool IsGlobal => Value is null;

    /// <summary>A scope owned by <paramref name="value"/>, for example a user id.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is <see langword="null"/> or empty.</exception>
    public static IdempotencyScope Of(string value)
    {
        Guard.NotNullOrEmpty(value);
        return new(value);
    }

    public override string ToString() => Value ?? "(global)";
}
