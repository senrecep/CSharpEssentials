using System.Text;

namespace CSharpEssentials.Json.Generators;

internal static class RegistryNames
{
    public const string Namespace = "CSharpEssentials.Json.Generated";

    public const string Prefix = "JsonPolymorphismRegistry_";

    public static string ForAssembly(string? assemblyName) => Prefix + Sanitize(assemblyName ?? "Assembly");

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

    private static bool IsIdentifierChar(char c) =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_';
}
