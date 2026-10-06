using System.Runtime.CompilerServices;

namespace CSharpEssentials.DependencyInjection;

internal static class Guard
{
    public static void NotNull(object? value, [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value, parameterName);
#else
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
#endif
    }
}
