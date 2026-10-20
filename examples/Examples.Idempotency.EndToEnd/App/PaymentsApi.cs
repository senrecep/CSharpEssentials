using System.Security.Claims;
using System.Text.Json;

using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using Mediator;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// The payments service. Two idempotency layers with two stores:
/// <list type="bullet">
/// <item><description>
/// HTTP: <c>.WithIdempotency()</c> on <c>POST /payments</c> replays the stored response of a retried request
/// (<c>Idempotency-Replayed: true</c>) from the default in-memory store, without running the endpoint again.
/// </description></item>
/// <item><description>
/// Mediator: <see cref="CapturePayment"/> and <see cref="RecordSettlement"/> run on the <c>payments</c> profile, whose
/// store is the EF Core store of <see cref="PaymentsDbContext"/>. The claim is written in the same transaction as the
/// payment, so a retry that reaches another instance, or a webhook resent by the provider, finds it and gets the
/// stored result instead of a second payment. The endpoint parses the header with
/// <see cref="IdempotencyKeyHeader.TryParse(string?, out string?)"/>, so a key sent as a quoted string is the same key
/// in both layers.
/// </description></item>
/// </list>
/// The message consumer uses the built-in <c>messaging</c> profile on the default store.
/// </summary>
public static class PaymentsApi
{
    public const string PaymentsProfile = "payments";
    public const string PaymentsStoreKey = "payments";
    public const string ConnectionStringName = "Payments";
    public const string PaymentsModeKey = "Idempotency:PaymentsMode";
    public const string WebhookSecretKey = "PaymentProvider:WebhookSecret";
    public const string SettlementEventType = "settlement.completed";

    /// <summary>
    /// Creates the application. <paramref name="configure"/> runs before the services are registered: set the
    /// configuration there, or register an <see cref="IPaymentProvider"/> that replaces the sandbox one.
    /// </summary>
    public static WebApplication Create(Action<WebApplicationBuilder>? configure = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        configure?.Invoke(builder);
        AddServices(builder.Services, builder.Configuration);

        WebApplication app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseIdempotency();

        app.MapPost("/payments", CaptureAsync)
            .RequireAuthorization()
            .WithIdempotency(endpoint => endpoint.RequireKey = true);
        app.MapPost("/webhooks/payment-provider", ReceiveWebhookAsync);
        return app;
    }

    private static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentsDbContext>(options => options.UseNpgsql(configuration.GetConnectionString(ConnectionStringName)));
        services.AddAuthentication(MerchantAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, MerchantAuthenticationHandler>(MerchantAuthenticationHandler.SchemeName, null);
        services.AddAuthorization();

        services.AddIdempotency(options => options.ScopeResolver = IdempotencyScopes.Client);

        services.AddIdempotencyExecutor(options => options.AddProfile(PaymentsProfile, profile =>
        {
            profile.StoreKey = PaymentsStoreKey;
            profile.Mode = configuration.GetValue(PaymentsModeKey, IdempotencyMode.Enforce);
        }));
        services.AddSingleton<IOptionsChangeTokenSource<IdempotencyExecutorOptions>>(
            new ConfigurationChangeTokenSource<IdempotencyExecutorOptions>(configuration));
        services.AddEfCoreIdempotencyRecordStore<PaymentsDbContext>(PaymentsStoreKey);
        services.AddEfCoreTransactionRunner<PaymentsDbContext>();
        services.AddMediatorTransactionRunnerBehavior();
        services.AddMediatorIdempotencyBehavior(options =>
        {
            options.Placement = IdempotencyPlacement.InsideTransaction;
            options.ForRequest<CapturePayment>(request => request.Profile = PaymentsProfile);
            options.ForRequest<RecordSettlement>(request => request.Profile = PaymentsProfile);
        });

        services.AddScoped<CommandSender>();
        services.AddScoped<ICommandHandler<CapturePayment, Result<CapturedPayment>>, CapturePaymentHandler>();
        services.AddScoped<ICommandHandler<RecordSettlement, Result<Guid>>, RecordSettlementHandler>();
        services.TryAddSingleton<IPaymentProvider, SandboxPaymentProvider>();

        services.AddSingleton<MessageBus>();
        services.AddHostedService<ReceiptConsumer>();
    }

    private static async Task<IResult> CaptureAsync(
        CapturePaymentRequest body,
        [FromHeader(Name = "Idempotency-Key")] string? header,
        ClaimsPrincipal user,
        CommandSender sender,
        MessageBus bus,
        CancellationToken cancellationToken)
    {
        if (!IdempotencyKeyHeader.TryParse(header, out string? key))
            return Error.Validation(IdempotencyErrorCodes.KeyInvalid, "Send a valid Idempotency-Key header.").ToProblemResult();

        string merchantId = user.FindFirstValue(IdempotencyScopes.ClientIdClaimType)!;
        Result<CapturedPayment> result = await sender.SendAsync<CapturePayment, Result<CapturedPayment>>(
            new CapturePayment(merchantId, key, body.Reference, body.Amount),
            cancellationToken);
        if (result.IsFailure)
            return result.ToProblemResult();

        CapturedPayment payment = result.Value;
        bus.Publish(new PaymentCaptured(payment.PaymentId.ToString("N"), merchantId, payment.PaymentId, payment.Amount));
        return Results.Ok(payment);
    }

    private static async Task<IResult> ReceiveWebhookAsync(
        HttpRequest request,
        IConfiguration configuration,
        CommandSender sender,
        CancellationToken cancellationToken)
    {
        using MemoryStream buffer = new();
        await request.Body.CopyToAsync(buffer, cancellationToken);
        byte[] secret = Convert.FromBase64String(configuration[WebhookSecretKey]
            ?? throw new InvalidOperationException($"{WebhookSecretKey} is not configured."));
        if (!WebhookSignature.IsValid(secret, buffer.GetBuffer().AsSpan(0, (int)buffer.Length), request.Headers[WebhookSignature.HeaderName]))
            return Results.Unauthorized();

        ProviderEvent? providerEvent = JsonSerializer.Deserialize<ProviderEvent>(buffer.ToArray(), JsonSerializerOptions.Web);
        if (providerEvent is null)
            return Results.BadRequest();
        if (providerEvent.Type != SettlementEventType)
            return Results.Ok();

        Result<Guid> result = await sender.SendAsync<RecordSettlement, Result<Guid>>(
            new RecordSettlement(providerEvent.Id, providerEvent.AccountId, providerEvent.Amount),
            cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new { settlementId = result.Value })
            : result.ToProblemResult(statusCode: StatusCodes.Status500InternalServerError);
    }
}
