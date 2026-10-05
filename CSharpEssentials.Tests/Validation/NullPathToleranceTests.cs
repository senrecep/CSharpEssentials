using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class NullPathToleranceTests
{
    // Inner is declared non-nullable so member-path lambdas compile without warnings;
    // null is injected at runtime via null! to simulate an uninitialized nested model.
    private sealed record InnerModel(string? Text, int Number, IEnumerable<string>? Tags);
    private sealed record OuterModel(InnerModel Inner);

    [Fact]
    public async Task For_ShouldTreatNullIntermediateSegment_AsNullValue()
    {
        OuterModel model = new(null!);

        Result<OuterModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Inner.Text).NotEmpty());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Inner.Text.NotEmpty");
    }

    [Fact]
    public async Task For_ShouldSkipNullTolerantValidators_WhenIntermediateSegmentIsNull()
    {
        OuterModel model = new(null!);

        Result<OuterModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Inner.Text).MaxLength(5).EmailAddress());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task For_ShouldEvaluateNormally_WhenPathIsFullyPopulated()
    {
        OuterModel model = new(new InnerModel("hello", 42, null));

        Result<OuterModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Inner.Text).NotEmpty().MaxLength(3));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Inner.Text.MaxLength");
    }

    [Fact]
    public async Task For_ShouldRethrow_WhenIntermediateSegmentIsNullAndValueTypeCannotBeNull()
    {
        OuterModel model = new(null!);

        Func<Task> act = async () => await Validator.ValidateAsync(model, (m, rules) =>
            rules.For(() => m.Inner.Number).GreaterThan(0));

        await act.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ForEach_ShouldProduceNoErrors_WhenCollectionPathHasNullSegment()
    {
        OuterModel model = new(null!);

        Result<OuterModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Inner.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForEachAsync_ShouldProduceNoErrors_WhenCollectionPathHasNullSegment()
    {
        OuterModel model = new(null!);

        Result<OuterModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.ForEachAsync(() => m.Inner.Tags, (tag, tagRules, token) =>
            {
                tagRules.For(() => tag).NotEmpty();
                return ValueTask.CompletedTask;
            }, ct));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForEach_ShouldIterateNormally_WhenPathIsFullyPopulated()
    {
        OuterModel model = new(new InnerModel(null, 0, ["ok", ""]));

        Result<OuterModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Inner.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Inner.Tags[1].NotEmpty");
    }
}
