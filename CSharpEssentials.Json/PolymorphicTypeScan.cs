using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSharpEssentials.Json;

internal static class PolymorphicTypeScan
{
    private static readonly string[] FrameworkPublicKeyTokens =
    [
        "b03f5f7f11d50a3a",
        "7cec85d7bea7798e",
        "cc7b13ffcd2ddd51",
        "adb9793829ddae60",
        "b77a5c561934e089",
        "31bf3856ad364e35",
    ];

    /// <summary>
    /// Whether <paramref name="assembly"/> is signed with a .NET framework key, so its types are never polymorphic candidates.
    /// </summary>
    internal static bool IsFrameworkAssembly(Assembly assembly)
    {
        byte[]? token = assembly.GetName().GetPublicKeyToken();
        if (token is null || token.Length == 0)
            return false;

        string hex = string.Concat(token.Select(static b => b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture)));
        return FrameworkPublicKeyTokens.Contains(hex, StringComparer.Ordinal);
    }

    /// <summary>
    /// The types of <paramref name="assembly"/> that can be loaded. A partially loadable assembly (a proxy assembly still emitting
    /// types, for example) must not break discovery.
    /// </summary>
    [RequiresUnreferencedCode(PolymorphicJson.Message)]
    internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }
}
