using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>Adds <c>"enriched": "yes"</c> to every problem.</summary>
internal sealed class MarkerEnricher : IProblemDetailsEnricher
{
    public const string Key = "enriched";

    public void Enrich(ProblemDetailsContext context) => context.ProblemDetails.Extensions[Key] = "yes";
}

/// <summary>Counts its invocations per request in <see cref="HttpContext.Items"/> and writes the count.</summary>
internal sealed class CountingEnricher : IProblemDetailsEnricher
{
    public const string Key = "enrichCount";

    public void Enrich(ProblemDetailsContext context)
    {
        int count = context.HttpContext.Items[Key] is int previous ? previous + 1 : 1;
        context.HttpContext.Items[Key] = count;
        context.ProblemDetails.Extensions[Key] = count;
    }
}

/// <summary>Maps <c>auth.unauthorized</c> to 401 and <c>auth.forbidden</c> to 403.</summary>
internal sealed class AuthStatusCodeMapper : DefaultErrorStatusCodeMapper
{
    public override int GetStatusCode(Error error) => error.Code switch
    {
        "auth.unauthorized" => StatusCodes.Status401Unauthorized,
        "auth.forbidden" => StatusCodes.Status403Forbidden,
        _ => base.GetStatusCode(error),
    };
}

/// <summary>Maps <see cref="InvalidOperationException"/> (normally a 500) to 422.</summary>
internal sealed class InvalidOperationProblemMapper : IExceptionProblemMapper
{
    public const string Title = "Rejected by policy";

    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        problem = exception is InvalidOperationException
            ? new ExceptionProblem(StatusCodes.Status422UnprocessableEntity, Title, "The operation was rejected.")
            : null;
        return problem is not null;
    }
}

/// <summary>Maps <see cref="TimeoutException"/> to 504 and declines everything else.</summary>
internal sealed class TimeoutProblemMapper : IExceptionProblemMapper
{
    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        problem = exception is TimeoutException
            ? new ExceptionProblem(StatusCodes.Status504GatewayTimeout, "Upstream timeout")
            : null;
        return problem is not null;
    }
}

/// <summary>Scoped mapper that writes a per-instance id into the title, so each request's scope is observable.</summary>
internal sealed class ScopedInstanceMapper : IExceptionProblemMapper
{
    public const string TitlePrefix = "scoped-";
    private readonly Guid _id = Guid.NewGuid();

    public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
    {
        problem = new ExceptionProblem(StatusCodes.Status422UnprocessableEntity, TitlePrefix + _id);
        return true;
    }
}

/// <summary>Scoped status mapper that records every instance created; maps every error to <see cref="Status"/>.</summary>
internal sealed class ScopedStatusCodeMapper : DefaultErrorStatusCodeMapper
{
    public const int Status = StatusCodes.Status418ImATeapot;
    public static ConcurrentBag<Guid> Created { get; } = [];

    public ScopedStatusCodeMapper() => Created.Add(Guid.NewGuid());

    public override int GetStatusCode(Error error) => Status;
}
