using System.ComponentModel.DataAnnotations.Schema;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Maybe;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

/// <summary><c>ConfigureNullableMaybeConventions</c> on complex types, owned types, inheritance and user configuration.</summary>
public sealed class NullableMaybeConventionSqliteTests
{
    private sealed class Address
    {
        public string City { get; set; } = "";
        public Maybe<string>? Street { get; set; }
        public Maybe<int>? Floor { get; set; }
    }

    private sealed class Contact
    {
        public Maybe<string>? Email { get; set; }
    }

    private sealed class Customer
    {
        public int Id { get; set; }
        public Address Address { get; set; } = new();
        public Contact Contact { get; set; } = new();
        public Maybe<int>? Custom { get; set; }

        [NotMapped]
        public Maybe<int>? Transient { get; set; }
    }

    private class Animal
    {
        public int Id { get; set; }
        public Maybe<int>? Legs { get; set; }
    }

    private sealed class Dog : Animal
    {
        public Maybe<string>? Breed { get; set; }
    }

    public sealed class NullableValueRow
    {
        public int Id { get; set; }
        public Maybe<int?>? Value { get; set; }
    }

    private static readonly ValueConverter<Maybe<int>, long?> CustomConverter = new(
        maybe => maybe.HasValue ? maybe.Value : null,
        value => value.HasValue ? Maybe<int>.From((int)value.Value) : Maybe<int>.None);

    private sealed class ShopContext(SqliteConnection connection) : DbContext
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Animal> Animals => Set<Animal>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connection);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureNullableMaybeConventions();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(customer =>
            {
                customer.ComplexProperty(c => c.Address);
                customer.OwnsOne(c => c.Contact);
                customer.Property(c => c.Custom).HasConversion(CustomConverter);
            });
            modelBuilder.Entity<Animal>().HasDiscriminator<string>("Kind").HasValue<Animal>("animal").HasValue<Dog>("dog");
        }
    }

    private sealed class NullableValueContext(SqliteConnection connection) : DbContext
    {
        public DbSet<NullableValueRow> Rows => Set<NullableValueRow>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite(connection);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureNullableMaybeConventions();
    }

    [Fact]
    public async Task ComplexProperty_Should_RoundTripMaybeColumns()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using (ShopContext writer = await CreateDatabaseAsync(connection))
        {
            writer.Customers.AddRange(
                new Customer { Id = 1, Address = new Address { City = "Ankara", Street = Maybe<string>.From("Atatürk"), Floor = Maybe<int>.From(0) } },
                new Customer { Id = 2, Address = new Address { City = "Izmir", Street = Maybe<string>.None } });
            await writer.SaveChangesAsync();
        }

        await using ShopContext reader = new(connection);
        Customer[] customers = await reader.Customers.OrderBy(c => c.Id).ToArrayAsync();

        customers[0].Address.Street.Should().Be(Maybe<string>.From("Atatürk"));
        customers[0].Address.Floor.Should().Be(Maybe<int>.From(0));
        customers[1].Address.Street.Should().BeNull();
        customers[1].Address.Floor.Should().BeNull();
    }

    [Fact]
    public async Task ComplexProperty_Should_MapMaybeColumnsAsNullable()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using ShopContext context = new(connection);

        IComplexType address = context.Model.FindEntityType(typeof(Customer))!
            .FindComplexProperty(nameof(Customer.Address))!.ComplexType;

        address.FindProperty(nameof(Address.Street))!.IsNullable.Should().BeTrue();
        address.FindProperty(nameof(Address.Floor))!.GetValueConverter()!.ProviderClrType.Should().Be<int?>();
    }

    [Fact]
    public async Task ComplexProperty_Should_TranslateNullFilter()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using ShopContext context = await CreateDatabaseAsync(connection);
        context.Customers.AddRange(
            new Customer { Id = 1, Address = new Address { Floor = Maybe<int>.From(3) } },
            new Customer { Id = 2, Address = new Address() });
        await context.SaveChangesAsync();

        int[] ids = await context.Customers.Where(c => c.Address.Floor == null).Select(c => c.Id).ToArrayAsync();

        ids.Should().Equal(2);
    }

    [Fact]
    public async Task OwnedType_Should_RoundTripMaybeColumn()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using (ShopContext writer = await CreateDatabaseAsync(connection))
        {
            writer.Customers.AddRange(
                new Customer { Id = 1, Contact = new Contact { Email = Maybe<string>.From("a@example.com") } },
                new Customer { Id = 2, Contact = new Contact { Email = Maybe<string>.None } });
            await writer.SaveChangesAsync();
        }

        await using ShopContext reader = new(connection);
        Customer[] customers = await reader.Customers.OrderBy(c => c.Id).ToArrayAsync();

        customers[0].Contact.Email.Should().Be(Maybe<string>.From("a@example.com"));
        customers[1].Contact.Email.Should().BeNull();
    }

    [Fact]
    public async Task Inheritance_Should_RoundTripBaseAndDerivedMaybeColumns()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using (ShopContext writer = await CreateDatabaseAsync(connection))
        {
            writer.Animals.AddRange(
                new Animal { Id = 1, Legs = Maybe<int>.From(8) },
                new Dog { Id = 2, Legs = Maybe<int>.From(4), Breed = Maybe<string>.From("Kangal") },
                new Dog { Id = 3 });
            await writer.SaveChangesAsync();
        }

        await using ShopContext reader = new(connection);
        Animal[] animals = await reader.Animals.OrderBy(a => a.Id).ToArrayAsync();
        int[] noBreed = await reader.Animals.OfType<Dog>().Where(d => d.Breed == null).Select(d => d.Id).ToArrayAsync();

        animals[0].Legs.Should().Be(Maybe<int>.From(8));
        animals[1].Should().BeOfType<Dog>().Which.Breed.Should().Be(Maybe<string>.From("Kangal"));
        animals[2].Legs.Should().BeNull();
        noBreed.Should().Equal(3);
    }

    [Fact]
    public async Task NotMapped_Should_NotBeMapped()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using ShopContext context = new(connection);

        IEntityType customer = context.Model.FindEntityType(typeof(Customer))!;

        customer.FindProperty(nameof(Customer.Transient)).Should().BeNull();
    }

    [Fact]
    public async Task UserConversion_Should_WinOverConvention()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using ShopContext context = new(connection);

        IProperty custom = context.Model.FindEntityType(typeof(Customer))!.FindProperty(nameof(Customer.Custom))!;

        custom.GetValueConverter().Should().BeSameAs(CustomConverter);
    }

    [Fact]
    public async Task NullableValueType_Should_FailModelBuild()
    {
        await using SqliteConnection connection = await OpenAsync();
        await using NullableValueContext context = new(connection);

        Func<IEntityType> build = () => context.Rows.EntityType;

        build.Should().Throw<InvalidOperationException>().WithMessage("*Maybe<int?>*");
    }

    private static async Task<ShopContext> CreateDatabaseAsync(SqliteConnection connection)
    {
        ShopContext context = new(connection);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static async Task<SqliteConnection> OpenAsync()
    {
        SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();
        return connection;
    }
}
