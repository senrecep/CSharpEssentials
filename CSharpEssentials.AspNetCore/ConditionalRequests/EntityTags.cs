using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

internal static class EntityTags
{
    /// <summary>
    /// The strong ETag of a version: the version itself when it is a valid opaque tag (RFC 9110 <c>etagc</c> without
    /// <c>obs-text</c>: <c>%x21 / %x23-7E</c>), otherwise the base64url SHA-256 of its UTF-8 bytes. Empty: none.
    /// </summary>
    public static EntityTagHeaderValue? FromVersion(string? version)
    {
        if (string.IsNullOrEmpty(version))
            return null;
        string tag = IsOpaqueTag(version) ? version : Hash(Encoding.UTF8.GetBytes(version));
        return new EntityTagHeaderValue($"\"{tag}\"");
    }

    public static string Hash(byte[] bytes) => WebEncoders.Base64UrlEncode(SHA256.HashData(bytes));

    private static bool IsOpaqueTag(string value)
    {
        foreach (char c in value)
        {
            if (c is not ('!' or >= '#' and <= '~'))
                return false;
        }
        return true;
    }
}
