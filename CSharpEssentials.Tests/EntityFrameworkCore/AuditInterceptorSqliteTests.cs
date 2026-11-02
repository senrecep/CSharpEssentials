using CSharpEssentials.Entity;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class AuditInterceptorSqliteTests : IDisposable
{
    private sealed class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    private sealed class Order : SoftDeletableEntityBase
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public Address ShippingAddress { get; set; } = new();
        public List<OrderLine> Lines { get; set; } = [];
    }

    private sealed class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Product { get; set; } = string.Empty;
    }

    private sealed class OrderContext(SqliteConnection connection, AuditInterceptor interceptor) : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> Lines => Set<OrderLine>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite(connection).AddInterceptors(interceptor);

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Order>(order =>
            {
                order.Ignore(x => x.DomainEvents);
                order.OwnsOne(x => x.ShippingAddress);
                order.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
    }

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ServiceProvider _provider;

    public AuditInterceptorSqliteTests()
    {
        _connection.Open();
        ServiceCollection services = new();
        services.AddAuditInterceptor(() => "auditor");
        _provider = services.BuildServiceProvider();

        using OrderContext context = CreateContext();
        context.Database.EnsureCreated();
        context.Orders.Add(new Order
        {
            Id = 1,
            Number = "A-1",
            ShippingAddress = new Address { Street = "Main St 1", City = "Springfield" },
            Lines = [new OrderLine { Id = 10, Product = "Apple" }, new OrderLine { Id = 11, Product = "Pear" }]
        });
        context.SaveChanges();
    }

    private OrderContext CreateContext() => new(_connection, _provider.GetRequiredService<AuditInterceptor>());

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }

    private void AssertOrderKept()
    {
        using OrderContext verify = CreateContext();
        Order order = verify.Orders.AsNoTracking().Single();
        order.IsDeleted.Should().BeTrue();
        order.DeletedBy.Should().Be("auditor");
        order.ShippingAddress.Should().NotBeNull();
        order.ShippingAddress.Street.Should().Be("Main St 1");
        order.ShippingAddress.City.Should().Be("Springfield");
        verify.Lines.AsNoTracking().Select(l => l.Id).OrderBy(id => id).Should().Equal(10, 11);
    }

    [Fact]
    public void SoftDelete_WithTrackedCascadeDependentsAndOwnedType_ShouldKeepThem()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Lines).Single();
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        AssertOrderKept();
    }

    [Fact]
    public async Task SoftDeleteAsync_WithTrackedCascadeDependentsAndOwnedType_ShouldKeepThem()
    {
        await using (OrderContext context = CreateContext())
        {
            Order order = await context.Orders.Include(x => x.Lines).SingleAsync();
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
        }

        AssertOrderKept();
    }

    [Fact]
    public void SoftDelete_WithoutLoadedDependents_ShouldKeepThem()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Single();
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        AssertOrderKept();
    }

    [Fact]
    public void SoftDelete_ShouldKeepPendingEditsOnCascadedEntries()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Lines).Single();
            order.ShippingAddress.Street = "Second St 2";
            order.Lines[0].Product = "Banana";
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        using OrderContext verify = CreateContext();
        verify.Orders.AsNoTracking().Single().ShippingAddress.Street.Should().Be("Second St 2");
        verify.Lines.AsNoTracking().Single(l => l.Id == 10).Product.Should().Be("Banana");
    }

    [Fact]
    public void HardDelete_ShouldStillCascade()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Lines).Single();
            order.MarkAsHardDeleted();
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        using OrderContext verify = CreateContext();
        verify.Orders.AsNoTracking().Should().BeEmpty();
        verify.Lines.AsNoTracking().Should().BeEmpty();
    }
}
