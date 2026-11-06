using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class NestedValueTypeValidatorsTests
{
    private readonly record struct Point(int X, int Y);

    private sealed record Shape(Point Origin, Point? Anchor);

    private sealed class PointValidator : Validator<Point>
    {
        protected override ValueTask Configure(Point model, RuleContext<Point> rules, CancellationToken ct = default)
        {
            rules.For(() => model.X).GreaterThanOrEqualTo(0);
            rules.For(() => model.Y).GreaterThanOrEqualTo(0);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class NullablePointValidator : IValidator<Point?>
    {
        public int Calls { get; private set; }

        public ValueTask<Result<Point?>> ValidateAsync(Point? instance, CancellationToken ct = default)
        {
            Calls++;
            return new ValueTask<Result<Point?>>(Error.Validation("Bad", "'Bad' is wrong."));
        }
    }

    private sealed class UndottedCodeValidator : IValidator<Point>
    {
        public ValueTask<Result<Point>> ValidateAsync(Point instance, CancellationToken ct = default) =>
            new(Error.Validation("Invalid", "'Invalid' point."));
    }

    [Fact]
    public async Task SetValidatorAsync_ValueType_ShouldPropagatePrefixedErrors()
    {
        Shape shape = new(new Point(-1, 2), null);

        Result<Shape> result = await Validator.ValidateAsync(shape, async (m, rules, ct) =>
            await rules.For(() => m.Origin).SetValidatorAsync(new PointValidator(), ct));

        result.Errors.Should().ContainSingle();
        result.FirstError.Code.Should().Be("Origin.X.GreaterThanOrEqualTo");
        result.FirstError.Description.Should().Be("'Origin.X' must be greater than or equal to 0.");
    }

    [Fact]
    public async Task SetValidatorAsync_ValueType_ShouldPass_WhenNestedValid()
    {
        Shape shape = new(new Point(1, 2), null);

        Result<Shape> result = await Validator.ValidateAsync(shape, async (m, rules, ct) =>
            await rules.For(() => m.Origin).SetValidatorAsync(new PointValidator(), ct));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetValidatorAsync_ValueType_ShouldMarkChainFailed_SoLaterRulesAreSkipped()
    {
        Shape shape = new(new Point(-1, -1), null);

        Result<Shape> result = await Validator.ValidateAsync(shape, async (m, rules, ct) =>
        {
            RuleChain<Shape, Point> chain = await rules.For(() => m.Origin).SetValidatorAsync(new PointValidator(), ct);
            chain.Must(_ => false);
        });

        result.Errors.Select(e => e.Code).Should().Equal("Origin.X.GreaterThanOrEqualTo", "Origin.Y.GreaterThanOrEqualTo");
    }

    [Fact]
    public async Task SetValidatorAsync_ValueType_ShouldSkip_WhenChainAlreadyFailed()
    {
        Shape shape = new(new Point(-1, 2), null);

        Result<Shape> result = await Validator.ValidateAsync(shape, async (m, rules, ct) =>
            await rules.For(() => m.Origin).Must(_ => false).SetValidatorAsync(new PointValidator(), ct));

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("Origin.Must");
    }

    [Fact]
    public async Task SetValidatorAsync_NullableValueType_ShouldSkip_WhenValueIsNull()
    {
        Shape shape = new(new Point(1, 1), null);
        NullablePointValidator validator = new();

        Result<Shape> result = await Validator.ValidateAsync(shape, async (m, rules, ct) =>
            await rules.For(() => m.Anchor).SetValidatorAsync(validator, ct));

        result.IsSuccess.Should().BeTrue();
        validator.Calls.Should().Be(0);
    }

    [Fact]
    public async Task SetValidatorAsync_ShouldPrefixCodeButKeepDescription_WhenNestedCodeHasNoDot()
    {
        Shape shape = new(new Point(1, 1), null);

        Result<Shape> result = await Validator.ValidateAsync(shape, async (m, rules, ct) =>
            await rules.For(() => m.Origin).SetValidatorAsync(new UndottedCodeValidator(), ct));

        result.FirstError.Code.Should().Be("Origin.Invalid");
        result.FirstError.Description.Should().Be("'Invalid' point.");
    }
}
