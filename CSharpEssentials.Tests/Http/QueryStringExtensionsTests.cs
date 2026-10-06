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

    [Fact]
    public void ToQueryString_FromObject_Should_Repeat_Key_For_Each_Collection_Item()
    {
        var obj = new { Ids = new[] { 1, 2, 3 }, Tags = new List<string?> { "a", null, "b" } };

        Result<string> result = obj.ToQueryString();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Ids=1&Ids=2&Ids=3&Tags=a&Tags=b");
    }

    [Fact]
    public void ToQueryString_FromObject_Should_Format_Collection_Items_With_Invariant_Culture()
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
        try
        {
            Result<string> result = new { Values = (IEnumerable<double>)[1.5, 2.25] }.ToQueryString();

            result.Value.Should().Be("Values=1.5&Values=2.25");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ToQueryString_FromObject_Should_Fail_When_SourceIsNull()
    {
        Result<string> result = ((object?)null).ToQueryString();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("QueryString.SourceRequired");
    }

    [Fact]
    public void ToQueryString_FromObject_Should_Throw_When_ConventionsAreNull()
    {
        Action act = () => new { Page = 1 }.ToQueryString(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("conventions");
    }

    [Fact]
    public void WithQueryString_Dictionary_Should_Fail_When_UriIsNull()
    {
        Result<Uri> result = ((Uri?)null).WithQueryString(new Dictionary<string, string?> { ["page"] = "1" });

        result.FirstError.Code.Should().Be("QueryString.UriRequired");
    }

    [Fact]
    public void WithQueryString_Dictionary_Should_Fail_When_AKeyIsEmpty()
    {
        Result<Uri> result = new Uri("https://test.com/api").WithQueryString(new Dictionary<string, string?> { [""] = "1" });

        result.FirstError.Code.Should().Be("QueryString.EmptyKey");
    }

    [Fact]
    public void WithQueryString_Object_Should_Fail_When_UriIsNull()
    {
        Result<Uri> result = ((Uri?)null).WithQueryString((object)new { Page = 1 });

        result.FirstError.Code.Should().Be("QueryString.UriRequired");
    }

    [Fact]
    public void WithQueryString_Object_Should_Fail_When_ParametersAreNull()
    {
        Result<Uri> result = new Uri("https://test.com/api").WithQueryString((object?)null);

        result.FirstError.Code.Should().Be("QueryString.ParametersRequired");
    }

    [Fact]
    public void WithQueryString_Object_Should_Fail_When_AnEnumPropertyIsUndefined()
    {
        Result<Uri> result = new Uri("https://test.com/api").WithQueryString((object)new { Status = (HttpOrderStatus)42 });

        result.FirstError.Code.Should().Be("QueryString.InvalidEnumValue");
    }

    [Fact]
    public void WithQueryString_NamedValue_Should_Fail_When_UriIsNull()
    {
        Result<Uri> result = ((Uri?)null).WithQueryString("status", (object?)HttpOrderStatus.Pending);

        result.FirstError.Code.Should().Be("QueryString.UriRequired");
    }

    [Fact]
    public void WithQueryString_NamedValue_Should_Fail_When_NameIsEmpty()
    {
        Result<Uri> result = new Uri("https://test.com/api").WithQueryString(string.Empty, (object?)HttpOrderStatus.Pending);

        result.FirstError.Code.Should().Be("QueryString.NameRequired");
    }

    [Fact]
    public void WithQueryString_NamedValue_Should_Fail_When_TheEnumValueIsUndefined()
    {
        Result<Uri> result = new Uri("https://test.com/api").WithQueryString("status", (object?)(HttpOrderStatus)42);

        result.FirstError.Code.Should().Be("QueryString.InvalidEnumValue");
    }

    [Fact]
    public void WithQueryString_NamedValue_Should_LeaveTheUriUnchanged_When_ValueIsNull()
    {
        Result<Uri> result = new Uri("https://test.com/api?page=1").WithQueryString("status", (object?)null);

        result.Value.Query.Should().Be("?page=1");
    }
}
