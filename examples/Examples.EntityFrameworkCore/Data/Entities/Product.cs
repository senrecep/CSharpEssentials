using CSharpEssentials.Entity;
using CSharpEssentials.Enums;
using System.ComponentModel.DataAnnotations;

namespace Examples.EntityFrameworkCore.Data;

/// <summary>
/// Product entity demonstrating SoftDeletableEntityBase.
/// When deleted, the row remains in the database but IsDeleted = true,
/// and it is automatically filtered out of normal queries.
/// </summary>
public class Product : SoftDeletableEntityBase<Guid>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    public ProductCategory Category { get; set; }
}

/// <summary>
/// Enum demonstrating storage as snake_case string via ConfigureEnumConventions.
/// The [StringEnum] attribute ensures enums are stored as readable strings in the database.
/// </summary>
[StringEnum]
public enum ProductCategory
{
    Electronics,
    Clothing,
    Food,
    Books,
    Home
}
