using CSharpEssentials.RequestResponseLogging;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.RequestResponseLogging;

public class LoggingOptionsTests
{
    [Fact]
    public void LoggingOptions_ShouldInitializeWithDefaults()
    {
        var options = new LoggingOptions();

        options.LoggingLevel.Should().Be(LogLevel.Information);
        options.UseSeparateContext.Should().BeTrue();
        options.LoggerCategoryName.Should().Be("RequestResponseLogger");
        options.HeaderKeys.Should().NotBeNull();
        options.LoggingFields.Should().NotBeNull();
    }

    [Fact]
    public void CreateAllFields_ShouldIncludeAllLogFields()
    {
        var options = LoggingOptions.CreateAllFields();

        options.LoggingFields.Should().Contain(LogFields.Request);
        options.LoggingFields.Should().Contain(LogFields.Response);
        options.LoggingFields.Should().Contain(LogFields.HostName);
        options.LoggingFields.Should().Contain(LogFields.Path);
        options.LoggingFields.Should().Contain(LogFields.Method);
        options.LoggingFields.Should().Contain(LogFields.QueryString);
        options.LoggingFields.Should().Contain(LogFields.Headers);
        options.LoggingFields.Should().Contain(LogFields.ResponseTiming);
        options.LoggingFields.Should().Contain(LogFields.RequestLength);
        options.LoggingFields.Should().Contain(LogFields.ResponseLength);
    }

    [Fact]
    public void LoggingOptions_ShouldAllowSettingProperties()
    {
        var options = new LoggingOptions
        {
            LoggingLevel = LogLevel.Warning,
            UseSeparateContext = false,
            LoggerCategoryName = "CustomLogger"
        };

        options.LoggingLevel.Should().Be(LogLevel.Warning);
        options.UseSeparateContext.Should().BeFalse();
        options.LoggerCategoryName.Should().Be("CustomLogger");
    }

    [Fact]
    public void LoggingOptions_ShouldAllowSettingHeaderKeys()
    {
        var options = new LoggingOptions();
        options.HeaderKeys.Add("User-Agent");
        options.HeaderKeys.Add("X-Correlation-ID");

        options.HeaderKeys.Should().HaveCount(2);
        options.HeaderKeys.Should().Contain("User-Agent");
        options.HeaderKeys.Should().Contain("X-Correlation-ID");
    }
}

