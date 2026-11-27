using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class FieldMetadataTests
{
    private sealed record Address(string? City);

    private sealed record Line(string? Sku);

    private sealed record ShippingLine(Address? Address);

    private sealed record Shipment(IEnumerable<ShippingLine>? Lines);

    private sealed record Order(string? Name, Address? Address, IEnumerable<Line>? Lines, ValidationOrderStatus Status);

    private sealed class AddressValidator : Validator<Address>
    {
        protected override ValueTask Configure(Address model, RuleContext<Address> rules, CancellationToken ct = default)
        {
            rules.For(() => model.City).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ModelLevelAddressValidator : Validator<Address>
    {
        protected override ValueTask Configure(Address model, RuleContext<Address> rules, CancellationToken ct = default)
        {
            rules.AddFailure("Address.Invalid", "The address is invalid.");
            return ValueTask.CompletedTask;
        }
    }

    private static readonly Error CustomError = Error.Validation("Name.Custom", "Custom message.");

    [Fact]
    public async Task BuiltInValidator_Should_SetFieldToPropertyName()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("", null, null, ValidationOrderStatus.Pending), (m, rules) =>
            rules.For(() => m.Name).NotEmpty());

        result.FirstError.Metadata!.GetField().Should().Be("Name");
        result.FirstError.Code.Should().Be("Name.NotEmpty");
    }

    [Fact]
    public async Task Must_WithCode_Should_SetField()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("x", null, null, ValidationOrderStatus.Pending), (m, rules) =>
            rules.For(() => m.Name).Must(_ => false, "Name.Taken", "Taken."));

        result.FirstError.Metadata!.GetField().Should().Be("Name");
    }

    [Fact]
    public async Task EnumValidator_Should_SetField()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("x", null, null, (ValidationOrderStatus)42), (m, rules) =>
            rules.For(() => m.Status).IsDefinedEnum());

        result.FirstError.Metadata!.GetField().Should().Be("Status");
    }

    [Fact]
    public async Task ForEach_Should_SetIndexedField()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("x", null, [new("a"), new("")], ValidationOrderStatus.Pending), (m, rules) =>
            rules.ForEach(() => m.Lines, (line, lineRules) => lineRules.For(() => line.Sku).NotEmpty()));

        result.FirstError.Metadata!.GetField().Should().Be("Lines[1].Sku");
    }

    [Fact]
    public async Task SetValidatorAsync_Should_PrefixNestedField()
    {
        Result<Order> result = await Validator.ValidateAsync(
            new Order("x", new Address(""), null, ValidationOrderStatus.Pending),
            async (m, rules, ct) => await rules.For(() => m.Address).SetValidatorAsync(new AddressValidator(), ct));

        result.FirstError.Code.Should().Be("Address.City.NotEmpty");
        result.FirstError.Metadata!.GetField().Should().Be("Address.City");
    }

    [Fact]
    public async Task ForEach_WithSetValidatorAsync_Should_PrefixIndexedNestedField()
    {
        Result<Shipment> result = await Validator.ValidateAsync(
            new Shipment([new ShippingLine(new Address(""))]),
            async (m, rules, ct) => await rules.ForEachAsync(
                () => m.Lines,
                async (line, lineRules, lineCt) => await lineRules.For(() => line.Address).SetValidatorAsync(new AddressValidator(), lineCt),
                ct));

        result.FirstError.Code.Should().Be("Lines[0].Address.City.NotEmpty");
        result.FirstError.Metadata!.GetField().Should().Be("Lines[0].Address.City");
    }

    [Fact]
    public async Task BuiltInValidatorError_Should_NotEqualErrorWithoutField_And_EqualErrorWithSameField()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("", null, null, ValidationOrderStatus.Pending), (m, rules) =>
            rules.For(() => m.Name).NotEmpty());
        Error recorded = result.FirstError;

        recorded.Should().NotBe(Error.Validation(recorded.Code, recorded.Description));
        recorded.Should().Be(Error.Validation(recorded.Code, recorded.Description, new ErrorMetadata(ErrorMetadata.FieldKey, "Name")));
    }

    [Fact]
    public async Task SetValidatorAsync_Should_UsePropertyName_When_NestedErrorHasNoField()
    {
        Result<Order> result = await Validator.ValidateAsync(
            new Order("x", new Address("Paris"), null, ValidationOrderStatus.Pending),
            async (m, rules, ct) => await rules.For(() => m.Address).SetValidatorAsync(new ModelLevelAddressValidator(), ct));

        result.FirstError.Metadata!.GetField().Should().Be("Address");
    }

    [Fact]
    public async Task CustomError_Should_BeRecordedAsGiven()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("", null, null, ValidationOrderStatus.Pending), (m, rules) =>
            rules.For(() => m.Name).NotEmpty(CustomError));

        result.FirstError.Should().Be(CustomError);
        result.FirstError.Metadata.Should().BeNull();
    }

    [Fact]
    public async Task AddError_WithCodeAndMessage_Should_SetField_And_HonorCascade()
    {
        Result<Order> result = await Validator.ValidateAsync(new Order("x", null, null, ValidationOrderStatus.Pending), (m, rules) =>
            rules.For(() => m.Name).AddError("Name.First", "First.").AddError("Name.Second", "Second."));

        result.Errors.Should().ContainSingle();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Metadata!.GetField().Should().Be("Name");
    }

    [Fact]
    public void ValidationError_Field_Should_ReadMetadata()
    {
        Error error = Error.Validation("Name.NotEmpty", "Empty.", new ErrorMetadata(ErrorMetadata.FieldKey, "Name"));

        new ValidationError(error.Code, error.Description, error.Metadata).Field.Should().Be("Name");
        new ValidationError("Name.NotEmpty", "Empty.").Field.Should().BeNull();
        new ValidationError("Name.NotEmpty", "Empty.", new ErrorMetadata(ErrorMetadata.FieldKey, 5)).Field.Should().BeNull();
    }

    [Fact]
    public void ValidationError_Field_Should_ReadJsonElement()
    {
        JsonElement field = JsonDocument.Parse("\"Address.City\"").RootElement.Clone();

        new ValidationError("c", "d", new ErrorMetadata(ErrorMetadata.FieldKey, field)).Field.Should().Be("Address.City");
    }

    [Fact]
    public void ValidationError_Field_Should_NotBeSerialized()
    {
        string json = JsonSerializer.Serialize(new ValidationError("c", "d", new ErrorMetadata(ErrorMetadata.FieldKey, "Name")));

        json.Should().NotContain("\"Field\"").And.Contain("\"field\":\"Name\"");
    }
}
