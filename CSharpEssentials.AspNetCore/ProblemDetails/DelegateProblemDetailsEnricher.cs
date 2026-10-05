using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore;

internal sealed class DelegateProblemDetailsEnricher(Action<ProblemDetails, HttpContext> enrich) : IProblemDetailsEnricher
{
    public void Enrich(ProblemDetailsContext context) => enrich(context.ProblemDetails, context.HttpContext);
}
