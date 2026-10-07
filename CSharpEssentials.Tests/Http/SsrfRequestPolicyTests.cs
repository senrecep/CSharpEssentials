using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public sealed class SsrfRequestPolicyTests
{
    [Theory]
    [InlineData("https://example.com/", true)]
    [InlineData("https://example.com:443/", true)]
    [InlineData("https://example.com:8443/", false)]
    [InlineData("http://example.com/", false)]
    [InlineData("ftp://example.com/", false)]
    [InlineData("file:///etc/passwd", false)]
    [InlineData("gopher://example.com:70/", false)]
    public void IsAllowed_DefaultOptions_Should_Allow_Only_Https_On_443(string address, bool expected)
    {
        var policy = new DefaultOutboundRequestPolicy(new SsrfGuardOptions());

        policy.IsAllowed(new Uri(address)).Should().Be(expected);
    }

    [Theory]
    [InlineData("http://example.com/", true)]
    [InlineData("https://example.com/", true)]
    [InlineData("http://example.com:8080/", false)]
    [InlineData("ws://example.com/", false)]
    public void IsAllowed_AllowHttp_Should_Allow_Port_80(string address, bool expected)
    {
        var policy = new DefaultOutboundRequestPolicy(new SsrfGuardOptions { AllowHttp = true });

        policy.IsAllowed(new Uri(address)).Should().Be(expected);
    }

    [Theory]
    [InlineData("https://example.com:8443/", true)]
    [InlineData("https://example.com/", false)]
    [InlineData("http://example.com:8443/", false)]
    public void IsAllowed_CustomPorts_Should_Replace_Default_Ports(string address, bool expected)
    {
        var options = new SsrfGuardOptions();
        options.AllowedPorts.Add(8443);
        var policy = new DefaultOutboundRequestPolicy(options);

        policy.IsAllowed(new Uri(address)).Should().Be(expected);
    }

    [Fact]
    public void IsAllowed_RelativeUri_Should_Return_False()
    {
        var policy = new DefaultOutboundRequestPolicy(new SsrfGuardOptions());

        policy.IsAllowed(new Uri("/path", UriKind.Relative)).Should().BeFalse();
    }

    [Fact]
    public void Options_Defaults_Should_Have_No_Limits_And_Three_Redirects()
    {
        var options = new SsrfGuardOptions();

        options.AllowHttp.Should().BeFalse();
        options.MaxRedirects.Should().Be(3);
        options.MaxResponseContentLength.Should().BeNull();
        options.Timeout.Should().BeNull();
        options.AllowedPorts.Should().BeEmpty();
    }

    [Fact]
    public void Options_InvalidValues_Should_Throw()
    {
        var options = new SsrfGuardOptions();

        ((Action)(() => options.MaxRedirects = -1)).Should().Throw<ArgumentOutOfRangeException>();
        ((Action)(() => options.MaxResponseContentLength = 0)).Should().Throw<ArgumentOutOfRangeException>();
        ((Action)(() => options.Timeout = TimeSpan.Zero)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Options_NullLimits_Should_Be_Accepted()
    {
        var options = new SsrfGuardOptions { MaxResponseContentLength = 10, Timeout = TimeSpan.FromSeconds(1) };

        options.MaxResponseContentLength = null;
        options.Timeout = null;

        options.MaxResponseContentLength.Should().BeNull();
        options.Timeout.Should().BeNull();
    }
}
