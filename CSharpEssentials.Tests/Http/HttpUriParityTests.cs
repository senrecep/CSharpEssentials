using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpUriParityTests
{
    [Theory]
    [InlineData("builder-noquery", "https://api.test/users", "https://api.test/users", "https://api.test/users")]
    [InlineData("builder-query", "https://api.test/users", "https://api.test/users?q=1 2&n=3", "https://api.test/users?q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test/users", "https://api.test/users?q=1 2", "https://api.test/users?q=1%202")]
    [InlineData("builder-noquery", "https://api.test/users?a=1", "https://api.test/users?a=1", "https://api.test/users?a=1")]
    [InlineData("builder-query", "https://api.test/users?a=1", "https://api.test/users?a=1&q=1 2&n=3", "https://api.test/users?a=1&q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test/users?a=1", "https://api.test/users?a=1&q=1 2", "https://api.test/users?a=1&q=1%202")]
    [InlineData("builder-noquery", "https://api.test/users?", "https://api.test/users?", "https://api.test/users?")]
    [InlineData("builder-query", "https://api.test/users?", "https://api.test/users?&q=1 2&n=3", "https://api.test/users?&q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test/users?", "https://api.test/users?&q=1 2", "https://api.test/users?&q=1%202")]
    [InlineData("builder-noquery", "https://api.test/users?a=1&", "https://api.test/users?a=1&", "https://api.test/users?a=1&")]
    [InlineData("builder-query", "https://api.test/users?a=1&", "https://api.test/users?a=1&&q=1 2&n=3", "https://api.test/users?a=1&&q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test/users?a=1&", "https://api.test/users?a=1&&q=1 2", "https://api.test/users?a=1&&q=1%202")]
    [InlineData("builder-noquery", "https://api.test/users#frag", "https://api.test/users#frag", "https://api.test/users#frag")]
    [InlineData("builder-query", "https://api.test/users#frag", "https://api.test/users?q=1 2&n=3#frag", "https://api.test/users?q=1%202&n=3#frag")]
    [InlineData("withquerystring", "https://api.test/users#frag", "https://api.test/users?q=1 2#frag", "https://api.test/users?q=1%202#frag")]
    [InlineData("builder-noquery", "https://api.test/users?a=1#frag", "https://api.test/users?a=1#frag", "https://api.test/users?a=1#frag")]
    [InlineData("builder-query", "https://api.test/users?a=1#frag", "https://api.test/users?a=1&q=1 2&n=3#frag", "https://api.test/users?a=1&q=1%202&n=3#frag")]
    [InlineData("withquerystring", "https://api.test/users?a=1#frag", "https://api.test/users?a=1&q=1 2#frag", "https://api.test/users?a=1&q=1%202#frag")]
    [InlineData("builder-noquery", "https://api.test:8443/users", "https://api.test:8443/users", "https://api.test:8443/users")]
    [InlineData("builder-query", "https://api.test:8443/users", "https://api.test:8443/users?q=1 2&n=3", "https://api.test:8443/users?q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test:8443/users", "https://api.test:8443/users?q=1 2", "https://api.test:8443/users?q=1%202")]
    [InlineData("builder-noquery", "https://api.test:443/users", "https://api.test/users", "https://api.test/users")]
    [InlineData("builder-query", "https://api.test:443/users", "https://api.test/users?q=1 2&n=3", "https://api.test/users?q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test:443/users", "https://api.test/users?q=1 2", "https://api.test/users?q=1%202")]
    [InlineData("builder-noquery", "https://user:pw@api.test/users", "https://user:pw@api.test/users", "https://user:pw@api.test/users")]
    [InlineData("builder-query", "https://user:pw@api.test/users", "https://user:pw@api.test/users?q=1 2&n=3", "https://user:pw@api.test/users?q=1%202&n=3")]
    [InlineData("withquerystring", "https://user:pw@api.test/users", "https://user:pw@api.test/users?q=1 2", "https://user:pw@api.test/users?q=1%202")]
    [InlineData("builder-noquery", "https://api.test/a%20b/users", "https://api.test/a b/users", "https://api.test/a%20b/users")]
    [InlineData("builder-query", "https://api.test/a%20b/users", "https://api.test/a b/users?q=1 2&n=3", "https://api.test/a%20b/users?q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test/a%20b/users", "https://api.test/a b/users?q=1 2", "https://api.test/a%20b/users?q=1%202")]
    [InlineData("builder-noquery", "https://API.Test/Users", "https://api.test/Users", "https://api.test/Users")]
    [InlineData("builder-query", "https://API.Test/Users", "https://api.test/Users?q=1 2&n=3", "https://api.test/Users?q=1%202&n=3")]
    [InlineData("withquerystring", "https://API.Test/Users", "https://api.test/Users?q=1 2", "https://api.test/Users?q=1%202")]
    [InlineData("builder-noquery", "https://bücher.test/x", "https://bücher.test/x", "https://bücher.test/x")]
    [InlineData("builder-query", "https://bücher.test/x", "https://bücher.test/x?q=1 2&n=3", "https://bücher.test/x?q=1%202&n=3")]
    [InlineData("withquerystring", "https://bücher.test/x", "https://bücher.test/x?q=1 2", "https://bücher.test/x?q=1%202")]
    [InlineData("builder-noquery", "https://api.test/a b", "https://api.test/a b", "https://api.test/a%20b")]
    [InlineData("builder-query", "https://api.test/a b", "https://api.test/a b?q=1 2&n=3", "https://api.test/a%20b?q=1%202&n=3")]
    [InlineData("withquerystring", "https://api.test/a b", "https://api.test/a b?q=1 2", "https://api.test/a%20b?q=1%202")]
    [InlineData("builder-noquery", " https://api.test/users ", "https://api.test/users", "https://api.test/users")]
    [InlineData("builder-query", " https://api.test/users ", "https://api.test/users?q=1 2&n=3", "https://api.test/users?q=1%202&n=3")]
    [InlineData("withquerystring", " https://api.test/users ", "https://api.test/users?q=1 2", "https://api.test/users?q=1%202")]
    [InlineData("builder-noquery", "/users", "/users", "-")]
    [InlineData("builder-query", "/users", "/users?q=1%202&n=3", "-")]
    [InlineData("withquerystring", "/users", "/users?q=1%202", "-")]
    [InlineData("builder-noquery", "/users?a=1", "/users?a=1", "-")]
    [InlineData("builder-query", "/users?a=1", "/users?a=1&q=1%202&n=3", "-")]
    [InlineData("withquerystring", "/users?a=1", "/users?a=1&q=1%202", "-")]
    [InlineData("builder-noquery", "/users?a=1&", "/users?a=1&", "-")]
    [InlineData("builder-query", "/users?a=1&", "/users?a=1&q=1%202&n=3", "-")]
    [InlineData("withquerystring", "/users?a=1&", "/users?a=1&q=1%202", "-")]
    [InlineData("builder-noquery", "/users#f", "/users#f", "-")]
    [InlineData("builder-query", "/users#f", "/users?q=1%202&n=3#f", "-")]
    [InlineData("withquerystring", "/users#f", "/users?q=1%202#f", "-")]
    [InlineData("builder-noquery", "users", "users", "-")]
    [InlineData("builder-query", "users", "users?q=1%202&n=3", "-")]
    [InlineData("withquerystring", "users", "users?q=1%202", "-")]
    [InlineData("builder-noquery", "/users?", "/users?", "-")]
    [InlineData("builder-query", "/users?", "/users?q=1%202&n=3", "-")]
    [InlineData("withquerystring", "/users?", "/users?q=1%202", "-")]
    [InlineData("builder-noquery", "https://api.test/users/{id}", "https://api.test/users/4 2", "https://api.test/users/4%202")]
    [InlineData("builder-query", "https://api.test/users/{id}", "https://api.test/users/4 2?q=1 2&n=3", "https://api.test/users/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "/users/{id}", "/users/4%202", "-")]
    [InlineData("builder-query", "/users/{id}", "/users/4%202?q=1%202&n=3", "-")]
    [InlineData("builder-noquery", "https://api.test/users/{id}?a=1#f", "https://api.test/users/4 2?a=1#f", "https://api.test/users/4%202?a=1#f")]
    [InlineData("builder-query", "https://api.test/users/{id}?a=1#f", "https://api.test/users/4 2?a=1&q=1 2&n=3#f", "https://api.test/users/4%202?a=1&q=1%202&n=3#f")]
    [InlineData("builder-noquery", " https://api.test/users/{id} ", "https://api.test/users/4 2", "https://api.test/users/4%202")]
    [InlineData("builder-query", " https://api.test/users/{id} ", "https://api.test/users/4 2?q=1 2&n=3", "https://api.test/users/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "https://api.test/users/{id}?", "https://api.test/users/4 2?", "https://api.test/users/4%202?")]
    [InlineData("builder-query", "https://api.test/users/{id}?", "https://api.test/users/4 2?&q=1 2&n=3", "https://api.test/users/4%202?&q=1%202&n=3")]
    [InlineData("builder-noquery", "https://api.test/users/{id}?a=1&", "https://api.test/users/4 2?a=1&", "https://api.test/users/4%202?a=1&")]
    [InlineData("builder-query", "https://api.test/users/{id}?a=1&", "https://api.test/users/4 2?a=1&&q=1 2&n=3", "https://api.test/users/4%202?a=1&&q=1%202&n=3")]
    [InlineData("builder-noquery", "https://api.test/users/{id}#frag", "https://api.test/users/4 2#frag", "https://api.test/users/4%202#frag")]
    [InlineData("builder-query", "https://api.test/users/{id}#frag", "https://api.test/users/4 2?q=1 2&n=3#frag", "https://api.test/users/4%202?q=1%202&n=3#frag")]
    [InlineData("builder-noquery", "https://api.test:443/users/{id}", "https://api.test/users/4 2", "https://api.test/users/4%202")]
    [InlineData("builder-query", "https://api.test:443/users/{id}", "https://api.test/users/4 2?q=1 2&n=3", "https://api.test/users/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "https://user:pw@api.test/users/{id}", "https://user:pw@api.test/users/4 2", "https://user:pw@api.test/users/4%202")]
    [InlineData("builder-query", "https://user:pw@api.test/users/{id}", "https://user:pw@api.test/users/4 2?q=1 2&n=3", "https://user:pw@api.test/users/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "https://api.test/a b/{id}", "https://api.test/a b/4 2", "https://api.test/a%20b/4%202")]
    [InlineData("builder-query", "https://api.test/a b/{id}", "https://api.test/a b/4 2?q=1 2&n=3", "https://api.test/a%20b/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "https://api.test/ü/{id}", "https://api.test/ü/4 2", "https://api.test/%C3%BC/4%202")]
    [InlineData("builder-query", "https://api.test/ü/{id}", "https://api.test/ü/4 2?q=1 2&n=3", "https://api.test/%C3%BC/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "https://API.Test/Users/{id}", "https://api.test/Users/4 2", "https://api.test/Users/4%202")]
    [InlineData("builder-query", "https://API.Test/Users/{id}", "https://api.test/Users/4 2?q=1 2&n=3", "https://api.test/Users/4%202?q=1%202&n=3")]
    [InlineData("builder-noquery", "/users/{id}?", "/users/4%202?", "-")]
    [InlineData("builder-query", "/users/{id}?", "/users/4%202?q=1%202&n=3", "-")]
    [InlineData("builder-noquery", "/users/{id}?a=1&", "/users/4%202?a=1&", "-")]
    [InlineData("builder-query", "/users/{id}?a=1&", "/users/4%202?a=1&q=1%202&n=3", "-")]
    [InlineData("builder-noquery", "/users/{id}#f", "/users/4%202#f", "-")]
    [InlineData("builder-query", "/users/{id}#f", "/users/4%202?q=1%202&n=3#f", "-")]
    public void Built_Uri_Should_Match_The_Expected_Form(string kind, string input, string expected, string expectedAbsolute)
    {
        Uri uri = kind switch
        {
            "builder-noquery" => Builder(input).Build().Value.RequestUri!,
            "builder-query" => Builder(input).WithQuery("q", "1 2").WithQuery("n", 3).Build().Value.RequestUri!,
            _ => new Uri(input, UriKind.RelativeOrAbsolute).WithQueryString("q", "1 2").Value,
        };

        uri.ToString().Should().Be(expected);
        (uri.IsAbsoluteUri ? uri.AbsoluteUri : "-").Should().Be(expectedAbsolute);
    }

    private static HttpRequestBuilder Builder(string input)
    {
        HttpRequestBuilder builder = HttpRequestBuilder.Get(input);
        return input.Contains("{id}", StringComparison.Ordinal) ? builder.WithRoute("id", "4 2") : builder;
    }
}
