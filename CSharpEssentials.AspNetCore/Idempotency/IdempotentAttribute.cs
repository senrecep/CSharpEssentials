namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Opts an MVC action or controller into <c>Idempotency-Key</c> handling by <c>UseIdempotency()</c>.
/// Minimal API endpoints and groups use <c>.WithIdempotency()</c>, which adds this attribute as metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class IdempotentAttribute : Attribute;
