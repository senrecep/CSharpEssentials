# Changelog

All notable changes to the CSharpEssentials packages are listed here. All packages share one version number.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [4.1.0] - 2026-10-06

### Added

- New package `CSharpEssentials.Endpoints`: source-generated endpoint registries (`IEndpoint`, `IEndpointGroup`, `[EndpointGroup]`), a reflection fallback (`MapEndpointsFromAssemblies`), `RouteOf<TEndpoint>`, `RequireRoles`/`RequirePolicies`/`RequireAuthSchemes`, and analyzers CSE1001 to CSE1011 with a code fix for CSE1004. See the [Readme](CSharpEssentials.Endpoints/Readme.MD), including a migration guide from Carter.
- New package `CSharpEssentials.DependencyInjection`: `[RegisterScoped]`/`[RegisterSingleton]`/`[RegisterTransient]` with keys, `As` and `Strategy`, `[Decorates]`, runtime `Decorate`/`TryDecorate`, a reflection fallback (`AddServicesFromAssemblies`), and analyzers CSE2001 to CSE2009 with a code fix for CSE2003. See the [Readme](CSharpEssentials.DependencyInjection/Readme.MD), including a migration guide from Scrutor.
- `CSharpEssentials.AspNetCore`: `MapVersionedGroup(version)` and the `WithValidation<T>()` endpoint filter for `CSharpEssentials.Validation` validators. The package now depends on `CSharpEssentials.Validation`.
- `CSharpEssentials.Core`: `IsBetween` (inclusive) and `IsBetweenExclusive` for any `IComparable<T>`.
- `CSharpEssentials.Time`: `NextDayOfWeek`/`PreviousDayOfWeek` for `DateTime` and `DateOnly`, and `GetAge(DateOnly, DateOnly)`/`GetAge(DateOnly, IDateTimeProvider)`.
- `CSharpEssentials.Json`: `JsonElement.ToClrObject()` converts a `JsonElement` to dictionaries, lists and CLR primitives.
- A Native AOT example for Endpoints and DependencyInjection, published in CI.

### Known issues

- The `net11.0` assets are built with the .NET 11 RC1 SDK. A patch release rebuilt with the .NET 11 GA SDK follows the GA release.

## [4.0.0] - 2026-10-06

4.0 changes default behavior in `CSharpEssentials.AspNetCore`, `CSharpEssentials.EntityFrameworkCore` and `CSharpEssentials.Resilience`. Each old behavior can be turned back on with a setting. Read [Migrating from 3.x to 4.0](docs/migration/v3-to-v4.md) before upgrading.

### Added

- Enum query and route binding for Minimal API and MVC: `services.AddEnumBinding()` and `app.UseEnumBinding()`.
- `IExceptionProblemMapper`, `IErrorStatusCodeMapper` and `IProblemDetailsEnricher` extension points for ProblemDetails.
- `services.ConfigureInvalidModelStateResponse()` writes the automatic `[ApiController]` 400 as an enhanced problem.
- `ConditionalStringEnumConverter.AllowUndefinedValues`.
- `RetryIfFailed` overloads with a `Func<Error, bool> shouldRetry` predicate.
- `SecretManagerConfigurationOptions.LoggerFactory` for GCP Secret Manager logging.
- Analyzer CSE0001 for `[StringEnum]` enums nested in a class or struct.

### Changed

- ProblemDetails output: W3C trace id, fewer request fields by default, RFC 9110 `type` URIs, validation-only `errors`, exception messages hidden in 500 responses. `o.UseLegacyDefaults()` restores the 3.x fields.
- Minimal API, MVC, `GlobalExceptionHandler` and status code pages all write ProblemDetails through `IProblemDetailsService`.
- `ResultEndpointFilter` returns ProblemDetails instead of a raw `Error[]`.
- `StringEnumNaming` is the single enum naming source for JSON, EF Core, Swagger and binding. `UseLegacySnakeCase` restores the 3.x EF Core spelling.
- The non-generic `ResiliencePolicy` retries failed `Result` values, and caller cancellation throws `OperationCanceledException`.
- `CSharpEssentials.Resilience` depends on `Polly.Core` instead of `Polly`. `CSharpEssentials.GcpSecretManager` depends on `CSharpEssentials.Resilience`.
- `Swashbuckle.AspNetCore` is bounded to `[8.1.0, 10)`, and `CSharpEssentials.EntityFrameworkCore` pins each target framework to its own EF Core major.
- `SecretManagerConfigurationOptions` is a `sealed record` (binary-breaking for `AddProject`).

### Fixed

- `PolymorphicJsonConverterFactory` no longer wraps collection and dictionary interfaces in a `$type` envelope.
- `JsonOptions.ApplyTo` keeps the target's `TypeInfoResolver`.
- GCP Secret Manager retries listing and secret access on transient errors, and loads secrets without a shared mutable dictionary.
- `AddValidator` and `AddValidatorsFromAssembly` no longer register the same validator twice.
- `default(ResiliencePolicy)` behaves like `Create()`.

## [3.2.3] - 2026-05-31

### Added

- `CSharpEssentials.These` is part of the meta-package.

## [3.2.2] - 2026-05-31

### Added

- New package `CSharpEssentials.These` with JSON serialization.
- More `Maybe` and `Result` extensions, `RuleEngine.FromPredicate` and `FakeDateTimeProvider`.

## [3.2.1] - 2026-05-31

### Added

- `ExceptionHandlingBehavior` pipeline behavior in `CSharpEssentials.Mediator`.

## [3.2.0] - 2026-05-31

### Added

- New package `CSharpEssentials.Resilience`.

### Fixed

- `CSharpEssentials.Http` restores the default 30 second timeout.
- The `[StringEnum]` generator skips nested enums instead of producing invalid code.

## [3.1.0] - 2026-05-27

### Added

- Batch APIs for `Result`, `Maybe` and `Any`.
- Railway validation bindings on `Result`, `Task` and `ValueTask`.

### Changed

- Failure paths of `Result` pipelines no longer allocate.

## [3.0.8] - 2026-05-20

### Changed

- `CSharpEssentials.Validation` allocates less on the valid path.

## [3.0.7] - 2026-05-20

### Fixed

- Validators support nullable types and concrete collections.

## [3.0.6] - 2026-05-20

### Added

- New package `CSharpEssentials.Validation`. `CSharpEssentials.Mediator` uses it and surfaces validation errors based on `TResponse`.

## [3.0.5] - 2026-05-07

### Fixed

- Static analysis warnings across the packages.

## [3.0.4] - 2026-05-06

### Added

- GitHub Pages landing page and per-package AI agent skills under `.well-known/agent-skills`.

## [3.0.3] - 2026-05-06

### Added

- New package `CSharpEssentials.Mediator` with pipeline behaviors.
- CQRS `DbContext` registration helpers and SQL connection factory abstractions in `CSharpEssentials.EntityFrameworkCore`.
- Endpoint route builder versioning extensions in `CSharpEssentials.AspNetCore`.
- Redirect following in `CSharpEssentials.Http`.
- `Result.Try` and `Result.TryAsync` overloads.

## [3.0.2] - 2026-05-05

### Added

- New package `CSharpEssentials.Http`.
- `[StringEnum]` incremental source generator.
- `ResultEndpointFilter` for Minimal APIs.
- `Combine`, `Recover` and `Unwrap` for `Result`.

## [3.0.1] - 2026-05-04

### Fixed

- Package metadata.

## [3.0.0] - 2026-05-04

### Added

- `Result`: `Ensure`, `MapError`, `TapError`, `ElseDo`, `Compensate`, `Deconstruct`, `Try`, `SuccessIf`, `FailureIf`, `ThenEnsure`, `Select`/`SelectMany`.
- `Maybe`: `Tap`, `TapIf`, `BindIf`, `MapIf`, `ToResult`, `AsMaybe`.
- `Any`: `Deconstruct`, `ToTuple`, `Is<T>`, `As<T>`, `TryAs<T>`.
- `Error`: implicit array conversion and an error combination operator.
- SourceLink and symbol packages.

[Unreleased]: https://github.com/senrecep/CSharpEssentials/compare/v4.1.0...HEAD
[4.1.0]: https://github.com/senrecep/CSharpEssentials/compare/v4.0.0...v4.1.0
[4.0.0]: https://github.com/senrecep/CSharpEssentials/compare/v3.2.3...v4.0.0
[3.2.3]: https://github.com/senrecep/CSharpEssentials/compare/v3.2.2...v3.2.3
[3.2.2]: https://github.com/senrecep/CSharpEssentials/compare/v3.2.1...v3.2.2
[3.2.1]: https://github.com/senrecep/CSharpEssentials/compare/v3.2.0...v3.2.1
[3.2.0]: https://github.com/senrecep/CSharpEssentials/compare/v3.1.0...v3.2.0
[3.1.0]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.8...v3.1.0
[3.0.8]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.7...v3.0.8
[3.0.7]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.6...v3.0.7
[3.0.6]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.5...v3.0.6
[3.0.5]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.4...v3.0.5
[3.0.4]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.3...v3.0.4
[3.0.3]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.2...v3.0.3
[3.0.2]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.1...v3.0.2
[3.0.1]: https://github.com/senrecep/CSharpEssentials/compare/v3.0.0...v3.0.1
[3.0.0]: https://github.com/senrecep/CSharpEssentials/releases/tag/v3.0.0
