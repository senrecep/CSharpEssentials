namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>How <see cref="OutboxModelBuilderExtensions.AddOutboxMessages"/> maps <see cref="OutboxMessage"/>.</summary>
public sealed class OutboxMessageModelOptions
{
    /// <summary>The default <see cref="TableName"/>.</summary>
    public const string DefaultTableName = "OutboxMessages";

    /// <summary>The maximum length of <see cref="OutboxMessage.Type"/>.</summary>
    public const int TypeMaxLength = 256;

    /// <summary>The table of the messages. Default: <see cref="DefaultTableName"/>.</summary>
    public string TableName { get; set; } = DefaultTableName;

    /// <summary>The schema of the table; <see langword="null"/> (the default) uses the model's default schema.</summary>
    public string? Schema { get; set; }
}
