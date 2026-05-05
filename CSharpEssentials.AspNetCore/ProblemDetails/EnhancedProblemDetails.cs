using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

public sealed class EnhancedProblemDetails : ProblemDetails
{
    [JsonPropertyName("errors")]
#if NET8_0_OR_GREATER
    public Error[] Errors { get; set; } = [];
#else
    public Error[] Errors { get; set; } = Array.Empty<Error>();
#endif

    [JsonPropertyName("errorCodes")]
    public HashSet<string> ErrorCodes { get; set; } = [];

    [JsonPropertyName("errorMessages")]
    public HashSet<string> ErrorMessages { get; set; } = [];
}
