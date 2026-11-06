using System.ComponentModel;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Enum 28 of the sample API: six defined, contiguous members.</summary>
[StringEnum]
[Description("Enum 28 state.")]
public enum Enum28
{
    Alpha = 0,
    [Description("Second value.")]
    Beta = 1,
    Gamma = 2,
    Delta = 3,
    Epsilon = 4,
    Zeta = 5,
}
