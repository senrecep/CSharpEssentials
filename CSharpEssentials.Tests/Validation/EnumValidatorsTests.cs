using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class EnumValidatorsTests
{
    private sealed record Model<TEnum>(TEnum Value);

    private static async Task<Result<Model<TEnum>>> Validate<TEnum>(TEnum value, Action<RuleChain<Model<TEnum>, TEnum>> configure) =>
        await Validator.ValidateAsync(new Model<TEnum>(value), (m, rules) => configure(rules.For(() => m.Value)));

    [Fact]
    public void Metadata_Fixtures_Should_Be_Registered_By_The_Generator()
    {
        EnumMetadata.IsRegistered(typeof(ValidationOrderStatus)).Should().BeTrue();
        EnumMetadata.IsRegistered(typeof(ValidationChannels)).Should().BeTrue();
        EnumMetadata.IsRegistered(typeof(PlainColor)).Should().BeFalse();
        EnumMetadata.IsRegistered(typeof(PlainAccess)).Should().BeFalse();
    }

    [Theory]
    [InlineData(ValidationOrderStatus.Pending)]
    [InlineData(ValidationOrderStatus.InProgress)]
    [InlineData(ValidationOrderStatus.Unknown)]
    public async Task IsDefinedEnum_Should_Pass_For_Defined_Members(ValidationOrderStatus value)
    {
        Result<Model<ValidationOrderStatus>> result = await Validate(value, static c => c.IsDefinedEnum());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Fail_With_Binding_Message_For_Undefined_Value()
    {
        Result<Model<ValidationOrderStatus>> result = await Validate((ValidationOrderStatus)42, static c => c.IsDefinedEnum());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("enum.invalid");
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("'42' is not a valid ValidationOrderStatus. Allowed values: pending, in_progress.");
        result.FirstError.Description.Should().Be(
            EnumMetadata.Get<ValidationOrderStatus>().CreateError("42", EnumReadMode.Input, "Value").Message);
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Accept_Combinations_Of_Defined_Flags()
    {
        Result<Model<ValidationChannels>> result = await Validate(ValidationChannels.Email | ValidationChannels.Sms, static c => c.IsDefinedEnum());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Reject_Undefined_Flag_Bits()
    {
        Result<Model<ValidationChannels>> result = await Validate(ValidationChannels.Email | (ValidationChannels)8, static c => c.IsDefinedEnum());

        result.FirstError.Code.Should().Be("enum.invalid");
        result.FirstError.Description.Should().Be("'9' is not a valid ValidationChannels. Allowed values: none, email, sms, push.");
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Use_Custom_Message_And_Error()
    {
        Result<Model<ValidationOrderStatus>> withMessage = await Validate((ValidationOrderStatus)42, static c => c.IsDefinedEnum("bad status"));
        Error custom = Error.Validation("status.custom", "custom");
        Result<Model<ValidationOrderStatus>> withError = await Validate((ValidationOrderStatus)42, c => c.IsDefinedEnum(custom));

        withMessage.FirstError.Code.Should().Be("enum.invalid");
        withMessage.FirstError.Description.Should().Be("bad status");
        withError.FirstError.Should().Be(custom);
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Use_Enum_IsDefined_For_Enums_Without_Metadata()
    {
        Result<Model<PlainColor>> valid = await Validate(PlainColor.Green, static c => c.IsDefinedEnum());
        Result<Model<PlainColor>> invalid = await Validate((PlainColor)7, static c => c.IsDefinedEnum());

        valid.IsSuccess.Should().BeTrue();
        invalid.FirstError.Code.Should().Be("enum.invalid");
        invalid.FirstError.Description.Should().Be("'7' is not a valid PlainColor. Allowed values: Red, Green, Blue.");
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Accept_Flag_Combinations_For_Enums_Without_Metadata()
    {
        Result<Model<PlainAccess>> combined = await Validate(PlainAccess.Read | PlainAccess.Write, static c => c.IsDefinedEnum());
        Result<Model<PlainAccess>> undefined = await Validate((PlainAccess)16, static c => c.IsDefinedEnum());

        combined.IsSuccess.Should().BeTrue();
        undefined.FirstError.Description.Should().Be("'16' is not a valid PlainAccess. Allowed values: None, Read, Write.");
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Skip_Null_And_Check_Present_Nullable_Values()
    {
        Result<Model<ValidationOrderStatus?>> none = await Validate<ValidationOrderStatus?>(null, static c => c.IsDefinedEnum());
        Result<Model<ValidationOrderStatus?>> defined = await Validate<ValidationOrderStatus?>(ValidationOrderStatus.Pending, static c => c.IsDefinedEnum());
        Result<Model<ValidationOrderStatus?>> undefined = await Validate<ValidationOrderStatus?>((ValidationOrderStatus)42, static c => c.IsDefinedEnum());
        Error custom = Error.Validation("custom");
        Result<Model<ValidationOrderStatus?>> withError = await Validate<ValidationOrderStatus?>((ValidationOrderStatus)42, c => c.IsDefinedEnum(custom));

        none.IsSuccess.Should().BeTrue();
        defined.IsSuccess.Should().BeTrue();
        undefined.FirstError.Code.Should().Be("enum.invalid");
        undefined.FirstError.Description.Should().StartWith("'42' is not a valid ValidationOrderStatus.");
        withError.FirstError.Should().Be(custom);
    }

    [Fact]
    public async Task IsOneOf_Should_Pass_For_Allowed_Value()
    {
        Result<Model<ValidationOrderStatus>> result = await Validate(
            ValidationOrderStatus.InProgress,
            static c => c.IsOneOf(ValidationOrderStatus.Pending, ValidationOrderStatus.InProgress));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IsOneOf_Should_List_Allowed_Wire_Names()
    {
        Result<Model<ValidationOrderStatus>> result = await Validate(
            ValidationOrderStatus.Pending,
            static c => c.IsOneOf(ValidationOrderStatus.InProgress));

        result.FirstError.Code.Should().Be("enum.not_allowed");
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Description.Should().Be("'pending' is not a valid ValidationOrderStatus. Allowed values: in_progress.");
    }

    [Fact]
    public async Task IsOneOf_Should_Write_Flags_And_Undefined_Values_Like_The_Wire()
    {
        Result<Model<ValidationChannels>> flags = await Validate(
            ValidationChannels.Email | ValidationChannels.Push,
            static c => c.IsOneOf(ValidationChannels.Email, ValidationChannels.Sms));
        Result<Model<ValidationOrderStatus>> undefined = await Validate(
            (ValidationOrderStatus)42,
            static c => c.IsOneOf(ValidationOrderStatus.Pending));

        flags.FirstError.Description.Should().Be("'email,push' is not a valid ValidationChannels. Allowed values: email, sms.");
        undefined.FirstError.Description.Should().Be("'42' is not a valid ValidationOrderStatus. Allowed values: pending.");
    }

    [Fact]
    public async Task IsOneOf_Should_Use_Member_Names_For_Enums_Without_Metadata()
    {
        Result<Model<PlainColor>> result = await Validate(PlainColor.Red, static c => c.IsOneOf(PlainColor.Green, PlainColor.Blue));

        result.FirstError.Code.Should().Be("enum.not_allowed");
        result.FirstError.Description.Should().Be("'Red' is not a valid PlainColor. Allowed values: Green, Blue.");
    }

    [Fact]
    public async Task IsOneOf_Should_Use_Custom_Message_And_Error()
    {
        ValidationOrderStatus[] allowed = [ValidationOrderStatus.InProgress];
        Error custom = Error.Validation("custom");

        Result<Model<ValidationOrderStatus>> withMessage = await Validate(ValidationOrderStatus.Pending, c => c.IsOneOf(allowed, "not now"));
        Result<Model<ValidationOrderStatus>> withError = await Validate(ValidationOrderStatus.Pending, c => c.IsOneOf(allowed, custom));
        Result<Model<ValidationOrderStatus>> passing = await Validate(ValidationOrderStatus.InProgress, c => c.IsOneOf(allowed, custom));

        withMessage.FirstError.Code.Should().Be("enum.not_allowed");
        withMessage.FirstError.Description.Should().Be("not now");
        withError.FirstError.Should().Be(custom);
        passing.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IsOneOf_Should_Skip_Null_And_Check_Present_Nullable_Values()
    {
        ValidationOrderStatus[] allowed = [ValidationOrderStatus.InProgress];
        Error custom = Error.Validation("custom");

        Result<Model<ValidationOrderStatus?>> none = await Validate<ValidationOrderStatus?>(null, static c => c.IsOneOf(ValidationOrderStatus.InProgress));
        Result<Model<ValidationOrderStatus?>> allowedValue = await Validate<ValidationOrderStatus?>(ValidationOrderStatus.InProgress, static c => c.IsOneOf(ValidationOrderStatus.InProgress));
        Result<Model<ValidationOrderStatus?>> rejected = await Validate<ValidationOrderStatus?>(ValidationOrderStatus.Pending, static c => c.IsOneOf(ValidationOrderStatus.InProgress));
        Result<Model<ValidationOrderStatus?>> withMessage = await Validate<ValidationOrderStatus?>(ValidationOrderStatus.Pending, c => c.IsOneOf(allowed, "not now"));
        Result<Model<ValidationOrderStatus?>> withError = await Validate<ValidationOrderStatus?>(ValidationOrderStatus.Pending, c => c.IsOneOf(allowed, custom));
        Result<Model<ValidationOrderStatus?>> nullWithError = await Validate<ValidationOrderStatus?>(null, c => c.IsOneOf(allowed, custom));

        none.IsSuccess.Should().BeTrue();
        allowedValue.IsSuccess.Should().BeTrue();
        rejected.FirstError.Description.Should().Be("'pending' is not a valid ValidationOrderStatus. Allowed values: in_progress.");
        withMessage.FirstError.Description.Should().Be("not now");
        withError.FirstError.Should().Be(custom);
        nullWithError.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task IsOneOf_Should_Throw_When_Allowed_Is_Null()
    {
        Func<Task> act = () => Validate(ValidationOrderStatus.Pending, static c => c.IsOneOf(null!, "message"));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(ValidationChannels.None)]
    [InlineData(ValidationChannels.Email | ValidationChannels.Sms | ValidationChannels.Push)]
    public async Task HasOnlyDefinedFlags_Should_Pass_For_Defined_Bits(ValidationChannels value)
    {
        Result<Model<ValidationChannels>> result = await Validate(value, static c => c.HasOnlyDefinedFlags());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HasOnlyDefinedFlags_Should_Fail_For_Undefined_Bits()
    {
        Result<Model<ValidationChannels>> result = await Validate((ValidationChannels)17, static c => c.HasOnlyDefinedFlags());

        result.FirstError.Code.Should().Be("enum.invalid");
        result.FirstError.Description.Should().Be("'17' is not a valid ValidationChannels. Allowed values: none, email, sms, push.");
    }

    [Fact]
    public async Task HasOnlyDefinedFlags_Should_Use_Mask_For_Enums_Without_Metadata()
    {
        Result<Model<PlainAccess>> valid = await Validate(PlainAccess.Read | PlainAccess.Write, static c => c.HasOnlyDefinedFlags());
        Result<Model<PlainAccess>> invalid = await Validate((PlainAccess)5, static c => c.HasOnlyDefinedFlags());

        valid.IsSuccess.Should().BeTrue();
        invalid.FirstError.Description.Should().Be("'5' is not a valid PlainAccess. Allowed values: None, Read, Write.");
    }

    [Fact]
    public async Task HasOnlyDefinedFlags_Should_Use_Custom_Message_Error_And_Skip_Null()
    {
        Error custom = Error.Validation("custom");

        Result<Model<ValidationChannels>> withMessage = await Validate((ValidationChannels)8, static c => c.HasOnlyDefinedFlags("bad flags"));
        Result<Model<ValidationChannels>> withError = await Validate((ValidationChannels)8, c => c.HasOnlyDefinedFlags(custom));
        Result<Model<ValidationChannels?>> none = await Validate<ValidationChannels?>(null, static c => c.HasOnlyDefinedFlags());
        Result<Model<ValidationChannels?>> present = await Validate<ValidationChannels?>((ValidationChannels)8, static c => c.HasOnlyDefinedFlags());
        Result<Model<ValidationChannels?>> nullableError = await Validate<ValidationChannels?>((ValidationChannels)8, c => c.HasOnlyDefinedFlags(custom));

        withMessage.FirstError.Code.Should().Be("enum.invalid");
        withMessage.FirstError.Description.Should().Be("bad flags");
        withError.FirstError.Should().Be(custom);
        none.IsSuccess.Should().BeTrue();
        present.FirstError.Code.Should().Be("enum.invalid");
        nullableError.FirstError.Should().Be(custom);
    }

    [Fact]
    public async Task Rules_Should_Not_Run_After_A_Previous_Failure()
    {
        Result<Model<ValidationOrderStatus>> result = await Validate(
            (ValidationOrderStatus)42,
            static c => c.IsDefinedEnum().IsOneOf(ValidationOrderStatus.Pending).HasOnlyDefinedFlags());

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("enum.invalid");
    }

    [Fact]
    public async Task IsDefinedEnum_Should_Handle_Unsigned_64_Bit_Enums_Without_Metadata()
    {
        Result<Model<PlainHuge>> max = await Validate(PlainHuge.Max, static c => c.IsDefinedEnum().HasOnlyDefinedFlags());
        Result<Model<PlainHuge>> undefined = await Validate((PlainHuge)2, static c => c.IsDefinedEnum());

        max.IsSuccess.Should().BeTrue();
        undefined.FirstError.Description.Should().Be("'2' is not a valid PlainHuge. Allowed values: Zero, Max.");
    }
}

[StringEnum]
public enum ValidationOrderStatus
{
    Pending,
    InProgress,

    [EnumFallback]
    Unknown,
}

[StringEnum]
[Flags]
public enum ValidationChannels
{
    None = 0,
    Email = 1,
    Sms = 2,
    Push = 4,
}

public enum PlainColor
{
    Red,
    Green,
    Blue,
}

[Flags]
public enum PlainAccess
{
    None = 0,
    Read = 1,
    Write = 2,
}

internal enum PlainHuge : ulong
{
    Zero = 0,
    Max = ulong.MaxValue,
}
