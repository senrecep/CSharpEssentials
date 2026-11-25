using System.Net;
using System.Text;
using System.Text.Json;
using CSharpEssentials.ResultPattern.Testing;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results.Testing;

public class ProblemDetailsAssertionExtensionsTests
{
    private const string ListProblem = """{"status":400,"title":"Validation","errorCodes":["Name.Required"],"errors":[{"code":"Name.Required","description":"Name is required."}]}""";
    private const string ByFieldProblem = """{"status":400,"errors":{"name":["Name is required."],"lines[0].sku":["Sku is required."]}}""";

    [Fact]
    public async Task ShouldBeProblemAsync_Should_Return_Body_When_Status_And_Code_Match()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.BadRequest, ListProblem);

        JsonElement problem = await response.ShouldBeProblemAsync(400, "Name.Required");

        problem.GetProperty("title").GetString().Should().Be("Validation");
    }

    [Fact]
    public async Task ShouldBeProblemAsync_Should_Find_Code_In_Errors_When_ErrorCodes_Is_Absent()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.Conflict, """{"status":409,"errors":{"Order.Taken":["Taken."]}}""");

        Func<Task> act = async () => await response.ShouldBeProblemAsync(409, "Order.Taken");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldBeProblemAsync_Should_Show_Request_And_Body_When_Status_Differs()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.BadRequest, ListProblem);

        Func<Task> act = async () => await response.ShouldBeProblemAsync(404);

        (await act.Should().ThrowAsync<ResultAssertionException>())
            .Which.Message.Should().Contain("Expected status 404, but it was 400.")
            .And.Contain("POST http://localhost/orders returned 400 BadRequest")
            .And.Contain("Name is required.");
    }

    [Fact]
    public async Task ShouldBeProblemAsync_Should_Throw_When_Code_Is_Missing()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.BadRequest, ListProblem);

        Func<Task> act = async () => await response.ShouldBeProblemAsync(code: "Order.Taken");

        await act.Should().ThrowAsync<ResultAssertionException>().WithMessage("Expected the problem to carry error code Order.Taken.*");
    }

    [Fact]
    public async Task ShouldBeProblemAsync_Should_Throw_When_Content_Type_Is_Not_Problem()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.BadRequest, ListProblem, "application/json");

        Func<Task> act = async () => await response.ShouldBeProblemAsync();

        await act.Should().ThrowAsync<ResultAssertionException>().WithMessage("Expected content type application/problem+json, but it was application/json.*");
    }

    [Fact]
    public async Task ShouldBeProblemAsync_Should_Throw_When_Body_Is_Not_Json()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.InternalServerError, "oops");

        Func<Task> act = async () => await response.ShouldBeProblemAsync();

        (await act.Should().ThrowAsync<ResultAssertionException>())
            .Which.InnerException.Should().BeAssignableTo<JsonException>();
    }

    [Fact]
    public async Task ShouldBeValidationProblemAsync_Should_Accept_Field_Keys()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.BadRequest, ByFieldProblem);

        Func<Task> act = async () => await response.ShouldBeValidationProblemAsync("name", "lines[0].sku");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ShouldBeValidationProblemAsync_Should_List_Keys_When_One_Is_Missing()
    {
        using HttpResponseMessage response = Response(HttpStatusCode.BadRequest, ListProblem);

        Func<Task> act = async () => await response.ShouldBeValidationProblemAsync("Name.Required", "Quantity.Min");

        await act.Should().ThrowAsync<ResultAssertionException>()
            .WithMessage("Expected errors to have the keys Quantity.Min, but it has Name.Required.*");
    }

    private static HttpResponseMessage Response(HttpStatusCode status, string body, string mediaType = "application/problem+json") =>
        new(status)
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType),
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri("http://localhost/orders")),
        };
}
