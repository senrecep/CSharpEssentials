using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore;

public class ResultEndpointFilterTests
{
    [Fact]
    public async Task InvokeAsync_WithSuccessResultT_Should_Return_Ok()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(42.ToResult())))!;

        var okResult = (Ok<object>)result;
        okResult.Value!.Should().Be(42);
    }

    [Fact]
    public async Task InvokeAsync_WithFailureResultT_Should_Return_ProblemResult()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(Result<int>.Failure(Error.NotFound("X", "Missing")))))!;

        var problem = (EnhancedProblemHttpResult)result;
        problem.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_WithSuccessResult_Should_Return_Ok()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(Result.Success())))!;

        result.Should().BeAssignableTo<Ok>();
    }

    [Fact]
    public async Task InvokeAsync_WithFailureResult_Should_Return_ProblemResult()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(Result.Failure(Error.Validation("V", "Invalid")))))!;

        var problem = (EnhancedProblemHttpResult)result;
        problem.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task InvokeAsync_WithNull_Should_Return_Null()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        object? result = await filter.InvokeAsync(context, _ => new ValueTask<object?>((object?)null));

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithPlainObject_Should_Return_Object()
    {
        var filter = new ResultEndpointFilter();
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>("hello")))!;

        result.Should().Be("hello");
    }

    [Fact]
    public async Task InvokeAsync_WithFailureResultTAndRegisteredMapper_Should_ResolveMapperFromRequestServices()
    {
        var filter = new ResultEndpointFilter();
        await using ServiceProvider services = new ServiceCollection()
            .AddSingleton<IResultErrorMapper, NotFoundErrorMapper>()
            .BuildServiceProvider();
        var context = CreateContext(services);

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(Result<int>.Failure(Error.NotFound("X", "Missing")))))!;

        var notFound = (NotFound<Error[]>)result;
        notFound.Value![0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_WithFailureResultAndScopedMapper_Should_ResolveMapperFromRequestScope()
    {
        var filter = new ResultEndpointFilter();
        await using ServiceProvider root = new ServiceCollection()
            .AddScoped<IResultErrorMapper, NotFoundErrorMapper>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        await using AsyncServiceScope scope = root.CreateAsyncScope();
        var context = CreateContext(scope.ServiceProvider);

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(Result.Failure(Error.Validation("V", "Invalid")))))!;

        var notFound = (NotFound<Error[]>)result;
        notFound.Value![0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task InvokeAsync_WithFailureResultAndNoMapperRegistered_Should_Return_ProblemResult()
    {
        var filter = new ResultEndpointFilter();
        await using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        var context = CreateContext(services);

        object result = (await filter.InvokeAsync(context, _ => new ValueTask<object?>(Result.Failure(Error.Conflict("C", "Conflict")))))!;

        ((EnhancedProblemHttpResult)result).StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    private static DefaultEndpointFilterInvocationContext CreateContext(IServiceProvider services) =>
        new(new DefaultHttpContext { RequestServices = services });

    private sealed class NotFoundErrorMapper : IResultErrorMapper
    {
        public Microsoft.AspNetCore.Http.IResult Map(Error[] errors) => TypedResults.NotFound(errors);
    }
}
