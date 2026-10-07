using System.Net;
using CSharpEssentials.Http;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public sealed class SsrfAddressPolicyTests
{
    private static readonly Uri RequestUri = new("https://example.com/");

    [Theory]
    [InlineData("0.0.0.0")]
    [InlineData("0.255.255.255")]
    [InlineData("10.0.0.0")]
    [InlineData("10.255.255.255")]
    [InlineData("100.64.0.0")]
    [InlineData("100.127.255.255")]
    [InlineData("127.0.0.0")]
    [InlineData("127.0.0.1")]
    [InlineData("127.255.255.255")]
    [InlineData("169.254.0.0")]
    [InlineData("169.254.169.254")]
    [InlineData("169.254.255.255")]
    [InlineData("172.16.0.0")]
    [InlineData("172.31.255.255")]
    [InlineData("192.0.0.0")]
    [InlineData("192.0.0.255")]
    [InlineData("192.0.2.0")]
    [InlineData("192.0.2.255")]
    [InlineData("192.88.99.0")]
    [InlineData("192.88.99.1")]
    [InlineData("192.88.99.255")]
    [InlineData("192.168.0.0")]
    [InlineData("192.168.255.255")]
    [InlineData("198.18.0.0")]
    [InlineData("198.19.255.255")]
    [InlineData("198.51.100.0")]
    [InlineData("198.51.100.255")]
    [InlineData("203.0.113.0")]
    [InlineData("203.0.113.255")]
    [InlineData("224.0.0.0")]
    [InlineData("239.255.255.255")]
    [InlineData("240.0.0.0")]
    [InlineData("254.255.255.255")]
    [InlineData("255.255.255.255")]
    public void IsAllowed_BlockedIPv4Range_Should_Return_False(string address)
    {
        DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse(address), RequestUri).Should().BeFalse();
    }

    [Theory]
    [InlineData("1.0.0.0")]
    [InlineData("8.8.8.8")]
    [InlineData("9.255.255.255")]
    [InlineData("11.0.0.0")]
    [InlineData("100.63.255.255")]
    [InlineData("100.128.0.0")]
    [InlineData("126.255.255.255")]
    [InlineData("128.0.0.0")]
    [InlineData("169.253.255.255")]
    [InlineData("169.255.0.0")]
    [InlineData("172.15.255.255")]
    [InlineData("172.32.0.0")]
    [InlineData("191.255.255.255")]
    [InlineData("192.0.1.0")]
    [InlineData("192.0.3.0")]
    [InlineData("192.88.98.255")]
    [InlineData("192.88.100.0")]
    [InlineData("192.167.255.255")]
    [InlineData("192.169.0.0")]
    [InlineData("198.17.255.255")]
    [InlineData("198.20.0.0")]
    [InlineData("198.51.99.255")]
    [InlineData("198.51.101.0")]
    [InlineData("203.0.112.255")]
    [InlineData("203.0.114.0")]
    [InlineData("223.255.255.255")]
    public void IsAllowed_PublicIPv4_Should_Return_True(string address)
    {
        DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse(address), RequestUri).Should().BeTrue();
    }

    [Theory]
    [InlineData("::")]
    [InlineData("::1")]
    [InlineData("fc00::")]
    [InlineData("fd12:3456:789a::1")]
    [InlineData("fdff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("fe80::")]
    [InlineData("fe80::1%1")]
    [InlineData("febf:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("fec0::")]
    [InlineData("fec0::1")]
    [InlineData("feff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("ff00::")]
    [InlineData("ff02::1")]
    [InlineData("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001:db8::")]
    [InlineData("2001:db8:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001::")]
    [InlineData("2001:0:4136:e378:8000:63bf:3fff:fdd2")]
    [InlineData("2001:0:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("64:ff9b:1::")]
    [InlineData("64:ff9b:1::7f00:1")]
    [InlineData("64:ff9b:1::808:808")]
    [InlineData("64:ff9b:1:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("100::")]
    [InlineData("100::1")]
    [InlineData("100::ffff:ffff:ffff:ffff")]
    [InlineData("2001:10::")]
    [InlineData("2001:10::1")]
    [InlineData("2001:1f:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("::ffff:0:0:0")]
    [InlineData("::ffff:0:808:808")]
    [InlineData("::ffff:0:a00:1")]
    [InlineData("::ffff:0:ffff:ffff")]
    [InlineData("2001:2::1")]
    [InlineData("2001:2:0:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("3fff::1")]
    [InlineData("3fff:fff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("5f00::1")]
    public void IsAllowed_BlockedIPv6Range_Should_Return_False(string address)
    {
        DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse(address), RequestUri).Should().BeFalse();
    }

    [Theory]
    [InlineData("2001:4860:4860::8888")]
    [InlineData("2606:4700:4700::1111")]
    [InlineData("2001:2:1::1")]
    [InlineData("3fff:1000::1")]
    [InlineData("5f01::1")]
    [InlineData("fbff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("fe00::")]
    [InlineData("fe7f:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001:1::")]
    [InlineData("2001:db7:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001:db9::")]
    [InlineData("64:ff9b:2::")]
    [InlineData("ff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("100:0:0:1::")]
    [InlineData("2001:f:ffff:ffff:ffff:ffff:ffff:ffff")]
    [InlineData("2001:20::")]
    [InlineData("::fffe:ffff:ffff:ffff")]
    [InlineData("::ffff:1:0:0")]
    public void IsAllowed_PublicIPv6_Should_Return_True(string address)
    {
        DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse(address), RequestUri).Should().BeTrue();
    }

    [Theory]
    [InlineData("::ffff:127.0.0.1")]
    [InlineData("::ffff:10.0.0.1")]
    [InlineData("::ffff:169.254.169.254")]
    [InlineData("::ffff:192.168.1.1")]
    [InlineData("::ffff:0.0.0.0")]
    [InlineData("::127.0.0.1")]
    [InlineData("::10.0.0.1")]
    [InlineData("::169.254.169.254")]
    [InlineData("::2")]
    [InlineData("64:ff9b::127.0.0.1")]
    [InlineData("64:ff9b::a9fe:a9fe")]
    [InlineData("64:ff9b::192.168.0.1")]
    [InlineData("2002:7f00:1::")]
    [InlineData("2002:a9fe:a9fe::1")]
    [InlineData("2002:c0a8:101:1::1")]
    [InlineData("2002:a00:1::")]
    [InlineData("2002:c058:6301::")]
    public void IsAllowed_EmbeddedBlockedIPv4_Should_Return_False(string address)
    {
        DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse(address), RequestUri).Should().BeFalse();
    }

    [Theory]
    [InlineData("::ffff:8.8.8.8")]
    [InlineData("::8.8.8.8")]
    [InlineData("64:ff9b::8.8.8.8")]
    [InlineData("2002:808:808::1")]
    [InlineData("2003:7f00:1::")]
    public void IsAllowed_EmbeddedPublicIPv4_Should_Return_True(string address)
    {
        DefaultOutboundAddressPolicy.Instance.IsAllowed(IPAddress.Parse(address), RequestUri).Should().BeTrue();
    }

    [Theory]
    [InlineData("::ffff:10.0.0.1", "10.0.0.1")]
    [InlineData("::10.0.0.1", "10.0.0.1")]
    [InlineData("64:ff9b::a9fe:a9fe", "169.254.169.254")]
    [InlineData("2002:c0a8:101:1::1", "192.168.1.1")]
    [InlineData("::ffff:0:a00:1", "10.0.0.1")]
    [InlineData("::ffff:0:808:808", "8.8.8.8")]
    [InlineData("::ffff:1:808:808", "::ffff:1:808:808")]
    [InlineData("2001:4860:4860::8888", "2001:4860:4860::8888")]
    [InlineData("8.8.8.8", "8.8.8.8")]
    public void Normalize_Should_Extract_Embedded_IPv4_When_Address_Embeds_One(string address, string expected)
    {
        IPAddress normalized = DefaultOutboundAddressPolicy.Normalize(IPAddress.Parse(address));

        normalized.Should().Be(IPAddress.Parse(expected));
    }

    [Theory]
    [InlineData("::")]
    [InlineData("::1")]
    public void Normalize_Should_Keep_IPv6_Address_When_Address_Is_Unspecified_Or_Loopback(string address)
    {
        IPAddress parsed = IPAddress.Parse(address);

        IPAddress normalized = DefaultOutboundAddressPolicy.Normalize(parsed);

        normalized.Should().Be(parsed);
    }

    [Fact]
    public void IsAllowed_NullAddress_Should_Throw()
    {
        Action act = () => DefaultOutboundAddressPolicy.Instance.IsAllowed(null!, RequestUri);

        act.Should().Throw<ArgumentNullException>();
    }
}
