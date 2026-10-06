using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Turns one route, query, header or form value into the text the framework binders accept (the C# member name), with
/// the rules of the JSON converter in <see cref="EnumReadMode.Input"/> mode: one token per value, numbers only when
/// <see cref="EnumConventions.AcceptNumbers"/>, never the fallback member. Unlike a JSON string, leading and trailing
/// whitespace of a value is trimmed first (route, query, header and form text is routinely padded). A comma separated value
/// of a flags enum or of a collection is split and each part trimmed, like the 4.x flags text the JSON converter still reads.
/// </summary>
internal abstract class EnumBindingNormalizer
{
    public static EnumBindingNormalizer Create(IEnumInfo info, EnumConventions conventions) =>
        info.Accept(new Factory(conventions));

    /// <summary>
    /// Adds the normalized values of <paramref name="value"/> to <paramref name="output"/>: one per part for a collection,
    /// one for a scalar. Returns <see langword="false"/> when a part is rejected.
    /// </summary>
    public abstract bool TryNormalize(string value, bool isCollection, List<string> output);

    /// <summary>The rejection of <paramref name="value"/>, located at <paramref name="key"/>.</summary>
    public abstract EnumValueError CreateError(string? value, string key);

    private sealed class Factory(EnumConventions conventions) : IEnumInfoVisitor<EnumBindingNormalizer>
    {
        public EnumBindingNormalizer Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum =>
            new Typed<TEnum>(info, conventions);
    }

    private sealed class Typed<TEnum>(EnumInfo<TEnum> info, EnumConventions conventions) : EnumBindingNormalizer
        where TEnum : struct, Enum
    {
        public override bool TryNormalize(string value, bool isCollection, List<string> output)
        {
            value = value.Trim();
            if (value.IndexOf(',', StringComparison.Ordinal) < 0)
                return TryAdd(value, output);
            if (!isCollection && !info.IsFlags)
                return false;

            ulong raw = 0;
            foreach (string part in value.Split(','))
            {
                string token = part.Trim();
                if (isCollection)
                {
                    if (!TryAdd(token, output))
                        return false;
                    continue;
                }

                if (!info.TryParse(token, EnumReadMode.Input, conventions, out TEnum flag, out _))
                    return false;
                raw |= info.ToRawValue(flag);
            }

            if (!isCollection)
                output.Add(ToBinderText(info.FromRawValue(raw)));
            return true;
        }

        public override EnumValueError CreateError(string? value, string key) =>
            info.CreateError(value, EnumReadMode.Input, key);

        private bool TryAdd(string token, List<string> output)
        {
            if (!info.TryParse(token, EnumReadMode.Input, conventions, out TEnum value, out _))
                return false;
            output.Add(ToBinderText(value));
            return true;
        }

        // A combination of flags is "Read, Write", which a header or form collection binder would split; its number is not.
        private static string ToBinderText(TEnum value)
        {
            string name = value.ToString();
            return name.Contains(',', StringComparison.Ordinal) ? value.ToString("D") : name;
        }
    }
}
