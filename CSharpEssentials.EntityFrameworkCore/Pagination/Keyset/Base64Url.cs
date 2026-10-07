namespace CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;

/// <summary>Base64url without padding (RFC 4648 §5); <c>System.Buffers.Text.Base64Url</c> is not available on net8.0.</summary>
internal static class Base64Url
{
    public static string Encode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static bool TryDecode(string value, out byte[] bytes)
    {
        bytes = [];
        if (value.Length == 0 || value.Length % 4 == 1)
            return false;

        char[] chars = new char[value.Length + (4 - value.Length % 4) % 4];
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c is '+' or '/' or '=')
                return false;
            chars[i] = c switch
            {
                '-' => '+',
                '_' => '/',
                _ => c,
            };
        }

        for (int i = value.Length; i < chars.Length; i++)
            chars[i] = '=';

        byte[] buffer = new byte[chars.Length / 4 * 3];
        if (!Convert.TryFromBase64Chars(chars, buffer, out int written))
            return false;

        bytes = buffer[..written];
        return true;
    }
}
