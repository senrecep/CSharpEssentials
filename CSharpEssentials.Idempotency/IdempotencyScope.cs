using System.Runtime.CompilerServices;
using System.Text;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// The owner of an idempotency key: a user, tenant or client, or <see cref="Global"/> when every caller shares the
/// key space. An <see cref="IIdempotencyScopeResolver"/> returns <see langword="null"/> instead of a scope when it cannot
/// tell who the caller is, so "unresolved" and "explicitly global" stay two different answers.
/// <para>
/// Scopes are built only by the factories, which write the kind of the owner in front of its id (<c>user:42</c>,
/// <c>tenant:42</c>), so ids of different kinds never share a scope: <c>Tenant("user:42")</c> is
/// <c>tenant:user:42</c>, not <c>User("42")</c>. Every scope other than <see cref="Global"/> contains a <c>:</c>, so
/// none can be the <c>global</c> scope part of a global key.
/// </para>
/// <para><see langword="default"/> is <see cref="Global"/>.</para>
/// </summary>
public readonly record struct IdempotencyScope
{
    private IdempotencyScope(string? value) => Value = value;

    /// <summary>The scope shared by every caller. Choose it explicitly; it is never the result of a failed lookup.</summary>
    public static IdempotencyScope Global => default;

    /// <summary>The scope as written in the storage key, for example <c>user:42</c>; <see langword="null"/> for <see cref="Global"/>.</summary>
    public string? Value { get; }

    /// <summary><see langword="true"/> for <see cref="Global"/>.</summary>
    public bool IsGlobal => Value is null;

    /// <summary>The user <paramref name="id"/>: <c>user:{id}</c>.</summary>
    /// <exception cref="ArgumentException"><paramref name="id"/> is <see langword="null"/> or empty.</exception>
    public static IdempotencyScope User(string id) => Kind("user", id);

    /// <summary>The tenant <paramref name="id"/>: <c>tenant:{id}</c>.</summary>
    /// <exception cref="ArgumentException"><paramref name="id"/> is <see langword="null"/> or empty.</exception>
    public static IdempotencyScope Tenant(string id) => Kind("tenant", id);

    /// <summary>The calling client <paramref name="id"/>, for example an OAuth client id: <c>client:{id}</c>.</summary>
    /// <exception cref="ArgumentException"><paramref name="id"/> is <see langword="null"/> or empty.</exception>
    public static IdempotencyScope Client(string id) => Kind("client", id);

    /// <summary>
    /// The user <paramref name="userId"/> within the tenant <paramref name="tenantId"/>:
    /// <c>{n}:tenant:{tenantId}/user:{userId}</c>, where <c>{n}</c> is the UTF-8 byte length of the tenant part, so no
    /// user and tenant pair can produce the scope of another pair.
    /// </summary>
    /// <exception cref="ArgumentException">An id is <see langword="null"/> or empty, or not well-formed UTF-16.</exception>
    public static IdempotencyScope UserAndTenant(string userId, string tenantId)
    {
        Guard.NotNullOrEmpty(userId);
        Guard.NotNullOrEmpty(tenantId);

        string tenantPart = "tenant:" + tenantId;
        return new(LengthPrefixed.Append(new StringBuilder(tenantPart.Length + userId.Length + 16), tenantPart)
            .Append("/user:")
            .Append(userId)
            .ToString());
    }

    /// <summary>An owner of your own kind: <c>{kind}:{value}</c>, for example <c>Custom("account", "acc-9")</c>.</summary>
    /// <param name="kind">Names the kind of owner. Must not be empty, contain <c>:</c> or consist only of digits.</param>
    /// <param name="value">The owner id. Must not be empty.</param>
    /// <exception cref="ArgumentException"><paramref name="kind"/> or <paramref name="value"/> is not valid.</exception>
    public static IdempotencyScope Custom(string kind, string value)
    {
        ValidateKind(kind);
        return Kind(kind, value);
    }

    /// <summary>
    /// A scope from its storage form, for code that reads scopes back and for tests of the storage key encoding.
    /// Public code builds scopes with the factories.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty or <c>global</c>.</exception>
    internal static IdempotencyScope Of(string value)
    {
        Guard.NotNullOrEmpty(value);
        if (value == IdempotencyRecordKey.GlobalScope)
            throw new ArgumentException("The scope 'global' is reserved for IdempotencyScope.Global.", nameof(value));

        return new(value);
    }

    /// <exception cref="ArgumentException"><paramref name="kind"/> is empty, contains <c>:</c> or is all digits.</exception>
    internal static void ValidateKind(string kind)
    {
        Guard.NotNullOrEmpty(kind);
        if (kind.Contains(':'))
            throw new ArgumentException("A scope kind cannot contain ':'.", nameof(kind));
        if (kind.All(static c => c is >= '0' and <= '9'))
            throw new ArgumentException("A scope kind cannot consist only of digits; it would look like the length prefix of a user and tenant scope.", nameof(kind));
    }

    private static IdempotencyScope Kind(string kind, string id, [CallerArgumentExpression(nameof(id))] string? parameterName = null)
    {
        Guard.NotNullOrEmpty(id, parameterName);
        return new(kind + ":" + id);
    }

    /// <summary>The scope as written in the storage key, or <c>(global)</c>.</summary>
    public override string ToString() => Value ?? "(global)";
}
