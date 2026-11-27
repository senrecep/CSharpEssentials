namespace CSharpEssentials.Time;

public interface IDateTimeProvider
{
    TimeZoneInfo TimeZone { get; }
    TimeZoneInfo TimeZoneUtc { get; }

    DateTime UtcNowDateTime { get; }
    DateTimeOffset UtcNow { get; }

#if NET6_0_OR_GREATER
    /// <summary>
    /// The UTC date of <see cref="UtcNowDateTime"/>. The default body keeps an implementation compiled against the
    /// netstandard2.1 asset, which has no such member, loadable on .NET 9 and later.
    /// </summary>
    DateOnly UtcNowDate => DateOnly.FromDateTime(UtcNowDateTime);

    /// <summary>
    /// The UTC time of day of <see cref="UtcNowDateTime"/>. The default body keeps an implementation compiled against the
    /// netstandard2.1 asset, which has no such member, loadable on .NET 9 and later.
    /// </summary>
    TimeOnly UtcNowTime => TimeOnly.FromDateTime(UtcNowDateTime);
#endif
}
