using CSharpEssentials.Entity;
using CSharpEssentials.Enums;
using Examples.EntityFrameworkCore.Data.Events;

namespace Examples.EntityFrameworkCore.Data;

public sealed class Product : SoftDeletableEntityBase<Guid>
{
    private Product() { }

    public string Name { get; private set; } = "";
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }

    public static Product Create(string name, decimal price, ProductCategory category)
    {
        Product product = new() { Id = Guid.NewGuid(), Name = name, Price = price, Category = category };
        product.Raise(new ProductCreatedEvent(name));
        return product;
    }

    public void ChangePrice(decimal price)
    {
        Raise(new ProductPriceChangedEvent(Id, Price, price));
        Price = price;
    }
}

[StringEnum]
public enum ProductCategory
{
    Electronics,
    HomeAndGarden,
    Books,
}
