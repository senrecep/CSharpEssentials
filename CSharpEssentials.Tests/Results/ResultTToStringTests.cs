using System.Globalization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Xunit;

namespace CSharpEssentials.Tests.Results;

public class ResultTToStringTests
{
    private sealed record Order(int Id);

    private sealed class PlainDto
    {
        public int Id { get; init; }
    }

    private sealed class NullText
    {
        public override string? ToString() => null;
    }

    [Fact]
    public void Success_Int_ShouldPrintValue() =>
        Result<int>.Success(42).ToString().Should().Be("Success: 42");

    [Fact]
    public void Success_String_ShouldPrintWithoutQuotes() =>
        Result<string>.Success("abc").ToString().Should().Be("Success: abc");

    [Fact]
    public void Success_EmptyString_ShouldPrintEmpty() =>
        Result<string>.Success(string.Empty).ToString().Should().Be("Success: ");

    [Fact]
    public void Default_ReferenceType_ShouldPrintNull() =>
        default(Result<string>).ToString().Should().Be("Success: null");

    [Fact]
    public void Default_NullableValueType_ShouldPrintNull() =>
        default(Result<int?>).ToString().Should().Be("Success: null");

    [Fact]
    public void Success_Double_ShouldUseInvariantCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            Result<double>.Success(1.5).ToString().Should().Be("Success: 1.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Success_DateTime_ShouldUseInvariantCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            Result<DateTime>.Success(new DateTime(2024, 3, 5, 6, 7, 8, DateTimeKind.Utc)).ToString()
                .Should().Be("Success: 03/05/2024 06:07:08");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Success_Bool_ShouldUseClrText() =>
        Result<bool>.Success(true).ToString().Should().Be("Success: True");

    [Fact]
    public void Success_Enum_ShouldUseClrName() =>
        Result<ErrorType>.Success(ErrorType.Validation).ToString().Should().Be("Success: Validation");

    [Fact]
    public void Success_Record_ShouldUseRecordToString() =>
        Result<Order>.Success(new Order(1)).ToString().Should().Be("Success: Order { Id = 1 }");

    [Fact]
    public void Success_ClassWithoutToStringOverride_ShouldPrintTypeName() =>
        Result<PlainDto>.Success(new PlainDto { Id = 1 }).ToString()
            .Should().Be($"Success: {typeof(PlainDto).FullName}");

    [Fact]
    public void Success_ToStringReturnsNull_ShouldPrintEmpty() =>
        Result<NullText>.Success(new NullText()).ToString().Should().Be("Success: ");

    [Fact]
    public void Success_Decimal_ShouldUseInvariantCulture()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Result<decimal>.Success(1234.5m).ToString().Should().Be("Success: 1234.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Failure_SingleError_ShouldKeepFormat()
    {
        Error error = Error.Failure("CODE", "Message");

        Result<int>.Failure(error).ToString().Should().Be($"Failure: 1 error, first error: {error}");
    }

    [Fact]
    public void Failure_ManyErrors_ShouldKeepFormat()
    {
        Error first = Error.Failure("FIRST", "First");
        Error last = Error.Validation("LAST", "Last");

        Result<int>.Failure(new[] { first, Error.Conflict("MID", "Mid"), last }).ToString()
            .Should().Be($"Failure: 3 errors, first error: {first}, last error: {last}");
    }
}
