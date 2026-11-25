using System.Data.Common;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// The connection and the open transaction of a unit of work, for Dapper or raw ADO.NET code that must take
/// part in the same transaction as the <c>DbContext</c>.
/// </summary>
public interface ISqlSession
{
    /// <summary>
    /// The connection of the unit of work. It is owned by the session: do not dispose or close it.
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    /// The transaction in progress on <see cref="Connection"/>, or <see langword="null"/> when none is open.
    /// Pass it to every command (Dapper's <c>transaction:</c> argument), because most providers reject a command
    /// without it while a transaction is open.
    /// </summary>
    DbTransaction? Transaction { get; }
}
