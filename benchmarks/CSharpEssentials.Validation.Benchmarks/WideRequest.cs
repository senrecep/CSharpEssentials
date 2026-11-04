using System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>Twelve required strings of at most 100 characters.</summary>
#if NET10_0_OR_GREATER
[Microsoft.Extensions.Validation.ValidatableType]
#endif
public sealed class WideRequest
{
    [Required, MaxLength(100)]
    public string? F1 { get; set; }

    [Required, MaxLength(100)]
    public string? F2 { get; set; }

    [Required, MaxLength(100)]
    public string? F3 { get; set; }

    [Required, MaxLength(100)]
    public string? F4 { get; set; }

    [Required, MaxLength(100)]
    public string? F5 { get; set; }

    [Required, MaxLength(100)]
    public string? F6 { get; set; }

    [Required, MaxLength(100)]
    public string? F7 { get; set; }

    [Required, MaxLength(100)]
    public string? F8 { get; set; }

    [Required, MaxLength(100)]
    public string? F9 { get; set; }

    [Required, MaxLength(100)]
    public string? F10 { get; set; }

    [Required, MaxLength(100)]
    public string? F11 { get; set; }

    [Required, MaxLength(100)]
    public string? F12 { get; set; }
}
