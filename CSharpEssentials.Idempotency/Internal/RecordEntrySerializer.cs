using System.Buffers.Binary;
using System.Text;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Binary format of a <see cref="DistributedCacheIdempotencyRecordStore"/> entry: version byte, token, fingerprint and,
/// once completed, the payload. An entry with another version byte, or one that is truncated or corrupt, is unreadable.
/// </summary>
internal static class RecordEntrySerializer
{
    public const byte Version = 3;

    public static byte[] Write(string token, string fingerprint, ReadOnlySpan<byte> payload, bool completed)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Version);
            writer.Write(token);
            writer.Write(fingerprint);
            writer.Write(completed);
        }

        if (!completed)
            return stream.ToArray();

        int head = (int)stream.Length;
        byte[] entry = new byte[head + sizeof(int) + payload.Length];
        stream.GetBuffer().AsSpan(0, head).CopyTo(entry);
        BinaryPrimitives.WriteInt32LittleEndian(entry.AsSpan(head), payload.Length);
        payload.CopyTo(entry.AsSpan(head + sizeof(int)));
        return entry;
    }

    public static bool TryRead(byte[] data, out string token, out string fingerprint, out ReadOnlyMemory<byte>? payload)
    {
        try
        {
            using BinaryReader reader = new(new MemoryStream(data, writable: false), Encoding.UTF8);
            if (reader.ReadByte() == Version)
            {
                token = reader.ReadString();
                fingerprint = reader.ReadString();
                payload = reader.ReadBoolean() ? ReadPayload(reader, data) : default(ReadOnlyMemory<byte>?);
                return true;
            }
        }
        catch (Exception exception) when (exception is InvalidDataException or EndOfStreamException or FormatException
                                              or ArgumentException or OverflowException)
        {
            token = string.Empty;
            fingerprint = string.Empty;
            payload = null;
            return false;
        }

        token = string.Empty;
        fingerprint = string.Empty;
        payload = null;
        return false;
    }

    /// <summary>The length can never exceed the bytes left, so a corrupt length never causes a huge allocation.</summary>
    private static ReadOnlyMemory<byte> ReadPayload(BinaryReader reader, byte[] data)
    {
        int length = reader.ReadInt32();
        Stream stream = reader.BaseStream;
        if (length < 0 || length != stream.Length - stream.Position)
            throw new InvalidDataException("Invalid idempotency entry.");
        return data.AsMemory((int)stream.Position, length);
    }
}
