using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class ForEachAndNestedValidatorsTests
{
    // =========================================================================
    // ForEach
    // =========================================================================

    private sealed record TagsModel(IEnumerable<string?>? Tags);

    private sealed record AddressModel(string? City, string? ZipCode);

    private sealed record OrderModel(IEnumerable<AddressModel>? Addresses);

    private sealed record LineModel(string? Sku, IEnumerable<string?>? Tags);
    private sealed record InvoiceModel(IEnumerable<LineModel>? Lines);

    [Fact]
    public async Task ForEach_ShouldProduceIndexedErrors_WhenItemsFailValidation()
    {
        TagsModel model = new(["valid", "", "also-valid"]);

        Result<TagsModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Tags[1].NotEmpty");
    }

    [Fact]
    public async Task ForEach_ShouldAccumulateErrors_AcrossMultipleItems()
    {
        TagsModel model = new(["", "", ""]);

        Result<TagsModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
        result.Errors[0].Code.Should().Be("Tags[0].NotEmpty");
        result.Errors[1].Code.Should().Be("Tags[1].NotEmpty");
        result.Errors[2].Code.Should().Be("Tags[2].NotEmpty");
    }

    [Fact]
    public async Task ForEach_ShouldPass_WhenAllItemsAreValid()
    {
        TagsModel model = new(["a", "b", "c"]);

        Result<TagsModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForEach_ShouldPass_WhenCollectionIsNull()
    {
        TagsModel model = new(null);

        Result<TagsModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForEach_ShouldPass_WhenCollectionIsEmpty()
    {
        TagsModel model = new([]);

        Result<TagsModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Tags, (tag, tagRules) =>
                tagRules.For(() => tag).NotEmpty()));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForEach_ShouldValidateNestedProperties_WithCorrectPath()
    {
        OrderModel model = new([
            new AddressModel("Istanbul", "34000"),
            new AddressModel("", "99999")
        ]);

        Result<OrderModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Addresses, (address, addrRules) =>
            {
                addrRules.For(() => address.City).NotEmpty();
                addrRules.For(() => address.ZipCode).NotEmpty();
            }));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Addresses[1].City.NotEmpty");
    }

    [Fact]
    public async Task ForEach_CascadeStop_ShouldApplyPerItem()
    {
        OrderModel model = new([
            new AddressModel(null, null),
            new AddressModel("Istanbul", "34000")
        ]);

        Result<OrderModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Addresses, (address, addrRules) =>
                addrRules.For(() => address.City).NotNull().NotEmpty()));

        // First item: City is null → NotNull fails → NotEmpty is cascade-skipped → 1 error total
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Addresses[0].City.NotNull");
    }

    [Fact]
    public async Task ForEachAsync_ShouldAccumulateErrors()
    {
        TagsModel model = new(["", "valid", ""]);

        Result<TagsModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.ForEachAsync(() => m.Tags,
                async (tag, tagRules, token) =>
                {
                    await Task.Yield();
                    tagRules.For(() => tag).NotEmpty();
                }, ct));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task ForEach_Nested_ShouldProduceFullyQualifiedPaths()
    {
        // Lines[0].Tags[1] is empty — expected error code: "Lines[0].Tags[1].NotEmpty"
        InvoiceModel model = new([
            new LineModel("SKU-1", ["valid", ""]),
            new LineModel("SKU-2", ["ok"])
        ]);

        Result<InvoiceModel> result = await Validator.ValidateAsync(model, (m, rules) =>
            rules.ForEach(() => m.Lines, (line, lineRules) =>
                lineRules.ForEach(() => line.Tags, (tag, tagRules) =>
                    tagRules.For(() => tag).NotEmpty())));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Lines[0].Tags[1].NotEmpty");
    }

    // =========================================================================
    // SetValidator
    // =========================================================================

    private sealed class AddressValidator : Validator<AddressModel>
    {
        protected override ValueTask Configure(AddressModel model, RuleContext<AddressModel> rules, CancellationToken ct = default)
        {
            rules.For(() => model.City).NotEmpty();
            rules.For(() => model.ZipCode).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private sealed record ShipmentModel(AddressModel? Destination);

    [Fact]
    public async Task SetValidator_ShouldPropagateNestedErrors_WhenNestedValidationFails()
    {
        ShipmentModel model = new(new AddressModel("", "34000"));

        Result<ShipmentModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Destination!).SetValidatorAsync(new AddressValidator(), ct));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Destination.City.NotEmpty");
    }

    [Fact]
    public async Task SetValidator_ShouldPass_WhenNestedValidationSucceeds()
    {
        ShipmentModel model = new(new AddressModel("Istanbul", "34000"));

        Result<ShipmentModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Destination!).SetValidatorAsync(new AddressValidator(), ct));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetValidator_ShouldSkip_WhenValueIsNull()
    {
        ShipmentModel model = new(null);

        Result<ShipmentModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Destination!).SetValidatorAsync(new AddressValidator(), ct));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetValidator_ShouldSkip_WhenChainAlreadyFailed()
    {
        ShipmentModel model = new(null);

        Result<ShipmentModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Destination!)
                .Must(v => v is not null, "Destination.Required", "Destination is required.")
                .SetValidatorAsync(new AddressValidator(), ct));

        result.Errors.Should().HaveCount(1);
        result.FirstError.Code.Should().Be("Destination.Required");
    }

    [Fact]
    public async Task SetValidator_ShouldPropagateAllErrors_FromNestedValidator()
    {
        ShipmentModel model = new(new AddressModel("", ""));

        Result<ShipmentModel> result = await Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Destination!).SetValidatorAsync(new AddressValidator(), ct));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    // =========================================================================
    // SetValidatorAsync
    // =========================================================================

    private static ValueTask<Result<ShipmentModel>> ValidateShipmentAsync(ShipmentModel model)
        => Validator.ValidateAsync(model, async (m, rules, ct) =>
            await rules.For(() => m.Destination!).SetValidatorAsync(new AddressValidator(), ct));

    [Fact]
    public async Task SetValidatorAsync_ShouldPropagateNestedErrors_WhenNestedValidationFails()
    {
        Result<ShipmentModel> result = await ValidateShipmentAsync(new(new AddressModel("", "34000")));

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Destination.City.NotEmpty");
    }

    [Fact]
    public async Task SetValidatorAsync_ShouldPass_WhenNestedValidationSucceeds()
    {
        Result<ShipmentModel> result = await ValidateShipmentAsync(new(new AddressModel("Istanbul", "34000")));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetValidatorAsync_ShouldSkip_WhenValueIsNull()
    {
        Result<ShipmentModel> result = await ValidateShipmentAsync(new(null));

        result.IsSuccess.Should().BeTrue();
    }

    // =========================================================================
    // Include
    // =========================================================================

    private sealed record UserModel(string? Name, string? Email, int Age);

    private sealed class BaseUserValidator : Validator<UserModel>
    {
        protected override ValueTask Configure(UserModel model, RuleContext<UserModel> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            rules.For(() => model.Email).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FullUserValidator : Validator<UserModel>
    {
        protected override async ValueTask Configure(UserModel model, RuleContext<UserModel> rules, CancellationToken ct = default)
        {
            await Include(new BaseUserValidator(), model, rules, ct);
            rules.For(() => model.Age).GreaterThan(0);
        }
    }

    [Fact]
    public async Task Include_ShouldMergeBaseValidatorRules()
    {
        FullUserValidator validator = new();
        UserModel model = new("", "", -1);

        Result<UserModel> result = await validator.ValidateAsync(model);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task Include_ShouldPass_WhenAllRulesFromBothValidatorsPass()
    {
        FullUserValidator validator = new();
        UserModel model = new("Alice", "alice@example.com", 30);

        Result<UserModel> result = await validator.ValidateAsync(model);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Include_ShouldOnlyReportBaseErrors_WhenOnlyBaseFails()
    {
        FullUserValidator validator = new();
        UserModel model = new("", "", 30);

        Result<UserModel> result = await validator.ValidateAsync(model);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().AllSatisfy(e =>
            e.Code.Should().BeOneOf("Name.NotEmpty", "Email.NotEmpty"));
    }
}
