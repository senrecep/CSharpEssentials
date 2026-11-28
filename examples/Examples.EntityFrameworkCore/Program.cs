using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Keyset;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;
using Examples.EntityFrameworkCore.Data;
using Examples.EntityFrameworkCore.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

await using SqliteConnection connection = new("DataSource=:memory:");
await connection.OpenAsync();

ServiceCollection services = new();
services.AddLogging();
services.AddAuditInterceptor(() => "demo-user");
services.AddSingleton<IDomainEventPublisher, ConsoleDomainEventPublisher>();
services.AddSingleton<DomainEventInterceptor>();
services.AddSlowQueryInterceptor(TimeSpan.FromMilliseconds(500));
services.AddDbContext<ShopDbContext>(options => options.UseSqlite(connection));
services.AddEfCoreTransactionRunner<ShopDbContext>();
services.AddDbErrorTranslation();
services.AddDbErrorTranslator<SqliteUniqueTranslator>();

await using ServiceProvider provider = services.BuildServiceProvider();

await using (AsyncServiceScope scope = provider.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<ShopDbContext>().Database.EnsureCreatedAsync();

await RunAsync("1. Audit fields and domain events", async (db, sp) =>
{
    db.Products.AddRange(
        Product.Create("Mouse", 25m, ProductCategory.Electronics),
        Product.Create("Lamp", 40m, ProductCategory.HomeAndGarden),
        Product.Create("Novel", 12m, ProductCategory.Books));
    await db.SaveChangesAsync();

    Product mouse = await db.Products.SingleAsync(p => p.Name == "Mouse");
    mouse.ChangePrice(20m);
    await db.SaveChangesAsync();
    Console.WriteLine($"   Mouse: created by {mouse.CreatedBy}, updated by {mouse.UpdatedBy}");
});

await RunAsync("2. Soft delete and the named filter", async (db, sp) =>
{
    Product lamp = await db.Products.SingleAsync(p => p.Name == "Lamp");
    db.Products.Remove(lamp);
    await db.SaveChangesAsync();

    int visible = await db.Products.CountAsync();
    int stored = await db.Products.IgnoreSoftDeleteQueryFilter().CountAsync();
    Console.WriteLine($"   Visible {visible}, stored {stored}; Lamp deleted by {lamp.DeletedBy}");

    lamp.Restore();
    await db.SaveChangesAsync();
    Console.WriteLine($"   After Restore: visible {await db.Products.CountAsync()}");

    int batch = await db.Products.Where(p => p.Category == ProductCategory.Books).SoftDeleteAsync(deletedBy: "batch-job");
    Console.WriteLine($"   SoftDeleteAsync updated {batch} row in one statement");
});

await RunAsync("3. Offset and keyset pagination", async (db, sp) =>
{
    for (int i = 1; i <= 7; i++)
        db.Products.Add(Product.Create($"Cable {i}", i, ProductCategory.Electronics));
    await db.SaveChangesAsync();

    PaginationResponse<Product> page = await db.Products.OrderBy(p => p.Name).PaginateAsync(pageNumber: 2, pageSize: 3);
    Console.WriteLine($"   Offset page {page.PageNumber}/{page.TotalPages}: {string.Join(", ", page.Items.Select(p => p.Name))}");

    KeysetOrdering<Product> byName = new KeysetOrdering<Product>().Ascending(p => p.Name).Ascending(p => p.Id);
    Result<KeysetPaginationResponse<Product>> first = await db.Products.KeysetPaginateAsync(new KeysetPaginationRequest { Limit = 3 }, byName);
    Result<KeysetPaginationResponse<Product>> second = await db.Products.KeysetPaginateAsync(
        new KeysetPaginationRequest { Limit = 3, After = first.Value.NextCursor }, byName);
    Console.WriteLine($"   Keyset page 2: {string.Join(", ", second.Value.Items.Select(p => p.Name))}");

    Result<KeysetPaginationResponse<Product>> bad = await db.Products.KeysetPaginateAsync(
        new KeysetPaginationRequest { After = "tampered" }, byName);
    Console.WriteLine($"   Bad cursor: {bad.FirstError.Code}");
});

await RunAsync("4. Result helpers and hard delete", async (db, sp) =>
{
    Result<Product> found = await db.Products.Where(p => p.Name == "Mouse").FirstOrDefaultAsResultAsync();
    Result<Product> missing = await db.Products.Where(p => p.Name == "Tablet")
        .FirstOrDefaultAsResultAsync(Error.NotFound("Product.NotFound", "No such product"));
    Console.WriteLine($"   Found {found.Value.Name}; missing: {missing.FirstError.Code}");

    Product cable = await db.Products.FirstAsync(p => p.Name == "Cable 7");
    db.HardDelete(cable);
    await db.SaveChangesAsync();
    Console.WriteLine($"   Cable 7 rows left: {await db.Products.IgnoreSoftDeleteQueryFilter().CountAsync(p => p.Name == "Cable 7")}");
});

await RunAsync("5. Enum and Maybe columns", async (db, sp) =>
{
    db.Customers.AddRange(Customer.Create("ada@example.com", Maybe<string>.From("ada")), Customer.Create("bob@example.com"));
    await db.SaveChangesAsync();

    string category = await db.Database
        .SqlQueryRaw<string>("SELECT Category AS Value FROM products WHERE Name = 'Lamp'").SingleAsync();
    List<string> noNickname = await db.Customers.Where(c => c.Nickname == null).Select(c => c.Email).ToListAsync();
    Console.WriteLine($"   Lamp category stored as '{category}'; customers without a nickname: {string.Join(", ", noNickname)}");
});

await RunAsync("6. Transaction runner and after-commit callbacks", async (db, sp) =>
{
    ITransactionRunner runner = sp.GetRequiredService<ITransactionRunner>();
    ITransactionCallbacks callbacks = sp.GetRequiredService<ITransactionCallbacks>();

    await runner.ExecuteAsync(async ct =>
    {
        db.Products.Add(Product.Create("Desk", 150m, ProductCategory.HomeAndGarden));
        await db.SaveChangesAsync(ct);
        await callbacks.OnCommittedAsync(_ => { Console.WriteLine("   after commit: Desk saved"); return ValueTask.CompletedTask; }, ct);
        return Result.Success();
    }, outcome => outcome.IsSuccess);

    await runner.ExecuteAsync(async ct =>
    {
        db.Products.Add(Product.Create("Chair", 80m, ProductCategory.HomeAndGarden));
        await db.SaveChangesAsync(ct);
        return Result.Failure(Error.Validation("Chair.OutOfStock", "Out of stock"));
    }, outcome => outcome.IsSuccess);

    db.ChangeTracker.Clear();
    Console.WriteLine($"   Desk stored: {await db.Products.AnyAsync(p => p.Name == "Desk")}, Chair stored: {await db.Products.AnyAsync(p => p.Name == "Chair")}");
});

await RunAsync("7. Database error translation", async (db, sp) =>
{
    DbErrorTranslation translation = sp.GetRequiredService<DbErrorTranslation>();
    db.Customers.Add(Customer.Create("ada@example.com"));
    Result<int> saved = await translation.SaveChangesAsync(db);
    Console.WriteLine($"   Duplicate email: {saved.FirstError.Type} {saved.FirstError.Code}");
});

async Task RunAsync(string title, Func<ShopDbContext, IServiceProvider, Task> step)
{
    Console.WriteLine(title);
    await using AsyncServiceScope scope = provider.CreateAsyncScope();
    await step(scope.ServiceProvider.GetRequiredService<ShopDbContext>(), scope.ServiceProvider);
}
