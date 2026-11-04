using System.ComponentModel;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Enum 21 of the sample API: six defined, contiguous members.</summary>
[StringEnum]
[Description("Enum 21 state.")]
public enum Enum21
{
    Alpha = 0,
    [Description("Second value.")]
    Beta = 1,
    Gamma = 2,
    Delta = 3,
    Epsilon = 4,
    Zeta = 5,
}
