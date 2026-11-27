using System.Net;
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Core;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.Json;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorTypeDomainTests
{
    private static readonly Dictionary<ErrorType, int> ExpectedStatus = new()
    {
        [ErrorType.Failure] = HttpCodes.InternalServerError,
        [ErrorType.Unexpected] = HttpCodes.InternalServerError,
        [ErrorType.Validation] = HttpCodes.BadRequest,
        [ErrorType.Conflict] = HttpCodes.Conflict,
        [ErrorType.NotFound] = HttpCodes.NotFound,
        [ErrorType.Unauthorized] = HttpCodes.Unauthorized,
        [ErrorType.Forbidden] = HttpCodes.Forbidden,
        [ErrorType.Unknown] = HttpCodes.InternalServerError,
        [ErrorType.Domain] = HttpCodes.UnprocessableEntity,
    };

    private static readonly Dictionary<ErrorType, string> ExpectedTitle = new()
    {
        [ErrorType.Failure] = "Server Failure",
        [ErrorType.Unexpected] = "Unexpected Error",
        [ErrorType.Validation] = "Validation Error",
        [ErrorType.Conflict] = "Conflict",
        [ErrorType.NotFound] = "Not Found",
        [ErrorType.Unauthorized] = "Unauthorized",
        [ErrorType.Forbidden] = "Forbidden",
        [ErrorType.Unknown] = "Unknown Error",
        [ErrorType.Domain] = "Domain Error",
    };

    private static readonly ErrorType[] OneToOne =
    [
        ErrorType.Validation, ErrorType.Unauthorized, ErrorType.Forbidden, ErrorType.NotFound,
        ErrorType.Conflict, ErrorType.Domain, ErrorType.Failure,
    ];

    private static readonly JsonSerializerOptions ConventionOptions =
        new JsonSerializerOptions().AddEnumConventions(EnumConventions.Default);

    [Fact]
    public void Domain_Should_BeAddedAfterUnknown_When_EnumIsDeclared()
    {
        ((int)ErrorType.Unknown).Should().Be(7);
        ((int)ErrorType.Domain).Should().Be(8);
    }

    [Fact]
    public void ToHttpStatusCode_Should_CoverEveryMember()
    {
        ExpectedStatus.Keys.Should().BeEquivalentTo(Enum.GetValues<ErrorType>());

        foreach (ErrorType type in Enum.GetValues<ErrorType>())
            type.ToHttpStatusCode().Should().Be(ExpectedStatus[type], type.ToString());
    }

    [Fact]
    public void ToErrorType_Should_ReverseOneToOneMembers()
    {
        foreach (ErrorType type in OneToOne)
            type.ToHttpStatusCode().ToErrorType().Should().Be(type);
    }

    [Fact]
    public void DefaultMapper_Should_CoverEveryMember()
    {
        ExpectedTitle.Keys.Should().BeEquivalentTo(Enum.GetValues<ErrorType>());
        DefaultErrorStatusCodeMapper mapper = DefaultErrorStatusCodeMapper.Instance;

        foreach (ErrorType type in Enum.GetValues<ErrorType>())
        {
            Error error = Error.Exception("Code", new InvalidOperationException("message"), type);
            int status = mapper.GetStatusCode(error);
            status.Should().Be(ExpectedStatus[type], type.ToString());
            mapper.GetTitle(error, status).Should().Be(ExpectedTitle[type], type.ToString());
        }
    }

    [Fact]
    public void HttpStatusCodeMapper_Should_RoundTripEveryOneToOneMember()
    {
        foreach (ErrorType type in OneToOne.Where(t => t != ErrorType.Failure))
        {
            var status = (HttpStatusCode)type.ToHttpStatusCode();
            HttpStatusCodeMapper.ToErrorType(status).Should().Be(type);
            HttpStatusCodeMapper.ToError(status).Type.Should().Be(type);
        }
    }

    [Fact]
    public void HttpStatusCodeMapper_Should_MapUnprocessableEntityToDomain()
    {
        Error error = HttpStatusCodeMapper.ToError(HttpStatusCode.UnprocessableEntity);

        error.Type.Should().Be(ErrorType.Domain);
        error.Code.Should().Be("Http.422");
    }

    [Fact]
    public void Domain_WithDefaults_ShouldCreateDomainError()
    {
        Error error = Error.Domain();

        error.Type.Should().Be(ErrorType.Domain);
        error.Code.Should().Be(nameof(ErrorType.Domain));
        error.Description.Should().Be("A domain rule rejected the operation.");
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Domain_WithValues_ShouldKeepThem()
    {
        ErrorMetadata metadata = new ErrorMetadata().AddMetadata("Amount", 10);

        Error error = Error.Domain("Payment.Declined", "The card was declined.", metadata);

        error.Code.Should().Be("Payment.Declined");
        error.Description.Should().Be("The card was declined.");
        error.Type.Should().Be(ErrorType.Domain);
        error.NumericType.Should().Be(8);
        error.Metadata.Should().BeSameAs(metadata);
    }

    [Fact]
    public void SelectPrimaryError_Should_PreferDomainOverClientErrors_And_ServerErrorsOverDomain()
    {
        DefaultErrorStatusCodeMapper mapper = DefaultErrorStatusCodeMapper.Instance;
        Error validation = Error.Validation();
        Error conflict = Error.Conflict();
        Error domain = Error.Domain();
        Error unexpected = Error.Unexpected();

        mapper.SelectPrimaryError([validation, conflict, domain]).Should().Be(domain);
        mapper.SelectPrimaryError([domain, conflict, unexpected]).Should().Be(unexpected);
    }

    [Fact]
    public async Task RetryIfFailed_Should_NotRetry_When_ErrorIsDomain()
    {
        int attempts = 0;
        Func<CancellationToken, Task<Result>> operation = _ =>
        {
            attempts++;
            return Task.FromResult(Result.Failure(Error.Domain()));
        };

        Result result = await operation.RetryIfFailed(maxAttempts: 2, delay: TimeSpan.FromMilliseconds(1));

        result.IsFailure.Should().BeTrue();
        attempts.Should().Be(1);
    }

    [Fact]
    public void Json_Should_ReadPayloadWrittenWithoutDomain()
    {
        const string plain = """{"Code":"User.NotFound","Description":"Missing","Type":4,"NumericType":4,"Metadata":null}""";
        const string named = """{"Code":"User.NotFound","Description":"Missing","Type":"not_found","NumericType":4,"Metadata":null}""";

        Error fromPlain = JsonSerializer.Deserialize<Error>(plain);
        Error fromNamed = JsonSerializer.Deserialize<Error>(named, ConventionOptions);

        fromPlain.Should().Be(Error.NotFound("User.NotFound", "Missing"));
        fromNamed.Should().Be(Error.NotFound("User.NotFound", "Missing"));
    }

    [Fact]
    public void Json_Should_RoundTripDomain()
    {
        Error error = Error.Domain("Payment.Declined", "The card was declined.");

        string named = JsonSerializer.Serialize(error, ConventionOptions);
        Error fromNamed = JsonSerializer.Deserialize<Error>(named, ConventionOptions);
        Error fromPlain = JsonSerializer.Deserialize<Error>(JsonSerializer.Serialize(error));

        named.Should().Contain("\"Type\":\"domain\"");
        fromNamed.Should().Be(error);
        fromPlain.Should().Be(error);
    }

    [Fact]
    public void Json_Should_FallBackToUnknown_When_TypeNameIsNotDefined()
    {
        const string future = """{"Code":"X","Description":"Y","Type":"some_future_type","NumericType":42,"Metadata":null}""";

        Error error = JsonSerializer.Deserialize<Error>(future, ConventionOptions);

        error.Type.Should().Be(ErrorType.Unknown);
        error.Type.ToHttpStatusCode().Should().Be(HttpCodes.InternalServerError);
    }

    [Fact]
    public void Json_Should_MapUndefinedNumberTo500_When_ReadWithoutConventions()
    {
        const string future = """{"Code":"X","Description":"Y","Type":42,"NumericType":42,"Metadata":null}""";

        Error error = JsonSerializer.Deserialize<Error>(future);

        error.Type.ToHttpStatusCode().Should().Be(HttpCodes.InternalServerError);
        DefaultErrorStatusCodeMapper.Instance.GetTitle(error, HttpCodes.InternalServerError).Should().Be("Unknown Error");
    }

    [Fact]
    public void ParseWire_Should_RejectUnknownName_When_ReadAsInput()
    {
        ErrorTypeExtensions.TryParseWire("some_future_type", out _).Should().BeFalse();
    }
}
