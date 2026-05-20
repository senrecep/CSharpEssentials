namespace CSharpEssentials.Validation.Benchmarks;

public sealed class User
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public List<string>? Tags { get; set; }
    public string? Password { get; set; }
}

public sealed class Address
{
    public string? City { get; set; }
    public string? ZipCode { get; set; }
}

public sealed class OrderItem
{
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public sealed class Order
{
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public int CustomerAge { get; set; }
    public bool IsBusiness { get; set; }
    public string? CompanyName { get; set; }
    public string? Password { get; set; }
    public Address? ShippingAddress { get; set; }
    public List<OrderItem>? Items { get; set; }
    public List<string>? Tags { get; set; }
}

// =========================================================================
// Wide model — 12 string fields, used in WideModel benchmarks
// =========================================================================

public sealed class WideModel
{
    public string? F1 { get; set; }
    public string? F2 { get; set; }
    public string? F3 { get; set; }
    public string? F4 { get; set; }
    public string? F5 { get; set; }
    public string? F6 { get; set; }
    public string? F7 { get; set; }
    public string? F8 { get; set; }
    public string? F9 { get; set; }
    public string? F10 { get; set; }
    public string? F11 { get; set; }
    public string? F12 { get; set; }
}

// =========================================================================
// Deep nested model — 3 levels: DeepOrder → DeepAddress → Street
// =========================================================================

public sealed class Street
{
    public string? Line1 { get; set; }
    public string? PostalCode { get; set; }
}

public sealed class DeepAddress
{
    public string? City { get; set; }
    public Street? Street { get; set; }
}

public sealed class DeepOrder
{
    public string? CustomerName { get; set; }
    public DeepAddress? ShippingAddress { get; set; }
}

// =========================================================================
// Product — used in nullable and equality benchmarks
// =========================================================================

public sealed class Product
{
    public string? Code { get; set; }
    public string? Status { get; set; }
    public int? Score { get; set; }
    public decimal? Price { get; set; }
}
