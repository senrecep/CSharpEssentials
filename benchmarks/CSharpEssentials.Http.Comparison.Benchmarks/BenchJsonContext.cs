using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(User[]))]
[JsonSerializable(typeof(NewUser))]
public sealed partial class BenchJsonContext : JsonSerializerContext;
