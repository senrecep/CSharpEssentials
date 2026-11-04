using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using CSharpEssentials.Idempotency;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// Deterministic keys. For: a canonical key under 255 characters against one over it (SHA-256 hashed). Uuid: CSE's
/// UUIDv5 against RFC 9562 section 5.5 by hand with <c>SHA1.HashData</c> and stackalloc (labelled: the hand-written
/// row receives the canonical name prepared in setup, CSE builds it per call). StorageKey: the flat storage key of a
/// global key and of a user-and-tenant scope.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class IdempotencyKeyBenchmarks
{
    private const string For = "For";
    private const string Uuid = "Uuid";
    private const string StorageKey = "StorageKey";

    private static readonly string LongId = Payloads.Text(300, Payloads.ReplaySeed);
    private static readonly IdempotencyRecordKey GlobalKey = new("http", IdempotencyScope.Global, "key-1");

    private string _canonical = null!;

    [GlobalSetup]
    public void Setup()
    {
        _canonical = IdempotencyKeys.For("order", "42");
        Parity.True(For, _canonical.Length < IdempotencyKeys.MaxKeyLength && CSharpEssentials_For_Long().Length <= IdempotencyKeys.MaxKeyLength, "the short key was hashed or the long key was not");
        Parity.Same(Uuid, ("expected", HandWritten_UuidV5().ToString()), (nameof(CSharpEssentials_ToUuidV5), CSharpEssentials_ToUuidV5().ToString()));
        Parity.True(StorageKey, CSharpEssentials_ToStorageKey().Length > 0 && CSharpEssentials_Scope_UserAndTenant().Length > 0, "empty storage key");
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#deterministic-keys
    [Benchmark(Baseline = true), BenchmarkCategory(For)]
    public string CSharpEssentials_For_Short() => IdempotencyKeys.For("order", "42");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#deterministic-keys
    [Benchmark, BenchmarkCategory(For)]
    public string CSharpEssentials_For_Long() => IdempotencyKeys.For("order", LongId);

    // https://www.rfc-editor.org/rfc/rfc9562#section-5.5
    [Benchmark(Baseline = true), BenchmarkCategory(Uuid)]
    [SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "RFC 9562 defines UUIDv5 as SHA-1; the hash names a key and protects nothing.")]
    [SuppressMessage("Security Hotspot", "S4790:Using weak hashing algorithms is security-sensitive", Justification = "RFC 9562 defines UUIDv5 as SHA-1; the hash names a key and protects nothing.")]
    public Guid HandWritten_UuidV5()
    {
        int nameLength = Encoding.UTF8.GetByteCount(_canonical);
        Span<byte> input = nameLength <= 256 ? stackalloc byte[16 + nameLength] : new byte[16 + nameLength];
        IdempotencyKeys.UuidNamespace.TryWriteBytes(input, bigEndian: true, out _);
        Encoding.UTF8.GetBytes(_canonical, input[16..]);
        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(input, hash);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        return new Guid(hash[..16], bigEndian: true);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#deterministic-keys
    [Benchmark, BenchmarkCategory(Uuid)]
    public Guid CSharpEssentials_ToUuidV5() => IdempotencyKeys.ToUuidV5("order", "42");

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#use-the-store-directly
    [Benchmark(Baseline = true), BenchmarkCategory(StorageKey)]
    public string CSharpEssentials_ToStorageKey() => GlobalKey.ToStorageKey();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Idempotency/Readme.MD#scopes-and-machine-callers
    [Benchmark, BenchmarkCategory(StorageKey)]
    public string CSharpEssentials_Scope_UserAndTenant() =>
        new IdempotencyRecordKey("http", IdempotencyScope.UserAndTenant("user-1", "tenant-1"), "key-1").ToStorageKey();
}
