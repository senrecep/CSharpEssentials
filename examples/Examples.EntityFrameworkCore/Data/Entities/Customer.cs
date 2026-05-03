using CSharpEssentials.Entity;
using System.ComponentModel.DataAnnotations;

namespace Examples.EntityFrameworkCore.Data;

/// <summary>
/// Customer entity demonstrating EntityBase with audit fields.
/// CreatedAt and UpdatedAt are managed automatically by EntityBase.
/// </summary>
public class Customer : EntityBase<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }
}
