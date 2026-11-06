using CSharpEssentials.EntityFrameworkCore.Extensions;
using CSharpEssentials.EntityFrameworkCore.Interceptors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public sealed class DbContextRegistrationOptionsTests
{
    private sealed class OptionsDbContext(DbContextOptions<OptionsDbContext> options) : DbContext(options);

    private static Action<IServiceProvider, DbContextOptionsBuilder> InMemory() =>
        (_, builder) => builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

    private static CoreOptionsExtension CoreOptions(DbContext context) =>
        context.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()!;

    private static CoreOptionsExtension ResolveFactoryContextOptions(
        Action<DbContextRegistrationOptions> configureOptions,
        Action<IServiceCollection>? configureServices = null)
    {
        ServiceCollection services = new();
        services.AddLogging();
        configureServices?.Invoke(services);
        services.RegisterDbContextFactory<OptionsDbContext>(configureOptions, InMemory());

        using ServiceProvider provider = services.BuildServiceProvider();
        using OptionsDbContext context = provider.GetRequiredService<IDbContextFactory<OptionsDbContext>>().CreateDbContext();
        return CoreOptions(context);
    }

    private static void RegisterAllInterceptors(IServiceCollection services)
    {
        services.AddAuditInterceptor(() => "auditor");
        services.AddSlowQueryInterceptor();
        services.AddSingleton<DomainEventInterceptor>();
    }

    [Fact]
    public void Defaults_Should_LeaveEveryOptionOff()
    {
        DbContextRegistrationOptions options = new();

        options.EnableDetailedErrors.Should().BeFalse();
        options.EnableSensitiveDataLogging.Should().BeFalse();
        options.QueryTrackingBehavior.Should().BeNull();
        options.EnableAuditInterceptor.Should().BeFalse();
        options.EnableDomainEventInterceptor.Should().BeFalse();
        options.EnableSlowQueryInterceptor.Should().BeFalse();
    }

    [Fact]
    public void RetryOptions_Defaults_Should_MatchDocumentedValues()
    {
        DbContextRetryOptions retry = new();

        retry.MaxRetryCount.Should().Be(3);
        retry.MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(5));
        retry.AdditionalErrorCodes.Should().BeEmpty();
    }

    [Fact]
    public void Apply_Should_LeaveProviderDefaults_When_NothingIsEnabled()
    {
        CoreOptionsExtension core = ResolveFactoryContextOptions(_ => { }, RegisterAllInterceptors);

        core.DetailedErrorsEnabled.Should().BeFalse();
        core.IsSensitiveDataLoggingEnabled.Should().BeFalse();
        core.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.TrackAll);
        core.Interceptors.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Apply_Should_EnableDetailedErrorsAndSensitiveDataLogging_When_Requested()
    {
        CoreOptionsExtension core = ResolveFactoryContextOptions(options =>
        {
            options.EnableDetailedErrors = true;
            options.EnableSensitiveDataLogging = true;
        });

        core.DetailedErrorsEnabled.Should().BeTrue();
        core.IsSensitiveDataLoggingEnabled.Should().BeTrue();
    }

    [Fact]
    public void Apply_Should_SetQueryTrackingBehavior_When_Given()
    {
        CoreOptionsExtension core = ResolveFactoryContextOptions(
            options => options.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution);

        core.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    }

    [Fact]
    public void Apply_Should_AttachEveryEnabledInterceptor_When_RegisteredInServices()
    {
        CoreOptionsExtension core = ResolveFactoryContextOptions(options =>
        {
            options.EnableAuditInterceptor = true;
            options.EnableDomainEventInterceptor = true;
            options.EnableSlowQueryInterceptor = true;
        }, RegisterAllInterceptors);

        core.Interceptors.Should().HaveCount(3);
        core.Interceptors.Should().ContainSingle(i => i is AuditInterceptor);
        core.Interceptors.Should().ContainSingle(i => i is DomainEventInterceptor);
        core.Interceptors.Should().ContainSingle(i => i is SlowQueryInterceptor);
    }

    [Fact]
    public void Apply_Should_SkipEnabledInterceptors_When_NotRegisteredInServices()
    {
        CoreOptionsExtension core = ResolveFactoryContextOptions(options =>
        {
            options.EnableAuditInterceptor = true;
            options.EnableDomainEventInterceptor = true;
            options.EnableSlowQueryInterceptor = true;
        });

        core.Interceptors.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Apply_Should_AttachOnlyTheEnabledInterceptor_When_AllAreRegistered()
    {
        CoreOptionsExtension core = ResolveFactoryContextOptions(
            options => options.EnableSlowQueryInterceptor = true,
            RegisterAllInterceptors);

        core.Interceptors.Should().ContainSingle().Which.Should().BeOfType<SlowQueryInterceptor>();
    }

    [Fact]
    public void AddPooledDbContext_WithOptions_Should_ApplyOptionsToPooledContext()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddAuditInterceptor(() => "auditor");
        services.AddPooledDbContext<OptionsDbContext>(options =>
        {
            options.EnableDetailedErrors = true;
            options.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            options.EnableAuditInterceptor = true;
        }, InMemory());

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        CoreOptionsExtension core = CoreOptions(scope.ServiceProvider.GetRequiredService<OptionsDbContext>());

        core.DetailedErrorsEnabled.Should().BeTrue();
        core.QueryTrackingBehavior.Should().Be(QueryTrackingBehavior.NoTracking);
        core.Interceptors.Should().ContainSingle().Which.Should().BeOfType<AuditInterceptor>();
    }
}
