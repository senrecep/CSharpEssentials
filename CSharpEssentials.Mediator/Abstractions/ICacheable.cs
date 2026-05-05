namespace CSharpEssentials.Mediator;

public interface ICacheable
{
    bool BypassCache { get; }
    bool CacheFailures { get; }
    string CacheKey { get; }
    TimeSpan Expiration { get; }
}
