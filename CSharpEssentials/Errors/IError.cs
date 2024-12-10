namespace CSharpEssentials;

public interface IError
{
    string Code { get; }
    string Description { get; }
    ErrorType Type { get; }
    int NumericType { get; }
    ErrorMetadata? Metadata { get; }
}
