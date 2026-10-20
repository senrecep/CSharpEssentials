using System.Security.Cryptography;
using System.Text;

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

    public static bool IsValid(byte[] secret, ReadOnlySpan<byte> body, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(Compute(secret, body)),
            Encoding.ASCII.GetBytes(signature));
    }
}
