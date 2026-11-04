# CSharpEssentials.AspNetCore.Swashbuckle: Performance benchmarks

**Status: draft.** The suite builds on net8.0 to net11.0, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet. Short-job (`--quick`) numbers are not published.

CSharpEssentials.AspNetCore.Swashbuckle (CSE) is compared with plain Swashbuckle.AspNetCore 10.3.1 on Microsoft.OpenApi 2.13.0. There is no competitor library. The baseline is Swashbuckle's documented setup: `AddEndpointsApiExplorer()`, `AddSwaggerGen()` and `UseSwagger()`.

The suite measures what it costs to describe the same app with the CSE conventions. The CSE rows write more output by design, so no CSE row is like-for-like with the plain row.

The suite lives in `benchmarks/CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks/`. The Microsoft.AspNetCore.OpenApi package has its own suite over the same sample API ([CSharpEssentials.AspNetCore.OpenApi/BENCHMARKS.md](../CSharpEssentials.AspNetCore.OpenApi/BENCHMARKS.md)).

---

## What is measured

| Class | Measured unit | Row | Label |
|---|---|---|---|
| `SwashbuckleDocumentComparison` | One warm `GET /swagger/v1/swagger.json` through TestServer, whole body read | `Plain_AddSwaggerGen` (baseline): `AddSwaggerGen()` | Baseline |
| `SwashbuckleDocumentComparison` | Same | `Plain_AddSwaggerGen_StringEnums`: `AddSwaggerGen()` with `JsonStringEnumConverter` | Different JSON options. See semantic differences |
| `SwashbuckleDocumentComparison` | Same | `CSharpEssentials_EnumConventions`: `o.AddEnumConventions()` | More output |
| `SwashbuckleDocumentComparison` | Same | `CSharpEssentials_IdempotencyConventions`: `o.AddIdempotencyConventions()` | More output |
| `SwashbuckleDocumentComparison` | Same | `CSharpEssentials_OptionalRouteParameters`: `o.AddOptionalRouteParameters()` | 70 operations against 50 |
| `SwashbuckleDocumentComparison` | Same | `CSharpEssentials_AllConventions`: all three on `AddSwaggerGen` | More output, 70 operations |
| `SwashbuckleDocumentComparison` | Same | `CSharpEssentials_AddSwagger`: `AddSwagger<DefaultConfigureSwaggerOptions>(...)` | Full Readme setup, more work |
| `SwashbuckleFirstDocumentComparison` | Fresh process to first document: build, start, one GET, stop, dispose | `Plain_AddSwaggerGen` (baseline), `CSharpEssentials_AllConventions` | Same labels as the warm rows |

Each method returns the document length in bytes, so the work cannot be removed by the JIT.

`CSharpEssentials_AddSwagger` adds a security scheme and requirement, the CSE schema ids, XML comments, `ResolveConflictingActions` and `MapType` for `TimeSpan` and `TimeOnly` on top of the three conventions. It is served with `UseSwagger` and the same route template as `UseVersionableSwagger`, without Swagger UI. Swagger UI is not part of document generation.

## Inputs

Every row serves the same sample API from `benchmarks/CSharpEssentials.AspNetCore.ApiDocs.Benchmarks.Shared/`:

- 30 enums, `Enum01` to `Enum30`. Each has `[StringEnum]`, a `[Description]` and six defined, contiguous members, `Alpha = 0` to `Zeta = 5`. `Beta` has its own `[Description]`. No `[Flags]`, `[Obsolete]` or `[EnumFallback]`.
- 10 records, `Resource01` to `Resource10`. Each has an `int`, a `string`, one enum, one nullable enum and one enum with `[DefaultValue(Beta)]`.
- 50 Minimal API endpoints, 5 per resource: a list with an enum query, a get by id, an archive route `{year:int?}/{month:int?}`, an idempotent POST (`.WithIdempotency()`) and a delete with an enum query.

Every host registers the same app services: `AddIdempotency(o => o.Mode = IdempotencyMode.Enforce)`, `AddEnumConventions()` and `AddEndpointsApiExplorer()`. The StringEnums row registers `JsonStringEnumConverter` instead of `AddEnumConventions()`. Only defined enum values are used anywhere.

## Methodology

- In-process TestServer, Production environment, logging providers cleared. Every row reads the whole response body.
- Every row serves the same 50 Minimal API endpoints and 30 enums. The plain and CSE hosts differ only in the document options, except the StringEnums row.
- Warm rows regenerate the document on every request, because Swashbuckle does not cache it. `SwaggerMiddleware` builds the document per request.
- Cold rows run one sample per fresh process, 15 launches (`RunStrategy.ColdStart`, no warmup). JIT, the Swashbuckle generator and the CSE per-provider caches are all cold inside the measurement.
- The CSE rows write more output by design. Each ratio is the cost of the extra description.
- Swashbuckle's default OpenAPI 3.0 document, as the CSE Readme says to keep.
- The library compiles against its declared ranges (Swashbuckle.AspNetCore `[10.2.3,11)`, Microsoft.OpenApi `[2.7.5,3)`) and runs on the pinned versions, the same as a consumer app does.
- net8.0, net9.0, net10.0 and net11.0. The package targets all four, and Swashbuckle 10.3.1 runs on all four.
- Plain `AddSwaggerGen` does not read XML comments. Only `CSharpEssentials_AddSwagger` calls `IncludeXmlComments`. The baseline of the OpenApi suite runs the framework XML comment generator, so ratios across the two suites are not comparable.
- Every class has `[MemoryDiagnoser]`. A doc URL sits above every row.

## What the parity check enforces

`[GlobalSetup]` runs the checks before every run of the warm class, and `--self-check` runs them for both classes without BenchmarkDotNet. A failure throws and the run stops.

- The document is larger than 10,000 bytes, and `openapi` is `3.0.x` and the same in every row.
- Document 1 and document 5 from the same host have the same SHA-256 hash, with 3 requests in between. This holds for every row, `CSharpEssentials_AddSwagger` included.
- The (method, path) set of the full forms is the same 50 entries in every row. The plain, enum and idempotency rows have exactly 50 operations. The optional route rows and `CSharpEssentials_AddSwagger` have exactly 70: the extra 20 are `GET /resources{k}/archive` and `GET /resources{k}/archive/{year}`, and every path parameter is `required: true`.
- Every row has the 10 `Resource{k}` schemas.
- CSE enum rows (`EnumConventions`, `AllConventions`, `AddSwagger`) have exactly 30 enum components. Each has `type: string`, `enum` = `alpha, beta, gamma, delta, epsilon, zeta`, the six member names in `x-enum-varnames` and `0` to `5` in `x-enum-numeric-values`. Every `Kind` property has `default: "beta"`.
- Rows without the enum conventions contain no `x-enum-varnames`. Their `Enum01` shape, the `Kind` default of every resource and the `stage` property of `Resource01` are asserted exactly (see the feature matrix).
- Idempotency rows (`IdempotencyConventions`, `AllConventions`, `AddSwagger`): exactly the 10 POSTs have the `Idempotency-Key` header, the `Idempotency-Replayed` header on a 2xx response, and `400`, `409` and `422` with `application/problem+json`. No other operation has any of these. Rows without the idempotency conventions contain no `Idempotency-Key`.
- The first-document rows are built by the same functions as the warm rows. Their document must pass the same checks and have the same hash as the warm document.

## Pinned versions

| Package | Version | License |
|---|---|---|
| Swashbuckle.AspNetCore | 10.3.1, the latest stable | MIT |
| Microsoft.OpenApi | 2.13.0, the latest 2.x. 3.11.0 is the latest overall, but 3.x is incompatible with this package | MIT |
| Microsoft.AspNetCore.TestHost | 8.0.30 (net8.0), 9.0.4 (net9.0), 10.0.11 (net10.0 and net11.0) | MIT |
| BenchmarkDotNet | `[0.14.0,)` from `Directory.Packages.props` | MIT |

The OpenAPI pins are exact and scoped to the OpenApi and Swashbuckle benchmark projects in `Directory.Packages.props`. Every other project keeps the ranges.

## Feature matrix

The plain columns come from the documents the sample hosts write on the pinned versions. The plain host registers the CSE JSON converter, and the StringEnums host registers `JsonStringEnumConverter`. The self-check asserts the shapes listed below the table. Rows marked "not in the sample" come from reading the docs and the source.

| Feature | Plain `AddSwaggerGen` | Plain + `JsonStringEnumConverter` | CSE conventions | CSE `AddSwagger` |
|---|---|---|---|---|
| Enum as a string schema with wire names | Yes: `type: string` with `alpha` ... Swashbuckle writes each value through the app JSON options, so it picks up the CSE converter | `type: string` with member names (`Alpha` ...) | Yes | Yes |
| One component per enum | Yes | Yes | Yes | Yes |
| `x-enum-varnames` / `x-enum-descriptions` / `x-enum-numeric-values` | No | No | Yes | Yes |
| Value table appended to the enum description | No. No description at all | No | Yes | Yes, after the XML summary |
| `[DefaultValue]` written as the wire name | No. No `default` | No. No `default` | Yes, `"beta"` | Yes, `"beta"` |
| `[Flags]` as an array with `uniqueItems` | No (not in the sample) | No (not in the sample) | Yes | Yes |
| Nullable enum usage by document version (3.0 `nullable`, 3.1 `oneOf` null) | No. A plain `$ref` | No. A plain `$ref` | Yes, 3.0 `allOf` with `nullable: true` | Yes |
| Number-format and mixed-format documents marked (`x-enum-wire-format`) | No | No | Yes | Yes |
| `Idempotency-Key` header (required per endpoint) | No | No | Yes | Yes |
| `Idempotency-Replayed` header on 2xx | No | No | Yes | Yes |
| 400/409/422 problem responses for idempotency | No | No | Yes | Yes |
| Optional route parameters split into compliant paths | No. One path per route | No | Yes | Yes |
| Required-only mode for optional route parameters | No | No | Yes (`OptionalRouteParameterMode.RequiredOnly`) | No. `ConfigureSwaggerOptions` calls `AddOptionalRouteParameters()`, which splits paths |
| Security scheme, schema ids, XML comments | No | No | No | Yes |

The asserted plain shapes, as written by Swashbuckle.AspNetCore 10.3.1 on every TFM:

- Plain `Enum01`: `{"enum": ["alpha", "beta", "gamma", "delta", "epsilon", "zeta"], "type": "string"}`.
- StringEnums `Enum01`: `{"enum": ["Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta"], "type": "string"}`.
- `Kind` of every resource, in both: no `default`.
- `Resource01.stage` in both: `{"$ref": "#/components/schemas/Enum02"}`, with no `nullable`.

## Semantic differences

- None of the CSE rows is like-for-like with the plain row on output. Each CSE row describes the same endpoints with more detail. The optional route rows and `AddSwagger` also describe 20 more operations.
- `Plain_AddSwaggerGen_StringEnums` uses `JsonStringEnumConverter` instead of the CSE converter. That is the framework's documented way to get string enums. Its documents describe member names, not CSE wire names.
- `CSharpEssentials_AddSwagger` does more work than `CSharpEssentials_AllConventions`: the security requirement, schema ids and XML comments come on top.

## Not compared

Unchase.Swashbuckle.AspNetCore.Extensions 2.7.2 (2024-12-17) adds enum descriptions and `x-enum-varnames` to Swashbuckle. It is left out for two reasons:

- It does not support Swashbuckle 10. It depends on Swashbuckle.AspNetCore.SwaggerGen 6.9.0, and its assembly uses the `Microsoft.OpenApi.Models` and `Microsoft.OpenApi.Any` namespaces. They exist only in Microsoft.OpenApi 1.x. Swashbuckle 10 uses Microsoft.OpenApi 2.x, so the extension would fail at type load. The upstream issues are open: [#43 Swashbuckle 10 Compatibility](https://github.com/unchase/Unchase.Swashbuckle.AspNetCore.Extensions/issues/43) and [#45](https://github.com/unchase/Unchase.Swashbuckle.AspNetCore.Extensions/issues/45).
- Its nuspec has no `<license>` element, so the package itself does not state a license. The repository's LICENSE file is Apache-2.0.

## Results

Pending the full default-job run (`./run-benchmarks.sh --project swashbuckle`). The reports will be committed under `benchmarks/results/swashbuckle/{tfm}/results/`.

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project swashbuckle                        # all TFMs (net8-net11), default job
./run-benchmarks.sh --project swashbuckle --tfm net10.0          # one TFM
./run-benchmarks.sh --project swashbuckle --quick                # net9.0, Short job
dotnet run -c Release --framework net10.0 --project CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks -- --self-check
```
