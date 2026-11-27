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
/// It closes the converter over the result's type arguments through reflection. Where dynamic code is not available
/// (Native AOT) it throws <see cref="NotSupportedException"/> instead: register the closed converter, for example
/// <c>TypedResultJsonConverter&lt;UserDto, NotFoundError&gt;</c>, in the source generation options or in
/// <see cref="JsonSerializerOptions.Converters"/>. A registered converter wins over this one.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TypedResultJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => ConverterDefinition(typeToConvert) is not null;

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Dynamic code is not available and no closed converter was registered.</exception>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type definition = ConverterDefinition(typeToConvert)
            ?? throw new ArgumentException($"{typeToConvert} is not a typed result.", nameof(typeToConvert));
        if (!RuntimeFeature.IsDynamicCodeSupported)
            throw new NotSupportedException(MissingConverterMessage(typeToConvert, definition));

        return CreateClosed(typeToConvert.GetGenericArguments());
    }

    internal static string MissingConverterMessage(Type typeToConvert, Type definition) =>
        $"Dynamic code is not available, so {TypedResultJson.TypeName(typeToConvert)} needs its converter registered: add " +
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
