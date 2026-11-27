using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Entity.Interfaces;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// The event types <see cref="JsonOutboxMessageSerializer"/> can store, each under a stable name. The name is what the
/// outbox row keeps, so renaming or moving an event class does not orphan stored messages; never reuse a name for
/// another type. Only registered types are read back, so a row cannot make the processor load an arbitrary type.
/// <para>
/// For trimming and Native AOT register each type with the <see cref="JsonTypeInfo{T}"/> of a source-generated
/// <c>JsonSerializerContext</c>; <see cref="Add{TEvent}(string, JsonSerializerOptions?)"/> uses reflection.
/// </para>
/// </summary>
public sealed class OutboxEventTypeRegistry
{
    private readonly Dictionary<string, JsonTypeInfo> _byName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, (string Name, JsonTypeInfo TypeInfo)> _byType = [];

    /// <summary>Registers <typeparamref name="TEvent"/> under <paramref name="name"/> with source-generated metadata.</summary>
    /// <param name="name">The stable type name; at most <see cref="OutboxMessageModelOptions.TypeMaxLength"/> characters.</param>
    /// <param name="typeInfo">The metadata, for example <c>AppJsonContext.Default.OrderPlaced</c>.</param>
    /// <returns>This registry.</returns>
    /// <exception cref="ArgumentException">The name is empty or too long, or the name or type is already registered.</exception>
    public OutboxEventTypeRegistry Add<TEvent>(string name, JsonTypeInfo<TEvent> typeInfo)
        where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        return Register(typeof(TEvent), name, typeInfo);
    }

    /// <summary>Registers <typeparamref name="TEvent"/> under <paramref name="name"/>, serialized with reflection.</summary>
    /// <param name="name">The stable type name; at most <see cref="OutboxMessageModelOptions.TypeMaxLength"/> characters.</param>
    /// <param name="options">The serializer options, copied so yours stay editable; <see cref="JsonSerializerOptions.Default"/> when <see langword="null"/>.</param>
    /// <returns>This registry.</returns>
    /// <exception cref="ArgumentException">The name is empty or too long, or the name or type is already registered.</exception>
    [RequiresUnreferencedCode("Reflection-based JSON metadata; use the JsonTypeInfo overload for trimming.")]
    [RequiresDynamicCode("Reflection-based JSON metadata; use the JsonTypeInfo overload for Native AOT.")]
    public OutboxEventTypeRegistry Add<TEvent>(string name, JsonSerializerOptions? options = null)
        where TEvent : IDomainEvent
    {
        JsonSerializerOptions resolved = options is null || options.IsReadOnly
            ? options ?? JsonSerializerOptions.Default
            : new JsonSerializerOptions(options);
        if (!resolved.IsReadOnly)
            resolved.MakeReadOnly(populateMissingResolver: true);
        return Register(typeof(TEvent), name, resolved.GetTypeInfo(typeof(TEvent)));
    }

    /// <summary>Gets the name and metadata of <paramref name="eventType"/>.</summary>
    /// <exception cref="InvalidOperationException">The type is not registered.</exception>
    internal (string Name, JsonTypeInfo TypeInfo) GetByType(Type eventType) =>
        _byType.TryGetValue(eventType, out (string Name, JsonTypeInfo TypeInfo) entry)
            ? entry
            : throw new InvalidOperationException(
                $"The domain event type '{eventType.FullName}' is not registered for the outbox. Register it with {nameof(OutboxEventTypeRegistry)}.{nameof(Add)}.");

    /// <summary>Gets the metadata registered under <paramref name="name"/>.</summary>
    /// <exception cref="InvalidOperationException">No type is registered under the name.</exception>
    internal JsonTypeInfo GetByName(string name) =>
        _byName.TryGetValue(name, out JsonTypeInfo? typeInfo)
            ? typeInfo
            : throw new InvalidOperationException($"No domain event type is registered for the outbox under the name '{name}'.");

    private OutboxEventTypeRegistry Register(Type type, string name, JsonTypeInfo typeInfo)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (name.Length > OutboxMessageModelOptions.TypeMaxLength)
            throw new ArgumentException($"The type name must be at most {OutboxMessageModelOptions.TypeMaxLength} characters.", nameof(name));
        if (_byName.ContainsKey(name))
            throw new ArgumentException($"The outbox type name '{name}' is already registered.", nameof(name));
        if (_byType.ContainsKey(type))
            throw new ArgumentException($"The type '{type.FullName}' is already registered for the outbox.", nameof(name));

        _byName.Add(name, typeInfo);
        _byType.Add(type, (name, typeInfo));
        return this;
    }
}
