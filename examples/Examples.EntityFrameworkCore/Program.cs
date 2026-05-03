using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using Examples.EntityFrameworkCore.Data;
using Examples.EntityFrameworkCore.Data.Events;
using Examples.EntityFrameworkCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.EntityFrameworkCore Demo");
Console.WriteLine("========================================\n");

// ============================================================================
// SERVICE CONFIGURATION
// ============================================================================

var services = new ServiceCollection();

// Add logging so we can see SQL queries and interceptor output
services.AddLogging(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Information));

// Register audit interceptor with a simple user ID factory
services.AddAuditInterceptor(() => "demo-user");

// Register slow query interceptor with 500ms threshold
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));

// Register domain event publisher for console output
services.AddSingleton<IDomainEventPublisher, ConsoleDomainEventPublisher>();
services.AddSingleton<DomainEventInterceptor>();

// Add SQLite with CSharpEssentials BaseDbContext + Interceptors
services.AddDbContext<ShopDbContext>((sp, options) =>
    options.UseSqlite("Data Source=shop.db")
           .EnableSensitiveDataLogging()
           .AddInterceptors(
               sp.GetRequiredService<AuditInterceptor>(),
               sp.GetRequiredService<DomainEventInterceptor>(),
               sp.GetRequiredService<SlowQueryInterceptor>()));

// Application services
services.AddScoped<IProductCatalogService, ProductCatalogService>();

var provider = services.BuildServiceProvider();

// ============================================================================
// DATABASE INITIALIZATION
// ============================================================================

using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    Console.WriteLine("Ensuring database is created...");
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    Console.WriteLine("Database ready.\n");

    if (!db.Products.IgnoreQueryFilters().Any())
    {
        Console.WriteLine("Seeding initial data...");
        db.Products.AddRange(
            new() { Name = "Wireless Mouse", Price = 29.99m, Category = ProductCategory.Electronics },
            new() { Name = "Running Shoes", Price = 89.50m, Category = ProductCategory.Clothing },
            new() { Name = "Coffee Beans", Price = 14.99m, Category = ProductCategory.Food }
        );
        db.SaveChanges();
        Console.WriteLine("Seed complete.\n");
    }
}

// ============================================================================
// DEMO: SOFT DELETE
// ============================================================================

using (var scope = provider.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IProductCatalogService>();
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    Console.WriteLine("--- DEMO: Soft Delete ---");
    var mouse = db.Products.First(p => p.Name == "Wireless Mouse");
    Console.WriteLine($"Deleting product: {mouse.Name} (Id: {mouse.Id})");

    service.DeleteProduct(mouse.Id);

    var allProducts = db.Products.IgnoreQueryFilters().ToList();
    var visibleProducts = db.Products.ToList();

    Console.WriteLine($"Visible products: {visibleProducts.Count}");
    Console.WriteLine($"Total products (with deleted): {allProducts.Count}");
    Console.WriteLine($"Deleted product IsDeleted: {allProducts.First(p => p.Name == "Wireless Mouse").IsDeleted}");
    Console.WriteLine();
}

// ============================================================================
// DEMO: PAGINATION (OFFSET)
// ============================================================================

using (var scope = provider.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IProductCatalogService>();

    Console.WriteLine("--- DEMO: Offset Pagination ---");

    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    for (int i = 1; i <= 25; i++)
    {
        db.Products.Add(new()
        {
            Name = $"Product {i:D3}",
            Price = i * 10m,
            Category = (ProductCategory)(i % 5)
        });
    }
    db.SaveChanges();

    var page1 = service.GetProducts(pageNumber: 1, pageSize: 5);
    Console.WriteLine($"Page 1: {page1.Items.Count} items, Total: {page1.TotalCount}, Pages: {page1.TotalPages}");
    foreach (var p in page1.Items)
        Console.WriteLine($"  - {p.Name} (${p.Price})");

    var page2 = service.GetProducts(pageNumber: 2, pageSize: 5);
    Console.WriteLine($"\nPage 2: {page2.Items.Count} items");
    foreach (var p in page2.Items)
        Console.WriteLine($"  - {p.Name} (${p.Price})");

    Console.WriteLine();
}

// ============================================================================
// DEMO: CURSOR PAGINATION
// ============================================================================

using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    Console.WriteLine("--- DEMO: Cursor Pagination ---");

    // Use Name (string) as cursor — SQLite does not support DateTimeOffset
    // or decimal in ORDER BY clauses.
    var request = new CursorPaginationRequest<string>
    {
        Limit = 3,
        Cursor = string.Empty
    };

    var response = await db.Products
        .PaginateAsync(request, p => p.Name);

    Console.WriteLine($"Cursor Page: {response.Items.Count} items, HasMore={response.HasMore}");
    foreach (var p in response.Items)
        Console.WriteLine($"  - {p.Name} (${p.Price})");

    if (response.HasMore)
    {
        var nextRequest = new CursorPaginationRequest<string>
        {
            Limit = 3,
            Cursor = response.Next
        };

        var nextResponse = await db.Products
            .PaginateAsync(nextRequest, p => p.Name);

        Console.WriteLine($"\nNext Cursor Page: {nextResponse.Items.Count} items, HasMore={nextResponse.HasMore}");
        foreach (var p in nextResponse.Items)
            Console.WriteLine($"  - {p.Name} (${p.Price})");
    }

    Console.WriteLine();
}

// ============================================================================
// DEMO: AUDIT INTERCEPTOR
// ============================================================================

using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    Console.WriteLine("--- DEMO: Audit Fields ---");

    var product = db.Products.First(p => p.Name == "Coffee Beans");
    Console.WriteLine($"Product created at: {product.CreatedAt:O}");
    Console.WriteLine($"Product created by: {product.CreatedBy}");
    Console.WriteLine($"Product last modified: {product.UpdatedAt?.ToString("O") ?? "(never)"}");

    product.Price = 16.99m;
    db.SaveChanges();

    db.Entry(product).Reload();
    Console.WriteLine($"After update — Last modified: {product.UpdatedAt:O}");
    Console.WriteLine($"After update — Last modified by: {product.UpdatedBy}");
    Console.WriteLine();
}

// ============================================================================
// DEMO: DOMAIN EVENTS
// ============================================================================

using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    Console.WriteLine("--- DEMO: Domain Events ---");

    // Create a product and raise a BeforeSave event
    var newProduct = new Product
    {
        Name = "Bluetooth Speaker",
        Price = 49.99m,
        Category = ProductCategory.Electronics
    };
    newProduct.Raise(new ProductCreatedEvent(newProduct.Name));
    db.Products.Add(newProduct);

    Console.WriteLine("Saving new product (triggers BeforeSave event)...");
    db.SaveChanges();

    // Update price and raise an AfterSave event
    decimal oldPrice = newProduct.Price;
    newProduct.Price = 39.99m;
    newProduct.Raise(new ProductPriceChangedEvent(newProduct.Id, oldPrice, newProduct.Price));

    Console.WriteLine("Updating price (triggers AfterSave event)...");
    db.SaveChanges();

    // Multiple events on multiple entities
    var speaker = db.Products.First(p => p.Name == "Bluetooth Speaker");
    var shoes = db.Products.First(p => p.Name == "Running Shoes");

    speaker.Price = 34.99m;
    speaker.Raise(new ProductPriceChangedEvent(speaker.Id, 39.99m, 34.99m));

    shoes.Price = 79.99m;
    shoes.Raise(new ProductPriceChangedEvent(shoes.Id, 89.50m, 79.99m));

    Console.WriteLine("Bulk update (events from multiple entities, processed in order)...");
    db.SaveChanges();

    Console.WriteLine();
}

// ============================================================================
// DEMO: ENUM TO STRING CONVERTER
// ============================================================================

using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();

    Console.WriteLine("--- DEMO: EnumToFormattedStringConverter ---");

    var electronicProduct = db.Products.First(p => p.Category == ProductCategory.Electronics);
    Console.WriteLine($"Product: {electronicProduct.Name}");
    Console.WriteLine($"Category (enum stored as string in DB): {electronicProduct.Category}");
    Console.WriteLine($"Category name: {electronicProduct.Category.ToString()}");
    Console.WriteLine();
}

Console.WriteLine("========================================");
Console.WriteLine("Demo complete. Press any key to exit.");
Console.WriteLine("========================================");
Console.ReadKey();

