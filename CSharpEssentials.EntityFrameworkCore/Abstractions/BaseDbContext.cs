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
        LogContextCreated(_instanceId);
    }

    ~BaseDbContext()
    {
        LogContextDestructed(_instanceId);
    }

    public override void Dispose()
    {
        LogContextDisposed(_instanceId);
        base.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Context {DbContextInstanceId} created")]
    private partial void LogContextCreated(Guid dbContextInstanceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Context {DbContextInstanceId} destructed")]
    private partial void LogContextDestructed(Guid dbContextInstanceId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Context {DbContextInstanceId} disposed")]
    private partial void LogContextDisposed(Guid dbContextInstanceId);
}
