using CsCheck;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests;

public sealed class TypedErrorPropertyTests
{
    private const long Iterations = 500;

    private static readonly (Func<string, string, ErrorMetadata?, TypedError> Create, ErrorType Type)[] Kinds =
    [
        ((code, description, metadata) => new ConflictError(code, description, metadata), ErrorType.Conflict),
        ((code, description, metadata) => new DomainError(code, description, metadata), ErrorType.Domain),
        ((code, description, metadata) => new FailureError(code, description, metadata), ErrorType.Failure),
        ((code, description, metadata) => new ForbiddenError(code, description, metadata), ErrorType.Forbidden),
        ((code, description, metadata) => new NotFoundError(code, description, metadata), ErrorType.NotFound),
        ((code, description, metadata) => new UnauthorizedError(code, description, metadata), ErrorType.Unauthorized),
        ((code, description, metadata) => new UnexpectedError(code, description, metadata), ErrorType.Unexpected),
        ((code, description, metadata) => new ValidationError(code, description, metadata), ErrorType.Validation),
    ];

    private static readonly Gen<(int Kind, string Code, string Description)> GenError =
        Gen.Select(Gen.Int[0, Kinds.Length - 1], Gen.String, Gen.String);

    private static TypedError Build((int Kind, string Code, string Description) sample, ErrorMetadata? metadata = null) =>
        Kinds[sample.Kind].Create(sample.Code, sample.Description, metadata);

    [Fact]
    public void ToError_Should_KeepCodeDescriptionTypeAndMetadata_For_AnyKind()
    {
        Gen.Select(GenError, Gen.Bool).Sample((sample, withMetadata) =>
        {
            ErrorMetadata? metadata = withMetadata ? new ErrorMetadata("key", sample.Code) : null;
            TypedError typed = Build(sample, metadata);

            Error error = typed.ToError();

            error.Code.Should().Be(sample.Code);
            error.Description.Should().Be(sample.Description);
            error.Type.Should().Be(Kinds[sample.Kind].Type);
            error.Metadata.Should().BeSameAs(typed.Metadata).And.BeSameAs(metadata);
        }, iter: Iterations);
    }

    [Fact]
    public void Equals_Should_BeReflexiveAndSymmetricWithEqualHashCodes_When_TheSameKindIsBuiltTwice()
    {
        GenError.Sample(sample =>
        {
            TypedError left = Build(sample);
            TypedError right = Build(sample);

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
            TypedError left = Build(sample);
            TypedError right = Build(((sample.Kind + offset) % Kinds.Length, sample.Code, sample.Description));

            left.Equals(right).Should().BeFalse();
        }, iter: Iterations);
    }
}
