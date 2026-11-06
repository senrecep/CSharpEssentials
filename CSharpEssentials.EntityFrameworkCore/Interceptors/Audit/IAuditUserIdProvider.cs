namespace CSharpEssentials.EntityFrameworkCore.Interceptors;

/// <summary>
/// Provides the current user identity for audit fields.
/// The interceptor resolves this interface to fill CreatedBy, UpdatedBy and DeletedBy.
/// </summary>
public interface IAuditUserIdProvider
{
    string GetCurrentUserId();
}

/// <summary>
/// Typed variant of <see cref="IAuditUserIdProvider"/>.
/// <para>
/// <see cref="IAuditUserIdProvider.GetCurrentUserId"/> has a default implementation that calls
/// <c>ToString()</c> on the typed ID, so implementations only write the typed overload.
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
