using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace AotProbe;

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

[JsonSerializable(typeof(NestedTypedResult))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(NotFoundError))]
internal sealed partial class UnregisteredTypedResultJsonContext : JsonSerializerContext;

public sealed record NestedTypedResult(Result<long, NotFoundError> Inner);

public readonly record struct ProbeLimit(int RetryAfter);
