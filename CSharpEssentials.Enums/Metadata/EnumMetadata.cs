using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NET9_0_OR_GREATER
using System.Collections.Frozen;
#endif

namespace CSharpEssentials.Enums;

/// <summary>
/// The registry of enum metadata. Generated code registers every <see cref="StringEnumAttribute"/> enum from a module initializer.
/// </summary>
public static class EnumMetadata
{
    internal const string ReflectionMessage =
        "Reflection metadata reads enum fields and attributes at runtime. Mark the enum [StringEnum] to use generated metadata.";

#if NET9_0_OR_GREATER
    private static readonly Lock Gate = new();
#else
    private static readonly object Gate = new();
#endif
    private static readonly ConcurrentDictionary<Module, byte> InitializedModules = new();
    private static readonly ConcurrentDictionary<(Type Type, EnumNaming Naming), IEnumInfo> ReflectionCache = new();
    private static Dictionary<Type, IEnumInfo> _registered = [];
#if NET9_0_OR_GREATER
    private static FrozenDictionary<Type, IEnumInfo>? _snapshot;
#endif

    /// <summary>Gets the generated metadata of <typeparamref name="TEnum"/>.</summary>
    /// <exception cref="InvalidOperationException">The enum is not marked <see cref="StringEnumAttribute"/> (or its project uses C# 8 or older).</exception>
    public static EnumInfo<TEnum> Get<TEnum>() where TEnum : struct, Enum =>
        EnumInfoCache<TEnum>.Value ?? throw new InvalidOperationException(
            $"Enum '{typeof(TEnum).FullName}' has no generated metadata. Mark it [StringEnum] (C# 9 or newer is required for registration).");

    /// <summary>Gets the generated metadata of <typeparamref name="TEnum"/>, if registered.</summary>
    public static bool TryGet<TEnum>([NotNullWhen(true)] out EnumInfo<TEnum>? info) where TEnum : struct, Enum
    {
        info = EnumInfoCache<TEnum>.Value;
        return info is not null;
    }

    /// <summary>Gets the generated metadata of <paramref name="enumType"/>, if registered.</summary>
    public static bool TryGet(Type enumType, [NotNullWhen(true)] out IEnumInfo? info)
    {
        _ = enumType ?? throw new ArgumentNullException(nameof(enumType));

        if (Lookup(enumType, out info))
            return true;

        // A type-only reference does not run the module initializer of the declaring assembly; run it once and retry.
        return enumType.IsEnum && TryRunModuleInitializer(enumType.Module) && Lookup(enumType, out info);
    }

    /// <summary>Whether <paramref name="type"/> is an enum with generated metadata.</summary>
    public static bool IsRegistered(Type type) => type is not null && TryGet(type, out _);

    /// <summary>
    /// Gets the generated metadata of <paramref name="enumType"/>, or builds (and caches) metadata with reflection for an enum
    /// without <see cref="StringEnumAttribute"/>. Reflection metadata is never registered.
    /// </summary>
    /// <param name="enumType">The enum type.</param>
    /// <param name="naming">The naming used when the enum declares none; <see cref="EnumNaming.Default"/> means <see cref="EnumNaming.SnakeCaseLower"/>.</param>
    [RequiresUnreferencedCode(ReflectionMessage)]
    [RequiresDynamicCode(ReflectionMessage)]
    public static IEnumInfo GetOrCreateWithReflection(Type enumType, EnumNaming naming = EnumNaming.SnakeCaseLower)
    {
        _ = enumType ?? throw new ArgumentNullException(nameof(enumType));
        if (!enumType.IsEnum)
            throw new ArgumentException($"'{enumType.FullName}' is not an enum type.", nameof(enumType));

        return TryGet(enumType, out IEnumInfo? info)
            ? info
            : ReflectionCache.GetOrAdd((enumType, naming), static key => ReflectionEnumInfoBuilder.Build(key.Type, key.Naming));
    }

    /// <summary>Registers generated metadata. Called by the generated module initializer.</summary>
    public static void Register<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum
    {
        _ = info ?? throw new ArgumentNullException(nameof(info));

        lock (Gate)
        {
            Dictionary<Type, IEnumInfo> copy = new(_registered) { [typeof(TEnum)] = info };
            Volatile.Write(ref _registered, copy);
#if NET9_0_OR_GREATER
            _snapshot = null;
#endif
            EnumInfoCache<TEnum>.Set(info);
        }
    }

    /// <summary>Runs the module initializer of <paramref name="module"/> on the first call for that module.</summary>
    internal static bool TryRunModuleInitializer(Module module)
    {
        if (!InitializedModules.TryAdd(module, 0))
            return false;
        RuntimeHelpers.RunModuleConstructor(module.ModuleHandle);
        return true;
    }

    private static bool Lookup(Type enumType, [NotNullWhen(true)] out IEnumInfo? info)
    {
#if NET9_0_OR_GREATER
        FrozenDictionary<Type, IEnumInfo>? snapshot = Volatile.Read(ref _snapshot);
        if (snapshot is null)
        {
            lock (Gate)
            {
                snapshot = _snapshot ??= _registered.ToFrozenDictionary();
            }
        }

        return snapshot.TryGetValue(enumType, out info);
#else
        return Volatile.Read(ref _registered).TryGetValue(enumType, out info);
#endif
    }
}
