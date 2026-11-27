namespace CSharpEssentials.Core;

/// <summary>
/// The namespace IDs that RFC 9562 section 6.6 defines for name-based GUIDs (<see cref="Guider.NewGuidV5(Guid, string)"/>).
/// </summary>
public static class GuidNamespaces
{
    /// <summary>Names are fully qualified domain names: <c>6ba7b810-9dad-11d1-80b4-00c04fd430c8</c>.</summary>
    public static Guid Dns { get; } = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>Names are URLs: <c>6ba7b811-9dad-11d1-80b4-00c04fd430c8</c>.</summary>
    public static Guid Url { get; } = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>Names are ISO object identifiers: <c>6ba7b812-9dad-11d1-80b4-00c04fd430c8</c>.</summary>
    public static Guid Oid { get; } = new("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>Names are X.500 distinguished names: <c>6ba7b814-9dad-11d1-80b4-00c04fd430c8</c>.</summary>
    public static Guid X500 { get; } = new("6ba7b814-9dad-11d1-80b4-00c04fd430c8");
}
