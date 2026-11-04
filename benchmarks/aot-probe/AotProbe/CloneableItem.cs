using CSharpEssentials.Clone;

namespace AotProbe;

internal sealed class CloneableItem(int value) : ICloneable<CloneableItem>
{
    public int Value { get; } = value;

    public CloneableItem Clone() => new(Value);
}
