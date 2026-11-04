namespace CSharpEssentials.Json;

internal interface IPolymorphicPropertyConverter
{
    Type BaseType { get; }

    string TypeDiscriminatorPropertyName { get; }
}
