using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
    private static readonly ConcurrentDictionary<(Type Type, EnumNaming Naming), IEnumInfo> ReflectionCache = new();
    private static readonly Dictionary<Type, Lazy<IEnumInfo>> Registered = [];
#if NET9_0_OR_GREATER
    private static FrozenDictionary<Type, Lazy<IEnumInfo>>? _snapshot;
#else
    private static Dictionary<Type, Lazy<IEnumInfo>>? _snapshot;
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

        if (!enumType.IsEnum)
            return false;

        // A type-only reference does not run the module initializer of the declaring assembly. The runtime runs it at most once
        // and blocks concurrent callers until it completes, so every caller sees the registrations after this call.
        RuntimeHelpers.RunModuleConstructor(enumType.Module.ModuleHandle);
        return Lookup(enumType, out info);
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

    /// <summary>Registers metadata built in code.</summary>
    public static void Register<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum
    {
        _ = info ?? throw new ArgumentNullException(nameof(info));

        lock (Gate)
        {
            Registered[typeof(TEnum)] = new Lazy<IEnumInfo>(() => info);
            _snapshot = null;
            EnumInfoCache<TEnum>.Set(info);
        }
    }

    /// <summary>
    /// Registers the metadata factories of one assembly in a single batch. Called by the generated module initializer; each factory
    /// runs on the first lookup of its enum.
    /// </summary>
    /// <param name="factories">The enum types and the factories of their metadata.</param>
    public static void RegisterRange(KeyValuePair<Type, Func<IEnumInfo>>[] factories)
    {
        _ = factories ?? throw new ArgumentNullException(nameof(factories));

        foreach (KeyValuePair<Type, Func<IEnumInfo>> factory in factories)
        {
            _ = factory.Key ?? throw new ArgumentException("An enum type is null.", nameof(factories));
            _ = factory.Value ?? throw new ArgumentException($"The factory of '{factory.Key.FullName}' is null.", nameof(factories));
        }

        lock (Gate)
        {
            foreach (KeyValuePair<Type, Func<IEnumInfo>> factory in factories)
            {
                Registered[factory.Key] = new Lazy<IEnumInfo>(factory.Value);
            }

            _snapshot = null;
        }
    }

    private static bool Lookup(Type enumType, [NotNullWhen(true)] out IEnumInfo? info)
    {
#if NET9_0_OR_GREATER
        FrozenDictionary<Type, Lazy<IEnumInfo>>? snapshot = Volatile.Read(ref _snapshot);
#else
        Dictionary<Type, Lazy<IEnumInfo>>? snapshot = Volatile.Read(ref _snapshot);
#endif
        if (snapshot is null)
        {
            lock (Gate)
            {
#if NET9_0_OR_GREATER
                snapshot = _snapshot ??= Registered.ToFrozenDictionary();
#else
                snapshot = _snapshot ??= Registered.ToDictionary(static entry => entry.Key, static entry => entry.Value);
#endif
            }
        }

        if (snapshot.TryGetValue(enumType, out Lazy<IEnumInfo>? entry))
        {
            info = entry.Value;
            return true;
        }

        info = null;
        return false;
    }
}
