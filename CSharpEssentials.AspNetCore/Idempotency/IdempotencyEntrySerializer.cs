using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Binary format of the <see cref="IdempotentResponse"/> stored as the payload of an idempotency record: version, status
/// code, headers and body. Payloads of another version (the version 2 entries written before the record store existed)
/// or truncated and corrupt payloads are unreadable.
/// </summary>
internal static class IdempotencyEntrySerializer
{
    public const byte Version = 3;

    public static byte[] Write(IdempotentResponse response)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Version);
            writer.Write(response.StatusCode);
            writer.Write(response.Headers.Count);
            foreach (KeyValuePair<string, string[]> header in response.Headers)
            {
                writer.Write(header.Key);
                writer.Write(header.Value.Length);
                foreach (string value in header.Value)
                    writer.Write(value);
            }
        }

        int head = (int)stream.Length;
        byte[] entry = new byte[head + sizeof(int) + response.Body.Length];
        stream.GetBuffer().AsSpan(0, head).CopyTo(entry);
        BinaryPrimitives.WriteInt32LittleEndian(entry.AsSpan(head), response.Body.Length);
        response.Body.Span.CopyTo(entry.AsSpan(head + sizeof(int)));
        return entry;
    }

    public static bool TryRead(ReadOnlyMemory<byte> payload, [NotNullWhen(true)] out IdempotentResponse? response)
    {
        response = null;
        try
        {
            MemoryStream stream = MemoryMarshal.TryGetArray(payload, out ArraySegment<byte> segment)
                ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
                : new MemoryStream(payload.ToArray(), writable: false);
            using BinaryReader reader = new(stream, Encoding.UTF8);
            if (reader.ReadByte() != Version)
                return false;

            int statusCode = reader.ReadInt32();
            int headerCount = ReadCount(reader);
            Dictionary<string, string[]> headers = new(headerCount, StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < headerCount; index++)
            {
                string name = reader.ReadString();
                string[] values = new string[ReadCount(reader)];
                for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
                    values[valueIndex] = reader.ReadString();
                headers[name] = values;
            }

            int bodyLength = ReadCount(reader);
            if (bodyLength != reader.BaseStream.Length - reader.BaseStream.Position)
                return false;

            response = new IdempotentResponse(statusCode, headers, payload[(int)stream.Position..]);
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException or EndOfStreamException or FormatException
                                              or ArgumentException or OverflowException)
        {
            response = null;
            return false;
        }
    }

    /// <summary>A count can never exceed the bytes left, so a corrupt length never causes a huge allocation.</summary>
    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        Stream stream = reader.BaseStream;
        if (count < 0 || count > stream.Length - stream.Position)
            throw new InvalidDataException("Invalid idempotency entry.");
        return count;
    }
}
