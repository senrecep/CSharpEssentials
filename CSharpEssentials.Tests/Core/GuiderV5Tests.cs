using System.Text;
using CSharpEssentials.Core;
using FluentAssertions;

namespace CSharpEssentials.Tests.Core;

public sealed class GuiderV5Tests
{
    [Fact]
    public void NewGuidV5_Should_Match_The_Rfc9562_Example()
    {
        Guider.NewGuidV5(GuidNamespaces.Dns, "www.example.com")
            .Should().Be(new Guid("2ed6657d-e927-568b-95e1-2665a8aea6a2"));
    }

    [Fact]
    public void NewGuidV5_Should_Set_Version5_And_The_Rfc_Variant()
    {
        Guid guid = Guider.NewGuidV5(GuidNamespaces.Url, "https://example.com/a");

        guid.Version.Should().Be(5);
        (guid.Variant & 0b1100).Should().Be(0b1000);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ascii")]
    [InlineData("çğüşöı 日本語 🙂")]
    public void NewGuidV5_Should_Give_The_Same_Guid_For_A_String_And_Its_Utf8_Bytes(string name)
    {
        Guider.NewGuidV5(GuidNamespaces.Oid, name)
            .Should().Be(Guider.NewGuidV5(GuidNamespaces.Oid, Encoding.UTF8.GetBytes(name)));
    }

    [Fact]
    public void NewGuidV5_Should_Handle_Names_Longer_Than_The_Stack_Buffer()
    {
        string name = new('x', 10_000);

        Guider.NewGuidV5(GuidNamespaces.X500, name)
            .Should().Be(Guider.NewGuidV5(GuidNamespaces.X500, Encoding.UTF8.GetBytes(name)))
            .And.NotBe(Guider.NewGuidV5(GuidNamespaces.X500, name[1..]));
    }

    [Fact]
    public void NewGuidV5_Should_Depend_On_The_Namespace()
    {
        Guider.NewGuidV5(GuidNamespaces.Dns, "a").Should().NotBe(Guider.NewGuidV5(GuidNamespaces.Url, "a"));
    }

    [Fact]
    public void NewGuidV5_Should_Reject_Null_And_Lone_Surrogates()
    {
        Action nullName = () => Guider.NewGuidV5(GuidNamespaces.Dns, (string)null!);
        Action loneSurrogate = () => Guider.NewGuidV5(GuidNamespaces.Dns, "a\uD800b");

        nullName.Should().Throw<ArgumentNullException>().WithParameterName("name");
        loneSurrogate.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GuidNamespaces_Should_Hold_The_Rfc9562_Values()
    {
        GuidNamespaces.Dns.Should().Be(new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8"));
        GuidNamespaces.Url.Should().Be(new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8"));
        GuidNamespaces.Oid.Should().Be(new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8"));
        GuidNamespaces.X500.Should().Be(new Guid("6ba7b814-9dad-11d1-80b4-00c04fd430c8"));
    }
}
