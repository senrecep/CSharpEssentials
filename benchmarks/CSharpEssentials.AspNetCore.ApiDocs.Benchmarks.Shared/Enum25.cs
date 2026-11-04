using System.ComponentModel;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Enum 25 of the sample API: six defined, contiguous members.</summary>
[StringEnum]
[Description("Enum 25 state.")]
public enum Enum25
{
    Alpha = 0,
    [Description("Second value.")]
    Beta = 1,
    Gamma = 2,
    Delta = 3,
    Epsilon = 4,
    Zeta = 5,
}
