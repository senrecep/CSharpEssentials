using System.Reflection;
using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class ModelConfigurationExtensionsTests
{
    [StringEnum]
    internal enum TestStatus
    {
        Active,
        Inactive,
        Pending
    }

    [StringEnum]
    internal enum AcronymStatus
    {
        HTTPStatus,
        IOError,
        Value1,
        Ok
    }

    private enum PlainColor
    {
        Red,
        Green
    }

    private sealed class AcronymEntity
    {
        public int Id { get; set; }
        public AcronymStatus Status { get; set; }
        public PlainColor Color { get; set; }
    }

    private sealed class DefaultConventionDbContext(DbContextOptions<DefaultConventionDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(typeof(DefaultConventionDbContext).Assembly);
    }

    private sealed class OptionsDefaultConventionDbContext(DbContextOptions<OptionsDefaultConventionDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(_ => { }, typeof(OptionsDefaultConventionDbContext).Assembly);
    }

    private sealed class LegacyConventionDbContext(DbContextOptions<LegacyConventionDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(o => o.UseLegacySnakeCase = true, typeof(LegacyConventionDbContext).Assembly);
    }

    private sealed class PredicateConventionDbContext(DbContextOptions<PredicateConventionDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(o => o.CanConvert = type => type == typeof(PlainColor), typeof(PredicateConventionDbContext).Assembly);
    }

    private sealed class PartiallyLoadableAssemblyDbContext(DbContextOptions<PartiallyLoadableAssemblyDbContext> options) : DbContext(options)
    {
        public static readonly Assembly BrokenAssembly = PartiallyLoadableAssembly.Create("EnumConventionScan.Broken");

        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(BrokenAssembly, typeof(PartiallyLoadableAssemblyDbContext).Assembly);
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
        DbContextOptions<EnumConventionDbContext> options = new DbContextOptionsBuilder<EnumConventionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new EnumConventionDbContext(options);

        IEntityType entityType = context.Model.FindEntityType(typeof(EnumEntity))!;
        IProperty property = entityType.FindProperty(nameof(EnumEntity.Status))!;

        property.GetMaxLength().Should().Be(8); // "inactive".Length
        property.GetValueConverter().Should().NotBeNull();
        property.GetValueConverter()!.ModelClrType.Should().Be<TestStatus>();
        property.GetValueConverter()!.ProviderClrType.Should().Be<string>();
        _ = new EnumEntity { Id = 1, Status = TestStatus.Active };
    }

    [Fact]
    public void ConfigureEnumConventions_ShouldUseCanonicalConverterAndJsonNameMaxLength()
    {
        using DefaultConventionDbContext context = new(CreateOptions<DefaultConventionDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        property.GetValueConverter().Should().BeOfType<EnumToFormattedStringConverter<AcronymStatus>>();
        property.GetMaxLength().Should().Be("http_status".Length);
    }

    [Fact]
    public void ConfigureEnumConventions_ShouldNotConvertEnumsWithoutStringEnumAttribute()
    {
        using DefaultConventionDbContext context = new(CreateOptions<DefaultConventionDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Color));

        property.GetValueConverter().Should().BeNull();
        property.GetMaxLength().Should().BeNull();
    }

    [Fact]
    public void ConfigureEnumConventions_WithDefaultOptions_ShouldMatchParameterlessOverload()
    {
        using OptionsDefaultConventionDbContext context = new(CreateOptions<OptionsDefaultConventionDbContext>());

        IProperty status = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));
        IProperty color = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Color));

        status.GetValueConverter().Should().BeOfType<EnumToFormattedStringConverter<AcronymStatus>>();
        status.GetMaxLength().Should().Be(11);
        color.GetValueConverter().Should().BeNull();
    }

    [Fact]
    public void ConfigureEnumConventions_WithUseLegacySnakeCase_ShouldUseLegacyConverterAndLegacyMaxLength()
    {
        using LegacyConventionDbContext context = new(CreateOptions<LegacyConventionDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        property.GetValueConverter().Should().BeOfType<LegacySnakeCaseEnumConverter<AcronymStatus>>();
        property.GetMaxLength().Should().Be("httpstatus".Length);
    }

    [Fact]
    public void ConfigureEnumConventions_WithCustomCanConvert_ShouldFailLoudForEnumsWithoutGeneratedMetadata()
    {
        using PredicateConventionDbContext context = new(CreateOptions<PredicateConventionDbContext>());

        Action build = () => GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Color));

        build.Should().Throw<InvalidOperationException>().WithMessage("*PlainColor*no generated metadata*");
    }

    [Fact]
    public async Task ConfigureEnumConventions_ShouldRoundTripThroughInMemoryProvider()
    {
        DbContextOptions<DefaultConventionDbContext> options = CreateOptions<DefaultConventionDbContext>();
        await using (DefaultConventionDbContext writeContext = new(options))
        {
            writeContext.Entities.Add(new AcronymEntity { Id = 1, Status = AcronymStatus.HTTPStatus, Color = PlainColor.Green });
            await writeContext.SaveChangesAsync();
        }

        await using DefaultConventionDbContext readContext = new(options);
        AcronymEntity entity = await readContext.Entities.SingleAsync();

        entity.Status.Should().Be(AcronymStatus.HTTPStatus);
        entity.Color.Should().Be(PlainColor.Green);
    }

    [Fact]
    public void ConfigureEnumConventions_WithPartiallyLoadableAssembly_ShouldScanLoadableTypes()
    {
        using PartiallyLoadableAssemblyDbContext context = new(CreateOptions<PartiallyLoadableAssemblyDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        PartiallyLoadableAssemblyDbContext.BrokenAssembly.Invoking(a => a.GetTypes()).Should().Throw<ReflectionTypeLoadException>();
        property.GetValueConverter().Should().BeOfType<EnumToFormattedStringConverter<AcronymStatus>>();
    }

    private static DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext =>
        new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static IProperty GetProperty<TEntity>(DbContext context, string propertyName) =>
        context.Model.FindEntityType(typeof(TEntity))!.FindProperty(propertyName)!;
}
