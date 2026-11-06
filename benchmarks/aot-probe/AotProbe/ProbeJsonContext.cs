using System.Text.Json.Serialization;

namespace AotProbe;

[JsonSerializable(typeof(ProbeDto))]
internal sealed partial class ProbeJsonContext : JsonSerializerContext;
