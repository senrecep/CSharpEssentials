using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.AspNetCore;

public class ValidateModelAttributeTests
{
    [Fact]
    public void ValidateModelAttribute_ShouldHaveCorrectAttributeUsage()
    {
        Type attributeType = typeof(ValidateModelAttribute);
        var usageAttribute = attributeType.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;

        usageAttribute?.Should().NotBeNull();
        usageAttribute?.ValidOn.Should().HaveFlag(AttributeTargets.Class);
        usageAttribute?.ValidOn.Should().HaveFlag(AttributeTargets.Method);
    }

    [Fact]
    public void OnActionExecuting_WithValidModelState_ShouldNotSetResult()
    {
        ValidateModelAttribute attribute = new();
        ActionExecutingContext context = CreateActionExecutingContext(isValid: true);

        attribute.OnActionExecuting(context);

        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnActionExecuting_WithInvalidModelState_ShouldSetResult()
    {
        ValidateModelAttribute attribute = new();
        ActionExecutingContext context = CreateActionExecutingContext(isValid: false);
        context.ModelState.AddModelError("Name", "Name is required");

        attribute.OnActionExecuting(context);

        context.Result.Should().NotBeNull();
    }

    [Fact]
    public void OnActionExecuting_WithMultipleErrors_ShouldSetResult()
    {
        ValidateModelAttribute attribute = new();
        ActionExecutingContext context = CreateActionExecutingContext(isValid: false);
        context.ModelState.AddModelError("Name", "Name is required");
        context.ModelState.AddModelError("Email", "Email is invalid");

        attribute.OnActionExecuting(context);

        context.Result.Should().NotBeNull();
    }

    [Fact]
    public void ValidateModelAttribute_ShouldBeActionFilterAttribute()
    {
        ValidateModelAttribute attribute = new();

        attribute.Should().BeAssignableTo<ActionFilterAttribute>();
    }

    private static ActionExecutingContext CreateActionExecutingContext(bool isValid)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/test";

        ActionContext actionContext = new(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        if (!isValid)
        {
            actionContext.ModelState.AddModelError("test", "test error");
        }

        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: null!);
    }
}

