using System.Security.Cryptography;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// The payment provider signs every webhook body with HMAC-SHA256 over a secret shared with the merchant. Verify the
/// signature before the event id is trusted as an idempotency key: an unsigned request could otherwise claim the id
/// of a real event and make the real delivery a duplicate.
/// </summary>
public static class WebhookSignature
{
    public const string HeaderName = "X-Provider-Signature";

    public static string Compute(byte[] secret, ReadOnlySpan<byte> body) =>
        Convert.ToHexStringLower(HMACSHA256.HashData(secret, body));

    /// <summary>
    /// Decodes the hex signature (in either case) and compares the raw HMAC bytes in constant time. A signature that is
    /// missing or not hex is invalid.
    /// </summary>
    public static bool IsValid(byte[] secret, ReadOnlySpan<byte> body, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
            return false;

        byte[] received;
        try
        {
            received = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(HMACSHA256.HashData(secret, body), received);
    }
}
