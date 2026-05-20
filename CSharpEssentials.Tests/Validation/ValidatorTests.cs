using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class ValidatorTests
{
    private sealed record UserRequest(string? Name, string? Email, int Age);

    private sealed class UserRequestValidator : Validator<UserRequest>
    {
        protected override ValueTask Configure(UserRequest model, RuleContext<UserRequest> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            rules.For(() => model.Email).NotEmpty().EmailAddress();
            rules.For(() => model.Age).GreaterThan(0);
            return ValueTask.CompletedTask;
        }
    }

    // -------------------------------------------------------------------------
    // Sync — class-based validator
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldReturnSuccess_WhenAllRulesPass()
    {
        UserRequestValidator validator = new();
        UserRequest request = new("Alice", "alice@example.com", 30);

        Result<UserRequest> result = await validator.ValidateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(request);
    }

    [Fact]
    public async Task Validate_ShouldReturnFailure_WhenOneRuleFails()
    {
        UserRequestValidator validator = new();
        UserRequest request = new("", "alice@example.com", 30);

        Result<UserRequest> result = await validator.ValidateAsync(request);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldReturnAllErrors_WhenMultipleRulesFail()
    {
        UserRequestValidator validator = new();
        UserRequest request = new("", "not-an-email", -1);

        Result<UserRequest> result = await validator.ValidateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task Validate_ShouldThrowArgumentNullException_WhenInstanceIsNull()
    {
        UserRequestValidator validator = new();

        Func<Task> act = () => validator.ValidateAsync(null!).AsTask();

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Validate_Errors_ShouldAllBeValidationType()
    {
        UserRequestValidator validator = new();
        UserRequest request = new("", "bad", 0);

        Result<UserRequest> result = await validator.ValidateAsync(request);

        result.Errors.Should().AllSatisfy(e => e.Type.Should().Be(ErrorType.Validation));
    }

    // -------------------------------------------------------------------------
    // Async — class-based validator
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenAllRulesPass()
    {
        UserRequestValidator validator = new();
        UserRequest request = new("Alice", "alice@example.com", 30);

        Result<UserRequest> result = await validator.ValidateAsync(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenRulesFail()
    {
        UserRequestValidator validator = new();
        UserRequest request = new("", "bad", 0);

        Result<UserRequest> result = await validator.ValidateAsync(request);

        result.IsFailure.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Inline static validator
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_Inline_ShouldReturnSuccess_WhenNoErrors()
    {
        UserRequest request = new("Alice", "alice@example.com", 25);

        Result<UserRequest> result = await Validator.ValidateAsync(request, (model, rules) =>
        {
            rules.For(() => model.Name).NotEmpty();
            rules.For(() => model.Age).GreaterThan(0);
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(request);
    }

    [Fact]
    public async Task Validate_Inline_ShouldReturnFailure_WhenErrorsExist()
    {
        UserRequest request = new(null, null, -5);

        Result<UserRequest> result = await Validator.ValidateAsync(request, (model, rules) =>
        {
            rules.For(() => model.Name).NotEmpty();
            rules.For(() => model.Age).GreaterThan(0);
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Validate_Inline_ShouldThrow_WhenInstanceIsNull()
    {
        Func<Task> act = () => Validator.ValidateAsync<UserRequest>(null!, (_, _) => { }).AsTask();

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ValidateAsync_Inline_ShouldReturnSuccess_WhenNoErrors()
    {
        UserRequest request = new("Bob", "bob@example.com", 20);

        Result<UserRequest> result = await Validator.ValidateAsync(request, (model, rules, _) =>
        {
            rules.For(() => model.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsSuccess.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // Conditional rules via native C# if
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_ShouldSkipRule_WhenNativeCSharpIfConditionIsFalse()
    {
        UserRequest request = new(null, null, 0);

        Result<UserRequest> result = await Validator.ValidateAsync(request, (model, rules) =>
        {
            if (model.Name is not null)
                rules.For(() => (string?)model.Name).EmailAddress();
        });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldApplyRule_WhenNativeCSharpIfConditionIsTrue()
    {
        UserRequest request = new("not-an-email", null, 0);

        Result<UserRequest> result = await Validator.ValidateAsync(request, (model, rules) =>
        {
            if (model.Name is not null)
                rules.For(() => (string?)model.Name).EmailAddress();
        });

        result.IsFailure.Should().BeTrue();
    }
}
