using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

public sealed class EnhancedProblemDetails : ProblemDetails
{
    [JsonPropertyName("errors")]
    public Error[] Errors { get; set; } = [];

    [JsonPropertyName("errorCodes")]
    public HashSet<string> ErrorCodes { get; set; } = [];

    [JsonPropertyName("errorMessages")]
    public HashSet<string> ErrorMessages { get; set; } = [];
}
