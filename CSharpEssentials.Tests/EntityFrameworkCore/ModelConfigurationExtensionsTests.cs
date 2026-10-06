using CSharpEssentials.EntityFrameworkCore;
using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

/// <summary>The 4.x registration overloads, kept as obsolete forwarders to the 5.0 convention.</summary>
public class ModelConfigurationExtensionsTests
{
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

    private sealed class NullConventionsDbContext(DbContextOptions<NullConventionsDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(null);
    }

    private sealed class DefaultConventionDbContext(DbContextOptions<DefaultConventionDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(typeof(DefaultConventionDbContext).Assembly);
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
            configurationBuilder.ConfigureEnumConventions(o => o.CanConvert = type => type == typeof(PlainColor));
    }

    private sealed class StatusOnlyConventionDbContext(DbContextOptions<StatusOnlyConventionDbContext> options) : DbContext(options)
    {
        public DbSet<AcronymEntity> Entities { get; set; } = null!;
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
            configurationBuilder.ConfigureEnumConventions(o => o.CanConvert = type => type == typeof(AcronymStatus));
    }

    [Fact]
    public void ConfigureEnumConventions_Should_StoreWireName_When_CalledWithAssemblies()
    {
        using DefaultConventionDbContext context = new(CreateOptions<DefaultConventionDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        property.GetValueConverter().Should().BeOfType<EnumWireNameConverter<AcronymStatus>>();
        property.GetValueConverter()!.ConvertToProvider(AcronymStatus.HTTPStatus).Should().Be("http_status");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_Bind_To_The_Convention_Overload_When_CalledWithNull()
    {
        using NullConventionsDbContext context = new(CreateOptions<NullConventionsDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        property.GetValueConverter().Should().BeOfType<EnumWireNameConverter<AcronymStatus>>();
    }

    [Fact]
    public void ConfigureEnumConventions_Should_LeaveEnumsWithoutStringEnumAttribute()
    {
        using DefaultConventionDbContext context = new(CreateOptions<DefaultConventionDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Color));

        property.GetValueConverter().Should().BeNull();
        property.GetMaxLength().Should().BeNull();
    }

    [Fact]
    public void ConfigureEnumConventions_Should_WriteLegacySnakeCase_When_UseLegacySnakeCase()
    {
        using LegacyConventionDbContext context = new(CreateOptions<LegacyConventionDbContext>());

        IProperty property = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        property.GetValueConverter()!.ConvertToProvider(AcronymStatus.HTTPStatus).Should().Be("httpstatus");
        property.GetValueConverter()!.ConvertFromProvider("http_status").Should().Be(AcronymStatus.HTTPStatus);
    }

    [Fact]
    public void ConfigureEnumConventions_Should_Throw_When_CanConvertSelectsEnumWithoutMetadata()
    {
        using PredicateConventionDbContext context = new(CreateOptions<PredicateConventionDbContext>());

        Action build = () => _ = context.Model;

        build.Should().Throw<InvalidOperationException>().WithMessage("*PlainColor*[StringEnum]*ConfigureEnumConventionsWithReflection*");
    }

    [Fact]
    public void ConfigureEnumConventions_Should_StoreEnumWithMetadata_When_CanConvertSelectsOnlyIt()
    {
        using StatusOnlyConventionDbContext context = new(CreateOptions<StatusOnlyConventionDbContext>());

        IProperty color = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Color));
        IProperty status = GetProperty<AcronymEntity>(context, nameof(AcronymEntity.Status));

        color.GetValueConverter().Should().BeNull();
        status.GetValueConverter().Should().BeOfType<EnumWireNameConverter<AcronymStatus>>();
    }

    [Fact]
    public async Task ConfigureEnumConventions_Should_RoundTripThroughInMemoryProvider()
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

    private static DbContextOptions<TContext> CreateOptions<TContext>() where TContext : DbContext =>
        new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static IProperty GetProperty<TEntity>(DbContext context, string propertyName) =>
        context.Model.FindEntityType(typeof(TEntity))!.FindProperty(propertyName)!;
}
