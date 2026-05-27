using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;

namespace CSharpEssentials.Tests.Validation;

public class ValidationResultExtensionsTests
{
    private sealed record TestModel(string? Name, int Age);

    private sealed class TestModelValidator : Validator<TestModel>
    {
        protected override ValueTask Configure(TestModel model, RuleContext<TestModel> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            rules.For(() => model.Age).GreaterThan(0);
            return ValueTask.CompletedTask;
        }
    }

    // ==========================================
    // Result<T> Tests
    // ==========================================

    [Fact]
    public async Task Result_ValidateWithAsync_Validator_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Result<TestModel> source = expectedError;
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
    }

    [Fact]
    public async Task Result_ValidateWithAsync_Validator_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Result<TestModel> source = Result.Success(model);
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(model);
    }

    [Fact]
    public async Task Result_ValidateWithAsync_Validator_ShouldReturnFailure_WhenValidationFails()
    {
        TestModel model = new("", -5);
        Result<TestModel> source = Result.Success(model);
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }


    [Fact]
    public void Result_ValidateWith_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Result<TestModel> source = expectedError;
        bool delegateCalled = false;

        Result<TestModel> result = source.ValidateWith((model, rules) =>
        {
            delegateCalled = true;
            rules.For(() => model.Name).NotEmpty();
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public void Result_ValidateWith_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Result<TestModel> source = Result.Success(model);

        Result<TestModel> result = source.ValidateWith((m, rules) =>
        {
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(model);
    }

    [Fact]
    public void Result_ValidateWith_ShouldReturnFailure_WhenValidationFails()
    {
        TestModel model = new("", 30);
        Result<TestModel> source = Result.Success(model);

        Result<TestModel> result = source.ValidateWith((m, rules) =>
        {
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task Result_ValidateWithAsync_SyncDelegate_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Result<TestModel> source = expectedError;
        bool delegateCalled = false;

        Result<TestModel> result = await source.ValidateWithAsync((model, rules) =>
        {
            delegateCalled = true;
            rules.For(() => model.Name).NotEmpty();
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_ValidateWithAsync_SyncDelegate_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Result<TestModel> source = Result.Success(model);

        Result<TestModel> result = await source.ValidateWithAsync((m, rules) =>
        {
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(model);
    }

    [Fact]
    public async Task Result_ValidateWithAsync_SyncDelegate_ShouldReturnFailure_WhenValidationFails()
    {
        TestModel model = new("", 30);
        Result<TestModel> source = Result.Success(model);

        Result<TestModel> result = await source.ValidateWithAsync((m, rules) =>
        {
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task Result_ValidateWithAsync_AsyncDelegate_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Result<TestModel> source = expectedError;
        bool delegateCalled = false;

        Result<TestModel> result = await source.ValidateWithAsync((model, rules, _) =>
        {
            delegateCalled = true;
            rules.For(() => model.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Result_ValidateWithAsync_AsyncDelegate_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Result<TestModel> source = Result.Success(model);

        Result<TestModel> result = await source.ValidateWithAsync((m, rules, _) =>
        {
            rules.For(() => m.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(model);
    }

    // ==========================================
    // Task<Result<T>> Tests
    // ==========================================

    [Fact]
    public async Task TaskResult_ValidateWithAsync_Validator_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Task<Result<TestModel>> source = Task.FromResult((Result<TestModel>)expectedError);
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_Validator_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Task<Result<TestModel>> source = Task.FromResult(Result.Success(model));
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(model);
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_Validator_ShouldCancel_WhileWaitingForTask()
    {
        var source = new TaskCompletionSource<Result<TestModel>>();
        TestModelValidator validator = new();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await source.Task.ValidateWithAsync(validator, cts.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(act);
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_SyncDelegate_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Task<Result<TestModel>> source = Task.FromResult((Result<TestModel>)expectedError);
        bool delegateCalled = false;

        Result<TestModel> result = await source.ValidateWithAsync((m, rules) =>
        {
            delegateCalled = true;
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_SyncDelegate_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Task<Result<TestModel>> source = Task.FromResult(Result.Success(model));

        Result<TestModel> result = await source.ValidateWithAsync((m, rules) =>
        {
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_SyncDelegate_ShouldCancel_WhileWaitingForTask()
    {
        var source = new TaskCompletionSource<Result<TestModel>>();
        bool delegateCalled = false;

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await source.Task.ValidateWithAsync((model, rules) =>
        {
            delegateCalled = true;
            rules.For(() => model.Name).NotEmpty();
        }, cts.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(act);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_AsyncDelegate_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        Task<Result<TestModel>> source = Task.FromResult((Result<TestModel>)expectedError);
        bool delegateCalled = false;

        Result<TestModel> result = await source.ValidateWithAsync((m, rules, _) =>
        {
            delegateCalled = true;
            rules.For(() => m.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResult_ValidateWithAsync_AsyncDelegate_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        Task<Result<TestModel>> source = Task.FromResult(Result.Success(model));

        Result<TestModel> result = await source.ValidateWithAsync((m, rules, _) =>
        {
            rules.For(() => m.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsSuccess.Should().BeTrue();
    }

    // ==========================================
    // ValueTask<Result<T>> Tests
    // ==========================================

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_Validator_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        ValueTask<Result<TestModel>> source = ValueTask.FromResult((Result<TestModel>)expectedError);
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
    }

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_Validator_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        ValueTask<Result<TestModel>> source = ValueTask.FromResult(Result.Success(model));
        TestModelValidator validator = new();

        Result<TestModel> result = await source.ValidateWithAsync(validator);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(model);
    }

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_SyncDelegate_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        ValueTask<Result<TestModel>> source = ValueTask.FromResult((Result<TestModel>)expectedError);
        bool delegateCalled = false;

        Result<TestModel> result = await source.ValidateWithAsync((m, rules) =>
        {
            delegateCalled = true;
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_SyncDelegate_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        ValueTask<Result<TestModel>> source = ValueTask.FromResult(Result.Success(model));

        Result<TestModel> result = await source.ValidateWithAsync((m, rules) =>
        {
            rules.For(() => m.Name).NotEmpty();
        });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_SyncDelegate_ShouldCancel_WhileWaitingForTask()
    {
        var source = new TaskCompletionSource<Result<TestModel>>();
        bool delegateCalled = false;

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await new ValueTask<Result<TestModel>>(source.Task).ValidateWithAsync((model, rules) =>
        {
            delegateCalled = true;
            rules.For(() => model.Name).NotEmpty();
        }, cts.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(act);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_AsyncDelegate_ShouldShortCircuit_WhenResultIsFailure()
    {
        Error expectedError = Error.Validation("Pre.Error", "Pre-existing error.");
        ValueTask<Result<TestModel>> source = ValueTask.FromResult((Result<TestModel>)expectedError);
        bool delegateCalled = false;

        Result<TestModel> result = await source.ValidateWithAsync((m, rules, _) =>
        {
            delegateCalled = true;
            rules.For(() => m.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(expectedError);
        delegateCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ValueTaskResult_ValidateWithAsync_AsyncDelegate_ShouldReturnSuccess_WhenValidationPasses()
    {
        TestModel model = new("Alice", 30);
        ValueTask<Result<TestModel>> source = ValueTask.FromResult(Result.Success(model));

        Result<TestModel> result = await source.ValidateWithAsync((m, rules, _) =>
        {
            rules.For(() => m.Name).NotEmpty();
            return ValueTask.CompletedTask;
        });

        result.IsSuccess.Should().BeTrue();
    }
}
