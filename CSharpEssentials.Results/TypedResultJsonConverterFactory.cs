using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// Creates the <see cref="TypedResultJsonConverter{T, E1}"/> family for <see cref="Result{T, E1}"/>,
/// <see cref="Result{T, E1, E2}"/> and <see cref="Result{T, E1, E2, E3}"/>, which carry it as their type-level converter.
/// It is public so that System.Text.Json source generation can create it.
/// </summary>
/// <remarks>
/// It returns the converter that <see cref="Register{T, E1}"/> (or a sibling overload) registered for the result type, and
/// otherwise closes the converter over the result's type arguments through reflection. The bundled source generator registers
/// every typed result listed in a <c>[JsonSerializable]</c> attribute from a module initializer, so Native AOT needs no
/// reflection for those. Where dynamic code is not available and the type is not registered it throws
/// <see cref="NotSupportedException"/>: list the result type in a <c>[JsonSerializable]</c> attribute, call
/// <see cref="Register{T, E1}"/>, or add the closed converter, for example
/// <c>TypedResultJsonConverter&lt;UserDto, NotFoundError&gt;</c>, to the source generation options or to
/// <see cref="JsonSerializerOptions.Converters"/>. A converter in the options wins over this factory.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TypedResultJsonConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, JsonConverter> Registered = new();

    /// <summary>
    /// Registers the converter of <see cref="Result{T, E1}"/> so this factory creates it without reflection. Registering a type
    /// twice keeps the first converter. The bundled source generator calls it for every typed result listed in a
    /// <c>[JsonSerializable]</c> attribute; call it yourself for a typed result that is only reachable through a property.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
    public static void Register<T, E1>()
        where T : notnull
        where E1 : notnull =>
        _ = Registered.TryAdd(typeof(Result<T, E1>), new TypedResultJsonConverter<T, E1>());

    /// <summary>Registers the converter of <see cref="Result{T, E1, E2}"/>; see <see cref="Register{T, E1}"/>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
    public static void Register<T, E1, E2>()
        where T : notnull
        where E1 : notnull
        where E2 : notnull =>
        _ = Registered.TryAdd(typeof(Result<T, E1, E2>), new TypedResultJsonConverter<T, E1, E2>());

    /// <summary>Registers the converter of <see cref="Result{T, E1, E2, E3}"/>; see <see cref="Register{T, E1}"/>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
    public static void Register<T, E1, E2, E3>()
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull =>
        _ = Registered.TryAdd(typeof(Result<T, E1, E2, E3>), new TypedResultJsonConverter<T, E1, E2, E3>());

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => ConverterDefinition(typeToConvert) is not null;

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Dynamic code is not available and no closed converter was registered.</exception>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type definition = ConverterDefinition(typeToConvert)
            ?? throw new ArgumentException($"{typeToConvert} is not a typed result.", nameof(typeToConvert));
        if (Registered.TryGetValue(typeToConvert, out JsonConverter? registered))
            return registered;
        if (!RuntimeFeature.IsDynamicCodeSupported)
            throw new NotSupportedException(MissingConverterMessage(typeToConvert, definition));

        return CreateClosed(typeToConvert.GetGenericArguments());
    }

    internal static string MissingConverterMessage(Type typeToConvert, Type definition) =>
        $"Dynamic code is not available, so {TypedResultJson.TypeName(typeToConvert)} needs its converter registered: list " +
        $"typeof({TypedResultJson.TypeName(typeToConvert)}) in a [JsonSerializable] attribute so the CSharpEssentials.Results " +
        "generator registers it, call TypedResultJsonConverterFactory.Register, or add " +
        $"typeof({definition.Namespace}.{definition.Name.Substring(0, definition.Name.IndexOf('`'))}" +
        $"<{string.Join(", ", typeToConvert.GetGenericArguments().Select(TypedResultJson.TypeName))}>) " +
        "to JsonSourceGenerationOptions.Converters or JsonSerializerOptions.Converters.";

#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Closes the typed result converter over the result's type arguments.")]
#endif
    private static JsonConverter CreateClosed(Type[] arguments)
    {
        Type closed = arguments.Length switch
        {
            2 => typeof(TypedResultJsonConverter<,>).MakeGenericType(arguments),
            3 => typeof(TypedResultJsonConverter<,,>).MakeGenericType(arguments),
            _ => typeof(TypedResultJsonConverter<,,,>).MakeGenericType(arguments),
        };
        return (JsonConverter)Activator.CreateInstance(closed)!;
    }

    private static Type? ConverterDefinition(Type type)
    {
        if (!type.IsGenericType || type.IsGenericTypeDefinition)
            return null;

        Type definition = type.GetGenericTypeDefinition();
        if (definition == typeof(Result<,>))
            return typeof(TypedResultJsonConverter<,>);
        if (definition == typeof(Result<,,>))
            return typeof(TypedResultJsonConverter<,,>);
        return definition == typeof(Result<,,,>) ? typeof(TypedResultJsonConverter<,,,>) : null;
    }
}
