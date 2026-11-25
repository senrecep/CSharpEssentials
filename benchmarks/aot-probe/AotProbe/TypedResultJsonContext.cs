using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace AotProbe;

[JsonSourceGenerationOptions(Converters = [
    typeof(TypedResultJsonConverter<ProbeDto, NotFoundError, ConflictError>),
    typeof(TypedResultJsonConverter<int, NotFoundError>),
    typeof(TypedResultJsonConverter<int, NotFoundError, ConflictError>),
    typeof(TypedResultJsonConverter<int, NotFoundError, ConflictError, ProbeLimit>)])]
[JsonSerializable(typeof(Result<ProbeDto, NotFoundError, ConflictError>))]
[JsonSerializable(typeof(Result<int, NotFoundError>))]
[JsonSerializable(typeof(Result<int, NotFoundError, ConflictError>))]
[JsonSerializable(typeof(Result<int, NotFoundError, ConflictError, ProbeLimit>))]
[JsonSerializable(typeof(ProbeDto))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(NotFoundError))]
[JsonSerializable(typeof(ConflictError))]
[JsonSerializable(typeof(ProbeLimit))]
internal sealed partial class TypedResultJsonContext : JsonSerializerContext;

[JsonSerializable(typeof(Result<long, NotFoundError>))]
internal sealed partial class UnregisteredTypedResultJsonContext : JsonSerializerContext;

public readonly record struct ProbeLimit(int RetryAfter);
