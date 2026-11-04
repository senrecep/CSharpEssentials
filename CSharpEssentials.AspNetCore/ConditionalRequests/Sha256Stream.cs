using System.Security.Cryptography;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A write-only stream that hashes what is written with SHA-256 instead of keeping it, so a value can be serialized
/// straight into its hash without buffering the whole payload.
/// </summary>
internal sealed class Sha256Stream : Stream
{
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public byte[] GetHash() => _hash.GetHashAndReset();

    public override void Write(byte[] buffer, int offset, int count) => _hash.AppendData(buffer, offset, count);
    public override void Write(ReadOnlySpan<byte> buffer) => _hash.AppendData(buffer);
    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _hash.Dispose();
        base.Dispose(disposing);
    }
}
