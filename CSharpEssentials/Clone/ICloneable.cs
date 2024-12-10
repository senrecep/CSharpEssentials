namespace CSharpEssentials.Clone;

public interface ICloneable<out T>
{
    T Clone();
}