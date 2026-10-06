using CSharpEssentials.DependencyInjection;

namespace CSharpEssentials.Tests.DependencyInjection.Scanning;

internal enum ScanRegion
{
    Us,
    Eu,
}

internal interface IOtherMarker;

internal interface IConventionalService;

[RegisterScoped]
internal sealed class ConventionalService : IConventionalService, IOtherMarker;

[RegisterTransient]
internal sealed class SelfOnlyService;

[RegisterScoped(typeof(IOtherMarker), Key = "explicit")]
internal sealed class ExplicitService : IOtherMarker;

internal interface IGenericAttributeService;

[RegisterSingleton<IGenericAttributeService>(Key = ScanRegion.Eu)]
internal sealed class GenericAttributeService : IGenericAttributeService;

internal interface ISharedService;

internal interface ISecondShared;

[RegisterSingleton(As = ServiceAs.SelfWithInterfaces)]
internal sealed class SharedService : ISharedService, ISecondShared;

internal interface IKeyedSharedService;

[RegisterScoped(Key = "shared", As = ServiceAs.SelfWithInterfaces)]
internal sealed class KeyedSharedService : IKeyedSharedService;

internal interface ISplitService;

internal interface ISecondSplit;

[RegisterSingleton(As = ServiceAs.ImplementedInterfaces)]
internal sealed class SplitService : ISplitService, ISecondSplit, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

internal interface ISelfExplicitService;

[RegisterScoped(As = ServiceAs.Self)]
internal sealed class SelfExplicitService : ISelfExplicitService;

internal interface IScanRepository<T>
{
    IReadOnlyList<T> All();
}

[RegisterScoped(typeof(IScanRepository<>))]
internal sealed class ScanRepository<T> : IScanRepository<T>
{
    public IReadOnlyList<T> All() => [];
}

internal interface IKeyedRepository<TKey, TValue>
{
    TValue? Find(TKey key);
}

[RegisterTransient(Key = 42)]
internal sealed class KeyedRepository<TKey, TValue> : IKeyedRepository<TKey, TValue>
{
    public TValue? Find(TKey key) => default;
}

internal interface IScanFormatter;

[RegisterSingleton(typeof(IScanFormatter))]
internal sealed class DefaultFormatter : IScanFormatter;

[RegisterSingleton(typeof(IScanFormatter), Strategy = RegistrationStrategy.TryAdd)]
internal sealed class TryFormatter : IScanFormatter;

[RegisterSingleton(typeof(IScanFormatter), Key = typeof(int))]
internal sealed class IntFormatter : IScanFormatter;

internal interface IScanReplaceable;

[RegisterScoped(typeof(IScanReplaceable))]
internal sealed class OriginalReplaceable : IScanReplaceable;

[RegisterScoped(typeof(IScanReplaceable), Strategy = RegistrationStrategy.Replace)]
internal sealed class ReplacingReplaceable : IScanReplaceable;

internal interface IThrowingService;

[RegisterScoped(Strategy = RegistrationStrategy.Throw)]
internal sealed class ThrowingService : IThrowingService;

internal interface IMultiService;

[RegisterScoped(Key = "a")]
[RegisterSingleton(Key = "b")]
internal sealed class MultiService : IMultiService;

internal interface IExcludedService;

[RegisterScoped]
[ExcludeFromRegistration]
internal sealed class ExcludedService : IExcludedService;

internal interface IScanGreeter
{
    string Greet();
}

[RegisterScoped]
internal sealed class ScanGreeter : IScanGreeter
{
    public string Greet() => "hi";
}

[RegisterScoped(typeof(IScanGreeter), Key = "k")]
internal sealed class KeyedScanGreeter : IScanGreeter
{
    public string Greet() => "keyed";
}

[Decorates(typeof(IScanGreeter), Order = 2)]
internal sealed class OuterScanGreeter(IScanGreeter inner) : IScanGreeter
{
    public string Greet() => $"outer({inner.Greet()})";
}

[Decorates<IScanGreeter>(Order = 1)]
internal sealed class InnerScanGreeter(IScanGreeter inner, SelfOnlyService dependency) : IScanGreeter
{
    public SelfOnlyService Dependency { get; } = dependency;

    public string Greet() => $"inner({inner.Greet()})";
}

[Decorates(typeof(IScanGreeter), Key = "k")]
internal sealed class KeyedDecoratorScanGreeter(IScanGreeter inner) : IScanGreeter
{
    public string Greet() => $"keyed-decorator({inner.Greet()})";
}
