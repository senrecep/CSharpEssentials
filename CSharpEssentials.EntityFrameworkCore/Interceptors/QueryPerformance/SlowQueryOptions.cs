namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Configuration options for <see cref="SlowQueryInterceptor"/>.
/// </summary>
public sealed class SlowQueryOptions
{
    /// <summary>
    /// Queries that exceed this duration are logged as warnings.
    /// Defaults to <b>1 second</b>.
    /// </summary>
    public TimeSpan Threshold { get; set; } = TimeSpan.FromSeconds(1);
}
