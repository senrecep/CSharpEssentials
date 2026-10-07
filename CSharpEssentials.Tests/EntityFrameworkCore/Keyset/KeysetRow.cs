namespace CSharpEssentials.Tests.EntityFrameworkCore.Keyset;

public sealed class KeysetRow
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid Token { get; set; }
    public long Sequence { get; set; }
    public decimal Price { get; set; }
    public double Score { get; set; }
    public DateTimeOffset At { get; set; }
    public KeysetRowStatus Status { get; set; }
    public DateOnly Day { get; set; }
    public TimeOnly Time { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Note { get; set; }
    public int? Rank { get; set; }
    public bool Active { get; set; }
}
