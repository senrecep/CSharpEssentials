using System.Text;

namespace CSharpEssentials.Endpoints.Generators;

internal static class RegistryNames
{
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

        if (builder.Length > 0 && char.IsDigit(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }

    private static bool IsIdentifierChar(char c) =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_';
}
