using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Source-generated metadata for the problem types and the extension values this package writes, appended after the
/// application's resolvers so problem responses serialize when reflection-based serialization is disabled (Native AOT).
/// </summary>
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(EnhancedProblemDetails))]
[JsonSerializable(typeof(HashSet<string>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Dictionary<string, string[]>))]
[JsonSerializable(typeof(Dictionary<string, string?>))]
[JsonSerializable(typeof(List<ProblemValidationError>))]
[JsonSerializable(typeof(Error[]))]
[JsonSerializable(typeof(ErrorMetadata))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
internal sealed partial class EnhancedProblemDetailsJsonContext : JsonSerializerContext;
