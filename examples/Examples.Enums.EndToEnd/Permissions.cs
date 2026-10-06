using CSharpEssentials.Enums;

namespace Examples.Enums.EndToEnd;

[StringEnum]
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}
