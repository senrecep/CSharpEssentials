using System.IO.Pipelines;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

public sealed class ConditionalRequestExtensionsTests
{
    [Fact]
    public void AddConditionalRequests_Should_AddMvcFiltersOnce_When_CalledTwice()
    {
        var once = new ServiceCollection();
        once.AddLogging().AddControllers();
        once.AddConditionalRequests();
        var twice = new ServiceCollection();
        twice.AddLogging().AddControllers();
        twice.AddConditionalRequests();

        twice.AddConditionalRequests();

        FilterCount(twice).Should().Be(FilterCount(once));
        twice.Count.Should().Be(once.Count);
    }

    [Fact]
    public void AddConditionalRequests_Should_UseLastOptions_When_CalledTwice()
    {
        var services = new ServiceCollection();
        services.AddConditionalRequests(o => o.UseBodyHashFallback = true);

        services.AddConditionalRequests();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<ConditionalRequestOptions>().UseBodyHashFallback.Should().BeFalse();
    }

    [Fact]
    public void AddConditionalRequests_Should_KeepGenerator_When_AlreadyRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IETagGenerator, FixedETagGenerator>();

        services.AddConditionalRequests();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IETagGenerator>().Should().BeOfType<FixedETagGenerator>();
    }

    [Fact]
    public void ConditionalRequestOptions_Should_DisableBodyHashFallback_When_Default()
    {
        var options = new ConditionalRequestOptions();

        bool fallback = options.UseBodyHashFallback;

        fallback.Should().BeFalse();
    }

    [Fact]
    public void AddConditionalRequests_Should_Throw_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddConditionalRequests();

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddETagSource_Should_Throw_When_ServicesIsNull()
    {
        IServiceCollection services = null!;

        Action act = () => services.AddETagSource<Document, DocumentETagSource>();

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void WithConditionalGet_Should_Throw_When_BuilderIsNull()
    {
        RouteHandlerBuilder builder = null!;

        Action act = () => builder.WithConditionalGet();

        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public void WithIfMatch_Should_Throw_When_BuilderIsNull()
    {
        RouteHandlerBuilder builder = null!;

        Action act = () => builder.WithIfMatch();

        act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Fact]
    public async Task WithIfMatch_Should_Throw_When_LoaderIsNull()
    {
        await using WebApplication app = WebApplication.CreateBuilder().Build();
        RouteHandlerBuilder builder = app.MapPut("/", () => "x");
        Func<HttpContext, CancellationToken, ValueTask<VersionedItem?>> loader = null!;

        Action act = () => builder.WithIfMatch(loader);

        act.Should().Throw<ArgumentNullException>().WithParameterName("loadCurrent");
    }

    [Fact]
    public void GetPreconditions_Should_Throw_When_HttpContextIsNull()
    {
        HttpContext httpContext = null!;

        Action act = () => httpContext.GetPreconditions();

        act.Should().Throw<ArgumentNullException>().WithParameterName("httpContext");
    }

    [Fact]
    public void GetPreconditions_Should_Throw_When_NoIfMatchFilterRan()
    {
        var httpContext = new DefaultHttpContext();

        Action act = () => httpContext.GetPreconditions();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PreconditionFailed_Should_HaveStableCode_When_Read()
    {
        var error = ConditionalRequestErrors.PreconditionFailed;

        string code = error.Code;

        code.Should().Be(ConditionalRequestErrors.PreconditionFailedCode);
    }

    [Fact]
    public void GetStatusCode_Should_Return412_When_ErrorIsPreconditionFailed()
    {
        var mapper = new DefaultErrorStatusCodeMapper();

        int status = mapper.GetStatusCode(ConditionalRequestErrors.PreconditionFailed);

        status.Should().Be(StatusCodes.Status412PreconditionFailed);
    }

    [Fact]
    public void GetStatusCode_Should_Return409_When_ConflictHasOtherCode()
    {
        var mapper = new DefaultErrorStatusCodeMapper();

        int status = mapper.GetStatusCode(CSharpEssentials.Errors.Error.Conflict("order.conflict", "Order already shipped"));

        status.Should().Be(StatusCodes.Status409Conflict);
    }

    private static int FilterCount(IServiceCollection services)
    {
        using ServiceProvider provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<MvcOptions>>().Value.Filters.Count;
    }

    [Fact]
    public void AddETagSource_Should_Throw_When_ResourceTypeIsInterface()
    {
        var services = new ServiceCollection();

        Action act = () => services.AddETagSource<IVersioned, VersionedInterfaceETagSource>();

        act.Should().Throw<ArgumentException>().WithMessage("*interface*");
    }

    [Theory]
    [InlineData("42", "\"42\"")]
    [InlineData("v1", "\"v1\"")]
    public void ToETag_Should_QuoteVersion_When_VersionIsValidEntityTag(string version, string expected)
    {
        var item = new VersionedItem(1, version);

        string? etag = item.ToETag()?.ToString();

        etag.Should().Be(expected);
    }

    [Fact]
    public void ToETag_Should_HashVersion_When_VersionIsNotValidEntityTag()
    {
        var item = new VersionedItem(1, "has space");

        string? etag = item.ToETag()?.ToString();

        etag.Should().MatchRegex("^\"[A-Za-z0-9_-]{43}\"$");
    }

    [Fact]
    public void ToETag_Should_ReturnNull_When_VersionIsEmpty()
    {
        var item = new VersionedItem(1, "");

        Microsoft.Net.Http.Headers.EntityTagHeaderValue? etag = item.ToETag();

        etag.Should().BeNull();
    }

    [Fact]
    public void ToETag_Should_Throw_When_VersionedIsNull()
    {
        IVersioned versioned = null!;

        Action act = () => versioned.ToETag();

        act.Should().Throw<ArgumentNullException>().WithParameterName("versioned");
    }

    [Fact]
    public void GetValidators_Should_ReturnNull_When_ValueIsStreamedOrDeferred()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddOptions()
            .AddConditionalRequests(o => o.UseBodyHashFallback = true)
            .BuildServiceProvider();
        IETagGenerator generator = provider.GetRequiredService<IETagGenerator>();
        var counter = new EnumerationCounter();
        int enumerated = 0;
        IEnumerable<int> Source()
        {
            enumerated++;
            yield return 1;
        }
        IQueryable<int> queryable = Source().AsQueryable();
        using var stream = new MemoryStream([1, 2, 3]);
        var pipe = new Pipe();

        ResourceValidators?[] validators =
        [
            generator.GetValidators(counter.Items()),
            generator.GetValidators(queryable),
            generator.GetValidators(stream),
            generator.GetValidators(pipe.Reader),
        ];

        validators.Should().AllSatisfy(v => v.Should().BeNull());
        enumerated.Should().Be(0);
        counter.Count.Should().Be(0);
        generator.GetValidators(new PlainItem("a")).Should().NotBeNull();
    }
}
