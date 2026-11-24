using CsCheck;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests;

public sealed class TypedErrorPropertyTests
{
    private const long Iterations = 500;

    private static readonly Func<string, string, TypedError>[] Kinds =
    [
        (code, description) => new ConflictError(code, description),
        (code, description) => new DomainError(code, description),
        (code, description) => new FailureError(code, description),
        (code, description) => new ForbiddenError(code, description),
        (code, description) => new NotFoundError(code, description),
        (code, description) => new UnauthorizedError(code, description),
        (code, description) => new UnexpectedError(code, description),
        (code, description) => new ValidationError(code, description),
    ];

    private static readonly Gen<(int Kind, string Code, string Description)> GenError =
        Gen.Select(Gen.Int[0, Kinds.Length - 1], Gen.String, Gen.String);

    [Fact]
    public void ToError_Should_KeepCodeDescriptionAndType_For_AnyKind()
    {
        GenError.Sample(sample =>
        {
            TypedError typed = Kinds[sample.Kind](sample.Code, sample.Description);

            Error error = typed.ToError();

            error.Code.Should().Be(sample.Code);
            error.Description.Should().Be(sample.Description);
            error.Type.Should().Be(typed.Type);
        }, iter: Iterations);
    }

    [Fact]
    public void Equals_Should_BeReflexiveAndSymmetricWithEqualHashCodes_When_TheSameKindIsBuiltTwice()
    {
        GenError.Sample(sample =>
        {
            TypedError left = Kinds[sample.Kind](sample.Code, sample.Description);
            TypedError right = Kinds[sample.Kind](sample.Code, sample.Description);

            left.Should().Be(left);
            left.Should().Be(right);
            right.Should().Be(left);
            left.GetHashCode().Should().Be(right.GetHashCode());
        }, iter: Iterations);
    }

    [Fact]
    public void Equals_Should_BeFalse_When_TheKindsDiffer()
    {
        Gen.Select(GenError, Gen.Int[1, Kinds.Length - 1]).Sample((sample, offset) =>
        {
            TypedError left = Kinds[sample.Kind](sample.Code, sample.Description);
            TypedError right = Kinds[(sample.Kind + offset) % Kinds.Length](sample.Code, sample.Description);

            left.Equals(right).Should().BeFalse();
        }, iter: Iterations);
    }
}
