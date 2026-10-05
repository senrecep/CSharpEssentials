using System.Text.Json.Serialization;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// One item of the <c>errors</c> list written for <see cref="ProblemErrorFields.ValidationErrors"/>.
/// </summary>
public sealed record ProblemValidationError(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("description")] string Description);
