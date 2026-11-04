namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>Counts the log events a host rendered and their characters; keeps the last message when <see cref="Capture"/> is on (parity only).</summary>
public sealed class LogTally
{
    private int _events;
    private long _characters;

    /// <summary>The number of events so far.</summary>
    public int Events => Volatile.Read(ref _events);

    /// <summary>The characters of every rendered message so far.</summary>
    public long Characters => Interlocked.Read(ref _characters);

    /// <summary>Keep the last message; off while measuring.</summary>
    public bool Capture { get; set; }

    /// <summary>The last message, when <see cref="Capture"/> was on.</summary>
    public string? LastMessage { get; private set; }

    /// <summary>Records one rendered message.</summary>
    /// <param name="message">The rendered message.</param>
    public void Record(string message)
    {
        Interlocked.Increment(ref _events);
        Interlocked.Add(ref _characters, message.Length);
        if (Capture)
            LastMessage = message;
    }
}
