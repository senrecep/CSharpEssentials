using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>Returns a failed <c>Result</c> through <c>ToActionResult</c>: an in-flight idempotency conflict or a business conflict.</summary>
[ApiController]
[Route("mvc/result")]
internal sealed class IdempotencyResultController : ControllerBase
{
    [HttpPost("in-flight")]
    public IActionResult InFlight() =>
        Result<int>.Failure(Error.Conflict(IdempotencyErrorCodes.InFlight, "Still processing.")).ToActionResult(HttpContext);

    [HttpPost("business")]
    public IActionResult Business() =>
        Result<int>.Failure(Error.Conflict("Order.Paid", "The order is already paid.")).ToActionResult(HttpContext);
}
