namespace CSharpEssentials.Enums;

/// <summary>
/// Marks the member that data reads map undefined values to (<see cref="UnknownEnumValueHandling.UseFallback"/>).
/// Requests never accept the fallback member.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class EnumFallbackAttribute : Attribute;
