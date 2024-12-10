using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorEqualityTests
{
    public static readonly TheoryData<string, string, ErrorMetadata?> ValidErrorData =
         new()
         {
            { "ErrorCode1", "ErrorDescription1", null },
            { "ErrorCode2", "ErrorDescription2", new ErrorMetadata { { "Key1", "Value1" }, { "Key2", "Value2" } } },
         };

    public static readonly TheoryData<Error, Error> UnequalErrorInstances =
        new()
        {
            { Error.Failure(), Error.Forbidden() },
            { Error.NotFound(), Error.NotFound(metadata: new ErrorMetadata { ["Key"] = "Value" }) },
            { Error.Unexpected(metadata: new ErrorMetadata { ["Key1"] = "Value1" }), Error.Unexpected() },
            {
                Error.Failure(metadata: new ErrorMetadata { ["Key1"] = "Value1" }),
                Error.Failure(metadata: new ErrorMetadata { ["Key1"] = "Value2" })
            },
            {
                Error.Conflict(metadata: new ErrorMetadata { ["Key2"] = "Value2" }),
                Error.Conflict(metadata: new ErrorMetadata { ["Key3"] = "Value3" })
            },
        };

    [Theory]
    [MemberData(nameof(ValidErrorData))]
    public void Equals_ShouldReturnTrue_WhenErrorsHaveIdenticalValues(
        string errorCode,
        string errorDescription,
        ErrorMetadata? errorMetadata)
    {
        var error1 = Error.Failure(errorCode, errorDescription, errorMetadata);
        ErrorMetadata? clonedMetadata = errorMetadata is null ? null : new ErrorMetadata(errorMetadata);
        var error2 = Error.Failure(errorCode, errorDescription, clonedMetadata);

        bool isEqual = error1.Equals(error2);

        isEqual.Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenErrorsShareSameMetadataInstance()
    {
        var sharedMetadata = new ErrorMetadata { { "Key", "SharedValue" } };
        var error1 = Error.Failure("Code", "Description", sharedMetadata);
        var error2 = Error.Failure("Code", "Description", sharedMetadata);

        bool isEqual = error1.Equals(error2);

        isEqual.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(UnequalErrorInstances))]
    public void Equals_ShouldReturnFalse_WhenErrorsHaveDifferentValues(Error error1, Error error2)
    {
        bool isEqual = error1.Equals(error2);

        isEqual.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(ValidErrorData))]
    public void GetHashCode_ShouldReturnSameHash_WhenErrorsHaveIdenticalValues(
        string errorCode,
        string errorDescription,
        ErrorMetadata? errorMetadata)
    {
        var error1 = Error.Failure(errorCode, errorDescription, errorMetadata);
        ErrorMetadata? clonedMetadata = errorMetadata is null ? null : new ErrorMetadata(errorMetadata);
        var error2 = Error.Failure(errorCode, errorDescription, clonedMetadata);

        int hashCode1 = error1.GetHashCode();
        int hashCode2 = error2.GetHashCode();

        hashCode1.Should().Be(hashCode2);
    }

    [Theory]
    [MemberData(nameof(UnequalErrorInstances))]
    public void GetHashCode_ShouldReturnDifferentHashes_WhenErrorsHaveDifferentValues(
        Error error1,
        Error error2)
    {
        int hashCode1 = error1.GetHashCode();
        int hashCode2 = error2.GetHashCode();

        hashCode1.Should().NotBe(hashCode2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString_WithAllProperties()
    {
        var metadata = new ErrorMetadata { { "Key", "Value" } };
        var error = Error.Failure("ErrorCode", "ErrorDescription", metadata);

        string result = error.ToString();

        result.Should().Contain("ErrorCode");
        result.Should().Contain("ErrorDescription");
        result.Should().Contain("Failure");
        result.Should().Contain("Key: Value");
    }

    [Fact]
    public void ToString_ShouldHandleNullMetadata_Gracefully()
    {
        var error = Error.Failure("ErrorCode", "ErrorDescription");

        string result = error.ToString();

        result.Should().Contain("ErrorCode");
        result.Should().Contain("ErrorDescription");
        result.Should().Contain("Failure");
        result.Should().Contain("Metadata");
    }
}
