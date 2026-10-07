using System.Diagnostics.Metrics;

using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;

using FluentAssertions;

using Mediator;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Mediator;

internal sealed record ModeOverrideCommand(string Name, ValidationMode ValidationMode)
    : ICommand<Result>, IValidationModeOverride;

internal sealed class ModeOverrideValidator : Validator<ModeOverrideCommand>
{
    protected override ValueTask Configure(ModeOverrideCommand model, RuleContext<ModeOverrideCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).Must(name => name.Length > 0, "NameRequired", "Name is required");
        return ValueTask.CompletedTask;
    }
}

internal sealed class CountingValidator : Validator<TestValidationCommand>
{
    public int Calls { get; private set; }

    protected override ValueTask Configure(TestValidationCommand model, RuleContext<TestValidationCommand> rules, CancellationToken ct = default)
    {
        Calls++;
        rules.For(() => model.Name).Must(name => name.Length > 0, "NameRequired", "Name is required");
        return ValueTask.CompletedTask;
    }
}

internal sealed class RecordingObserver : IValidationFailureObserver
{
    public List<ValidationFailureContext> Failures { get; } = [];

    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken)
    {
        Failures.Add(failure);
        return default;
    }
}

internal sealed class ThrowingObserver : IValidationFailureObserver
{
    public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Observer failed");
}

public class ValidationBehaviorModeTests
{
    private static ValidationBehaviorOptions Options(ValidationMode mode) => new() { DefaultMode = mode };

    private static MessageHandlerDelegate<TMessage, Result> CountingNext<TMessage>(Action onCall)
        where TMessage : IMessage =>
        (_, _) =>
        {
            onCall();
            return new ValueTask<Result>(Result.Success());
        };

    [Fact]
    public async Task Enforce_Should_Return_Failure_And_Notify_Observers()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new StubValidator("NameRequired", "Name is required")], [observer]);
        bool nextCalled = false;

        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => nextCalled = true), default);

        result.IsFailure.Should().BeTrue();
        nextCalled.Should().BeFalse();
        ValidationFailureContext failure = observer.Failures.Should().ContainSingle().Subject;
        failure.Mode.Should().Be(ValidationMode.Enforce);
        failure.RequestType.Should().Be<TestValidationCommand>();
        failure.Errors.Should().ContainSingle(error => error.Code == "NameRequired");
    }

    [Fact]
    public async Task Default_Options_Should_Enforce()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new StubValidator("NameRequired", "Name is required")], [], new ValidationBehaviorOptions());

        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => { }), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task LogOnly_Should_Notify_Observers_And_Continue()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new StubValidator("NameRequired", "Name is required")], [observer], Options(ValidationMode.LogOnly));
        bool nextCalled = false;

        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => nextCalled = true), default);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
        ValidationFailureContext failure = observer.Failures.Should().ContainSingle().Subject;
        failure.Mode.Should().Be(ValidationMode.LogOnly);
        failure.Errors.Should().ContainSingle(error => error.Code == "NameRequired");
    }

    [Fact]
    public async Task LogOnly_Should_Continue_With_Errors_From_Every_Validator_Group()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, Result> behavior = new(
            [new OrderedStubValidator(2, "Second", "second"), new OrderedStubValidator(1, "First", "first")],
            [observer],
            Options(ValidationMode.LogOnly));

        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => { }), default);

        result.IsSuccess.Should().BeTrue();
        observer.Failures.Should().ContainSingle()
            .Which.Errors.Select(error => error.Code).Should().Equal("First", "Second");
    }

    [Fact]
    public async Task LogOnly_Should_Not_Throw_For_Non_Result_Response()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, string> behavior =
            new([new StubValidator("NameRequired", "Name is required")], [observer], Options(ValidationMode.LogOnly));

        string response = await behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<string>("ok"), default);

        response.Should().Be("ok");
        observer.Failures.Should().ContainSingle();
    }

    [Fact]
    public async Task LogOnly_Should_Report_Validator_Exceptions_And_Continue()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new ThrowingValidator()], [observer], Options(ValidationMode.LogOnly));

        Result result = await behavior.Handle(new TestValidationCommand("x"), CountingNext<TestValidationCommand>(() => { }), default);

        result.IsSuccess.Should().BeTrue();
        observer.Failures.Should().ContainSingle();
    }

    [Fact]
    public async Task Off_Should_Skip_Validators_And_Observers()
    {
        CountingValidator validator = new();
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([validator], [observer], Options(ValidationMode.Off));
        bool nextCalled = false;

        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => nextCalled = true), default);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
        validator.Calls.Should().Be(0);
        observer.Failures.Should().BeEmpty();
    }

    [Fact]
    public async Task Valid_Request_Should_Not_Notify_Observers()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator()], [observer]);

        Result result = await behavior.Handle(new TestValidationCommand("ok"), CountingNext<TestValidationCommand>(() => { }), default);

        result.IsSuccess.Should().BeTrue();
        observer.Failures.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ValidationMode.Enforce, ValidationMode.LogOnly, true)]
    [InlineData(ValidationMode.Off, ValidationMode.Enforce, false)]
    [InlineData(ValidationMode.LogOnly, ValidationMode.Off, true)]
    public async Task Request_Override_Should_Win_Over_Default_Mode(
        ValidationMode defaultMode, ValidationMode requestMode, bool expectSuccess)
    {
        RecordingObserver observer = new();
        ValidationBehavior<ModeOverrideCommand, Result> behavior =
            new([new ModeOverrideValidator()], [observer], Options(defaultMode));

        Result result = await behavior.Handle(new ModeOverrideCommand("", requestMode), CountingNext<ModeOverrideCommand>(() => { }), default);

        result.IsSuccess.Should().Be(expectSuccess);
        if (requestMode == ValidationMode.Off)
            observer.Failures.Should().BeEmpty();
        else
            observer.Failures.Should().ContainSingle().Which.Mode.Should().Be(requestMode);
    }

    [Fact]
    public async Task Observers_Should_Run_In_Registration_Order()
    {
        List<string> calls = [];
        ValidationBehavior<TestValidationCommand, Result> behavior = new(
            [new StubValidator("NameRequired", "Name is required")],
            [new CallbackObserver(_ => calls.Add("first")), new CallbackObserver(_ => calls.Add("second"))]);

        await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => { }), default);

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task Enforce_Should_Notify_Observers_Before_Throwing_For_Non_Result_Response()
    {
        RecordingObserver observer = new();
        ValidationBehavior<TestValidationCommand, string> behavior =
            new([new StubValidator("NameRequired", "Name is required")], [observer]);

        Func<Task> act = () => behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<string>("ok"), default).AsTask();

        await act.Should().ThrowAsync<EnhancedValidationException>();
        observer.Failures.Should().ContainSingle().Which.Mode.Should().Be(ValidationMode.Enforce);
    }

    [Fact]
    public async Task Observer_Should_Not_Be_Able_To_Change_Returned_Errors()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new(
            [new StubValidator("NameRequired", "Name is required")],
            [new CallbackObserver(failure => (failure.Errors as Error[])?.SetValue(Error.Validation("Changed", "changed"), 0))]);

        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => { }), default);

        result.FirstError.Code.Should().Be("NameRequired");
    }

    [Fact]
    public async Task Observer_Exception_Should_Propagate()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new(
            [new StubValidator("NameRequired", "Name is required")],
            [new ThrowingObserver()],
            Options(ValidationMode.LogOnly));

        Func<Task> act = () => behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => { }), default).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Observer failed");
    }

    [Fact]
    public async Task Resolved_Behavior_Should_Use_Configured_Mode_And_Default_Observer()
    {
        ServiceCollection services = new();
        services.AddScoped<IValidator<TestValidationCommand>>(_ => new StubValidator("NameRequired", "Name is required"));
        services.AddMediatorValidationBehavior(options => options.DefaultMode = ValidationMode.LogOnly);
        await using ServiceProvider provider = services.BuildServiceProvider(validateScopes: true);
        using IServiceScope scope = provider.CreateScope();

        IPipelineBehavior<TestValidationCommand, Result> behavior =
            scope.ServiceProvider.GetRequiredService<IPipelineBehavior<TestValidationCommand, Result>>();
        Result result = await behavior.Handle(new TestValidationCommand(""), CountingNext<TestValidationCommand>(() => { }), default);

        result.IsSuccess.Should().BeTrue();
        scope.ServiceProvider.GetServices<IValidationFailureObserver>()
            .Should().ContainSingle().Which.Should().BeOfType<LoggingValidationFailureObserver>();
    }

    private sealed class CallbackObserver(Action<ValidationFailureContext> callback) : IValidationFailureObserver
    {
        public ValueTask OnValidationFailedAsync(ValidationFailureContext failure, CancellationToken cancellationToken)
        {
            callback(failure);
            return default;
        }
    }
}

public class LoggingValidationFailureObserverTests
{
    private sealed record MeteredCommand : ICommand<Result>;

    private static ValidationFailureContext Failure(ValidationMode mode) => new(
        typeof(MeteredCommand),
        new MeteredCommand(),
        [Error.Validation("NameRequired", "Name is required"), Error.Validation("EmailInvalid", "Email is invalid")],
        mode);

    [Theory]
    [InlineData(ValidationMode.LogOnly, LogLevel.Warning, "log_only")]
    [InlineData(ValidationMode.Enforce, LogLevel.Debug, "enforce")]
    public async Task Should_Log_At_Mode_Level(ValidationMode mode, LogLevel expectedLevel, string modeText)
    {
        CapturingLogger logger = new();
        LoggingValidationFailureObserver observer = new(logger);

        await observer.OnValidationFailedAsync(Failure(mode), default);

        (LogLevel level, string message) = logger.Entries.Should().ContainSingle().Subject;
        level.Should().Be(expectedLevel);
        message.Should().Be($"Validation of MeteredCommand failed in {modeText} mode with 2 error(s): NameRequired, EmailInvalid");
    }

    [Fact]
    public async Task Should_Count_Failures_With_Request_And_Mode_Tags()
    {
        List<(long Value, string? Request, string? Mode)> measurements = [];
        using MeterListener listener = new();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == LoggingValidationFailureObserver.MeterName
                && instrument.Name == LoggingValidationFailureObserver.FailureCounterName)
                meterListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
        {
            string? request = null;
            string? mode = null;
            foreach (KeyValuePair<string, object?> tag in tags)
            {
                if (tag.Key == "request") request = tag.Value as string;
                if (tag.Key == "mode") mode = tag.Value as string;
            }
            if (request == nameof(MeteredCommand))
                lock (measurements) measurements.Add((value, request, mode));
        });
        listener.Start();

        await new LoggingValidationFailureObserver().OnValidationFailedAsync(Failure(ValidationMode.LogOnly), default);

        measurements.Should().ContainSingle().Which.Should().Be((1L, nameof(MeteredCommand), "log_only"));
    }

    [Fact]
    public async Task Should_Not_Log_When_Level_Is_Disabled()
    {
        CapturingLogger logger = new(minimumLevel: LogLevel.Information);

        await new LoggingValidationFailureObserver(logger).OnValidationFailedAsync(Failure(ValidationMode.Enforce), default);

        logger.Entries.Should().BeEmpty();
    }

    private sealed class CapturingLogger(LogLevel minimumLevel = LogLevel.Trace) : ILogger<LoggingValidationFailureObserver>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= minimumLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
                Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
