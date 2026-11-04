using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace CSharpEssentials.Json;

/// <summary>
/// Fills System.Text.Json's polymorphism for bases marked with <see cref="System.Text.Json.Serialization.JsonPolymorphicAttribute"/> or
/// <see cref="System.Text.Json.Serialization.JsonDerivedTypeAttribute"/> with derived types from any assembly.
/// </summary>
public static class JsonPolymorphismExtensions
{
    internal const string ReflectionMessage =
        "AddPolymorphismWithReflection scans assemblies for derived types and reads attributes at runtime. Use AddPolymorphism with " +
        "[JsonTypeDiscriminator] and a JsonSerializerContext for trimmed or native AOT applications.";

    private static readonly ConditionalWeakTable<IJsonTypeInfoResolver, StrongBox<PolymorphismScanner?>> Wrapped = [];

    /// <summary>
    /// Adds the derived types registered by generated code (<see cref="JsonTypeDiscriminatorAttribute"/>) and the per-property names
    /// (<see cref="JsonPolymorphicPropertyAttribute"/>). No reflection, so it is trim and native AOT safe. Wraps
    /// <see cref="JsonSerializerOptions.TypeInfoResolver"/>, which must be set, and sets <c>AllowOutOfOrderMetadataProperties</c>.
    /// </summary>
    /// <remarks>
    /// Call it after the resolver is complete: a resolver inserted into <see cref="JsonSerializerOptions.TypeInfoResolverChain"/>
    /// later bypasses it. With a source-generated context, copy its options (<c>new JsonSerializerOptions(AppJsonContext.Default.Options)</c>)
    /// and list every derived type with <c>[JsonSerializable]</c>; a derived type missing from the context is left out.
    /// </remarks>
    /// <param name="options">The options to change; they must not be in use yet.</param>
    /// <returns><paramref name="options"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="JsonSerializerOptions.TypeInfoResolver"/> is not set.</exception>
    public static JsonSerializerOptions AddPolymorphism(this JsonSerializerOptions options)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        IJsonTypeInfoResolver inner = options.TypeInfoResolver ?? throw new InvalidOperationException(
            "Set TypeInfoResolver (for example to a JsonSerializerContext) before AddPolymorphism, or use AddPolymorphismWithReflection.");
        return Apply(options, inner, new PolymorphismModifier(inner, null));
    }

    /// <summary>
    /// Like <see cref="AddPolymorphism"/>, and for bases without <see cref="System.Text.Json.Serialization.JsonDerivedTypeAttribute"/> also
    /// scans for derived types by reflection. A derived type without <see cref="JsonTypeDiscriminatorAttribute"/> gets its full CLR name.
    /// Also enables <see cref="JsonAcceptClrTypeNamesAttribute"/>. Uses a <see cref="DefaultJsonTypeInfoResolver"/> when no resolver is set.
    /// </summary>
    /// <param name="options">The options to change; they must not be in use yet.</param>
    /// <param name="trustedAssemblies">
    /// The assemblies to scan. Empty scans every loaded assembly except .NET framework assemblies.
    /// </param>
    /// <returns><paramref name="options"/>.</returns>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public static JsonSerializerOptions AddPolymorphismWithReflection(this JsonSerializerOptions options, params Assembly[] trustedAssemblies)
    {
        _ = options ?? throw new ArgumentNullException(nameof(options));
        _ = trustedAssemblies ?? throw new ArgumentNullException(nameof(trustedAssemblies));
        IJsonTypeInfoResolver inner = options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
        Apply(options, inner, new PolymorphismModifier(inner, new PolymorphismScanner([.. trustedAssemblies])));
        if (!options.Converters.Any(static converter => converter is ClrTypeNameConverterFactory))
            options.Converters.Add(new ClrTypeNameConverterFactory());

        return options;
    }

    /// <summary>
    /// When <paramref name="source"/> has a resolver wrapped by this class and <paramref name="target"/> has its own resolver, wraps the
    /// target's resolver the same way, so contexts the target already holds stay in use.
    /// </summary>
    internal static bool TryApplyTo(JsonSerializerOptions source, JsonSerializerOptions target)
    {
        if (source.TypeInfoResolver is not { } wrapped || !Wrapped.TryGetValue(wrapped, out StrongBox<PolymorphismScanner?>? scanner) ||
            target.TypeInfoResolver is not { } inner)
        {
            return false;
        }

        if (!Wrapped.TryGetValue(inner, out _))
            Apply(target, inner, new PolymorphismModifier(inner, scanner.Value));

        return true;
    }

    private static JsonSerializerOptions Apply(JsonSerializerOptions options, IJsonTypeInfoResolver inner, PolymorphismModifier modifier)
    {
        IJsonTypeInfoResolver resolver = inner.WithAddedModifier(modifier.Modify);
        Wrapped.AddOrUpdate(resolver, new StrongBox<PolymorphismScanner?>(modifier.Scanner));
        options.TypeInfoResolver = resolver;
        options.AllowOutOfOrderMetadataProperties = true;
        return options;
    }
}
