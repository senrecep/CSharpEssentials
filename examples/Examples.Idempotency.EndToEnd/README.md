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
| Mediator (`IdempotencyBehavior`, `InsideTransaction`) | The header carried in `CapturePayment`; the provider event id in `RecordSettlement` | `payments` profile, `EfCoreIdempotencyRecordStore<PaymentsDbContext>` | A second payment when the retry reaches another instance, or when the provider resends a webhook; the claim commits with the payment |
| Messaging (`IdempotencyInbox`) | `IdempotencyKeys.FromMessage(messageId, "receipts")` | `messaging` profile, default store | A second receipt when the broker redelivers a message |

The `payments` profile reads its mode from `Idempotency:PaymentsMode`, so the database layer can be rolled out in
`Observe` first and switched to `Enforce` without a deploy.

## Files

| File | Content |
|------|---------|
| `App/PaymentsApi.cs` | Service registration, the two endpoints and the pipeline order |
| `App/CapturePayment.cs`, `App/CapturePaymentHandler.cs` | The payment command: header key, merchant scope, payments profile |
| `App/RecordSettlement.cs`, `App/RecordSettlementHandler.cs` | The webhook command: provider event id as the key, constant fingerprint |
| `App/WebhookSignature.cs` | HMAC check that runs before the event id is trusted as a key |
| `App/ReceiptConsumer.cs`, `App/MessageBus.cs` | The message consumer on the inbox, and an in-process at-least-once bus |
| `App/CommandSender.cs` | Composes the registered Mediator behaviors around a handler (the example has no Mediator source generator) |
| `App/MerchantAuthenticationHandler.cs` | Stand-in for machine authentication: `X-Merchant-Id` becomes the `client_id` claim |
| `Client/PaymentsClient.cs`, `Client/IdempotentRetryHandler.cs` | The merchant's client: `HttpRequestBuilder.WithIdempotencyKey` and `RetryUnsafeMethodsOnlyWithIdempotencyKey` |
| `ClientRetryTests.cs` | A lost response retried on the same instance (replayed) and on another instance (answered from the database) |
| `WebhookTests.cs` | The same provider event delivered twice; an unsigned event rejected without claiming its id |
| `MessageConsumerTests.cs` | A redelivered message issues one receipt |
| `HybridStoreTests.cs` | The payments profile writes to the database, HTTP and messaging to the default store |
| `ObserveModeTests.cs` | `Observe` does not block a concurrent duplicate while the first transaction is open |

The default store is in memory to keep the example self-contained. With more than one instance, give the HTTP layer
and the messaging profile a shared store (Redis, see `Examples.Idempotency.Redis`); the database layer already is.

## Running

Docker must be running.

```bash
dotnet test examples/Examples.Idempotency.EndToEnd/Examples.Idempotency.EndToEnd.csproj
```

CI runs the same command on every build.
