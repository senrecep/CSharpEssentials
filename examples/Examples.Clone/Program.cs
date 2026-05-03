using CSharpEssentials.Clone;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Clone Example");
Console.WriteLine("========================================\n");

// ============================================================================
// ICLONEABLE INTERFACE
// ============================================================================
Console.WriteLine("--- ICloneable<T> ---");

Person original = new()
{
    Name = "Alice",
    Age = 30,
    Address = new Address { Street = "123 Main St", City = "New York" }
};

Person clone = original.Clone();

Console.WriteLine($"Original: {original.Name}, {original.Age}, {original.Address.City}");
Console.WriteLine($"Clone: {clone.Name}, {clone.Age}, {clone.Address.City}");
Console.WriteLine($"Are same reference? {ReferenceEquals(original, clone)}");
Console.WriteLine($"Are addresses same reference? {ReferenceEquals(original.Address, clone.Address)}");

// Modify clone to verify deep copy
clone.Name = "Bob";
clone.Address.City = "Los Angeles";

Console.WriteLine($"\nAfter modifying clone:");
Console.WriteLine($"Original: {original.Name}, {original.Address.City}");
Console.WriteLine($"Clone: {clone.Name}, {clone.Address.City}");
Console.WriteLine();

// ============================================================================
// EXTENSION METHOD
// ============================================================================
Console.WriteLine("--- Clone Extension ---");

Product product = new() { Id = 1, Name = "Laptop", Price = 999.99m };
Product productClone = product.Clone();

Console.WriteLine($"Original: {product.Name} (${product.Price})");
Console.WriteLine($"Clone: {productClone.Name} (${productClone.Price})");
Console.WriteLine($"Same reference? {ReferenceEquals(product, productClone)}");
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

// ============================================================================
// MODELS
// ============================================================================

public class Person : ICloneable<Person>
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address Address { get; set; } = new();

    public Person Clone()
    {
        return new Person
        {
            Name = Name,
            Age = Age,
            Address = Address.Clone()
        };
    }
}

public class Address : ICloneable<Address>
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public Address Clone()
    {
        return new Address { Street = Street, City = City };
    }
}

public class Product : ICloneable<Product>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Product Clone()
    {
        return new Product { Id = Id, Name = Name, Price = Price };
    }
}
