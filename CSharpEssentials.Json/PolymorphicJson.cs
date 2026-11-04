using System.Text.Json;

namespace CSharpEssentials.Json;

internal static class PolymorphicJson
{
    internal const string Message =
        "Polymorphic JSON discovers derived types by scanning the loaded assemblies and serializes them by reflection. Use [JsonPolymorphic] with source generation for trimmed or native AOT applications.";

    private const int _defaultMaxDepth = 64;

    [ThreadStatic]
    private static int _depth;

    /// <summary>
    /// Counts one nested polymorphic serializer call and throws once the nesting passes the configured maximum depth.
    /// </summary>
    internal static void EnterNested(JsonSerializerOptions options)
    {
        int maxDepth = options.MaxDepth == 0 ? _defaultMaxDepth : options.MaxDepth;
        if (_depth >= maxDepth)
        {
            throw new JsonException($"Polymorphic nesting exceeded the maximum depth of {maxDepth}. The object graph may contain a cycle.");
        }
        _depth++;
    }

    internal static void ExitNested() => _depth--;
}
