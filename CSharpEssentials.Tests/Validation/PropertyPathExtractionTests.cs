using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class PropertyPathExtractionTests
{
    private sealed record Model(string? Name, string[] Tags);

    [Fact]
    public async Task For_ShouldUseVariableName_WhenLambdaReturnsCapturedVariableWithBang()
    {
        string? name = null;

        Result<Model> result = await Validator.ValidateAsync(new Model(null, []), (_, rules) =>
            rules.For<string?>(() => name!).NotEmpty());

        result.FirstError.Code.Should().Be("name.NotEmpty");
    }

    [Fact]
    public async Task For_ShouldStopAtMethodCall_WhenPathEndsWithInvocationAfterBang()
    {
        Result<Model> result = await Validator.ValidateAsync(new Model("   ", []), (m, rules) =>
            rules.For<string?>(() => m.Name!.Trim()).NotEmpty());

        result.FirstError.Code.Should().Be("Name.NotEmpty");
    }

    [Fact]
    public async Task For_ShouldStripNullConditional_WhenPathEndsWithInvocation()
    {
        Result<Model> result = await Validator.ValidateAsync(new Model("   ", []), (m, rules) =>
            rules.For(() => m.Name?.Trim()).NotEmpty());

        result.FirstError.Code.Should().Be("Name.NotEmpty");
    }

    [Fact]
    public async Task For_ShouldKeepIndexer_WhenPathIndexesBeforeInvocation()
    {
        Result<Model> result = await Validator.ValidateAsync(new Model(null, ["   "]), (m, rules) =>
            rules.For<string?>(() => m.Tags[0].Trim()).NotEmpty());

        result.FirstError.Code.Should().Be("Tags[0].NotEmpty");
    }
}
