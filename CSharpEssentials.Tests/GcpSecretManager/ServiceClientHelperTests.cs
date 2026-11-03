using CSharpEssentials.GcpSecretManager.Infrastructure;
using FluentAssertions;

namespace CSharpEssentials.Tests.GcpSecretManager;

public sealed class ServiceClientHelperTests
{
    private static readonly string MissingCredentialsPath =
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithMissingCredentialsPath_ShouldThrowArgumentException(string? credentialsPath)
    {
        var helper = new ServiceClientHelper();

        Action act = () => helper.Create(credentialsPath!);

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(credentialsPath));
    }

    [Fact]
    public void Create_WithNonexistentCredentialsFile_ShouldThrowFileNotFoundException()
    {
        var helper = new ServiceClientHelper();

        Action act = () => helper.Create(MissingCredentialsPath);

        act.Should().Throw<FileNotFoundException>().Which.Message.Should().Contain(MissingCredentialsPath);
    }

    [Fact]
    public void CreateWithRegion_WithNonexistentCredentialsFile_ShouldThrowFileNotFoundException()
    {
        var helper = new ServiceClientHelper();

        Action act = () => helper.CreateWithRegion(MissingCredentialsPath, null);

        act.Should().Throw<FileNotFoundException>().Which.Message.Should().Contain(MissingCredentialsPath);
    }
}
