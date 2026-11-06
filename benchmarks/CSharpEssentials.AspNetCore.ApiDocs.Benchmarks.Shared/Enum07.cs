using System.ComponentModel;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Enum 07 of the sample API: six defined, contiguous members.</summary>
[StringEnum]
[Description("Enum 07 state.")]
public enum Enum07
{
    Alpha = 0,
    [Description("Second value.")]
    Beta = 1,
    Gamma = 2,
    Delta = 3,
    Epsilon = 4,
    Zeta = 5,
}
