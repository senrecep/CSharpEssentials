using CSharpEssentials.Core;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class ExpressionPathNormalizationTests
{
    private sealed record Address(string? City);
    private sealed record Line(string? Sku, bool IsActive);
    private sealed record Model(IEnumerable<string?>? Tags, Address? Addr, IEnumerable<Line>? Lines);

    [Fact]
    public async Task ForEach_WithoutNulls_ShouldUseCollectionNameAndFilteredIndex()
    {
        Model model = new(["ok", null, "", "fine"], null, null);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Tags.WithoutNulls(), (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.FirstError.Code.Should().Be("Tags[1].NotEmpty");
    }

    [Fact]
    public async Task ForEach_WhereIf_ShouldUseCollectionNameAndFilteredIndex()
    {
        Model model = new(null, null, [new Line("a", false), new Line("", true), new Line("", true)]);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Lines.WhereIf(true, l => l.IsActive), (line, lineRules) =>
                lineRules.For(() => line.Sku).NotEmpty()));

        result.Errors.Select(e => e.Code).Should().Equal("Lines[0].Sku.NotEmpty", "Lines[1].Sku.NotEmpty");
    }

    [Fact]
    public async Task For_NullForgivingMemberPath_ShouldOmitBang()
    {
        Model model = new(null, new Address(""), null);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Addr!.City).NotEmpty());

        result.FirstError.Code.Should().Be("Addr.City.NotEmpty");
    }

    [Fact]
    public async Task For_NullConditionalMemberPath_ShouldOmitQuestionMark()
    {
        Model model = new(null, new Address(""), null);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Addr?.City).NotEmpty());

        result.FirstError.Code.Should().Be("Addr.City.NotEmpty");
    }

    [Fact]
    public async Task ForEach_NullForgivingCollectionPath_ShouldOmitBang()
    {
        Model model = new(null, null, [new Line("", true)]);

        Result<Model> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Lines!, (line, lineRules) =>
                lineRules.For(() => line.Sku).NotEmpty()));

        result.FirstError.Code.Should().Be("Lines[0].Sku.NotEmpty");
    }
}
