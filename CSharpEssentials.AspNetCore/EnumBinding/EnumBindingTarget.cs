namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A query or route value bound to an enum (or a collection of enums) by an endpoint.
/// </summary>
/// <param name="Source">Where the value is read from.</param>
/// <param name="Key">The query or route key.</param>
/// <param name="EnumType">The enum type the value binds to.</param>
/// <param name="AllowEmpty">Nullable or collection: blank values are dropped instead of rejected.</param>
/// <param name="SkipWhenPrefixPresent">
/// MVC complex type fallback key: skipped when the request has a value under this model prefix, because MVC then
/// binds the prefixed keys only.
/// </param>
internal sealed record EnumBindingTarget(
    EnumBindingSource Source,
    string Key,
    Type EnumType,
    bool AllowEmpty,
    string? SkipWhenPrefixPresent = null);
