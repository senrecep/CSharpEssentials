namespace CSharpEssentials.Enums;

/// <summary>
/// Reaches typed code from an <see cref="IEnumInfo"/> without <c>MakeGenericType</c>.
/// </summary>
/// <typeparam name="TResult">The result of the visit.</typeparam>
public interface IEnumInfoVisitor<out TResult>
{
    /// <summary>Called with the typed metadata.</summary>
    TResult Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum;
}
