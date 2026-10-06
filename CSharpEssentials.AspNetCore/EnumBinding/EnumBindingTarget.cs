namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A route, query, header or form value bound to an enum (or a collection of enums) by an endpoint.
/// </summary>
/// <param name="Source">Where the value is read from.</param>
/// <param name="Key">The route, query, header or form key.</param>
/// <param name="Normalizer">Parses the value with the enum conventions.</param>
/// <param name="AllowEmpty">Nullable or collection: blank values are dropped instead of rejected.</param>
/// <param name="IsCollection">An array or collection: a comma separated value is split into several values.</param>
/// <param name="SkipWhenPrefixPresent">
/// MVC complex type fallback key: skipped when the request has a value under this model prefix, because MVC then
/// binds the prefixed keys only.
/// </param>
/// <param name="FirstSourceWins">
/// MVC parameter without an explicit source: MVC reads the first value provider that has the key (form, route, query), so
/// only that source is validated.
/// </param>
internal sealed record EnumBindingTarget(
    EnumBindingSource Source,
    string Key,
    EnumBindingNormalizer Normalizer,
    bool AllowEmpty,
    bool IsCollection,
    string? SkipWhenPrefixPresent = null,
    bool FirstSourceWins = false);
