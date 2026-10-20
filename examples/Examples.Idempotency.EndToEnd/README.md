# Idempotency End-to-End Example

One payments service with every idempotency layer wired together, run against PostgreSQL in a container:

```
merchant client ──POST /payments──▶ HTTP layer ──▶ CapturePayment ──▶ payment provider
 (key + retry)     Idempotency-Key  (in-memory)    (EF Core store,      │
                                                    payments profile)   ▼
                                                                  PaymentCaptured ──▶ receipt consumer
                                                                                      (inbox, messaging profile)
payment provider ──POST /webhooks/payment-provider──▶ RecordSettlement (EF Core store, payments profile)
```

| Layer | Key | Store | What it prevents |
|-------|-----|-------|------------------|
| Client | One `Idempotency-Key` per logical payment, reused by every retry | | A lost response turning into a second charge |
| HTTP (`.WithIdempotency()`) | The header, scoped to the merchant's `client_id` | Default in-memory store | Running the endpoint again for a retry on the same instance; the retry gets the stored response with `Idempotency-Replayed: true` |
| Mediator (`IdempotencyBehavior`, `InsideTransaction`) | The parsed header carried in `CapturePayment`; the provider event id in `RecordSettlement`, scoped to `Custom("account", accountId)` | `payments` profile, `EfCoreIdempotencyRecordStore<PaymentsDbContext>` | A second payment when the retry reaches another instance, or when the provider resends a webhook; the claim commits with the payment |
| Messaging (`IdempotencyInbox`) | `IdempotencyKeys.FromMessage(messageId, "receipts")` | `messaging` profile, default store | A second receipt when the broker redelivers a message |

The `payments` profile reads its mode from `Idempotency:PaymentsMode`, so the database layer can be rolled out in
`Observe` first and switched to `Enforce` without a deploy or a restart: `PaymentsApi` registers a
`ConfigurationChangeTokenSource<IdempotencyExecutorOptions>`, so a configuration reload rebuilds the executor options
and the next command runs in the new mode.

The endpoint parses `Idempotency-Key` with `IdempotencyKeyHeader.TryParse` before it builds the command, so a key sent
as a quoted string (`"abc"`) is `abc` in both layers. The handler sends the payment provider a key derived with
`IdempotencyKeys.For("charge", merchantId, key)`, because the provider call is outside the transaction: when the
transaction rolls back after a charge, the retry gets the provider's first charge back.

## Files

| File | Content |
|------|---------|
| `App/PaymentsApi.cs` | Service registration, the two endpoints and the pipeline order |
| `App/CapturePayment.cs`, `App/CapturePaymentHandler.cs` | The payment command: header key, merchant scope, payments profile |
| `App/RecordSettlement.cs`, `App/RecordSettlementHandler.cs` | The webhook command: provider event id as the key, constant fingerprint |
| `App/IPaymentProvider.cs`, `App/SandboxPaymentProvider.cs` | The provider call, deduplicated by the provider on the derived key |
| `App/WebhookSignature.cs` | HMAC check over the raw body that runs before the event id is trusted as a key |
| `App/ReceiptConsumer.cs`, `App/MessageBus.cs` | The message consumer on the inbox, and an in-process at-least-once bus that redelivers after a delay |
| `App/CommandSender.cs` | Composes the registered Mediator behaviors around a handler (the example has no Mediator source generator) |
| `App/MerchantAuthenticationHandler.cs` | Stand-in for machine authentication: `X-Merchant-Id` becomes the `client_id` claim |
| `Client/PaymentsClient.cs`, `Client/IdempotentRetryHandler.cs` | The merchant's client: `HttpRequestBuilder.WithIdempotencyKey` and `RetryUnsafeMethodsOnlyWithIdempotencyKey` |
| `ClientRetryTests.cs` | A lost response retried on the same instance (replayed) and on another instance (answered from the database); the same key with another amount rejected by the HTTP layer and, on another instance, by the database layer (422 `Idempotency.KeyReused`) |
| `KeyHeaderTests.cs` | A quoted key charged once and replayed; the quoted and bare forms of a key are one payment across instances |
| `PaymentProviderTests.cs` | A retry after a lost provider response does not charge twice |
| `WebhookTests.cs` | The same provider event delivered twice; an unsigned event rejected without claiming its id; hex signatures compared as bytes |
| `MessageConsumerTests.cs` | A redelivered message issues one receipt |
| `HybridStoreTests.cs` | The payments profile writes to the database, HTTP and messaging to the default store |
| `ObserveModeTests.cs` | `Observe` does not block a concurrent duplicate while the first transaction is open; a configuration reload switches it to `Enforce` |
| `PostgresFixture.cs`, `PostgresCollection.cs` | The shared PostgreSQL container and the test collection that uses it |

The default store is in memory to keep the example self-contained. With more than one instance, give the HTTP layer
and the messaging profile a shared store (Redis, see `Examples.Idempotency.Redis`); the database layer already is.

## Running

Docker must be running.

```bash
dotnet test examples/Examples.Idempotency.EndToEnd/Examples.Idempotency.EndToEnd.csproj
```

CI runs the same command on every build.
