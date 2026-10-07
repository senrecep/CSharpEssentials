using System.Text;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Binary format of a <see cref="DistributedCacheIdempotencyStore"/> entry: version, token, fingerprint and, once completed,
/// the response.
/// </summary>
internal static class IdempotencyEntrySerializer
{
    private const byte Version = 2;

    public static byte[] Write(string token, string fingerprint, IdempotentResponse? response)
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(Version);
            writer.Write(token);
            writer.Write(fingerprint);
            writer.Write(response is not null);
            if (response is not null)
            {
                writer.Write(response.StatusCode);
                writer.Write(response.Headers.Count);
                foreach (KeyValuePair<string, string[]> header in response.Headers)
                {
                    writer.Write(header.Key);
                    writer.Write(header.Value.Length);
                    foreach (string value in header.Value)
                        writer.Write(value);
                }

                writer.Write(response.Body.Length);
                writer.Write(response.Body.Span);
            }
        }

        return stream.ToArray();
    }

    public static bool TryRead(byte[] data, out string token, out string fingerprint, out IdempotentResponse? response)
    {
        token = string.Empty;
        fingerprint = string.Empty;
        response = null;
        try
        {
            using BinaryReader reader = new(new MemoryStream(data, writable: false), Encoding.UTF8);
            if (reader.ReadByte() != Version)
                return false;

            token = reader.ReadString();
            fingerprint = reader.ReadString();
            if (!reader.ReadBoolean())
                return true;

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
            byte[] body = reader.ReadBytes(bodyLength);
            if (body.Length != bodyLength)
                return false;

            response = new IdempotentResponse(statusCode, headers, body);
            return true;
        }
        catch (InvalidDataException)
        {
            token = string.Empty;
            fingerprint = string.Empty;
            response = null;
            return false;
        }
        catch (Exception exception) when (exception is EndOfStreamException or FormatException or ArgumentException or OverflowException)
        {
            token = string.Empty;
            fingerprint = string.Empty;
            response = null;
            return false;
        }
    }

    /// <summary>
    /// A count can never exceed the bytes left, so a corrupt length never causes a huge allocation.
    /// </summary>
    private static int ReadCount(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        Stream stream = reader.BaseStream;
        if (count < 0 || count > stream.Length - stream.Position)
            throw new InvalidDataException("Invalid idempotency entry.");
        return count;
    }
}
