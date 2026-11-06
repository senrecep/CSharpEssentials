namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// The byte layout of the GUID keys that <see cref="ModelConfigurationExtensions.UseOrderedGuidKeys"/> generates.
/// </summary>
public enum OrderedGuidLayout
{
    /// <summary>
    /// RFC 9562 version 7 from <c>Guider.NewMonotonicGuid()</c>. Sorts by creation time as big-endian bytes, as a string and with
    /// <see cref="Guid.CompareTo(Guid)"/>: PostgreSQL <c>uuid</c>, SQLite <c>TEXT</c>, MySQL <c>CHAR(36)</c>, <c>CHAR(32)</c> and big-endian <c>BINARY(16)</c>.
    /// </summary>
    Version7 = 0,

    /// <summary>
    /// RFC 9562 version 8 from <c>Guider.NewSqlServerGuid()</c>. Sorts by creation time in SQL Server <c>uniqueidentifier</c> order only.
    /// </summary>
    SqlServer,
}
