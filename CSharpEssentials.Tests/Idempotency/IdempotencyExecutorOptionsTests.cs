using System.Text;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.Idempotency;

public sealed class IdempotencyExecutorOptionsTests
{
    private static readonly IIdempotencyResultCodec<string> TextCodec = IdempotencyResultCodec.Create<string>(
        static value => Encoding.UTF8.GetBytes(value),
        static payload => Encoding.UTF8.GetString(payload.Span));

    [Fact]
    public void Defaults_Should_Match_The_Documented_Values()
    {
        IdempotencyExecutorOptions options = new();

        options.Mode.Should().Be(IdempotencyMode.Enforce);
        options.Retention.Should().Be(TimeSpan.FromHours(24));
        options.Lease.Should().Be(TimeSpan.FromMinutes(1));
        options.FailureRetention.Should().Be(TimeSpan.FromMinutes(10));
        options.MaxLease.Should().Be(TimeSpan.FromMinutes(15));
        options.Profiles[IdempotencyExecutorOptions.MessagingProfile].Retention.Should().Be(TimeSpan.FromDays(7));
    }

    [Fact]
    public void AddProfile_Should_Reject_A_Duplicate_Name_In_Any_Casing()
    {
        IdempotencyExecutorOptions options = new();
        options.AddProfile("payments", _ => { });

        Action act = () => options.AddProfile("PAYMENTS", _ => { });

        act.Should().Throw<ArgumentException>().WithParameterName("name").WithMessage("*already registered*");
    }

    [Fact]
    public void AddProfile_Should_Replace_The_Built_In_Messaging_Profile_Once()
    {
        IdempotencyExecutorOptions options = new();

        options.AddProfile("Messaging", profile => profile.Retention = TimeSpan.FromDays(14));
        Action again = () => options.AddProfile("messaging", _ => { });

        options.Profiles["messaging"].Retention.Should().Be(TimeSpan.FromDays(14));
        again.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddProfile_Should_Reject_A_Profile_That_Names_Another_Profile()
    {
        IdempotencyExecutorOptions options = new();

        Action act = () => options.AddProfile("nested", profile => profile.Profile = "messaging");

        act.Should().Throw<ArgumentException>().WithMessage("*cannot name another profile*");
        options.Profiles.Should().NotContainKey("nested");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddProfile_Should_Reject_An_Empty_Name(string name)
    {
        Action act = () => new IdempotencyExecutorOptions().AddProfile(name, _ => { });

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(name));
    }

    [Fact]
    public void ResolveMode_Should_Prefer_The_Operation_Then_The_Profile_Then_The_Default()
    {
        IdempotencyExecutorOptions options = new() { Mode = IdempotencyMode.Enforce };
        options.AddProfile("quiet", profile => profile.Mode = IdempotencyMode.Off);

        IdempotencyMode fromDefault = options.ResolveMode(null);
        IdempotencyMode fromProfile = options.ResolveMode(new IdempotencyExecutionOptions { Profile = "quiet" });
        IdempotencyMode fromOperation = options.ResolveMode(new IdempotencyExecutionOptions { Profile = "quiet", Mode = IdempotencyMode.Observe });

        fromDefault.Should().Be(IdempotencyMode.Enforce);
        fromProfile.Should().Be(IdempotencyMode.Off);
        fromOperation.Should().Be(IdempotencyMode.Observe);
    }

    [Fact]
    public void ResolveMode_Should_Throw_For_An_Unknown_Profile()
    {
        IdempotencyExecutorOptions options = new();

        Action act = () => options.ResolveMode(new IdempotencyExecutionOptions { Profile = "missing", Mode = IdempotencyMode.Off });

        act.Should().Throw<InvalidOperationException>().WithMessage("*profile 'missing'*not registered*");
    }

    [Fact]
    public void AddIdempotencyExecutor_Should_Register_The_Executor_And_A_Default_Store()
    {
        ServiceCollection services = new();

        services.AddIdempotencyExecutor(options => options.Retention = TimeSpan.FromHours(2));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>().Should().BeOfType<IdempotencyExecutor>();
        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeOfType<InMemoryIdempotencyRecordStore>();
        provider.GetRequiredService<IdempotencyExecutorOptions>().Retention.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void AddIdempotencyExecutor_Should_Keep_A_Registered_Store()
    {
        ServiceCollection services = new();
        InMemoryIdempotencyRecordStore store = new();
        services.AddSingleton<IIdempotencyRecordStore>(store);

        services.AddIdempotencyExecutor();
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IIdempotencyRecordStore>().Should().BeSameAs(store);
    }

    [Fact]
    public void AddIdempotencyExecutor_Without_Configure_Should_Keep_Registered_Options()
    {
        ServiceCollection services = new();

        services.AddIdempotencyExecutor(options => options.Mode = IdempotencyMode.Observe);
        services.AddIdempotencyExecutor();
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IdempotencyExecutorOptions>().Mode.Should().Be(IdempotencyMode.Observe);
    }

    [Fact]
    public void AddIdempotencyExecutor_With_Configure_Should_Replace_Registered_Options()
    {
        ServiceCollection services = new();

        services.AddIdempotencyExecutor(options => options.Mode = IdempotencyMode.Observe);
        services.AddIdempotencyExecutor(options => options.Mode = IdempotencyMode.Off);
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IdempotencyExecutorOptions>().Mode.Should().Be(IdempotencyMode.Off);
    }

    [Fact]
    public void AddIdempotencyExecutor_With_Configure_Should_Not_Run_The_Replaced_Configuration()
    {
        ServiceCollection services = new();

        services.AddIdempotencyExecutor(options => options.AddProfile("payments", profile => profile.Retention = TimeSpan.FromDays(1)));
        services.AddIdempotencyExecutor(options => options.AddProfile("payments", profile => profile.Retention = TimeSpan.FromDays(2)));
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IdempotencyExecutorOptions>().Profiles["payments"].Retention.Should().Be(TimeSpan.FromDays(2));
    }

    [Fact]
    public async Task AddIdempotencyExecutor_Should_Apply_A_Mode_Changed_By_A_Configuration_Reload()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mode"] = "Off" })
            .Build();
        ServiceCollection services = new();
        services.AddIdempotencyExecutor();
        services.Configure<IdempotencyExecutorOptions>(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IIdempotencyExecutor executor = scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>();
        IdempotencyRecordKey key = new("reload", IdempotencyScope.Global, "k1");
        int runs = 0;
        ValueTask<string> Operation(CancellationToken _)
        {
            runs++;
            return new ValueTask<string>("done");
        }

        await executor.ExecuteAsync(key, "fp", null, Operation, TextCodec);
        configuration["Mode"] = "Enforce";
        configuration.Reload();
        await executor.ExecuteAsync(key, "fp", null, Operation, TextCodec);
        IdempotencyExecution<string> replayed = await executor.ExecuteAsync(key, "fp", null, Operation, TextCodec);

        replayed.Status.Should().Be(IdempotencyExecutionStatus.Replayed);
        runs.Should().Be(2);
    }

    [Fact]
    public async Task AddIdempotencyExecutor_Should_Throw_On_The_Next_Operation_When_A_Reload_Makes_The_Options_Invalid()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mode"] = "Enforce" })
            .Build();
        ServiceCollection services = new();
        services.AddIdempotencyExecutor();
        services.Configure<IdempotencyExecutorOptions>(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IIdempotencyExecutor executor = scope.ServiceProvider.GetRequiredService<IIdempotencyExecutor>();

        configuration["Lease"] = "02:00:00";
        Action reload = configuration.Reload;
        Func<Task> act = () => executor.ExecuteAsync(
            new IdempotencyRecordKey("reload-invalid", IdempotencyScope.Global, "k1"),
            "fp",
            null,
            static _ => new ValueTask<string>("done"),
            TextCodec).AsTask();

        reload.Should().Throw<AggregateException>().WithInnerException<OptionsValidationException>();
        await act.Should().ThrowAsync<OptionsValidationException>().WithMessage("*Lease*");
    }

    [Fact]
    public void AddIdempotencyExecutor_Should_Throw_For_Invalid_Options()
    {
        Action act = () => new ServiceCollection().AddIdempotencyExecutor(options => options.Lease = TimeSpan.FromHours(1));

        act.Should().Throw<InvalidOperationException>().WithMessage("*executor options*Lease*");
    }

    [Fact]
    public void AddIdempotencyExecutor_Should_Throw_For_An_Invalid_Profile()
    {
        Action act = () => new ServiceCollection().AddIdempotencyExecutor(options =>
            options.AddProfile("short", profile => profile.FailureRetention = TimeSpan.FromDays(2)));

        act.Should().Throw<InvalidOperationException>().WithMessage("*profile 'short'*FailureRetention*");
    }

    [Fact]
    public void Codec_Create_Should_Store_Every_Result_By_Default()
    {
        IIdempotencyResultCodec<int> codec = IdempotencyResultCodec.Create<int>(static value => new[] { (byte)value }, static payload => payload.Span[0]);

        codec.Classify(42).Should().Be(IdempotencyResultHandling.Store);
        codec.Deserialize(codec.Serialize(42)).Should().Be(42);
    }

    [Fact]
    public void Codec_Create_Should_Reject_Missing_Delegates()
    {
        Action noSerialize = () => IdempotencyResultCodec.Create<int>(null!, static _ => 0);
        Action noDeserialize = () => IdempotencyResultCodec.Create<int>(static _ => ReadOnlyMemory<byte>.Empty, null!);

        noSerialize.Should().Throw<ArgumentNullException>();
        noDeserialize.Should().Throw<ArgumentNullException>();
    }
}
