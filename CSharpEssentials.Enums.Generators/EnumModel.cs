namespace CSharpEssentials.Enums.Generators;

/// <summary>
/// Value-equatable model of one <c>[StringEnum]</c> enum.
/// </summary>
/// <param name="Namespace">The containing namespace, empty for the global namespace.</param>
/// <param name="ExtensionsClassName">The generated class name (<c>Order_StateExtensions</c> for a nested enum).</param>
/// <param name="MetadataName">The type name without namespace, nested types joined with <c>+</c> (<c>Order+State</c>); unique per enum.</param>
/// <param name="FullyQualifiedName">The <c>global::</c> qualified enum name.</param>
/// <param name="IsPublic">Whether the enum is accessible outside its assembly.</param>
/// <param name="UnderlyingType">The C# keyword of the underlying type.</param>
/// <param name="IsSigned">Whether the underlying type is signed.</param>
/// <param name="IsFlags">Whether the enum is marked <c>[Flags]</c>.</param>
/// <param name="Naming">The <c>StringEnumAttribute.Naming</c> value.</param>
/// <param name="Storage">The <c>StringEnumAttribute.Storage</c> value.</param>
/// <param name="Members">The members in declaration order.</param>
internal sealed record EnumModel(
    string Namespace,
    string ExtensionsClassName,
    string MetadataName,
    string FullyQualifiedName,
    bool IsPublic,
    string UnderlyingType,
    bool IsSigned,
    bool IsFlags,
    int Naming,
    int Storage,
    EquatableArray<EnumMemberModel> Members);
