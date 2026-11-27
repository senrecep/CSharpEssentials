using System.Reflection;
using System.Text.Json;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class TypedErrorTests
{
    internal static class UserErrors
    {
        internal static class Codes
        {
            public const string NotFound = "User.NotFound";
            public const string Deleted = "User.Deleted";
            public const string Locked = "User.Locked";
        }

        public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
        public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

        public static NotFoundError NotFoundById(Guid id) => new(Codes.NotFound, $"User {id} not found.");

        internal sealed record Locked(Guid Id) : ConflictError(Codes.Locked, "User is locked.");
    }

    [ErrorKind(ErrorType.Conflict)]
    internal readonly record struct StructLocked(int Id) : IError
    {
        public string Code => "User.Locked";
        public string Description => $"User {Id} is locked.";
        public ErrorType Type => ErrorType.Conflict;
        public int NumericType => (int)Type;
        public ErrorMetadata? Metadata => null;
    }

    internal readonly record struct StructWithCodeField(string Code, string Description) : IError
    {
        public ErrorType Type => ErrorType.Conflict;
        public int NumericType => (int)Type;
        public ErrorMetadata? Metadata => null;
    }

    public static TheoryData<Type, ErrorType> Kinds => new()
    {
        { typeof(NotFoundError), ErrorType.NotFound },
        { typeof(ConflictError), ErrorType.Conflict },
        { typeof(ValidationError), ErrorType.Validation },
        { typeof(UnauthorizedError), ErrorType.Unauthorized },
        { typeof(ForbiddenError), ErrorType.Forbidden },
        { typeof(UnexpectedError), ErrorType.Unexpected },
        { typeof(FailureError), ErrorType.Failure },
        { typeof(DomainError), ErrorType.Domain },
    };

    private static TypedError Create(Type kind, string code = "C", string description = "d", ErrorMetadata? metadata = null) =>
        (TypedError)Activator.CreateInstance(kind, code, description, metadata)!;

    [Theory]
    [MemberData(nameof(Kinds))]
    public void Kind_ShouldCarryItsTypeAndMatchingAttribute(Type kind, ErrorType expected)
    {
        TypedError error = Create(kind);

        error.Type.Should().Be(expected);
        error.NumericType.Should().Be((int)expected);
        kind.GetCustomAttribute<ErrorKindAttribute>()!.Type.Should().Be(expected);
        kind.IsSealed.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ToError_ShouldCarryCodeDescriptionTypeAndSameMetadata(Type kind, ErrorType expected)
    {
        ErrorMetadata metadata = new("id", 5);
        TypedError typed = Create(kind, "K.Code", "desc", metadata);

        Error error = typed.ToError();

        error.Code.Should().Be("K.Code");
        error.Description.Should().Be("desc");
        error.Type.Should().Be(expected);
        error.NumericType.Should().Be((int)expected);
        error.Metadata.Should().BeSameAs(metadata);
    }

    [Fact]
    public void Kinds_ShouldCoverEveryErrorTypeExceptUnknownWithMatchingAttribute()
    {
        Type[] kinds = [.. typeof(TypedError).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(TypedError).IsAssignableFrom(t))];

        foreach (Type kind in kinds)
            kind.GetCustomAttribute<ErrorKindAttribute>()!.Type.Should().Be(Create(kind).Type, kind.Name);

        kinds.Select(k => Create(k).Type).Should().BeEquivalentTo(
            Enum.GetValues<ErrorType>().Where(t => t != ErrorType.Unknown));
    }

    [Theory]
    [InlineData(ErrorType.Unknown)]
    [InlineData(ErrorType.Domain)]
    public void ErrorCreate_ShouldKeepAnyTypeAndMetadata(ErrorType type)
    {
        ErrorMetadata metadata = new("id", 5);

        Error error = Error.Create("C", "d", type, metadata);

        error.Type.Should().Be(type);
        error.NumericType.Should().Be((int)type);
        error.Metadata.Should().BeSameAs(metadata);
        Error.Create("C", "d", type).Metadata.Should().BeNull();
    }

    [Fact]
    public void ErrorCreate_WithNullCodeOrDescription_ShouldThrow()
    {
        Action code = () => _ = Error.Create(null!, "d", ErrorType.Failure);
        Action description = () => _ = Error.Create("C", null!, ErrorType.Failure);

        code.Should().Throw<ArgumentNullException>().WithParameterName("code");
        description.Should().Throw<ArgumentNullException>().WithParameterName("description");
    }

    [Fact]
    public void ToError_WithoutMetadata_ShouldKeepNullMetadata() =>
        UserErrors.NotFound.ToError().Metadata.Should().BeNull();

    [Fact]
    public void Constructor_WithNullCode_ShouldThrow()
    {
        Action act = () => _ = new NotFoundError(null!, "d");

        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldThrow()
    {
        Action act = () => _ = new NotFoundError("C", null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("description");
    }

    [Fact]
    public void With_NullCodeOrDescription_ShouldThrow()
    {
        Action code = () => _ = UserErrors.NotFound with { Code = null! };
        Action description = () => _ = UserErrors.NotFound with { Description = null! };

        code.Should().Throw<ArgumentNullException>().WithParameterName("Code");
        description.Should().Throw<ArgumentNullException>().WithParameterName("Description");
    }

    [Fact]
    public void With_ShouldKeepRuntimeType()
    {
        TypedError locked = new UserErrors.Locked(Guid.NewGuid());

        TypedError copy = locked with { Description = "changed" };

        copy.Should().BeOfType<UserErrors.Locked>();
        copy.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Equals_SameKindWithEquivalentMetadataCopies_ShouldBeEqualWithSameHashCode()
    {
        var left = new NotFoundError("C", "d", new ErrorMetadata { ["id"] = 5, ["n"] = new ErrorMetadata("k", "v") });
        var right = new NotFoundError("C", "d", new ErrorMetadata { ["id"] = 5, ["n"] = new ErrorMetadata("k", "v") });

        (left == right).Should().BeTrue();
        left.Equals(right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentMetadata_ShouldNotBeEqual()
    {
        var left = new NotFoundError("C", "d", new ErrorMetadata("id", 5));
        var right = new NotFoundError("C", "d", new ErrorMetadata("id", 6));

        (left == right).Should().BeFalse();
        (left == new NotFoundError("C", "d")).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentKindsWithSameCode_ShouldNotBeEqual()
    {
        TypedError notFound = new NotFoundError("C", "d");
        TypedError conflict = new ConflictError("C", "d");

        notFound.Equals(conflict).Should().BeFalse();
        conflict.Equals(notFound).Should().BeFalse();
    }

    [Fact]
    public void Equals_TwoLeafInstancesWithSameState_ShouldBeEqual()
    {
        var id = Guid.NewGuid();

        (new UserErrors.Locked(id) == new UserErrors.Locked(id)).Should().BeTrue();
        (new UserErrors.Locked(id) == new UserErrors.Locked(Guid.NewGuid())).Should().BeFalse();
        new UserErrors.Locked(id).GetHashCode().Should().Be(new UserErrors.Locked(id).GetHashCode());
    }

    [Fact]
    public void Equals_LeafAndBaseKindWithSameCode_ShouldNotBeEqualInEitherDirection()
    {
        ConflictError leaf = new UserErrors.Locked(Guid.Empty);
        var kind = new ConflictError(UserErrors.Codes.Locked, "User is locked.");

        leaf.Equals(kind).Should().BeFalse();
        kind.Equals(leaf).Should().BeFalse();
        (leaf == kind).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ShouldBeFalse()
    {
        NotFoundError? missing = null;

        UserErrors.NotFound.Equals(missing).Should().BeFalse();
        (UserErrors.NotFound == missing).Should().BeFalse();
    }

    [Fact]
    public void JsonRoundTrip_WithoutMetadata_ShouldStayEqual()
    {
        NotFoundError back = JsonSerializer.Deserialize<NotFoundError>(JsonSerializer.Serialize(UserErrors.NotFound))!;

        back.Should().Be(UserErrors.NotFound);
        back.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void JsonRoundTrip_WithMetadataValues_ShouldKeepCodeAndTypeButNotEquality()
    {
        var original = new NotFoundError("C", "d", new ErrorMetadata("id", 5));

        NotFoundError back = JsonSerializer.Deserialize<NotFoundError>(JsonSerializer.Serialize(original))!;

        back.Code.Should().Be(original.Code);
        back.Should().BeOfType<NotFoundError>();
        back.Metadata!["id"].Should().BeOfType<JsonElement>();
        (back == original).Should().BeFalse();
    }

    [Fact]
    public void PredefinedInstance_ShouldEqualFreshEquivalentAndBridgeCodeAndType()
    {
        UserErrors.NotFound.Should().Be(new NotFoundError(UserErrors.Codes.NotFound, "User not found."));
        UserErrors.NotFound.Should().NotBe(UserErrors.Deleted);

        Error bridged = UserErrors.Deleted.ToError();
        bridged.Code.Should().Be(UserErrors.Codes.Deleted);
        bridged.Type.Should().Be(ErrorType.NotFound);
        UserErrors.NotFoundById(Guid.Empty).Code.Should().Be(UserErrors.Codes.NotFound);
    }

    [Fact]
    public void PredefinedInstancesOfOneKind_ShouldBeToldApartByConstCode()
    {
        static string Describe(TypedError error) => error switch
        {
            NotFoundError { Code: UserErrors.Codes.Deleted } => "gone",
            NotFoundError => "missing",
            UserErrors.Locked locked => $"locked {locked.Id}",
            _ => "other",
        };

        Describe(UserErrors.Deleted).Should().Be("gone");
        Describe(UserErrors.NotFound).Should().Be("missing");
        Describe(new UserErrors.Locked(Guid.Empty)).Should().Be($"locked {Guid.Empty}");
    }

    [Fact]
    public void DerivedLeaf_ShouldKeepKindTypeAndAttribute()
    {
        var locked = new UserErrors.Locked(Guid.NewGuid());

        locked.Should().BeAssignableTo<ConflictError>();
        locked.Type.Should().Be(ErrorType.Conflict);
        locked.Code.Should().Be(UserErrors.Codes.Locked);
        locked.ToError().Type.Should().Be(ErrorType.Conflict);
        typeof(UserErrors.Locked).GetCustomAttribute<ErrorKindAttribute>()!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void StructErrorType_ShouldExposeErrorKindAndWorkThroughIError()
    {
        IError[] errors = [new StructLocked(7)];
        IError boxed = errors[0];

        typeof(StructLocked).GetCustomAttribute<ErrorKindAttribute>()!.Type.Should().Be(ErrorType.Conflict);
        boxed.Type.Should().Be(ErrorType.Conflict);
        boxed.Description.Should().Be("User 7 is locked.");
        Error.Create(boxed.Code, boxed.Description, boxed.Type, boxed.Metadata).Code.Should().Be("User.Locked");
    }

    [Fact]
    public void StructErrorType_Default_ShouldHaveNullCode()
    {
        IError boxed = default(StructWithCodeField);

        boxed.Code.Should().BeNull();
        boxed.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void ErrorKindAttribute_ShouldBeInheritedAndSingle()
    {
        AttributeUsageAttribute usage = typeof(ErrorKindAttribute).GetCustomAttribute<AttributeUsageAttribute>()!;

        usage.Inherited.Should().BeTrue();
        usage.AllowMultiple.Should().BeFalse();
        usage.ValidOn.Should().Be(AttributeTargets.Class | AttributeTargets.Struct);
    }
}
