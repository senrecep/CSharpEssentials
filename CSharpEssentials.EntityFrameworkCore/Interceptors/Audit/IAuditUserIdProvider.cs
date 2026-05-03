namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Provides the current user identity for audit fields.
/// The interceptor resolves this interface to populate CreatedBy / UpdatedBy / DeletedBy.
/// </summary>
public interface IAuditUserIdProvider
{
    string GetCurrentUserId();
}

/// <summary>
/// Generic variant for type-safe user ID resolution.
/// <para>
/// The <see cref="IAuditUserIdProvider.GetCurrentUserId"/> is auto-implemented
/// via <c>ToString()</c> — implementors only need the typed overload.
/// </para>
/// </summary>
/// <example>
/// <code>
/// // Register in DI:
/// services.AddScoped&lt;IAuditUserIdProvider, MyUserIdProvider&gt;();
///
/// // Implement:
/// sealed class MyUserIdProvider : IAuditUserIdProvider&lt;Guid&gt;
/// {
///     public Guid GetCurrentUserId() => currentUser.Id;
///     // string version is auto-provided
/// }
/// </code>
/// </example>
public interface IAuditUserIdProvider<out TUserId> : IAuditUserIdProvider
    where TUserId : notnull
{
    new TUserId GetCurrentUserId();

    string IAuditUserIdProvider.GetCurrentUserId() =>
        GetCurrentUserId().ToString() ?? string.Empty;
}
