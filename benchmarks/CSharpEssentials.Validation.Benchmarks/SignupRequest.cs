using System.ComponentModel.DataAnnotations;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>Four-property sign-up model; the DataAnnotations attributes mirror the CSE and FluentValidation rules.</summary>
#if NET10_0_OR_GREATER
[Microsoft.Extensions.Validation.ValidatableType]
#endif
public sealed class SignupRequest
{
    [Required, MaxLength(100)]
    public string? Name { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [Range(1, 149)]
    public int Age { get; set; }

    [Required, MinLength(1)]
    public List<string>? Tags { get; set; }
}
