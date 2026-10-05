using Microsoft.AspNetCore.Http;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Adds or changes fields on every problem response: CSharpEssentials results, <see cref="GlobalExceptionHandler"/>,
/// status code pages and the framework exception handler. Enrichers run after the built-in enrichment, in
/// registration order. Register with <c>services.AddProblemDetailsEnricher&lt;T&gt;()</c>.
/// </summary>
public interface IProblemDetailsEnricher
{
    /// <summary>
    /// Enriches <see cref="ProblemDetailsContext.ProblemDetails"/>.
    /// </summary>
    void Enrich(ProblemDetailsContext context);
}
