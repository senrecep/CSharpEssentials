
using CSharpEssentials.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore;

public abstract partial class BaseDbContext<TContext> : DbContext
    where TContext : DbContext
{
    private readonly Guid _instanceId = Guider.NewGuid();
    protected readonly ILogger<TContext> Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected BaseDbContext(
        DbContextOptions<TContext> options, IServiceScopeFactory serviceScopeFactory) : base(options)
    {
        IServiceProvider serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<TContext>>();
        ServiceProvider = serviceProvider;
        Logger.LogInformation("Context {DbContextInstanceId} created", _instanceId);
    }

    ~BaseDbContext()
    {
        Logger.LogInformation("Context {DbContextInstanceId} destructed", _instanceId);
    }

    public override void Dispose()
    {
        Logger.LogInformation("Context {DbContextInstanceId} disposed", _instanceId);
        base.Dispose();
    }
}