using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.Maybe;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class PropertyBuilderExtensionsTests
{
    private sealed class JsonEntity
    {
        public Guid Id { get; set; }
        public Maybe<string> MaybeName { get; set; }
        public MyData JsonData { get; set; } = null!;
    }

    private sealed class MyData
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class PropertyDbContext : DbContext
    {
        public DbSet<JsonEntity> JsonEntities { get; set; } = null!;
        public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonEntity>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.MaybeName).MaybeConversion();
                b.Property(x => x.JsonData).HasJsonConversion();
            });
        }
    }

    private static DbContextOptions<PropertyDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<PropertyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void MaybeConversion_ShouldConvertValue()
    {
        using var context = new PropertyDbContext(CreateOptions());
        var entity = new JsonEntity { Id = Guid.NewGuid(), MaybeName = Maybe<string>.From("hello"), JsonData = new MyData { Value = "world" } };
        context.JsonEntities.Add(entity);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var loaded = context.JsonEntities.Find(entity.Id);
        loaded!.MaybeName.HasValue.Should().BeTrue();
        loaded.MaybeName.Value.Should().Be("hello");
    }



    [Fact]
    public void HasJsonConversion_ShouldConvertObjectToJson()
    {
        using var context = new PropertyDbContext(CreateOptions());
        var entity = new JsonEntity { Id = Guid.NewGuid(), MaybeName = Maybe<string>.From("dummy"), JsonData = new MyData { Value = "test" } };
        context.JsonEntities.Add(entity);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var loaded = context.JsonEntities.Find(entity.Id);
        loaded!.JsonData.Should().NotBeNull();
        loaded.JsonData.Value.Should().Be("test");
    }

    [Fact]
    public void HasJsonConversion_ShouldSetColumnTypeAnnotation()
    {
        var options = new DbContextOptionsBuilder<PropertyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new PropertyDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(JsonEntity))!;
        var property = entityType.FindProperty(nameof(JsonEntity.JsonData))!;
        property.FindAnnotation("Relational:ColumnType")?.Value.Should().Be("jsonb");
    }
}
