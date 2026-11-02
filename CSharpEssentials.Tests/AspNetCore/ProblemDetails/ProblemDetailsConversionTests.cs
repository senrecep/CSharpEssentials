using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

public class ProblemDetailsConversionTests
{
    public static TheoryData<Error, int, string> TitleCases => new()
    {
        { Error.Validation("v", "invalid"), 400, "Validation Error" },
        { Error.Unauthorized("u", "login"), 401, "Unauthorized" },
        { Error.Forbidden("f", "denied"), 403, "Forbidden" },
        { Error.NotFound("n", "missing"), 404, "Not Found" },
        { Error.Conflict("c", "exists"), 409, "Conflict" },
        { Error.Failure("fail", "failed"), 500, "Server Failure" },
        { Error.Unexpected("unexpected", "boom"), 500, "Unexpected Error" },
        { Error.Exception(new InvalidOperationException("unknown"), ErrorType.Unknown), 500, "Unknown Error" },
    };

    [Theory]
    [MemberData(nameof(TitleCases))]
    public void ToProblemDetails_Should_UseErrorTypeTitle_When_StatusMatchesErrorType(Error error, int status, string title)
    {
        EnhancedProblemDetails problem = error.ToProblemDetails();

        problem.Status.Should().Be(status);
        problem.Title.Should().Be(title);
    }

    [Fact]
    public void ToActionResult_WithResult_Should_Throw_When_ResultIsSuccess()
    {
        Result result = Result.Success();

        Action action = () => result.ToActionResult();

        action.Should().Throw<InvalidOperationException>().WithMessage("*successful result*action result*");
    }

    [Fact]
    public void ToProblemResult_WithResult_Should_Throw_When_ResultIsSuccess()
    {
        Result result = Result.Success();

        Action action = () => result.ToProblemResult();

        action.Should().Throw<InvalidOperationException>().WithMessage("*successful result*problem result*");
    }

    [Fact]
    public void ControllerProblem_WithResult_Should_UseControllerRequestPathAsInstance()
    {
        ProblemProbeController controller = CreateController("/orders/7");

        IActionResult result = controller.Problem(Result.Failure(Error.NotFound("order.notFound", "Order not found")));

        EnhancedProblemDetails problem = ProblemOf(result);
        problem.Status.Should().Be(404);
        problem.Instance.Should().Be("/orders/7");
        problem.ErrorCodes.Should().Equal("order.notFound");
    }

    [Fact]
    public void ControllerProblem_WithError_Should_HonorStatusCodeAndExtensions()
    {
        ProblemProbeController controller = CreateController("/orders");

        IActionResult result = controller.Problem(
            Error.Conflict("order.conflict", "Already shipped"),
            new ErrorMetadata("tenant", "t1"),
            StatusCodes.Status422UnprocessableEntity);

        EnhancedProblemDetails problem = ProblemOf(result);
        problem.Status.Should().Be(422);
        problem.Extensions.Should().ContainKey("tenant").WhoseValue.Should().Be("t1");
        problem.Instance.Should().Be("/orders");
    }

    [Fact]
    public void ControllerProblem_WithErrors_Should_UseMostSevereErrorStatus()
    {
        ProblemProbeController controller = CreateController("/orders");

        IActionResult result = controller.Problem(
        [
            Error.Validation("name.required", "Name is required"),
            Error.NotFound("order.notFound", "Order not found"),
        ]);

        EnhancedProblemDetails problem = ProblemOf(result);
        problem.Status.Should().Be(404);
        problem.ErrorCodes.Should().BeEquivalentTo(["name.required", "order.notFound"]);
    }

    [Fact]
    public void ConfigureModelValidatorResponse_Should_AddValidateModelFilter_And_SuppressAutomaticResponse()
    {
        ServiceCollection services = new();
        services.ConfigureModelValidatorResponse();
        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<MvcOptions>>().Value.Filters
            .Should().ContainSingle(filter => filter is ValidateModelAttribute);
        provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value.SuppressModelStateInvalidFilter.Should().BeTrue();
    }

    [Fact]
    public void InvalidModelStateResponse_Should_WriteGenericRequestError_When_ModelStateHasNoErrors()
    {
        using ServiceProvider provider = CreateInvalidModelStateServices();

        EnhancedProblemDetails problem = InvokeInvalidModelStateFactory(provider, new ModelStateDictionary());

        problem.Status.Should().Be(400);
        problem.Errors.Should().ContainSingle().Which.Should().Match<Error>(error =>
            error.Code == "request" && error.Description == "The request is invalid." && error.Type == ErrorType.Validation);
    }

    [Fact]
    public void InvalidModelStateResponse_Should_UseRequestCodeAndGenericDescription_When_KeyAndMessageAreEmpty()
    {
        using ServiceProvider provider = CreateInvalidModelStateServices();
        ModelStateDictionary modelState = new();
        modelState.AddModelError(string.Empty, string.Empty);
        modelState.AddModelError("name", "Name is required");

        EnhancedProblemDetails problem = InvokeInvalidModelStateFactory(provider, modelState);

        problem.Errors.Select(error => (error.Code, error.Description)).Should().BeEquivalentTo(
        [
            ("request", "The request is invalid."),
            ("name", "Name is required"),
        ]);
    }

    private static ServiceProvider CreateInvalidModelStateServices()
    {
        ServiceCollection services = new();
        services.AddOptions();
        services.ConfigureInvalidModelStateResponse();
        return services.BuildServiceProvider();
    }

    private static EnhancedProblemDetails InvokeInvalidModelStateFactory(IServiceProvider provider, ModelStateDictionary modelState)
    {
        ApiBehaviorOptions options = provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        DefaultHttpContext httpContext = new() { RequestServices = provider };
        ActionContext context = new(httpContext, new RouteData(), new ActionDescriptor(), modelState);

        return ProblemOf(options.InvalidModelStateResponseFactory(context));
    }

    private static ProblemProbeController CreateController(string path)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = path;
        return new ProblemProbeController { ControllerContext = new ControllerContext { HttpContext = httpContext } };
    }

    private static EnhancedProblemDetails ProblemOf(IActionResult result) =>
        result.Should().BeOfType<EnhancedProblemObjectResult>().Subject.Value.Should().BeOfType<EnhancedProblemDetails>().Subject;

    internal sealed class ProblemProbeController : ControllerBase;
}
