using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.RequestResponseLogging;
using Microsoft.AspNetCore.Mvc;

namespace Examples.RequestResponseLogging.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    [HttpGet("hello")]
    public IActionResult Hello() => Ok(new { message = "Hello" });

    [HttpPost("echo")]
    public IActionResult Echo(EchoRequest request) => Ok(new { received = request });

    [HttpGet("notfound")]
    public IActionResult NotFoundDemo() => Error.NotFound("demo.not_found", "Demo resource was not found.").ToActionResult(HttpContext);

    [HttpGet("error")]
    public IActionResult Fail() => throw new InvalidOperationException("Something went wrong.");

    [HttpGet("slow")]
    public async Task<IActionResult> Slow()
    {
        await Task.Delay(300);
        return Ok(new { message = "Slow response" });
    }

    [HttpPost("login")]
    [SkipRequestLogging]
    public IActionResult Login(LoginRequest request) => Ok(new { token = "not-a-real-token" });

    [HttpGet("export")]
    [SkipResponseLogging]
    public IActionResult Export() => Ok(Enumerable.Range(1, 5));

    [HttpPost("secret")]
    [SkipRequestResponseLogging]
    public IActionResult Secret(LoginRequest request) => Ok(new { stored = true });
}

public sealed record EchoRequest(string Message, int? RepeatCount);

public sealed record LoginRequest(string User, string Password);
