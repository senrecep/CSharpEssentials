namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Lets the self-check ask, from inside a <see cref="ReserveStock"/> handler, whether the lock for its key is held.
/// <see cref="Check"/> is <see langword="null"/> during measurement, so every column pays the same null check.
/// </summary>
public static class LockProbe
{
    /// <summary>Returns <see langword="true"/> when the lock for the key is held; set by the self-check only.</summary>
    public static Func<string, bool>? Check { get; set; }

    /// <summary>The last answer of <see cref="Check"/>, or <see langword="null"/> when it did not run.</summary>
    public static bool? Held { get; set; }
}
