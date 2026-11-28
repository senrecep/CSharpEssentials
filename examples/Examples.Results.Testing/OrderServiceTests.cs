using System.Net;
using System.Text;
using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Testing;

public sealed class OrderServiceTests
{
    private readonly OrderService _service = new();

    [Fact]
    public void Create_returns_the_order()
    {
        Order order = _service.Create(3, "Lamp").ShouldBeSuccess();

        Assert.Equal("Lamp", order.Name);
    }

    [Fact]
    public void Create_without_a_name_fails_with_a_validation_error()
    {
        Error[] errors = _service.Create(3, "").ShouldBeFailure();
        Error error = _service.Create(3, "").ShouldHaveError(ErrorType.Validation, OrderErrors.Codes.NameRequired);

        Assert.Single(errors);
        Assert.Equal("Name is required.", error.Description);
    }

    [Fact]
    public void Delete_of_a_missing_order_fails_with_not_found()
    {
        _service.Delete(1).ShouldBeSuccess();

        _service.Delete(1).ShouldHaveError(ErrorType.NotFound);
        _service.Delete(1).ShouldHaveError(OrderErrors.Codes.NotFound);
    }

    [Fact]
    public void Find_returns_a_maybe()
    {
        Order order = _service.Find(1).ShouldHaveValue();

        Assert.Equal("Book", order.Name);
        _service.Find(99).ShouldBeNone();
    }

    [Fact]
    public void Reserve_returns_typed_errors()
    {
        Order order = _service.Reserve(1).ShouldBeSuccess();
        ConflictError taken = _service.Reserve(2).ShouldBeFailureOf<ConflictError>();
        NotFoundError missing = _service.Reserve(99).ShouldBeFailureOf<NotFoundError>();

        Assert.Equal(1, order.Id);
        Assert.Equal(OrderErrors.Codes.Taken, taken.Code);
        _service.Reserve(99).ShouldHaveError(ErrorType.NotFound, missing.Code);
    }

    [Fact]
    public void A_wrong_case_throws_with_the_case_in_the_message()
    {
        var exception = Assert.Throws<ResultAssertionException>(() => _service.Reserve(2).ShouldBeFailureOf<NotFoundError>());

        Assert.Contains("Expected an error of type NotFoundError", exception.Message, StringComparison.Ordinal);
        Assert.Contains("ConflictError", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_failed_success_assertion_lists_every_error()
    {
        var exception = Assert.Throws<ResultAssertionException>(() => _service.Create(3, "").ShouldBeSuccess());

        Assert.Contains("[0] Validation Order.Name.Required: Name is required.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Validation_problem_has_every_error_key()
    {
        using HttpResponseMessage response = Problem(HttpStatusCode.BadRequest,
            """{"status":400,"title":"Validation Error","errors":[{"code":"Name.NotEmpty","description":"'Name' must not be empty."}]}""");

        JsonElement body = await response.ShouldBeValidationProblemAsync("Name.NotEmpty");

        Assert.Equal("Validation Error", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Problem_has_status_and_code()
    {
        using HttpResponseMessage response = Problem(HttpStatusCode.NotFound,
            """{"status":404,"title":"Not Found","errorCodes":["Order.NotFound"]}""");

        await response.ShouldBeProblemAsync(404, OrderErrors.Codes.NotFound);
    }

    [Fact]
    public async Task Wrong_status_throws_with_request_and_body()
    {
        using HttpResponseMessage response = Problem(HttpStatusCode.BadRequest, """{"status":400}""");

        var exception = await Assert.ThrowsAsync<ResultAssertionException>(() => response.ShouldBeProblemAsync(404));

        Assert.StartsWith("Expected status 404, but it was 400.", exception.Message, StringComparison.Ordinal);
        Assert.Contains("POST http://localhost/orders returned 400 BadRequest", exception.Message, StringComparison.Ordinal);
    }

    // In a real test the response comes from WebApplicationFactory's HttpClient.
    private static HttpResponseMessage Problem(HttpStatusCode status, string json) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/problem+json"),
        RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost/orders"),
    };
}
