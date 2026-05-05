using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class QueryStringExtensionsTests
{
    [Fact]
    public void ToQueryString_FromDictionary_Should_Return_Encoded_String()
    {
        var parameters = new Dictionary<string, string?>
        {
            { "name", "Alice" },
            { "age", "30" },
            { "nullKey", null }
        };

        Result<string> result = parameters.ToQueryString();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("name=Alice");
        result.Value.Should().Contain("age=30");
        result.Value.Should().NotContain("nullKey");
    }

    [Fact]
    public void ToQueryString_FromObject_Should_Return_Encoded_String()
    {
        var obj = new { Name = "Bob", Age = 25 };

        Result<string> result = obj.ToQueryString();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Name=Bob");
        result.Value.Should().Contain("Age=25");
    }

    [Fact]
    public void WithQueryString_Dictionary_Should_Append_To_Uri()
    {
        var uri = new Uri("https://test.com/api");
        var parameters = new Dictionary<string, string?> { { "page", "1" } };

        Result<Uri> result = uri.WithQueryString(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Query.Should().Contain("page=1");
    }

    [Fact]
    public void WithQueryString_Object_Should_Append_To_Uri()
    {
        var uri = new Uri("https://test.com/api");

        Result<Uri> result = uri.WithQueryString(new { limit = "10" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Query.Should().Contain("limit=10");
    }

    [Fact]
    public void WithQueryString_Single_Should_Append_To_Uri()
    {
        var uri = new Uri("https://test.com/api");

        Result<Uri> result = uri.WithQueryString("sort", "desc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Query.Should().Contain("sort=desc");
    }

    [Fact]
    public void WithQueryString_Should_Merge_Existing_Query()
    {
        var uri = new Uri("https://test.com/api?existing=true");

        Result<Uri> result = uri.WithQueryString("new", "value");

        result.IsSuccess.Should().BeTrue();
        result.Value.Query.Should().Contain("existing=true");
        result.Value.Query.Should().Contain("new=value");
    }

    [Fact]
    public void WithQueryString_NullUri_Should_Return_Failure()
    {
        Uri? uri = null;

        Result<Uri> result = uri!.WithQueryString("key", "value");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void WithQueryString_EmptyName_Should_Return_Failure()
    {
        var uri = new Uri("https://test.com/api");

        Result<Uri> result = uri.WithQueryString("", "value");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToQueryString_EmptyKey_Should_Return_Failure()
    {
        var parameters = new Dictionary<string, string?> { { "", "value" } };

        Result<string> result = parameters.ToQueryString();

        result.IsFailure.Should().BeTrue();
    }
}
