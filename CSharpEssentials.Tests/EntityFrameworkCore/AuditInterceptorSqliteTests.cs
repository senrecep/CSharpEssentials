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
        public List<Shipment> Shipments { get; set; } = [];
        public List<Allocation> Allocations { get; set; } = [];
    }

    private sealed class Warehouse
    {
        public int Id { get; set; }
        public List<Allocation> Allocations { get; set; } = [];
    }

    private sealed class Allocation
    {
        public int Id { get; set; }
    }

    private sealed class Shipment : SoftDeletableEntityBase
    {
        public int Id { get; set; }
        public string Carrier { get; set; } = string.Empty;
    }

    private sealed class OrderLine
    {
        public int Id { get; set; }
        public string Product { get; set; } = string.Empty;
    }

    private sealed class Folder : SoftDeletableEntityBase
    {
        public byte[] Id { get; set; } = [];
        public List<FolderItem> Items { get; set; } = [];
    }

    private sealed class FolderItem
    {
        public int Id { get; set; }
    }

    private sealed class OrderContext(SqliteConnection connection, AuditInterceptor interceptor) : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> Lines => Set<OrderLine>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<Folder> Folders => Set<Folder>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Allocation> Allocations => Set<Allocation>();
        public DbSet<FolderItem> FolderItems => Set<FolderItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite(connection).AddInterceptors(interceptor);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(order =>
            {
                order.Ignore(x => x.DomainEvents);
                order.OwnsOne(x => x.ShippingAddress);
                order.HasMany(x => x.Lines).WithOne().HasForeignKey("OrderId").IsRequired().OnDelete(DeleteBehavior.Cascade);
                order.HasMany(x => x.Allocations).WithOne().HasForeignKey("OrderId").IsRequired().OnDelete(DeleteBehavior.Cascade);
                order.HasMany(x => x.Shipments).WithOne().HasForeignKey("OrderId").IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Shipment>().Ignore(x => x.DomainEvents);
            modelBuilder.Entity<Warehouse>()
                .HasMany(x => x.Allocations).WithOne().HasForeignKey("WarehouseId").IsRequired().OnDelete(DeleteBehavior.ClientCascade);
            modelBuilder.Entity<Folder>(folder =>
            {
                folder.Ignore(x => x.DomainEvents);
                folder.HasMany(x => x.Items).WithOne().HasForeignKey("FolderId").IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
        }
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
        Allocation allocation = new() { Id = 50 };
        context.Warehouses.Add(new Warehouse { Id = 40, Allocations = [allocation] });
        context.Orders.Add(new Order
        {
            Allocations = [allocation],
            Id = 1,
            Number = "A-1",
            ShippingAddress = new Address { Street = "Main St 1", City = "Springfield" },
            Lines = [new OrderLine { Id = 10, Product = "Apple" }, new OrderLine { Id = 11, Product = "Pear" }],
            Shipments = [new Shipment { Id = 20, Carrier = "Post" }]
        });
        context.Folders.Add(new Folder { Id = [1, 2, 3], Items = [new FolderItem { Id = 30 }] });
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

    [Fact]
    public void SoftDelete_WithHardDeletedSoftDeletableDependent_ShouldDeleteDependent()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Lines).Include(x => x.Shipments).Single();
            order.Shipments[0].MarkAsHardDeleted();
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        AssertOrderKept();
        using OrderContext verify = CreateContext();
        verify.Shipments.AsNoTracking().IgnoreQueryFilters().Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_WithBinaryKey_ShouldKeepCascadedDependents()
    {
        using (OrderContext context = CreateContext())
        {
            Folder folder = context.Folders.Include(x => x.Items).Single();
            context.Folders.Remove(folder);
            context.SaveChanges();
        }

        using OrderContext verify = CreateContext();
        verify.Folders.AsNoTracking().Single().IsDeleted.Should().BeTrue();
        verify.FolderItems.AsNoTracking().Select(i => i.Id).Should().Equal(30);
    }

    [Fact]
    public void SoftDelete_WithDependentOfAnotherDeletedPrincipal_ShouldDeleteDependent()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Lines).Include(x => x.Allocations).Single();
            Warehouse warehouse = context.Warehouses.Single();
            context.Orders.Remove(order);
            context.Warehouses.Remove(warehouse);
            context.SaveChanges();
        }

        AssertOrderKept();
        using OrderContext verify = CreateContext();
        verify.Warehouses.AsNoTracking().Should().BeEmpty();
        verify.Allocations.AsNoTracking().Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_WithSoftDeletableDependent_ShouldSoftDeleteDependent()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Shipments).Single();
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        AssertOrderKept();
        using OrderContext verify = CreateContext();
        Shipment shipment = verify.Shipments.AsNoTracking().IgnoreQueryFilters().Single();
        shipment.IsDeleted.Should().BeTrue();
        shipment.DeletedBy.Should().Be("auditor");
    }

    [Fact]
    public void SoftDelete_WithDependentRemovedInSameSave_ShouldKeepDependent()
    {
        using (OrderContext context = CreateContext())
        {
            Order order = context.Orders.Include(x => x.Lines).Single();
            context.Lines.Remove(order.Lines[0]);
            context.Orders.Remove(order);
            context.SaveChanges();
        }

        AssertOrderKept();
    }
}
