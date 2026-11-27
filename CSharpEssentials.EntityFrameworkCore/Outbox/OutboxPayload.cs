namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>A serialized event: the stable <paramref name="Type"/> name and the <paramref name="Content"/>.</summary>
/// <param name="Type">The type name stored in <see cref="OutboxMessage.Type"/>.</param>
/// <param name="Content">The text stored in <see cref="OutboxMessage.Content"/>.</param>
public readonly record struct OutboxPayload(string Type, string Content);
