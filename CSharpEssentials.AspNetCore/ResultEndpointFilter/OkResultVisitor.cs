using CSharpEssentials.ResultPattern.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Builds the 200 response for a successful <c>Result&lt;T&gt;</c>. Returns <c>Ok&lt;T&gt;</c> when the value's runtime type
/// is exactly <c>T</c>, so serialization skips the runtime type lookup; otherwise <c>Ok&lt;object&gt;</c>, which serializes
/// the runtime type and keeps the body identical for derived, polymorphic, nullable and null values.
/// </summary>
internal sealed class OkResultVisitor : IValueResultVisitor
{
    public static readonly OkResultVisitor Instance = new();

    private OkResultVisitor()
    {
    }

    public object? Visit<TValue>(TValue value) =>
        value is not null && value.GetType() == typeof(TValue)
            ? TypedResults.Ok(value)
            : TypedResults.Ok<object?>(value);
}
