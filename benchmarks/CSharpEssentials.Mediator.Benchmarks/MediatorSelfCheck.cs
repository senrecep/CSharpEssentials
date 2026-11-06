using System.Reflection;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Fails unless every column returns the same result and the behaviors and handlers it claims to run actually ran.</summary>
public static class MediatorSelfCheck
{
    /// <summary>The error codes an invalid <see cref="CreateOrder"/> must produce, in order.</summary>
    public const string InvalidCodes = "Name.NotEmpty,Quantity.InclusiveBetween";

    /// <summary>The name that passes validation and makes both <c>CreateOrder</c> handlers throw; self-check only.</summary>
    public const string ThrowingName = "throw";

    /// <summary>The single error code every exception behavior returns for the throwing order: <c>Error.Exception(ex)</c>'s type name.</summary>
    public const string ThrowingCode = nameof(InvalidOperationException);

    /// <summary>Runs every benchmark class's setup check, each of which starts with the package version check.</summary>
    public static void VerifyAll()
    {
        foreach (int behaviors in (int[])[0, 2])
        {
            SendBenchmarks send = new() { Behaviors = behaviors };
            send.Setup();
            send.Cleanup();
        }

        foreach (int handlers in (int[])[1, 5])
        {
            PublishBenchmarks publish = new() { Handlers = handlers };
            publish.Setup();
            publish.Cleanup();
        }

        new StartupBenchmarks().Setup();
        foreach (OrderInput input in Enum.GetValues<OrderInput>())
        {
            BehaviorBenchmarks behavior = new() { Input = input };
            behavior.Setup();
            behavior.Cleanup();
        }
    }

    /// <summary>
    /// Throws unless MediatR is 12.5.0 and Mediator.Abstractions is 3.0.2 at run time. The generated code only carries the
    /// generator's assembly version (3.0.0.0); its package is pinned exactly in Directory.Packages.props.
    /// </summary>
    public static void VerifyVersions()
    {
        string mediatR = Informational(typeof(global::MediatR.IMediator).Assembly);
        string abstractions = Informational(typeof(global::Mediator.IMediator).Assembly);
        if (!mediatR.StartsWith("12.5.0", StringComparison.Ordinal)
            || !abstractions.StartsWith("3.0.2", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected MediatR 12.5.0 and Mediator 3.0.2, got MediatR {mediatR}, Mediator.Abstractions {abstractions}.");
        }
    }

    /// <summary>Throws unless each column returns 42 for <c>Ping(41)</c> and runs exactly <paramref name="behaviors"/> behaviors.</summary>
    /// <param name="behaviors">The behaviors the containers were built with.</param>
    /// <param name="columns">The benchmark methods.</param>
    public static void VerifySend(int behaviors, params Func<int>[] columns)
    {
        foreach (Func<int> column in columns)
        {
            Probe.BehaviorCalls = 0;
            int response = column();
            if (response != 42 || Probe.BehaviorCalls != behaviors)
            {
                throw new InvalidOperationException($"{column.Method.Name} with {behaviors} behaviors returned {response} after {Probe.BehaviorCalls} behavior calls, expected 42 after {behaviors}.");
            }
        }
    }

    /// <summary>Throws unless each column calls exactly <paramref name="handlers"/> notification handlers.</summary>
    /// <param name="handlers">The handlers of the published notification.</param>
    /// <param name="columns">The benchmark methods.</param>
    public static void VerifyPublish(int handlers, params Action[] columns)
    {
        foreach (Action column in columns)
        {
            Probe.NotificationCalls = 0;
            column();
            if (Probe.NotificationCalls != handlers)
            {
                throw new InvalidOperationException($"{column.Method.Name} called {Probe.NotificationCalls} handlers, expected {handlers}.");
            }
        }
    }

    /// <summary>Throws unless both startups register exactly 50 request handlers and return 41 for <c>Request0(41)</c>.</summary>
    public static void VerifyStartup()
    {
        (string Name, int Handlers, Func<int> Send)[] variants =
        [
            ("MediatR", MediatRApp.StartupHandlerCount(), MediatRApp.StartupFirstSend),
            ("Mediator", MediatorApp.StartupHandlerCount(), MediatorApp.StartupFirstSend),
        ];
        foreach ((string name, int handlers, Func<int> send) in variants)
        {
            int response = send();
            if (handlers != 50 || response != 41)
            {
                throw new InvalidOperationException($"{name} startup registered {handlers} request handlers and returned {response}, expected 50 and 41.");
            }
        }
    }

    /// <summary>The <see cref="CreateOrder"/> a <see cref="BehaviorBenchmarks"/> row sends.</summary>
    /// <param name="input">The row's input.</param>
    /// <returns><c>("a", 5)</c> or <c>("", 0)</c>.</returns>
    public static CreateOrder Order(OrderInput input) => input == OrderInput.Valid ? new("a", 5) : new("", 0);

    /// <summary>
    /// Throws unless each container resolves its two behaviors in order (validation first) and returns success 6 for the
    /// valid order, exactly <see cref="InvalidCodes"/> for the invalid one, and exactly <see cref="ThrowingCode"/> for a valid
    /// order whose handler throws, which proves the exception behavior runs in each dispatch path.
    /// </summary>
    /// <param name="cse">The container of <see cref="MediatorApp.Cse"/>.</param>
    /// <param name="mediator">The container of <see cref="MediatorApp.HandWritten"/>.</param>
    /// <param name="mediatR">The container of <see cref="MediatRApp.HandWritten"/>.</param>
    public static void VerifyBehaviors(IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR)
    {
        VerifyPipeline("Cse", MediatorPipeline(cse), [typeof(ValidationBehavior<CreateOrder, Result<int>>), typeof(ExceptionHandlingBehavior<CreateOrder, Result<int>>)]);
        VerifyPipeline("MediatorHandWritten", MediatorPipeline(mediator), [typeof(MediatorValidationBehavior), typeof(MediatorExceptionHandlingBehavior)]);
        VerifyPipeline("MediatRHandWritten", MediatRPipeline(mediatR), [typeof(MediatRValidationBehavior), typeof(MediatRExceptionHandlingBehavior)]);

        global::Mediator.IMediator cseMediator = cse.GetRequiredService<global::Mediator.IMediator>();
        global::Mediator.IMediator handWritten = mediator.GetRequiredService<global::Mediator.IMediator>();
        global::MediatR.IMediator mediatRMediator = mediatR.GetRequiredService<global::MediatR.IMediator>();
        foreach (OrderInput input in Enum.GetValues<OrderInput>())
        {
            CreateOrder order = Order(input);
            VerifyResult("Cse", input, Sync.Run(cseMediator.Send(order)));
            VerifyResult("MediatorHandWritten", input, Sync.Run(handWritten.Send(order)));
            VerifyResult("MediatRHandWritten", input, Sync.Run(mediatRMediator.Send(order)));
        }

        CreateOrder throwing = new(ThrowingName, 5);
        VerifyThrowing("Cse", Sync.Run(cseMediator.Send(throwing)));
        VerifyThrowing("MediatorHandWritten", Sync.Run(handWritten.Send(throwing)));
        VerifyThrowing("MediatRHandWritten", Sync.Run(mediatRMediator.Send(throwing)));
    }

    private static Type[] MediatorPipeline(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        return [.. scope.ServiceProvider.GetServices<global::Mediator.IPipelineBehavior<CreateOrder, Result<int>>>().Select(static b => b.GetType())];
    }

    private static Type[] MediatRPipeline(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        return [.. scope.ServiceProvider.GetServices<global::MediatR.IPipelineBehavior<CreateOrder, Result<int>>>().Select(static b => b.GetType())];
    }

    private static void VerifyPipeline(string variant, Type[] actual, Type[] expected)
    {
        if (!actual.SequenceEqual(expected))
        {
            throw new InvalidOperationException($"{variant} resolves behaviors [{string.Join(", ", actual.Select(static t => t.Name))}], expected [{string.Join(", ", expected.Select(static t => t.Name))}].");
        }
    }

    private static void VerifyResult(string variant, OrderInput input, Result<int> result)
    {
        bool ok = input == OrderInput.Valid
            ? result.IsSuccess && result.Value == 6
            : result.IsFailure && string.Join(",", result.Errors.Select(static e => e.Code)) == InvalidCodes;
        if (!ok)
        {
            string actual = result.IsSuccess ? $"success {result.Value}" : "failure " + string.Join(",", result.Errors.Select(static e => e.Code));
            throw new InvalidOperationException($"{variant} returned {actual} for the {input} order.");
        }
    }

    private static void VerifyThrowing(string variant, Result<int> result)
    {
        string codes = result.IsFailure ? string.Join(",", result.Errors.Select(static e => e.Code)) : "success";
        if (codes != ThrowingCode)
        {
            throw new InvalidOperationException($"{variant} returned {codes} for the throwing order, expected {ThrowingCode}.");
        }
    }

    private static string Informational(Assembly assembly) =>
        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "none";
}
