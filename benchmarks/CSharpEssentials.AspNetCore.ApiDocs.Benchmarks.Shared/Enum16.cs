using System.ComponentModel;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>Enum 16 of the sample API: six defined, contiguous members.</summary>
[StringEnum]
[Description("Enum 16 state.")]
public enum Enum16
{
    Alpha = 0,
    [Description("Second value.")]
    Beta = 1,
    Gamma = 2,
    Delta = 3,
    Epsilon = 4,
    Zeta = 5,
}
