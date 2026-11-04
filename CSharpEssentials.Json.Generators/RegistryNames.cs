using System.Text;

namespace CSharpEssentials.Json.Generators;

internal static class RegistryNames
{
    public const string Namespace = "CSharpEssentials.Json.Generated";

    public const string Prefix = "JsonPolymorphismRegistry_";

    public static string ForAssembly(string? assemblyName)
    {
        string name = assemblyName ?? "Assembly";
        return Prefix + Sanitize(name) + "_" + Fnv1a(name).ToString("x8", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string Sanitize(string name)
    {
        StringBuilder builder = new(name.Length + 1);
        bool startOfSegment = true;
        foreach (char c in name)
        {
            if (!IsIdentifierChar(c))
            {
                startOfSegment = true;
                continue;
            }

            builder.Append(startOfSegment ? char.ToUpperInvariant(c) : c);
            startOfSegment = false;
        }

        return builder.ToString();
    }

    private static uint Fnv1a(string value)
    {
        uint hash = 2166136261;
        foreach (char c in value)
        {
            hash = (hash ^ c) * 16777619;
        }

        return hash;
    }

    private static bool IsIdentifierChar(char c) =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_';
}
