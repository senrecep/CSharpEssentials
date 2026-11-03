using CSharpEssentials.GcpSecretManager.Infrastructure;
using FluentAssertions;

namespace CSharpEssentials.Tests.GcpSecretManager;

public sealed class ServiceClientHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreateWithRegion_Should_ThrowArgumentException_When_CredentialsPathIsNullOrEmpty(string? credentialsPath)
    {
        var helper = new ServiceClientHelper();

        Action act = () => helper.CreateWithRegion(credentialsPath!, "europe-west1");

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("credentialsPath");
    }
}
