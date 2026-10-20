using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.Idempotency;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

public sealed class EfCoreIdempotencyStoreRegistrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddEfCoreIdempotencyRecordStore_Should_Replace_The_Default_Store_In_Any_Order(bool storeFirst)
    {
        ServiceCollection services = new();
        services.AddDbContext<IdempotencyTestDbContext>(options => options.UseSqlite("DataSource=:memory:"));
        if (storeFirst)
            services.AddEfCoreIdempotencyRecordStore<IdempotencyTestDbContext>().AddIdempotencyExecutor();
        else
            services.AddIdempotencyExecutor().AddEfCoreIdempotencyRecordStore<IdempotencyTestDbContext>();
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        IIdempotencyRecordStore store = scope.ServiceProvider.GetRequiredService<IIdempotencyRecordStore>();

        store.Should().BeSameAs(scope.ServiceProvider.GetRequiredService<EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>>());
    }

    [Fact]
    public async Task AddEfCoreIdempotencyRecordStore_Should_Register_A_Keyed_Store_Next_To_The_Default_One()
    {
        ServiceCollection services = new();
        services.AddDbContext<IdempotencyTestDbContext>(options => options.UseSqlite("DataSource=:memory:"));
        services.AddIdempotencyExecutor().AddEfCoreIdempotencyRecordStore<IdempotencyTestDbContext>("payments");
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();

        IIdempotencyRecordStore keyed = scope.ServiceProvider.GetRequiredKeyedService<IIdempotencyRecordStore>("payments");

        keyed.Should().BeOfType<EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>>();
        scope.ServiceProvider.GetRequiredService<IIdempotencyRecordStore>().Should().BeOfType<InMemoryIdempotencyRecordStore>();
    }

    [Fact]
    public void AddEfCoreIdempotencyRecordStore_Should_Reject_An_Empty_Key()
    {
        ServiceCollection services = new();

        Action empty = () => services.AddEfCoreIdempotencyRecordStore<IdempotencyTestDbContext>(string.Empty);

        empty.Should().Throw<ArgumentException>();
    }
}
