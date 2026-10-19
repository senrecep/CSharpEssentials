using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Idempotency;

/// <summary>Argument checks that compile on every target, including <c>netstandard2.1</c>.</summary>
internal static class Guard
{
    public static void NotNull([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value, parameterName);
#else
        if (value is null)
            throw new ArgumentNullException(parameterName);
#endif
    }

    public static void NotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NET8_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
#else
        if (value is null)
            throw new ArgumentNullException(parameterName);
        if (value.Length == 0)
            throw new ArgumentException("The value cannot be an empty string.", parameterName);
#endif
    }

    public static void Positive(TimeSpan value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero, parameterName);
#else
        if (value <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be positive.");
#endif
    }
}
