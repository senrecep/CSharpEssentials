namespace CSharpEssentials.Tests.Enums.Runtime;

/// <summary>Bits 2 and 4 have no single flag member, only the composite <see cref="WriteDelete"/> (what CSE0007 reports for [StringEnum] enums).</summary>
[Flags]
public enum ParserCompositeAccess
{
    None = 0,
    Read = 1,
    WriteDelete = 6,
}
