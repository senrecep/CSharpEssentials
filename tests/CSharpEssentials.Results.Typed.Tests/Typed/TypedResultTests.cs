using System.Runtime.CompilerServices;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class TypedResultTests
{
    private static readonly ValidationError Invalid = new(UserErrors.Codes.Invalid, "User is invalid.");

    private static Result<UserDto, NotFoundError, UserErrors.Locked> Get(int id) => id switch
    {
        0 => UserErrors.NotFound,
        1 => UserErrors.Deleted,
        7 => new UserErrors.Locked(id),
        _ => new UserDto(id),
    };

    private static string Describe(Result<UserDto, NotFoundError, UserErrors.Locked> result) => result switch
    {
        UserDto user => $"ok {user.Id}",
        NotFoundError { Code: UserErrors.Codes.Deleted } => "gone",
        NotFoundError => "not found",
        UserErrors.Locked locked => $"locked {locked.Id}",
    };

    [Fact]
    public void Constructor_Should_HoldTheValue_When_CreatedFromAValue()
    {
        var dto = new UserDto(5);

        var result = new Result<UserDto, NotFoundError>(dto);

        result.HasValue.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().BeSameAs(dto);
        result.TryGetValue(out UserDto value).Should().BeTrue();
        value.Should().BeSameAs(dto);
        result.TryGetValue(out NotFoundError _).Should().BeFalse();
    }

    [Fact]
    public void Constructor_Should_HoldEachErrorInItsOwnSlot_For_Arity3()
    {
        var locked = new UserErrors.Locked(3);

        var first = new Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError>(UserErrors.NotFound);
        var second = new Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError>(locked);
        var third = new Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError>(Invalid);

        first.TryGetValue(out NotFoundError notFound).Should().BeTrue();
        notFound.Should().BeSameAs(UserErrors.NotFound);
        first.TryGetValue(out UserErrors.Locked _).Should().BeFalse();
        first.TryGetValue(out ValidationError _).Should().BeFalse();

        second.TryGetValue(out UserErrors.Locked lockedOut).Should().BeTrue();
        lockedOut.Should().BeSameAs(locked);
        second.TryGetValue(out NotFoundError _).Should().BeFalse();

        third.TryGetValue(out ValidationError invalid).Should().BeTrue();
        invalid.Should().BeSameAs(Invalid);
        third.TryGetValue(out UserDto _).Should().BeFalse();

        foreach (Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError> result in new[] { first, second, third })
        {
            result.HasValue.Should().BeTrue();
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
        }

        third.Value.Should().BeSameAs(Invalid);
    }

    [Fact]
    public void Constructor_Should_Throw_When_TheValueOrTheErrorIsNull()
    {
        Action nullValue = () => _ = new Result<UserDto, NotFoundError>((UserDto)null!);
        Action nullError = () => _ = new Result<UserDto, NotFoundError, ConflictError>((ConflictError)null!);

        nullValue.Should().ThrowExactly<ArgumentNullException>().WithParameterName("value");
        nullError.Should().ThrowExactly<ArgumentNullException>().WithParameterName("error");
    }

    [Theory]
    [InlineData(5, "ok 5")]
    [InlineData(0, "not found")]
    [InlineData(1, "gone")]
    [InlineData(7, "locked 7")]
    public void ImplicitReturn_Should_PutEachCaseInItsSlot(int id, string expected)
    {
        string described = Describe(Get(id));

        described.Should().Be(expected);
    }

    [Fact]
    public void ImplicitReturn_Should_KeepThePredefinedInstance()
    {
        Get(0).TryGetValue(out NotFoundError notFound).Should().BeTrue();

        notFound.Should().BeSameAs(UserErrors.NotFound);
    }

    [Fact]
    public void ImplicitReturn_Should_PutADerivedErrorInItsBaseSlot()
    {
        Result<UserDto, ConflictError> result = new UserErrors.Locked(9);

        result.TryGetValue(out ConflictError conflict).Should().BeTrue();
        conflict.Should().BeOfType<UserErrors.Locked>().Which.Id.Should().Be(9);
    }

    [Fact]
    public void Default_Should_HoldNoCase()
    {
        Result<UserDto, NotFoundError, UserErrors.Locked> result = default;

        result.HasValue.Should().BeFalse();
        result.Value.Should().BeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.TryGetValue(out UserDto _).Should().BeFalse();
        result.TryGetValue(out NotFoundError _).Should().BeFalse();
        result.TryGetValue(out UserErrors.Locked _).Should().BeFalse();
    }

    [Fact]
    public void Default_Should_ThrowSwitchExpressionException_When_SwitchedWithoutANullArm()
    {
        Action act = () => Describe(default);

        act.Should().ThrowExactly<SwitchExpressionException>();
    }

    [Fact]
    public void Default_Should_TakeTheNullArm_When_OneIsWritten()
    {
        Result<UserDto, NotFoundError> result = default;

        string described = result switch
        {
            UserDto => "ok",
            NotFoundError => "not found",
            null => "default",
        };

        described.Should().Be("default");
    }

    [Fact]
    public void Default_Should_ThrowInvalidOperationException_From_MatchSwitchAndToResult()
    {
        Result<UserDto, NotFoundError, ConflictError, ValidationError> result = default;

        Action match = () => result.Match(_ => 1, _ => 2, _ => 3, _ => 4);
        Action @switch = () => result.Switch(_ => { }, _ => { }, _ => { }, _ => { });
        Action toResult = () => result.ToResult();

        match.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        @switch.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
        toResult.Should().ThrowExactly<InvalidOperationException>().WithMessage("*default-initialized*");
    }

    [Fact]
    public void Default_Should_LookLikeAnUnexpectedFailure_Through_IResultBase()
    {
        IResultBase result = default(Result<UserDto, NotFoundError>);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Result.Default");
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.LastError.Should().Be(result.FirstError);
        result.Errors.Should().Equal(result.FirstError);
        result.ErrorsOrEmptyArray.Should().Equal(result.FirstError);
    }

    [Theory]
    [InlineData(5, "ok")]
    [InlineData(0, "not found")]
    [InlineData(7, "locked")]
    public void Match_Should_CallOnlyTheFunctionOfTheHeldCase(int id, string expected)
    {
        int calls = 0;

        string matched = Get(id).Match(
            _ => Count("ok"),
            error => Count(error.Code == UserErrors.Codes.NotFound ? "not found" : "other"),
            _ => Count("locked"));

        matched.Should().Be(expected);
        calls.Should().Be(1);

        string Count(string value)
        {
            calls++;
            return value;
        }
    }

    [Fact]
    public void Match_Should_PassTheHeldErrorInstance()
    {
        var locked = new UserErrors.Locked(2);
        Result<UserDto, NotFoundError, UserErrors.Locked> result = locked;

        object matched = result.Match<object>(dto => dto, notFound => notFound, error => error);

        matched.Should().BeSameAs(locked);
    }

    [Fact]
    public void Switch_Should_CallOnlyTheActionOfTheHeldCase()
    {
        var calls = new List<string>();
        Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError> result = Invalid;

        result.Switch(_ => calls.Add("ok"), _ => calls.Add("not found"), _ => calls.Add("locked"), error => calls.Add(error.Code));

        calls.Should().Equal(UserErrors.Codes.Invalid);
    }

    [Fact]
    public void Widening_Should_KeepTheCase_For_EverySourceSlot()
    {
        Result<UserDto, NotFoundError> fromFirst = UserErrors.NotFound;
        Result<UserDto, UserErrors.Locked> fromSecond = new UserErrors.Locked(4);
        Result<UserDto, NotFoundError> fromValue = new UserDto(8);
        Result<UserDto, NotFoundError> fromDefault = default;

        Result<UserDto, NotFoundError, UserErrors.Locked> first = fromFirst;
        Result<UserDto, NotFoundError, UserErrors.Locked> second = fromSecond;
        Result<UserDto, NotFoundError, UserErrors.Locked> value = fromValue;
        Result<UserDto, NotFoundError, UserErrors.Locked> none = fromDefault;

        Describe(first).Should().Be("not found");
        Describe(second).Should().Be("locked 4");
        Describe(value).Should().Be("ok 8");
        none.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Widening_Should_KeepTheCase_From_Arity1And2_To_Arity3()
    {
        Result<UserDto, ValidationError> validation = Invalid;
        Result<UserDto, ValidationError, NotFoundError> pair = UserErrors.NotFound;

        Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError> fromOne = validation;
        Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError> fromTwo = pair;

        fromOne.TryGetValue(out ValidationError invalid).Should().BeTrue();
        invalid.Should().BeSameAs(Invalid);
        fromTwo.TryGetValue(out NotFoundError notFound).Should().BeTrue();
        notFound.Should().BeSameAs(UserErrors.NotFound);
    }

    [Fact]
    public void Reorder_Should_KeepTheCase_For_EveryPermutation()
    {
        var locked = new UserErrors.Locked(1);
        Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError>[] sources = [UserErrors.NotFound, locked, Invalid, new UserDto(3)];

        foreach (Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError> source in sources)
        {
            object? expected = source.Value;

            Result<UserDto, ValidationError, NotFoundError, UserErrors.Locked> rotated = source;
            Result<UserDto, UserErrors.Locked, ValidationError, NotFoundError> rotatedTwice = rotated;
            Result<UserDto, NotFoundError, UserErrors.Locked, ValidationError> back = rotatedTwice;
            Result<UserDto, ValidationError, UserErrors.Locked, NotFoundError> reversed = source;

            rotated.Value.Should().BeSameAs(expected);
            rotatedTwice.Value.Should().BeSameAs(expected);
            back.Value.Should().BeSameAs(expected);
            reversed.Value.Should().BeSameAs(expected);
            back.IsSuccess.Should().Be(source.IsSuccess);
        }

        Result<UserDto, UserErrors.Locked, NotFoundError> swapped = Get(7);
        swapped.TryGetValue(out UserErrors.Locked swappedLocked).Should().BeTrue();
        swappedLocked.Id.Should().Be(7);
    }

    [Fact]
    public void ToResult_Should_KeepTheValue_When_Successful()
    {
        Result<UserDto> result = Get(5).ToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(5);
    }

    [Fact]
    public void ToResult_Should_KeepCodeDescriptionTypeAndTheMetadataInstance_When_TheErrorIsATypedError()
    {
        Result<UserDto> result = Get(1).ToResult();

        result.IsFailure.Should().BeTrue();
        Error error = result.Errors.Should().ContainSingle().Subject;
        error.Code.Should().Be(UserErrors.Codes.Deleted);
        error.Description.Should().Be(UserErrors.Deleted.Description);
        error.Type.Should().Be(ErrorType.NotFound);
        error.Metadata.Should().BeSameAs(UserErrors.Deleted.Metadata);
    }

    [Fact]
    public void ToResult_Should_KeepTheError_When_TheSlotIsError()
    {
        Error conflict = Error.Conflict("User.Taken", "Name is taken.");
        Result<UserDto, Error> typed = conflict;

        typed.ToResult().FirstError.Should().Be(conflict);
    }

    [Fact]
    public void ToResult_Should_UseErrorException_When_TheSlotIsAnException()
    {
        var exception = new TimeoutException("Timed out.");
        Result<UserDto, NotFoundError, TimeoutException> typed = exception;

        Error error = typed.ToResult().FirstError;

        error.Code.Should().Be(nameof(TimeoutException));
        error.Description.Should().Be("Timed out.");
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void ToResult_Should_FallBackToUnexpected_When_TheSlotIsNotAnIError()
    {
        Result<UserDto, PlainError> typed = new PlainError("quota");

        Error error = typed.ToResult().FirstError;

        error.Code.Should().Be(nameof(PlainError));
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void ToResult_Should_BridgeAStructIError_And_FallBackForItsDefault()
    {
        Result<UserDto, RateLimited> limited = new RateLimited("Api.RateLimited");
        Result<UserDto, RateLimited> defaultStruct = default(RateLimited);

        limited.ToResult().FirstError.Should().Match<Error>(e => e.Code == "Api.RateLimited" && e.Type == ErrorType.Failure);
        defaultStruct.IsFailure.Should().BeTrue();
        defaultStruct.ToResult().FirstError.Should().Match<Error>(e => e.Code == nameof(RateLimited) && e.Type == ErrorType.Unexpected);
    }

    [Fact]
    public void IResultBase_Should_MatchToResult_For_EveryCase()
    {
        foreach (int id in new[] { 0, 1, 5, 7 })
        {
            Result<UserDto, NotFoundError, UserErrors.Locked> typed = Get(id);
            IResultBase view = typed;
            Result<UserDto> classic = typed.ToResult();

            view.IsSuccess.Should().Be(classic.IsSuccess);
            view.IsFailure.Should().Be(classic.IsFailure);
            view.Errors.Should().Equal(classic.Errors);
            view.FirstError.Should().Be(classic.FirstError);
            view.LastError.Should().Be(classic.LastError);
            view.ErrorsOrEmptyArray.Should().Equal(classic.ErrorsOrEmptyArray);
        }
    }

    [Fact]
    public void Errors_Should_NotThrow_For_AnySlotType()
    {
        IResultBase[] results =
        [
            (Result<UserDto, TimeoutException>)new TimeoutException(),
            (Result<UserDto, PlainError>)new PlainError("x"),
            (Result<UserDto, RateLimited>)default(RateLimited),
            default(Result<UserDto, NotFoundError, ConflictError, ValidationError>),
        ];

        foreach (IResultBase result in results)
        {
            Action read = () => _ = (result.Errors, result.FirstError, result.LastError, result.ErrorsOrEmptyArray);
            read.Should().NotThrow();
            result.Errors.Should().ContainSingle();
        }
    }

    [Theory]
    [InlineData(typeof(Result<,>))]
    [InlineData(typeof(Result<,,>))]
    [InlineData(typeof(Result<,,,>))]
    public void Shape_Should_ImplementTheResultInterfaces_But_NotTheFailureFactory(Type type)
    {
        string[] interfaces = [.. type.GetInterfaces().Select(i => i.Name)];

        type.IsValueType.Should().BeTrue();
        interfaces.Should().Contain(["IUnion", nameof(IResultBase), "IValueResult"]);
        interfaces.Should().NotContain(name => name.StartsWith("IFailureFactory", StringComparison.Ordinal));
        type.GetCustomAttributes(inherit: false).Should().Contain(a => a.GetType().FullName == "System.Runtime.CompilerServices.UnionAttribute");
    }

    [Fact]
    public void Shape_Should_KeepOneValueFieldOneErrorFieldAndATag()
    {
        Type[] fields = [.. typeof(Result<int, NotFoundError, ConflictError>)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Select(f => f.FieldType)];

        fields.Should().Equal(typeof(int), typeof(object), typeof(byte));
        Unsafe.SizeOf<Result<int, NotFoundError, ConflictError>>().Should().Be(16);
    }
}
