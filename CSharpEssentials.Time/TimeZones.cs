using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Time;

/// <summary>
/// Looks up a <see cref="TimeZoneInfo"/> by an IANA id (<c>Europe/Istanbul</c>) or a Windows id (<c>Turkey Standard Time</c>) on
/// any operating system.
/// </summary>
/// <remarks>
/// On .NET 8 and later an id the system does not know directly is converted with
/// <c>TimeZoneInfo.TryConvertIanaIdToWindowsId</c> or
/// <c>TimeZoneInfo.TryConvertWindowsIdToIanaId</c> and looked up again. The conversion needs ICU, so
/// an app with <c>InvariantGlobalization</c> on finds only the ids of its own system. The netstandard2.1 build relies on
/// <see cref="TimeZoneInfo.FindSystemTimeZoneById(string)"/>, which converts on .NET 6 and later.
/// </remarks>
public static class TimeZones
{
    /// <summary>Finds the time zone with the IANA or Windows id <paramref name="id"/>.</summary>
    /// <param name="id">The id, for example <c>Europe/Istanbul</c>, <c>Turkey Standard Time</c> or <c>UTC</c>.</param>
    /// <returns>The time zone.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="TimeZoneNotFoundException">No time zone on this system has the id, as IANA or as Windows id.</exception>
    public static TimeZoneInfo Find(string id)
    {
        _ = id ?? throw new ArgumentNullException(nameof(id));
        return TryFind(id, out TimeZoneInfo? zone)
            ? zone
            : throw new TimeZoneNotFoundException($"No time zone with the IANA or Windows id '{id}' was found on this system.");
    }

    /// <summary>Finds the time zone with the IANA or Windows id <paramref name="id"/>, without throwing.</summary>
    /// <param name="id">The id; <see langword="null"/>, empty or blank finds nothing.</param>
    /// <param name="zone">The time zone, or <see langword="null"/> when none has the id.</param>
    /// <returns><see langword="true"/> when the time zone was found.</returns>
    public static bool TryFind(string? id, [NotNullWhen(true)] out TimeZoneInfo? zone)
    {
        zone = null;
        if (string.IsNullOrWhiteSpace(id))
            return false;
#if NET8_0_OR_GREATER
        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out zone))
            return true;

        return (TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out string? converted)
                || TimeZoneInfo.TryConvertWindowsIdToIanaId(id, out converted))
            && TimeZoneInfo.TryFindSystemTimeZoneById(converted, out zone);
#else
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById(id);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
#endif
    }
}
