using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

/// <summary>
/// Minimal API specific enum binding behavior: <c>[AsParameters]</c> classes and records.
/// </summary>
public class EnumBindingMinimalApiTests
{
    private static Task<EnumBindingHost> Start() => EnumBindingHost.StartAsync(EbHostKind.MinimalApi);

    [Fact]
    public async Task AsParametersClass_Should_NormalizeProperties()
    {
        await using EnumBindingHost host = await Start();

        var response = await host.GetAsync("/asparams-class?status=in_progress&p=read,write");

        response.Body.Should().Be("InProgress|Read, Write");
    }

    [Fact]
    public async Task AsParametersClass_Should_ReturnErrorPerInvalidProperty_UsingPropertyNameAndFromQueryName()
    {
        await using EnumBindingHost host = await Start();

        var response = await host.GetAsync("/asparams-class?status=x&p=y");

        response.ShouldBeEnumBindingProblem().Select(e => e.Code).Should().Equal("Status", "p");
    }

    [Fact]
    public async Task AsParametersRecord_Should_HonorFromQueryNameOnConstructorParameter()
    {
        await using EnumBindingHost host = await Start();

        var response = await host.GetAsync("/asparams-record?st=in_progress&other=http_error");

        response.Body.Should().Be("InProgress|HTTPError");
    }

    [Fact]
    public async Task AsParametersRecord_Should_BindNullableNull_When_KeyIsMissing()
    {
        await using EnumBindingHost host = await Start();

        var response = await host.GetAsync("/asparams-record?st=active");

        response.Body.Should().Be("Active|null");
    }

    [Fact]
    public async Task AsParametersRecord_Should_UseConstructorAttributeNameAsErrorCode_When_ValueIsInvalid()
    {
        await using EnumBindingHost host = await Start();

        var response = await host.GetAsync("/asparams-record?st=garbage&other=active");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("st");
    }

    [Fact]
    public async Task AsParametersRecord_Should_BindNullableNull_When_ValueIsEmpty()
    {
        await using EnumBindingHost host = await Start();

        var response = await host.GetAsync("/asparams-record?st=active&other=");

        response.Body.Should().Be("Active|null");
    }
}
