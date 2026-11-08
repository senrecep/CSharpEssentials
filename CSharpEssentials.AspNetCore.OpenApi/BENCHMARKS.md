# CSharpEssentials.AspNetCore.OpenApi: performance benchmarks

Results for net10.0 are in [Results](#results); the parity self-check passes. In short: with all three conventions, the warm document takes 1.49 times the plain time and 1.52 times the plain allocation, and the first document in a fresh process takes 1.09 times the plain time. Every CSE row costs more than the plain row, because every CSE row writes more output; see [Where CSE costs more](#where-cse-costs-more).

CSharpEssentials.AspNetCore.OpenApi (CSE) is compared with plain Microsoft.AspNetCore.OpenApi 10.0.14 on Microsoft.OpenApi 2.13.0. There is no competitor library. The baseline is the framework's documented setup: `AddOpenApi()` and `MapOpenApi()`.

The suite lives in `benchmarks/CSharpEssentials.AspNetCore.OpenApi.Benchmarks/`. The Swashbuckle package has its own suite over the same sample API ([CSharpEssentials.AspNetCore.Swashbuckle/BENCHMARKS.md](../CSharpEssentials.AspNetCore.Swashbuckle/BENCHMARKS.md)).

---

## What is measured

| Class | Measured unit | Row | Label |
|---|---|---|---|
| `OpenApiDocumentComparison` | One warm `GET /openapi/v1.json` through TestServer, whole body read | `Plain_AddOpenApi` (baseline): `AddOpenApi("v1")` | Baseline |
| `OpenApiDocumentComparison` | Same | `Plain_AddOpenApi_StringEnums`: `AddOpenApi("v1")` with `JsonStringEnumConverter` | Different JSON options. See semantic differences |
| `OpenApiDocumentComparison` | Same | `CSharpEssentials_EnumConventions`: `o.AddEnumConventions()` | More output |
| `OpenApiDocumentComparison` | Same | `CSharpEssentials_IdempotencyConventions`: `o.AddIdempotencyConventions()` | More output |
| `OpenApiDocumentComparison` | Same | `CSharpEssentials_OptionalRouteParameters`: `o.AddOptionalRouteParameters()` | 70 operations against 50 |
| `OpenApiDocumentComparison` | Same | `CSharpEssentials_AllConventions`: all three, the Readme setup | More output, 70 operations |
| `OpenApiFirstDocumentComparison` | Fresh process to first document: build, start, one GET, stop, dispose | `Plain_AddOpenApi` (baseline), `CSharpEssentials_AllConventions` | Same labels as the warm rows |

Each method returns the document length in bytes, so the work cannot be removed by the JIT.

## Inputs

Every row serves the same sample API from `benchmarks/CSharpEssentials.AspNetCore.ApiDocs.Benchmarks.Shared/`:

- 30 enums, `Enum01` to `Enum30`. Each has `[StringEnum]`, a `[Description]` and six defined, contiguous members, `Alpha = 0` to `Zeta = 5`. `Beta` has its own `[Description]`. No `[Flags]`, `[Obsolete]` or `[EnumFallback]`.
- 10 records, `Resource01` to `Resource10`. Each has an `int`, a `string`, one enum, one nullable enum and one enum with `[DefaultValue(Beta)]`.
- 50 Minimal API endpoints, 5 per resource: a list with an enum query, a get by id, an archive route `{year:int?}/{month:int?}`, an idempotent POST (`.WithIdempotency()`) and a delete with an enum query.

Every host registers the same app services: `AddIdempotency(o => o.Mode = IdempotencyMode.Enforce)` and `AddEnumConventions()`. The StringEnums row registers `JsonStringEnumConverter` instead of `AddEnumConventions()`. Only defined enum values are used anywhere.

## Methodology

- In-process TestServer, Production environment, logging providers cleared. Every row reads the whole response body.
- The plain and CSE hosts differ only in the document options, except the StringEnums row.
- Warm rows regenerate the document on every request, because Microsoft.AspNetCore.OpenApi does not cache it. `MapOpenApi` calls the document service per request.
- Cold rows run one sample per fresh process, 15 launches (`RunStrategy.ColdStart`, no warmup). JIT, the framework schema store and the CSE per-provider caches are all cold inside the measurement.
- The library compiles against its declared ranges (Microsoft.AspNetCore.OpenApi `[10.0.0,11)`, Microsoft.OpenApi `[2.7.5,3)`) and runs on the pinned versions, the same as a consumer app does.
- net10.0 only, the package's only target. On net11.0 the package's net10.0 build would run Microsoft.AspNetCore.OpenApi 10.x on an ASP.NET Core 11 release candidate runtime, which is not a supported pairing. Microsoft.AspNetCore.OpenApi 11.x needs Microsoft.OpenApi 3.x, which the package does not support yet.
- The repo builds with `GenerateDocumentationFile=true`. So the built-in XML comment generator of Microsoft.AspNetCore.OpenApi runs in every row, the baseline included. That is its default for such projects. The plain Swashbuckle rows do not read XML comments, so ratios across the two suites are not comparable.
- Every class has `[MemoryDiagnoser]`. A doc URL sits above every row.

## What the parity check enforces

`[GlobalSetup]` runs the checks before every run of the warm class, and `--self-check` runs them for both classes without BenchmarkDotNet. A failure throws and the run stops.

- The document is larger than 10,000 bytes, and `openapi` is `3.1.x` and the same in every row.
- Document 1 and document 5 from the same host have the same SHA-256 hash, with 3 requests in between.
- The (method, path) set of the full forms is the same 50 entries in every row. The plain, enum and idempotency rows have exactly 50 operations. The optional route rows have exactly 70: the extra 20 are `GET /resources{k}/archive` and `GET /resources{k}/archive/{year}`, and every path parameter is `required: true`.
- Every row has the 10 `Resource{k}` schemas.
- CSE enum rows have exactly 30 enum components. Each has `type: string`, `enum` = `alpha, beta, gamma, delta, epsilon, zeta`, the six member names in `x-enum-varnames` and `0` to `5` in `x-enum-numeric-values`. Every `Kind` property has `default: "beta"`.
- Rows without the enum conventions contain no `x-enum-varnames`. Their `Enum01` shape, the `Kind` default of every resource and the `stage` property of `Resource01` are asserted exactly (see the feature matrix).
- Idempotency rows: exactly the 10 POSTs have the `Idempotency-Key` header, the `Idempotency-Replayed` header on a 2xx response, and `400`, `409` and `422` with `application/problem+json`. No other operation has any of these. Rows without the idempotency conventions contain no `Idempotency-Key`.
- The first-document rows are built by the same functions as the warm rows. Their document must pass the same checks and have the same hash as the warm document.

## Pinned versions

| Package | Version | License |
|---|---|---|
| Microsoft.AspNetCore.OpenApi | 10.0.14, the latest stable. 11.0.0 exists only as a release candidate | MIT |
| Microsoft.OpenApi | 2.13.0, the latest 2.x. 3.11.0 is the latest overall, but 3.x is incompatible with this package and with Microsoft.AspNetCore.OpenApi 10.x, which asks for `[2.12.2, 3.0.0)` | MIT |
| Microsoft.AspNetCore.TestHost | 10.0.11 | MIT |
| BenchmarkDotNet | `[0.14.0,)` from `Directory.Packages.props` | MIT |

The three OpenAPI pins are exact and scoped to the OpenApi and Swashbuckle benchmark projects in `Directory.Packages.props`. Every other project keeps the ranges.

## Feature matrix

The plain columns come from the documents the sample hosts write on the pinned versions. The plain host registers the CSE JSON converter, and the StringEnums host registers `JsonStringEnumConverter`. The self-check asserts the shapes listed below the table. Rows marked "not in the sample" come from reading the docs and the source.

| Feature | Plain `AddOpenApi` | Plain + `JsonStringEnumConverter` | CSE `AddEnumConventions` / `AddIdempotencyConventions` / `AddOptionalRouteParameters` |
|---|---|---|---|
| Enum as a string schema with wire names | No. The component has only the description, no `type` and no `enum` | Member names in `enum` (`Alpha` ...), no `type` | Yes: `type: string`, wire names (`alpha` ...) |
| One component per enum | Yes | Yes | Yes |
| `x-enum-varnames` / `x-enum-descriptions` / `x-enum-numeric-values` | No | No | Yes |
| Value table appended to the enum description | No | No | Yes |
| `[DefaultValue]` written as the wire name | Yes, `"beta"`: the value goes through the app JSON options | `"Beta"`, the member name | Yes, `"beta"` |
| `[Flags]` as an array with `uniqueItems` | No (not in the sample) | No (not in the sample) | Yes |
| Nullable enum usage by document version (3.0 `nullable`, 3.1 `oneOf` null) | 3.1 `oneOf` with `{"type": "null"}` | 3.1 `oneOf` with `{"type": "null"}` | Yes, by version |
| Number-format and mixed-format documents marked (`x-enum-wire-format`) | No | No | Yes |
| `Idempotency-Key` header (required per endpoint) | No | No | Yes |
| `Idempotency-Replayed` header on 2xx | No | No | Yes |
| 400/409/422 problem responses for idempotency | No | No | Yes |
| Optional route parameters split into compliant paths | No. One path per route | No | Yes |
| Required-only mode for optional route parameters | No | No | Yes (`OpenApiOptionalRouteParameterMode.RequiredOnly`) |

The asserted plain shapes, as written by Microsoft.AspNetCore.OpenApi 10.0.14:

- Plain `Enum01`: `{"description": "Enum 01 of the sample API: six defined, contiguous members."}`. The framework does not see through the CSE converter, so it writes an empty schema with the XML summary.
- StringEnums `Enum01`: `{"enum": ["Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta"], "description": "..."}`.
- `Kind` default of every resource: `"beta"` in the plain rows, `"Beta"` in the StringEnums row.
- `Resource01.stage` in both: `{"oneOf": [{"type": "null"}, {"description": "A nullable enum.", "$ref": "#/components/schemas/Enum02"}]}`.

## Semantic differences

- None of the CSE rows is like-for-like with the plain row on output. Each CSE row describes the same endpoints with more detail. The optional route rows also describe 20 more operations.
- `Plain_AddOpenApi_StringEnums` uses `JsonStringEnumConverter` instead of the CSE converter. That is the framework's documented way to get string enums. Its documents describe member names, not CSE wire names.
- Both plain rows keep the CSE converter or `JsonStringEnumConverter` registered for the app, so their JSON output is still strings. Only the document differs.

## Results

### How these numbers were produced

- BenchmarkDotNet v0.14.0, full runs (no `--quick`), one suite at a time, on a 6-core, 12-thread x64 desktop CPU on Linux. SDK and runtime builds are in the header of each report.
- TFM: net10.0 only (.NET 10.0.11), the only target of the package.
- `OpenApiDocumentComparison` ran on the default job. `OpenApiFirstDocumentComparison` ran on its own `cold` job: `RunStrategy=ColdStart`, `LaunchCount=15`, `IterationCount=1`, `WarmupCount=0`, so each of the 15 samples is one fresh process.
- The 1-minute load average stayed between 1.55 and 1.96 during the suite. No row has a CV above 5%.
- Reports: [`benchmarks/results/openapi/net10.0/results/`](../benchmarks/results/openapi/net10.0/results/), one `*-report-github.md` and one `.csv` per class. Every number below is copied from the `-report-github.md` files.

Ratios are against `Plain_AddOpenApi`.

### Warm document, one `GET /openapi/v1.json` (net10.0, default job)

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| `Plain_AddOpenApi` (baseline) | 5.176 ms | 1.00 | 3.11 MB | 1.00 |
| `Plain_AddOpenApi_StringEnums` | 5.768 ms | 1.11 | 3.76 MB | 1.21 |
| `CSharpEssentials_EnumConventions` | 6.341 ms | 1.23 | 3.99 MB | 1.28 |
| `CSharpEssentials_IdempotencyConventions` | 5.621 ms | 1.09 | 3.52 MB | 1.13 |
| `CSharpEssentials_OptionalRouteParameters` | 5.391 ms | 1.04 | 3.43 MB | 1.10 |
| `CSharpEssentials_AllConventions` | 7.715 ms | 1.49 | 4.72 MB | 1.52 |

### First document in a fresh process (net10.0, `cold` job, 15 launches)

| Method | Mean | Ratio | Allocated | Alloc Ratio |
|---|---:|---:|---:|---:|
| `Plain_AddOpenApi` (baseline) | 762.4 ms | 1.00 | 8.72 MB | 1.00 |
| `CSharpEssentials_AllConventions` | 828.4 ms | 1.09 | 10.89 MB | 1.25 |

### Where CSE costs more

Every CSE row is slower and allocates more than the plain baseline. Each writes more output, so none is a like-for-like loss, but the cost is real for an app that serves the document:

- The Readme setup, `CSharpEssentials_AllConventions`, takes 7.715 ms against 5.176 ms per warm request (1.49) and allocates 4.72 MB against 3.11 MB (1.52). Microsoft.AspNetCore.OpenApi regenerates the document on every request, so an app pays this on every `GET /openapi/v1.json` unless it caches the response.
- In a fresh process the same setup takes 828.4 ms to the first document against 762.4 ms (1.09), with 10.89 MB against 8.72 MB allocated (1.25).
- `CSharpEssentials_EnumConventions` (1.23, 3.99 MB) costs more than the framework's own string enum setup, `Plain_AddOpenApi_StringEnums` (1.11, 3.76 MB). The CSE row writes wire names, `type: string` and three `x-enum-*` extensions per enum, which the StringEnums row does not.
- `CSharpEssentials_IdempotencyConventions` costs 1.09 in time and 1.13 in allocation. `CSharpEssentials_OptionalRouteParameters` costs 1.04 and 1.10 while it describes 70 operations instead of 50.

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project openapi           # net10.0, both classes, default job
./run-benchmarks.sh --project openapi --quick   # net10.0, Short job
dotnet run -c Release --framework net10.0 --project CSharpEssentials.AspNetCore.OpenApi.Benchmarks -- --self-check
```
