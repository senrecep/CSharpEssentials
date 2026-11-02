using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorEqualityTests
{
    [Fact]
    public void Equals_WithEquivalentMetadata_ShouldBeEqualWithSameHashCode()
    {
        var left = Error.Conflict("C", "d", new ErrorMetadata { ["a"] = 1, ["b"] = "x" });
        var right = Error.Conflict("C", "d", new ErrorMetadata { ["a"] = 1, ["b"] = "x" });

        left.Should().Be(right);
        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_WithSharedMetadataInstance_ShouldBeEqual()
    {
        ErrorMetadata metadata = new("key", "value");

        Error.Failure("F", "d", metadata).Should().Be(Error.Failure("F", "d", metadata));
    }

    [Fact]
    public void Equals_WhenOnlyOneSideHasMetadata_ShouldNotBeEqual()
    {
        var withMetadata = Error.NotFound("N", "d", new ErrorMetadata("key", "value"));
        var withoutMetadata = Error.NotFound("N", "d");

        withMetadata.Should().NotBe(withoutMetadata);
        withoutMetadata.Should().NotBe(withMetadata);
    }

    [Fact]
    public void Equals_WithDifferentMetadataCount_ShouldNotBeEqual()
    {
        var left = Error.Validation("V", "d", new ErrorMetadata("a", 1));
        var right = Error.Validation("V", "d", new ErrorMetadata { ["a"] = 1, ["b"] = 2 });

        left.Should().NotBe(right);
    }

    [Fact]
    public void Equals_WithDifferentMetadataValue_ShouldNotBeEqual()
    {
        var left = Error.Validation("V", "d", new ErrorMetadata("a", 1));
        var right = Error.Validation("V", "d", new ErrorMetadata("a", 2));

        left.Should().NotBe(right);
    }

    [Fact]
    public void Equals_WithDifferentMetadataKey_ShouldNotBeEqual()
    {
        var left = Error.Validation("V", "d", new ErrorMetadata("a", 1));
        var right = Error.Validation("V", "d", new ErrorMetadata("b", 1));

        left.Should().NotBe(right);
    }

    [Fact]
    public void Equals_WithSameCodeButDifferentType_ShouldNotBeEqual()
    {
        Error.Failure("Same", "d").Should().NotBe(Error.Unexpected("Same", "d"));
    }

    [Fact]
    public void Equals_WithDifferentDescription_ShouldNotBeEqual()
    {
        Error.Forbidden("F", "one").Should().NotBe(Error.Forbidden("F", "two"));
    }

    [Fact]
    public void GetHashCode_WithoutMetadata_ShouldMatchForEqualErrors()
    {
        Error.Unauthorized().GetHashCode().Should().Be(Error.Unauthorized().GetHashCode());
    }

    [Fact]
    public void HashSet_ShouldDeduplicateEqualErrors()
    {
        HashSet<Error> set =
        [
            Error.Conflict("C", "d", new ErrorMetadata("a", 1)),
            Error.Conflict("C", "d", new ErrorMetadata("a", 1)),
            Error.Conflict("C", "d"),
        ];

        set.Should().HaveCount(2);
    }

    [Fact]
    public void FactoryDefaults_ShouldUseTypeNameAsCode()
    {
        Error.Failure().Code.Should().Be("Failure");
        Error.Unexpected().Code.Should().Be("Unexpected");
        Error.Validation().Code.Should().Be("Validation");
        Error.Conflict().Code.Should().Be("Conflict");
        Error.NotFound().Code.Should().Be("NotFound");
        Error.Unauthorized().Code.Should().Be("Unauthorized");
        Error.Forbidden().Description.Should().Be("A 'Forbidden' error has occurred.");
    }

    [Fact]
    public void NumericType_ShouldMatchErrorType()
    {
        Error.Forbidden().NumericType.Should().Be((int)ErrorType.Forbidden);
    }

    [Fact]
    public void Factory_WithNullCode_ShouldThrowArgumentNullException()
    {
        Action act = () => Error.Failure(code: null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Factory_WithNullDescription_ShouldThrowArgumentNullException()
    {
        Action act = () => Error.Validation("V", description: null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("description");
    }

    [Fact]
    public void CreateMany_WithNestedSequences_ShouldFlattenInOrder()
    {
        Error a = Error.Failure("A");
        Error b = Error.Failure("B");
        Error c = Error.Failure("C");

        IEnumerable<Error> first = [a, b];
        IEnumerable<Error> second = [c];

        Error[] errors = Error.CreateMany(first, second);

        errors.Should().Equal(a, b, c);
    }

    [Fact]
    public void Exception_WithTypeAndMetadata_ShouldCombineMetadataWithoutOverridingDetails()
    {
        var exception = new InvalidOperationException("boom");

        Error error = Error.Exception(exception, ErrorType.Conflict, new ErrorMetadata { ["extra"] = 1, ["exceptionMessage"] = "ignored" });

        error.Code.Should().Be(nameof(InvalidOperationException));
        error.Description.Should().Be("boom");
        error.Type.Should().Be(ErrorType.Conflict);
        error.Metadata!["extra"].Should().Be(1);
        error.Metadata["exceptionMessage"].Should().Be("boom");
    }

    [Fact]
    public void StaticErrors_ShouldHaveDocumentedCodesAndTypes()
    {
        Error.NoFirstError.Code.Should().Be("Result.NoFirstError");
        Error.NoLastError.Code.Should().Be("Result.NoLastError");
        Error.NoErrors.Code.Should().Be("Result.NoErrors");
        Error.NoErrors.Type.Should().Be(ErrorType.Unexpected);
        Error.False.Code.Should().Be("False");
        Error.False.Type.Should().Be(ErrorType.Validation);
    }
}
