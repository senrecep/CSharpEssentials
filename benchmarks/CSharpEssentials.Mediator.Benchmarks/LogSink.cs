namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Where <see cref="CountingLogger{T}"/> writes: a running character count, and the messages while <see cref="Capture"/> is on.</summary>
public static class LogSink
{
    /// <summary>The total length of every formatted message.</summary>
    public static long Characters { get; set; }

    /// <summary>Keeps the formatted messages in <see cref="Messages"/>; on in the self-check only.</summary>
    public static bool Capture { get; set; }

    /// <summary>The formatted messages since the self-check last cleared them.</summary>
    public static List<string> Messages { get; } = [];
}
