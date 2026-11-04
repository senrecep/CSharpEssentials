using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// Deterministic pseudo-random <c>[a-z0-9]</c> text from a fixed seed. Never a repeated character: IdempotentAPI gzips
/// what it stores, so a padded payload would make its store and replay artificially cheap.
/// </summary>
internal static class Payloads
{
    public const int ReplaySeed = 1;
    public const int ArticleSeed = 2;
    public const int RequestLogSeed = 3;
    public const int ResponseLogSeed = 4;

    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "A fixed seed makes the benchmark payload reproducible; it protects nothing.")]
    public static string Text(int length, int seed)
    {
        Random random = new(seed);
        return string.Create(length, random, static (span, r) =>
        {
            for (int i = 0; i < span.Length; i++)
                span[i] = Alphabet[r.Next(Alphabet.Length)];
        });
    }

    public static int Overhead<T>(T empty) => JsonSerializer.SerializeToUtf8Bytes(empty, Web).Length;

    public static byte[] Json<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Web);

    public static byte[] DataEnvelope(int size, int seed) =>
        System.Text.Encoding.UTF8.GetBytes("{\"data\":\"" + Text(size - 11, seed) + "\"}");
}
