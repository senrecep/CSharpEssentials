using System.Text.Json.Serialization;
using JsonAotCore;

namespace JsonAot;

[JsonSerializable(typeof(IEvent))]
[JsonSerializable(typeof(UserCreated))]
[JsonSerializable(typeof(UserDeleted))]
[JsonSerializable(typeof(Data))]
[JsonSerializable(typeof(TextValue))]
[JsonSerializable(typeof(NumberValue))]
[JsonSourceGenerationOptions(System.Text.Json.JsonSerializerDefaults.Web, AllowOutOfOrderMetadataProperties = true)]
public sealed partial class AppJsonContext : JsonSerializerContext;
