using CSharpEssentials.Enums;
using CSharpEssentials.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class ModelConfigurationExtensionsTests
{
    [StringEnum]
    private enum TestStatus
    {
        Active,
        Inactive,
        Pending
    }

    private sealed class EnumEntity
    {
        public int Id { get; set; }
        public TestStatus Status { get; set; }
    }

    private sealed class EnumConventionDbContext : DbContext
    {
        public DbSet<EnumEntity> EnumEntities { get; set; } = null!;
        public EnumConventionDbContext(DbContextOptions<EnumConventionDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnumEntity>().HasKey(x => x.Id);
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureEnumConventions(typeof(EnumConventionDbContext).Assembly);
        }
    }

    [Fact]
    public void ConfigureEnumConventions_ShouldApplyConverterAndMaxLength()
    {
        var options = new DbContextOptionsBuilder<EnumConventionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new EnumConventionDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(EnumEntity))!;
        var property = entityType.FindProperty(nameof(EnumEntity.Status))!;

        property.GetMaxLength().Should().Be(8); // "inactive".Length
        property.GetValueConverter().Should().NotBeNull();
        property.GetValueConverter()!.ModelClrType.Should().Be<TestStatus>();
        property.GetValueConverter()!.ProviderClrType.Should().Be<string>();
        _ = new EnumEntity { Id = 1, Status = TestStatus.Active };
    }
}
