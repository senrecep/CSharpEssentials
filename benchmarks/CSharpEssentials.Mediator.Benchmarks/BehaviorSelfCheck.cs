using System.Security.Cryptography;
using System.Text.Json;
using CSharpEssentials.Locking;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// Fails unless every column of the behavior suites returns the same result and the behavior it claims to run actually
/// ran: the right pipeline types, the same responses and error codes, the handler calls each path implies, the lock
/// held inside the handler, the same cached bytes and the same log messages.
/// </summary>
public static class BehaviorSelfCheck
{
    /// <summary>The error codes an invalid <see cref="CreateCustomer"/> must produce, in order.</summary>
    public const string InvalidCustomerCodes = "Name.NotEmpty,Email.NotEmpty,Age.InclusiveBetween";

    /// <summary>The product id every <see cref="CachingBehaviorBenchmarks"/> row asks for.</summary>
    public const int ProductId = 42;

    /// <summary>The cache key of <see cref="ProductId"/>.</summary>
    public const string ProductKey = "product:42";

    /// <summary>Runs every behavior suite's setup for every parameter value, then its cleanup.</summary>
    public static void VerifyAll()
    {
        foreach (ValidatorGroups groups in Enum.GetValues<ValidatorGroups>())
        {
            foreach (CustomerInput input in Enum.GetValues<CustomerInput>())
            {
                ValidationFanOutBenchmarks fanOut = new() { Groups = groups, Input = input };
                fanOut.Setup();
                fanOut.Cleanup();
            }
        }

        ValidationScopeBenchmarks scope = new();
        scope.Setup();
        scope.Cleanup();
        foreach (IdempotencyPath path in Enum.GetValues<IdempotencyPath>())
        {
            foreach (FingerprintSource fingerprint in Enum.GetValues<FingerprintSource>())
            {
                IdempotencyBehaviorBenchmarks idempotency = new() { Path = path, Fingerprint = fingerprint };
                idempotency.Setup();
                idempotency.Cleanup();
            }
        }

        LockBehaviorBenchmarks lockBehavior = new();
        lockBehavior.Setup();
        lockBehavior.Cleanup();
        foreach (LockKeys keys in Enum.GetValues<LockKeys>())
        {
            LockContentionBenchmarks contention = new() { Keys = keys };
            contention.Setup();
            contention.Cleanup();
        }

        foreach (CachePath path in Enum.GetValues<CachePath>())
        {
            CachingBehaviorBenchmarks caching = new() { Path = path };
            caching.Setup();
            caching.Cleanup();
        }

        foreach (LoggerMode logger in Enum.GetValues<LoggerMode>())
        {
            foreach (LogPayload payload in Enum.GetValues<LogPayload>())
            {
                LoggingBehaviorBenchmarks logging = new() { Logger = logger, Payload = payload };
                logging.Setup();
                logging.Cleanup();
            }
        }
    }

    /// <summary>The <see cref="CreateCustomer"/> a <see cref="ValidationFanOutBenchmarks"/> row sends.</summary>
    /// <param name="input">The row's input.</param>
    /// <returns><c>("a", "a@b.c", 30)</c> or <c>("", "", 0)</c>.</returns>
    public static CreateCustomer Customer(CustomerInput input) => input == CustomerInput.Valid ? new("a", "a@b.c", 30) : new("", "", 0);

    /// <summary>
    /// Throws unless each container runs only its validation behavior, returns success 31 for the valid customer and
    /// exactly <see cref="InvalidCustomerCodes"/> for the invalid one.
    /// </summary>
    /// <param name="groups">The validator layout the containers were built with.</param>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    public static void VerifyValidation(ValidatorGroups groups, IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR)
    {
        VerifyPipelines<CreateCustomer, Result<int>>(
            cse, typeof(ValidationBehavior<CreateCustomer, Result<int>>),
            mediator, typeof(MediatorFanOutValidationBehavior),
            mediatR, typeof(MediatRFanOutValidationBehavior));
        foreach (CustomerInput input in Enum.GetValues<CustomerInput>())
        {
            CreateCustomer customer = Customer(input);
            string expected = input == CustomerInput.Valid ? "success 31" : "failure " + InvalidCustomerCodes;
            Expect($"Cse ({groups})", Describe(SendMediator(cse, customer)), expected);
            Expect($"MediatorHandWritten ({groups})", Describe(SendMediator(mediator, customer)), expected);
            Expect($"MediatRHandWritten ({groups})", Describe(SendMediatR(mediatR, customer)), expected);
        }
    }

    /// <summary>Throws unless each column returns 1 and each scope resolves the expected validation behavior type.</summary>
    /// <param name="mediatorColumn">The hand-written Mediator column.</param>
    /// <param name="cseColumn">The CSE column.</param>
    /// <param name="mediatRColumn">The hand-written MediatR column.</param>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    public static void VerifyScope(Func<int> mediatorColumn, Func<int> cseColumn, Func<int> mediatRColumn, IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR)
    {
        Expect("MediatorHandWritten scope", mediatorColumn().ToString(System.Globalization.CultureInfo.InvariantCulture), "1");
        Expect("Cse scope", cseColumn().ToString(System.Globalization.CultureInfo.InvariantCulture), "1");
        Expect("MediatRHandWritten scope", mediatRColumn().ToString(System.Globalization.CultureInfo.InvariantCulture), "1");
        VerifyPipelines<CreateCustomer, Result<int>>(
            cse, typeof(ValidationBehavior<CreateCustomer, Result<int>>),
            mediator, typeof(MediatorFanOutValidationBehavior),
            mediatR, typeof(MediatRFanOutValidationBehavior));
    }

    /// <summary>The <see cref="PlaceOrder"/> an <see cref="IdempotencyBehaviorBenchmarks"/> row sends.</summary>
    /// <param name="path">The measured path.</param>
    /// <param name="fingerprint">Where the fingerprint comes from.</param>
    /// <returns>The request.</returns>
    public static PlaceOrder Order(IdempotencyPath path, FingerprintSource fingerprint)
    {
        string notes = fingerprint == FingerprintSource.DefaultLarge ? new string('x', 4096) : "n";
        string? supplied = fingerprint == FingerprintSource.Supplied ? "fp-1" : null;
        return path == IdempotencyPath.Replay
            ? new PlaceOrder("replay-1", "sku-1", 5, notes, supplied)
            : new PlaceOrder("execute-1", "sku-1", 0, notes, supplied);
    }

    /// <summary>
    /// For <see cref="IdempotencyPath.Replay"/>: the first call of each column stores success 6, and two more calls
    /// replay it without running the handler. For <see cref="IdempotencyPath.ExecuteRelease"/>: each call returns
    /// <c>Quantity.Zero</c> and runs the handler exactly once.
    /// </summary>
    /// <param name="path">The measured path.</param>
    /// <param name="request">The request the columns send.</param>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    /// <param name="columns">The benchmark methods.</param>
    public static void VerifyIdempotency(IdempotencyPath path, PlaceOrder request, IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR, params Func<Result<int>>[] columns)
    {
        VerifyPipelines<PlaceOrder, Result<int>>(
            cse, typeof(IdempotencyBehavior<PlaceOrder, Result<int>>),
            mediator, typeof(MediatorIdempotencyBehavior),
            mediatR, typeof(MediatRIdempotencyBehavior));
        VerifyFingerprint(cse, mediator, mediatR);
        foreach (Func<Result<int>> column in columns)
        {
            string name = $"{column.Method.Name} ({path}, key {request.IdempotencyKey})";
            if (path == IdempotencyPath.Replay)
            {
                Expect(name + " first send", Describe(column()), "success 6");
            }

            for (int i = 0; i < 2; i++)
            {
                int before = HandlerProbe.HandlerCalls;
                Result<int> result = column();
                int calls = HandlerProbe.HandlerCalls - before;
                string expected = path == IdempotencyPath.Replay ? "success 6 after 0 handler calls" : "failure Quantity.Zero after 1 handler calls";
                Expect(name, $"{Describe(result)} after {calls} handler calls", expected);
            }
        }
    }

    /// <summary>
    /// Black-box check that each column computes the default fingerprint as the uppercase hex SHA-256 of the request
    /// serialized with the default <c>System.Text.Json</c> options: a request with no fingerprint is stored, then the
    /// same key with that value as a supplied fingerprint must replay instead of failing with <c>Idempotency.KeyReused</c>.
    /// </summary>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    // https://learn.microsoft.com/dotnet/api/system.security.cryptography.sha256.hashdata
    private static void VerifyFingerprint(IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR)
    {
        PlaceOrder first = new("fingerprint-check", "sku-1", 5, "n", null);
        PlaceOrder second = first with { IdempotencyFingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(first))) };
        (string Name, Func<PlaceOrder, Result<int>> Send)[] columns =
        [
            ("Cse", r => SendMediator(cse, r)),
            ("MediatorHandWritten", r => SendMediator(mediator, r)),
            ("MediatRHandWritten", r => SendMediatR(mediatR, r)),
        ];
        foreach ((string name, Func<PlaceOrder, Result<int>> send) in columns)
        {
            Expect(name + " fingerprint store", Describe(send(first)), "success 6");
            int before = HandlerProbe.HandlerCalls;
            Result<int> replay = send(second);
            Expect(name + " fingerprint replay", $"{Describe(replay)} after {HandlerProbe.HandlerCalls - before} handler calls", "success 6 after 0 handler calls");
        }
    }

    /// <summary>
    /// Throws unless each container runs only its lock behavior, returns success 6, holds the lock for the SKU while the
    /// handler runs and frees it after the send.
    /// </summary>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    /// <param name="request">The request the columns send.</param>
    public static void VerifyLock(IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR, ReserveStock request)
    {
        VerifyPipelines<ReserveStock, Result<int>>(
            cse, typeof(LockBehavior<ReserveStock, Result<int>>),
            mediator, typeof(MediatorLockBehavior),
            mediatR, typeof(MediatRLockBehavior));
        IResourceLock resourceLock = cse.GetRequiredService<IResourceLock>();
        (string Name, Func<Result<int>> Send, Func<string, bool> Held)[] columns =
        [
            ("Cse", () => SendMediator(cse, request), key => CseHeld(resourceLock, key)),
            ("MediatorHandWritten", () => SendMediator(mediator, request), key => MediatorLockBehavior.Locker.IsInUse(key)),
            ("MediatRHandWritten", () => SendMediatR(mediatR, request), key => MediatRLockBehavior.Locker.IsInUse(key)),
        ];
        foreach ((string name, Func<Result<int>> send, Func<string, bool> held) in columns)
        {
            LockProbe.Held = null;
            LockProbe.Check = held;
            Result<int> result;
            try
            {
                result = send();
            }
            finally
            {
                LockProbe.Check = null;
            }

            Expect(name + " lock", $"{Describe(result)}, held inside {LockProbe.Held}, held after {held(request.Sku)}", "success 6, held inside True, held after False");
            LockProbe.Held = null;
        }
    }

    /// <summary>Throws unless each column counts exactly 2048 increments, three times in a row.</summary>
    /// <param name="keys">The key layout.</param>
    /// <param name="columns">The benchmark methods.</param>
    public static void VerifyContention(LockKeys keys, params Func<Task<long>>[] columns)
    {
        foreach (Func<Task<long>> column in columns)
        {
            for (int i = 0; i < 3; i++)
            {
                long total = column().GetAwaiter().GetResult();
                if (total != LockContentionBenchmarks.Operations)
                {
                    throw new InvalidOperationException($"{column.Method.Name} with {keys} keys counted {total}, expected {LockContentionBenchmarks.Operations}.");
                }
            }
        }
    }

    /// <summary>
    /// Throws unless each container runs only its caching behavior and stores the same bytes for the same response,
    /// and each column returns the expected view with the handler calls its path implies: one for the first send and
    /// none after it on <see cref="CachePath.Hit"/>, one per call on <see cref="CachePath.Miss"/>. On Hit the entry stays
    /// stored for the measurement.
    /// </summary>
    /// <param name="path">The measured path.</param>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    /// <param name="columns">The benchmark methods.</param>
    public static void VerifyCaching(CachePath path, IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR, params Func<ProductView>[] columns)
    {
        VerifyPipelines<GetProduct, ProductView>(
            cse, typeof(CachingBehavior<GetProduct, ProductView>),
            mediator, typeof(MediatorCachingBehavior),
            mediatR, typeof(MediatRCachingBehavior));
        GetProduct request = new(ProductId);
        ProductView expected = new(ProductId, "p", 9.5m);
        ProductView[] views = [SendMediator(cse, request), SendMediator(mediator, request), SendMediatR(mediatR, request)];
        IDistributedCache[] caches =
        [
            cse.GetRequiredService<IDistributedCache>(),
            mediator.GetRequiredService<IDistributedCache>(),
            mediatR.GetRequiredService<IDistributedCache>(),
        ];
        byte[][] stored = [.. caches.Select(static c => c.Get(ProductKey) ?? [])];
        if (!views.All(v => v == expected) || stored[0].Length == 0 || !stored.All(s => s.AsSpan().SequenceEqual(stored[0])))
        {
            throw new InvalidOperationException(
                $"Caching sends returned [{string.Join(", ", views.AsEnumerable())}] and stored [{string.Join(" | ", stored.Select(static s => System.Text.Encoding.UTF8.GetString(s)))}].");
        }

        foreach (IDistributedCache cache in caches)
        {
            cache.Remove(ProductKey);
        }

        foreach (Func<ProductView> column in columns)
        {
            int before = HandlerProbe.HandlerCalls;
            ProductView first = column();
            ProductView second = column();
            int calls = HandlerProbe.HandlerCalls - before;
            int want = path == CachePath.Hit ? 1 : 2;
            if (first != expected || second != expected || calls != want)
            {
                throw new InvalidOperationException($"{column.Method.Name} ({path}) returned {first} and {second} after {calls} handler calls, expected {expected} after {want}.");
            }
        }
    }

    /// <summary>
    /// Throws unless each container runs only its logging behavior, every column returns success 42, and with
    /// <see cref="LoggerMode.Enabled"/> every column logs exactly three messages, the first two equal across columns and
    /// the third the elapsed-time message.
    /// </summary>
    /// <param name="logger">The logger the containers were built with.</param>
    /// <param name="payload">The request the columns send.</param>
    /// <param name="cse">The CSE container.</param>
    /// <param name="mediator">The hand-written Mediator container.</param>
    /// <param name="mediatR">The hand-written MediatR container.</param>
    /// <param name="columns">The benchmark methods.</param>
    public static void VerifyLogging(LoggerMode logger, LogPayload payload, IServiceProvider cse, IServiceProvider mediator, IServiceProvider mediatR, params Func<Result<int>>[] columns)
    {
        VerifyPipelines<LogPing, Result<int>>(
            cse, typeof(LoggingBehavior<LogPing, Result<int>>),
            mediator, typeof(MediatorLoggingBehavior<LogPing, Result<int>>),
            mediatR, typeof(MediatRLoggingBehavior<LogPing, Result<int>>));
        VerifyPipelines<LogPayloadPing, Result<int>>(
            cse, typeof(LoggingBehavior<LogPayloadPing, Result<int>>),
            mediator, typeof(MediatorLoggingBehavior<LogPayloadPing, Result<int>>),
            mediatR, typeof(MediatRLoggingBehavior<LogPayloadPing, Result<int>>));
        string elapsed = "Request " + (payload == LogPayload.None ? nameof(LogPing) : nameof(LogPayloadPing)) + " took ";
        int expectedMessages = logger == LoggerMode.Enabled ? 3 : 0;
        string[]? reference = null;
        LogSink.Capture = true;
        try
        {
            foreach (Func<Result<int>> column in columns)
            {
                LogSink.Messages.Clear();
                Result<int> result = column();
                string[] messages = [.. LogSink.Messages];
                bool ok = Describe(result) == "success 42" && messages.Length == expectedMessages;
                if (ok && expectedMessages == 3)
                {
                    reference ??= messages;
                    ok = messages[0] == reference[0] && messages[1] == reference[1] && messages[2].StartsWith(elapsed, StringComparison.Ordinal);
                }

                if (!ok)
                {
                    throw new InvalidOperationException(
                        $"{column.Method.Name} ({logger}, {payload}) returned {Describe(result)} and logged [{string.Join(" | ", messages)}], expected success 42 and {expectedMessages} messages matching [{string.Join(" | ", reference ?? [])}].");
                }
            }
        }
        finally
        {
            LogSink.Capture = false;
            LogSink.Messages.Clear();
        }
    }

    private static bool CseHeld(IResourceLock resourceLock, string key)
    {
        Maybe<IAsyncDisposable> handle = Sync.Run(resourceLock.TryAcquireAsync(key));
        if (!handle.HasValue)
        {
            return true;
        }

        Sync.Run(handle.Value.DisposeAsync());
        return false;
    }

    private static TResponse SendMediator<TResponse>(IServiceProvider provider, global::Mediator.ICommand<TResponse> command) =>
        Sync.Run(provider.GetRequiredService<global::Mediator.IMediator>().Send(command));

    private static TResponse SendMediator<TResponse>(IServiceProvider provider, global::Mediator.IQuery<TResponse> query) =>
        Sync.Run(provider.GetRequiredService<global::Mediator.IMediator>().Send(query));

    private static TResponse SendMediatR<TResponse>(IServiceProvider provider, global::MediatR.IRequest<TResponse> request) =>
        Sync.Run(provider.GetRequiredService<global::MediatR.IMediator>().Send(request));

    private static void VerifyPipelines<TRequest, TResponse>(IServiceProvider cse, Type cseType, IServiceProvider mediator, Type mediatorType, IServiceProvider mediatR, Type mediatRType)
        where TRequest : notnull, global::Mediator.IMessage
    {
        Expect("Cse pipeline", MediatorPipeline<TRequest, TResponse>(cse), cseType.Name);
        Expect("MediatorHandWritten pipeline", MediatorPipeline<TRequest, TResponse>(mediator), mediatorType.Name);
        Expect("MediatRHandWritten pipeline", MediatRPipeline<TRequest, TResponse>(mediatR), mediatRType.Name);
        Expect("Cse pipeline type", MediatorPipelineType<TRequest, TResponse>(cse), cseType.FullName);
        Expect("MediatorHandWritten pipeline type", MediatorPipelineType<TRequest, TResponse>(mediator), mediatorType.FullName);
        Expect("MediatRHandWritten pipeline type", MediatRPipelineType<TRequest, TResponse>(mediatR), mediatRType.FullName);
    }

    private static string MediatorPipeline<TRequest, TResponse>(IServiceProvider provider)
        where TRequest : notnull, global::Mediator.IMessage
    {
        using IServiceScope scope = provider.CreateScope();
        return string.Join(",", scope.ServiceProvider.GetServices<global::Mediator.IPipelineBehavior<TRequest, TResponse>>().Select(static b => b.GetType().Name));
    }

    private static string? MediatorPipelineType<TRequest, TResponse>(IServiceProvider provider)
        where TRequest : notnull, global::Mediator.IMessage
    {
        using IServiceScope scope = provider.CreateScope();
        return scope.ServiceProvider.GetServices<global::Mediator.IPipelineBehavior<TRequest, TResponse>>().Single().GetType().FullName;
    }

    private static string MediatRPipeline<TRequest, TResponse>(IServiceProvider provider)
        where TRequest : notnull
    {
        using IServiceScope scope = provider.CreateScope();
        return string.Join(",", scope.ServiceProvider.GetServices<global::MediatR.IPipelineBehavior<TRequest, TResponse>>().Select(static b => b.GetType().Name));
    }

    private static string? MediatRPipelineType<TRequest, TResponse>(IServiceProvider provider)
        where TRequest : notnull
    {
        using IServiceScope scope = provider.CreateScope();
        return scope.ServiceProvider.GetServices<global::MediatR.IPipelineBehavior<TRequest, TResponse>>().Single().GetType().FullName;
    }

    private static string Describe(Result<int> result) =>
        result.IsSuccess
            ? "success " + result.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : "failure " + string.Join(",", result.Errors.Select(static e => e.Code));

    private static void Expect(string what, string? actual, string? expected)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"{what}: got '{actual}', expected '{expected}'.");
        }
    }
}
