using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.RequestResponseLogging;
using CSharpEssentials.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using ErrorType = CSharpEssentials.Errors.Error;

namespace Examples.RequestResponseLogging.Controllers;

/// <summary>
/// Demo controller with various endpoints to show request/response logging behavior.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    /// <summary>
    /// GET /api/demo/hello
    /// Simple successful request. The middleware will log the 200 OK response.
    /// </summary>
    [HttpGet("hello")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Hello()
    {
        return Ok(new { Message = "Hello, World!", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// POST /api/demo/echo
    /// Echoes back the request body. Demonstrates request body logging.
    /// </summary>
    [HttpPost("echo")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Echo([FromBody] EchoRequest request)
    {
        return Ok(new
        {
            Received = request,
            ServerTime = DateTime.UtcNow
        });
    }

    /// <summary>
    /// GET /api/demo/error
    /// Simulates a server error. The middleware will log the 500 response
    /// and the global exception handler will format it as ProblemDetails.
    /// </summary>
    [HttpGet("error")]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowError()
    {
        throw new InvalidOperationException("Something went wrong in the server!");
    }

    /// <summary>
    /// GET /api/demo/notfound
    /// Returns a 404 using the Result pattern.
    /// </summary>
    [HttpGet("notfound")]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public IActionResult NotFoundDemo()
    {
        Result result = Result.Failure(ErrorType.NotFound("Demo resource was not found."));
        return result.Match(
            onSuccess: () => Ok(),
            onFailure: errors => errors.ToActionResult()
        );
    }

    /// <summary>
    /// POST /api/demo/validation
    /// Returns validation errors when the input is invalid.
    /// </summary>
    [HttpPost("validation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public IActionResult ValidationDemo([FromBody] CreateUserRequest request)
    {
        List<ErrorType> errors = new();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(ErrorType.Validation("Name", "Name is required."));

        if (request.Name?.Length > 50)
            errors.Add(ErrorType.Validation("Name", "Name must not exceed 50 characters."));

        if (request.Age is < 18 or > 120)
            errors.Add(ErrorType.Validation("Age", "Age must be between 18 and 120."));

        if (errors.Count > 0)
        {
            return Result.Failure(errors.ToArray()).Match(
                onSuccess: () => Ok(),
                onFailure: e => e.ToActionResult()
            );
        }

        return Ok(new { Message = $"User '{request.Name}' created successfully." });
    }

    /// <summary>
    /// GET /api/demo/slow
    /// Simulates a slow endpoint. The middleware logs the duration.
    /// </summary>
    [HttpGet("slow")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Slow()
    {
        await Task.Delay(1500);
        return Ok(new { Message = "Slow response completed", DurationMs = 1500 });
    }

    /// <summary>
    /// GET /api/demo/skip-request
    /// This endpoint skips request logging only.
    /// </summary>
    [HttpGet("skip-request")]
    [SkipRequestLogging]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult SkipRequestLogging()
    {
        return Ok(new { Message = "Request was not logged, but response is." });
    }

    /// <summary>
    /// GET /api/demo/skip-response
    /// This endpoint skips response logging only.
    /// </summary>
    [HttpGet("skip-response")]
    [SkipResponseLogging]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult SkipResponseLogging()
    {
        return Ok(new { Message = "Response was not logged, but request is." });
    }

    /// <summary>
    /// GET /api/demo/skip-all
    /// This endpoint skips both request and response logging.
    /// </summary>
    [HttpGet("skip-all")]
    [SkipRequestResponseLogging]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult SkipAllLogging()
    {
        return Ok(new { Message = "Neither request nor response was logged." });
    }
}

public sealed record EchoRequest(string Message, int RepeatCount);
public sealed record CreateUserRequest(string Name, int Age);
