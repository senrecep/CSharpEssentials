using CSharpEssentials.EntityFrameworkCore.Extensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class DbContextOptionsBuilderExtensionsTests
{
    [Fact]
    public void UseAsWriteContext_Should_ReturnSameBuilder_When_Called()
    {
        var builder = new DbContextOptionsBuilder();

        var result = builder.UseAsWriteContext();

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void UseAsWriteContext_Should_SetTrackAllBehavior_When_Called()
    {
        var builder = new DbContextOptionsBuilder();

        builder.UseAsWriteContext();

        builder.Options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>()!
            .QueryTrackingBehavior
            .Should().Be(QueryTrackingBehavior.TrackAll);
    }

    [Fact]
    public void UseAsWriteContext_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        DbContextOptionsBuilder options = null!;

        Action act = () => options.UseAsWriteContext();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UseAsReadContext_Should_ReturnSameBuilder_When_Called()
    {
        var builder = new DbContextOptionsBuilder();

        var result = builder.UseAsReadContext();

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void UseAsReadContext_Should_SetNoTrackingBehavior_When_Called()
    {
        var builder = new DbContextOptionsBuilder();

        builder.UseAsReadContext();

        builder.Options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>()!
            .QueryTrackingBehavior
            .Should().Be(QueryTrackingBehavior.NoTracking);
    }

    [Fact]
    public void UseAsReadContext_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        DbContextOptionsBuilder options = null!;

        Action act = () => options.UseAsReadContext();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UseAsReadContextWithIdentityResolution_Should_ReturnSameBuilder_When_Called()
    {
        var builder = new DbContextOptionsBuilder();

        var result = builder.UseAsReadContextWithIdentityResolution();

        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void UseAsReadContextWithIdentityResolution_Should_SetNoTrackingWithIdentityResolutionBehavior_When_Called()
    {
        var builder = new DbContextOptionsBuilder();

        builder.UseAsReadContextWithIdentityResolution();

        builder.Options.FindExtension<Microsoft.EntityFrameworkCore.Infrastructure.CoreOptionsExtension>()!
            .QueryTrackingBehavior
            .Should().Be(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    }

    [Fact]
    public void UseAsReadContextWithIdentityResolution_Should_ThrowArgumentNullException_When_OptionsIsNull()
    {
        DbContextOptionsBuilder options = null!;

        Action act = () => options.UseAsReadContextWithIdentityResolution();

        act.Should().Throw<ArgumentNullException>();
    }
}
