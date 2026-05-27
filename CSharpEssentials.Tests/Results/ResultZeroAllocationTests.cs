using Xunit;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Tests.Results;

public class ResultZeroAllocationTests
{
    [Fact]
    public void ResultFailure_WithErrorsArray_ShouldNotCopyArray()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];

        // Act
        Result result = Result.Failure(errors);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Same(errors, result.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultGenericFailure_WithErrorsArray_ShouldNotCopyArray()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];

        // Act
        Result<int> result = Result<int>.Failure(errors);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Same(errors, result.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultExtensionsToResult_WithErrorsArray_ShouldNotCopyArray()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];

        // Act
        Result result = errors.ToResult();
        Result<int> resultGeneric = errors.ToResult<int>();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Same(errors, result.ErrorsOrEmptyArray);
        Assert.True(resultGeneric.IsFailure);
        Assert.Same(errors, resultGeneric.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultImplicitConversions_WithErrorsArray_ShouldNotCopyArray()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];

        // Act
        Result result = errors;
        Result<int> resultGeneric = errors;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Same(errors, result.ErrorsOrEmptyArray);
        Assert.True(resultGeneric.IsFailure);
        Assert.Same(errors, resultGeneric.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultBind_OnFailure_ShouldReturnOriginalErrorsArrayReference()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result result = Result.Failure(errors);

        // Act
        Result<string> boundGeneric = result.Bind(() => Result.Success("test"));
        Result boundNonGeneric = result.Bind(() => Result.Success());

        // Assert
        Assert.True(boundGeneric.IsFailure);
        Assert.Same(errors, boundGeneric.ErrorsOrEmptyArray);

        Assert.True(boundNonGeneric.IsFailure);
        Assert.Same(errors, boundNonGeneric.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultTBind_OnFailure_ShouldReturnOriginalErrorsArrayReference()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result<int> result = Result<int>.Failure(errors);

        // Act
        Result<string> boundGeneric = result.Bind(val => Result.Success("test"));
        Result boundNonGeneric = result.Bind(val => Result.Success());

        // Assert
        Assert.True(boundGeneric.IsFailure);
        Assert.Same(errors, boundGeneric.ErrorsOrEmptyArray);

        Assert.True(boundNonGeneric.IsFailure);
        Assert.Same(errors, boundNonGeneric.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultMap_OnFailure_ShouldReturnOriginalErrorsArrayReference()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result result = Result.Failure(errors);

        // Act
        Result<string> mapped = result.Map(() => "test");
        Result<string> mappedResult = result.Map(() => Result.Success("test"));

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.Same(errors, mapped.ErrorsOrEmptyArray);

        Assert.True(mappedResult.IsFailure);
        Assert.Same(errors, mappedResult.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultTThen_OnFailure_ShouldReturnOriginalErrorsArrayReference()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result<int> result = Result<int>.Failure(errors);

        // Act
        Result<string> thenResult = result.Then(val => Result.Success("test"));
        Result<int> thenDoResult = result.ThenDo(val => { });
        Result<string> thenValueResult = result.Then(val => "test");

        // Assert
        Assert.True(thenResult.IsFailure);
        Assert.Same(errors, thenResult.ErrorsOrEmptyArray);

        Assert.True(thenDoResult.IsFailure);
        Assert.Same(errors, thenDoResult.ErrorsOrEmptyArray);

        Assert.True(thenValueResult.IsFailure);
        Assert.Same(errors, thenValueResult.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultTThenEnsure_OnFailure_ShouldReturnOriginalErrorsArrayReference()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result<int> result = Result<int>.Failure(errors);

        // Act
        Result<int> ensureResult = result.ThenEnsure(val => Result<int>.Success(val));
        Result<int> ensureNonGenericResult = result.ThenEnsure(val => Result.Success());

        // Assert
        Assert.True(ensureResult.IsFailure);
        Assert.Same(errors, ensureResult.ErrorsOrEmptyArray);

        Assert.True(ensureNonGenericResult.IsFailure);
        Assert.Same(errors, ensureNonGenericResult.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultTMapError_OnFailure_ShouldPropagateMappedErrorsZeroAllocation()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result<int> result = Result<int>.Failure(errors);
        Error[] mappedErrors = [Error.Conflict("Conflict", "Message")];

        // Act
        Result<int> mappedResult = result.MapError(errs => mappedErrors);

        // Assert
        Assert.True(mappedResult.IsFailure);
        Assert.Same(mappedErrors, mappedResult.ErrorsOrEmptyArray);
    }

    [Fact]
    public void ResultTElse_OnFailure_ShouldPropagateMappedErrorsZeroAllocation()
    {
        // Arrange
        Error[] errors = [Error.Validation("Code", "Message")];
        Result<int> result = Result<int>.Failure(errors);
        Error[] newErrors = [Error.Conflict("Conflict", "Message")];

        // Act
        Result<int> elseResult = result.Else(errs => newErrors);

        // Assert
        Assert.True(elseResult.IsFailure);
        Assert.Same(newErrors, elseResult.ErrorsOrEmptyArray);
    }
}
