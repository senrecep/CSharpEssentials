using System.Text;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

/// <summary>
/// Enum values bound through explicit <c>[FromRoute]</c>, <c>[FromHeader]</c> and <c>[FromBody]</c> attributes, on
/// Minimal API parameters and on MVC query model properties.
/// </summary>
public class EnumBindingSourceTests
{
    [Fact]
    public async Task MinimalApi_FromRoute_Should_NormalizeRouteValue()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/minimal/route/in_progress");

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task MinimalApi_FromRouteWithName_Should_UseAttributeNameAsErrorCode_When_ValueIsInvalid()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/minimal/renamed-route/garbage");

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("st");
    }

    [Fact]
    public async Task MinimalApi_FromHeaderWithName_Should_NormalizeHeaderValue()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/minimal/header", ("X-Status", "http_error"));

        response.Body.Should().Be("HTTPError");
    }

    [Fact]
    public async Task MinimalApi_FromHeaderWithName_Should_ReturnProblem_When_ValueIsInvalid()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/minimal/header", ("X-Status", "garbage"));

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("X-Status");
    }

    [Fact]
    public async Task MinimalApi_FromHeaderWithoutName_Should_UseParameterName()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/minimal/header-default", ("status", "in_progress"));

        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task MinimalApi_FromBody_Should_IgnoreQueryValueWithTheSameName()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await PostJsonAsync(host, "/minimal/body?status=garbage", "1");

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("InProgress");
    }

    [Fact]
    public async Task Mvc_QueryModelProperties_Should_NormalizeRenamedQueryHeaderAndRouteValues()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/mvc/dto/http_error?st=in_progress", ("X-Status", "in_progress"));

        response.Body.Should().Be("InProgress|InProgress|HTTPError|HTTPError");
    }

    [Fact]
    public async Task Mvc_QueryModelProperties_Should_ReturnErrorPerInvalidExplicitSource()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/mvc/dto/bad-route?st=bad-query&readOnly=bad-ignored", ("X-Status", "bad-header"));

        response.ShouldBeEnumBindingProblem().Select(error => error.Code).Should().BeEquivalentTo("st", "X-Status", "Kind");
    }

    [Fact]
    public async Task Mvc_QueryModelProperties_Should_ValidatePrefixedKey_And_SkipUnprefixedKey_When_PrefixIsPresent()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await GetAsync(host, "/mvc/dto/active?dto.st=garbage&st=in_progress", ("X-Status", "active"));

        response.ShouldBeEnumBindingProblem().Should().ContainSingle().Which.Code.Should().Be("dto.st");
    }

    [Fact]
    public async Task Mvc_FromBody_Should_IgnoreQueryValueWithTheSameName()
    {
        await using EnumBindingSourceHost host = await EnumBindingSourceHost.StartAsync();

        var response = await PostJsonAsync(host, "/mvc/body?status=garbage", "1");

        response.Status.Should().Be(200, response.Body);
        response.Body.Should().Be("InProgress");
    }

    private static async Task<(int Status, string Body, string? ContentType)> GetAsync(
        EnumBindingSourceHost host,
        string url,
        params (string Name, string Value)[] headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        foreach ((string name, string value) in headers)
            request.Headers.TryAddWithoutValidation(name, value);
        return await host.SendAsync(request);
    }

    private static async Task<(int Status, string Body, string? ContentType)> PostJsonAsync(EnumBindingSourceHost host, string url, string json)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        return await host.SendAsync(request);
    }
}
