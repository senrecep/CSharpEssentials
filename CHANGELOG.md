# Changelog

All notable changes to the CSharpEssentials packages are listed here. All packages share one version number.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [6.3.0](https://github.com/senrecep/CSharpEssentials/compare/v6.2.0...v6.3.0) (2026-10-08)


### Behaviour changes

* **results:** `MapError(Func<Error, Error>)` now maps every error of a failed result, in order. It used to map only the first error and drop the rest.
* **results:** `TryAsync` no longer turns the caller's cancellation into an error `Result`. When the passed token is cancelled, `OperationCanceledException` propagates. Cancellation from any other token still becomes an error. This also applies to `SaveChangesAsResultAsync` and to async rules evaluated by `RuleEngine.Evaluate` unless an enclosing sync rule returns it as an error `Result`.


### Added

* **efcore:** add nullable Maybe&lt;T&gt;? column mapping ([a42be18](https://github.com/senrecep/CSharpEssentials/commit/a42be18177869a51bdf56101fe10e5489aba427c)), closes [#106](https://github.com/senrecep/CSharpEssentials/issues/106)
* **maybe:** add Flatten for Maybe&lt;T&gt;? ([8765bc9](https://github.com/senrecep/CSharpEssentials/commit/8765bc95d035dbf122b5bbf4fd020e3111b9bbde))
* **results:** add MapErrorAsync overloads ([e0e78e2](https://github.com/senrecep/CSharpEssentials/commit/e0e78e2982b2f2425ca30636f0053b0539e20d72))
* **rules:** add async RuleEngine.EvaluateAsync that awaits async rules ([e66f8bc](https://github.com/senrecep/CSharpEssentials/commit/e66f8bcb9c76c594d4bca7d5c8243f00d57279e4))


### Fixed

* **efcore:** map Maybe&lt;T&gt;? in complex types and document convention limits ([10ca7ef](https://github.com/senrecep/CSharpEssentials/commit/10ca7efe1de4fcbbe2a7236000e430bdd1886531))
* **results:** map every error in MapError(Func&lt;Error, Error&gt;) ([8778617](https://github.com/senrecep/CSharpEssentials/commit/87786173348f203d2ef1f1918dc2cd7378bd894f)), closes [#107](https://github.com/senrecep/CSharpEssentials/issues/107)
* **results:** propagate caller cancellation from TryAsync instead of converting it to an Error ([a126b51](https://github.com/senrecep/CSharpEssentials/commit/a126b51d8c6ec13ba5e7f4d78d958ed06150091f))
* **rules:** document that sync Evaluate throws caller cancellation for async root rules ([1800cce](https://github.com/senrecep/CSharpEssentials/commit/1800cce4b687048c4abf4e1b3296e40881d0a212))


### Changed

* **core:** skip allocations in WithCancellation when task completed or token cannot cancel ([f9553f9](https://github.com/senrecep/CSharpEssentials/commit/f9553f9a299c6ed9c09b15a92da671a698aaea65))

## [6.2.0](https://github.com/senrecep/CSharpEssentials/compare/v6.1.0...v6.2.0) (2026-10-07)


### Added

* **openapi:** describe optional route parameters in Microsoft.AspNetCore.OpenApi documents ([33855a3](https://github.com/senrecep/CSharpEssentials/commit/33855a3860a6efa0812cb3865752b04d0f2c0b24))
* **openapi:** warn when SplitPaths keeps an operation on one path ([593de9b](https://github.com/senrecep/CSharpEssentials/commit/593de9b53fdefc62c337a2cad26b613d1ac07ce7))


### Fixed

* **openapi:** drop the `{type: null}` branch of a required path parameter's oneOf or anyOf (nullable enum route parameters on OpenAPI 3.1), also in the Swashbuckle package ([cb5c1b2](https://github.com/senrecep/CSharpEssentials/commit/cb5c1b2b1e4f5d6ebf764da13b94448d4fe7ce14))

## [6.1.0](https://github.com/senrecep/CSharpEssentials/compare/v6.0.0...v6.1.0) (2026-10-07)


### Added

* **openapi:** emit spec-valid optional route parameters by splitting paths ([e32afea](https://github.com/senrecep/CSharpEssentials/commit/e32afea0328900e37e7355cac9a21c9cc6f09a77)), closes [#101](https://github.com/senrecep/CSharpEssentials/issues/101)

## [6.0.0](https://github.com/senrecep/CSharpEssentials/compare/v5.2.1...v6.0.0) (2026-10-07)


### ⚠ BREAKING CHANGES

* **openapi:** CSharpEssentials.AspNetCore.Swashbuckle needs Swashbuckle.AspNetCore [10.2.3,11) and Microsoft.OpenApi [2.7.5,3) on every target. AddSwagger<T>(securityScheme, assembly) is now AddSwagger<T>(securitySchemeName, securityScheme, assembly); pass SecuritySchemes.JwtBearerSchemeName ("Bearer") for the previous behavior. SecuritySchemes.JwtBearerTokenSecurity no longer sets Reference. Custom Swashbuckle filters and MapType factories must move to the Microsoft.OpenApi 2.x API (using Microsoft.OpenApi; IOpenApiSchema; JsonSchemaType; JsonNode).
* **efcore:** offset pagination returns at most 100 rows per page unless a larger cap is given. Pass maxPageSize: 500 (or int.MaxValue for the 5.x behavior) to PaginateAsync/Paginate, or call Normalize(int.MaxValue). The sync Paginate overloads gained a parameter, so assemblies compiled against 5.x must be recompiled. Custom IPaginationRequest types that override Normalize() must also override Normalize(int). See docs/migration/v5-to-v6.md.
* **efcore:** the single-column cursor `PaginateAsync<T, TCursor>` is `[Obsolete]`; use `KeysetPaginateAsync`, which supports composite keys, opaque cursors, backward paging and a maximum limit ([d431ad7](https://github.com/senrecep/CSharpEssentials/commit/d431ad7)).

### Added

* **efcore:** cap offset page size at 100 by default ([74badad](https://github.com/senrecep/CSharpEssentials/commit/74badad9db80607a798a41c282e064a2d3578985))
* **openapi:** require Swashbuckle 10 and Microsoft.OpenApi 2 on all targets ([ca352cf](https://github.com/senrecep/CSharpEssentials/commit/ca352cf0675b2105adbbae6adb0fc4dc2fdd1d91)), closes [#69](https://github.com/senrecep/CSharpEssentials/issues/69)


### Fixed

* **openapi:** keep enum type summaries off enum properties ([efb7d53](https://github.com/senrecep/CSharpEssentials/commit/efb7d53f03e9ead45dd4b31d93be45d7d6ff3c76))

## [5.2.1](https://github.com/senrecep/CSharpEssentials/compare/v5.2.0...v5.2.1) (2026-10-07)


### Fixed

* **efcore:** prevent int overflow in pagination skip and take ([07bfc4a](https://github.com/senrecep/CSharpEssentials/commit/07bfc4a879023499bfe7be343534b9c38d39ee26))
* **http:** block more reserved IPv6 and 6to4 relay ranges in SSRF guard ([904f979](https://github.com/senrecep/CSharpEssentials/commit/904f97959ace1e37d6ecda162e163b244d27d881))

## [5.2.0](https://github.com/senrecep/CSharpEssentials/compare/v5.1.0...v5.2.0) (2026-10-07)


### Added

* **efcore:** add opt-in max limit to pagination request normalization ([4a77d5b](https://github.com/senrecep/CSharpEssentials/commit/4a77d5b445342039cf4711d8e923cc512cc0efcc)) (refs [#92](https://github.com/senrecep/CSharpEssentials/issues/92))
* **http:** add SSRF guard for outbound HttpClient requests ([c97b0cc](https://github.com/senrecep/CSharpEssentials/commit/c97b0cc1a7df6c48d43ec188053ecdf7afe8cdf6)), closes [#80](https://github.com/senrecep/CSharpEssentials/issues/80)

## [5.1.0](https://github.com/senrecep/CSharpEssentials/compare/v5.0.0...v5.1.0) (2026-10-07)


### Added

* **aspnetcore:** add conditional GET (ETag/Last-Modified, 304) and If-Match preconditions ([5f1e229](https://github.com/senrecep/CSharpEssentials/commit/5f1e22935277b00d7d53849f80738a536679fe4b)), closes [#78](https://github.com/senrecep/CSharpEssentials/issues/78)
* **aspnetcore:** add Idempotency-Key middleware with pluggable IIdempotencyStore ([60caf33](https://github.com/senrecep/CSharpEssentials/commit/60caf33c7ee68f60e7082ffd447d85d8e3224832)), closes [#79](https://github.com/senrecep/CSharpEssentials/issues/79)
* **efcore:** add composite keyset pagination with opaque cursors ([3835744](https://github.com/senrecep/CSharpEssentials/commit/38357441eb23b42146b9fe2a1eea46dafab38e9c)), closes [#87](https://github.com/senrecep/CSharpEssentials/issues/87)
* **efcore:** add EF Core 10 named query filter helpers and IgnoreQueryFilters analyzer ([3e91857](https://github.com/senrecep/CSharpEssentials/commit/3e91857d981ee0be19db80b277769f263fdac6d0)), closes [#77](https://github.com/senrecep/CSharpEssentials/issues/77)
* **efcore:** add IDbErrorTranslator with SQLSTATE default ([ad06306](https://github.com/senrecep/CSharpEssentials/commit/ad0630634c0df084668f476470b43670f21e8003)), closes [#74](https://github.com/senrecep/CSharpEssentials/issues/74)
* **mediator:** add IResourceLock and LockBehavior ([2d08bfb](https://github.com/senrecep/CSharpEssentials/commit/2d08bfb17b00873314acc48a60a89d924c71c9f3)), closes [#76](https://github.com/senrecep/CSharpEssentials/issues/76)
* **mediator:** add ITransactionRunner and TransactionBehavior ([105a3ac](https://github.com/senrecep/CSharpEssentials/commit/105a3ac6f74fb8c53ebdbd43d7a27d3c1f227245)), closes [#72](https://github.com/senrecep/CSharpEssentials/issues/72)
* **mediator:** add validation modes and failure observers ([ecf9873](https://github.com/senrecep/CSharpEssentials/commit/ecf98739cfe431413a26ccaec1b22dba6fcd7eed)), closes [#73](https://github.com/senrecep/CSharpEssentials/issues/73)


### Fixed

* **mediator:** roll back TransactionScopeBehavior on failed results ([a692870](https://github.com/senrecep/CSharpEssentials/commit/a692870751d7ebb4c1418fb954bf3e32e15650fd))

## [5.0.0](https://github.com/senrecep/CSharpEssentials/compare/v4.1.0...v5.0.0) (2026-10-06)


### ⚠ BREAKING CHANGES

* **enums:** the CSE enum analyzers (CSE0002-CSE0016) and the generator now run in every project that references any CSharpEssentials package that depends on CSharpEssentials.Enums, including .Validation, .Mediator, .Any, .Maybe, .Entity, .Rules, .These, .Resilience and .GcpSecretManager. New CSE warnings and errors can appear; fix them or set their severity in .editorconfig.
* **enums:** the CSE enum analyzers (CSE0002-CSE0016) and the generator now also run in every project that references CSharpEssentials.Errors, .Results, .AspNetCore.OpenApi or .AspNetCore.Swashbuckle. New CSE diagnostics can appear; fix them or set their severity in .editorconfig.
* **enums:** the CSE enum analyzers (CSE0002-CSE0016) and the generator now run in every project that references CSharpEssentials.Json, .EntityFrameworkCore, .AspNetCore, .Http or the CSharpEssentials meta-package. New CSE warnings can appear (errors under TreatWarningsAsErrors); fix them or set their severity in .editorconfig.
* **swashbuckle:** EnumSchemaFilter takes an IServiceProvider and no longer writes the "Possible values" description.
* **aspnetcore:** hosts that use AddSwagger or the Swagger filters must reference CSharpEssentials.AspNetCore.Swashbuckle.
* **json:** enums without generated metadata are no longer converted by AddEnumConventions, ConditionalStringEnumConverter or a custom CanHandle predicate; use AddEnumConventionsWithReflection. StringEnumNaming, and the EF Core and binding helpers built on it, throw InvalidOperationException for such enums.
* **json:** ConditionalStringEnumConverter.AllowUndefinedValues is removed and undefined values are rejected on read and write. Naming policies other than SnakeCaseLower throw NotSupportedException; set the naming at build time instead.

### Added

* **aspnetcore:** add AddEnumConventions, enum wire format and route binding through the conventions ([331d94b](https://github.com/senrecep/CSharpEssentials/commit/331d94bca1201be90def9807bcc17d1c64da2719))
* **aspnetcore:** move the Swagger support to CSharpEssentials.AspNetCore.Swashbuckle ([e74f257](https://github.com/senrecep/CSharpEssentials/commit/e74f25754d1647f1ddd33c343cdfc8ecc28b1a6e))
* **efcore:** add enum column migration helpers and data audit ([ad5d7a4](https://github.com/senrecep/CSharpEssentials/commit/ad5d7a48caab62cd13c309e5703e2fbfb56e4fb3))
* **efcore:** store enums as wire names or integers with check constraints ([59c81f9](https://github.com/senrecep/CSharpEssentials/commit/59c81f93c769524e71606071abfccf202b6a6a5d)), closes [#64](https://github.com/senrecep/CSharpEssentials/issues/64)
* **enums:** add analyzers for wire names, aliases, fallbacks and flags ([0f1d441](https://github.com/senrecep/CSharpEssentials/commit/0f1d441593f115debd6ee9c9a8271a1ac208e192))
* **enums:** add code fixes for explicit values and none member ([57f8a8a](https://github.com/senrecep/CSharpEssentials/commit/57f8a8a4ab9189b2ceceecaea3777ba47f0888e7))
* **enums:** add enum metadata, conventions, parser and formatter ([c2d3d66](https://github.com/senrecep/CSharpEssentials/commit/c2d3d66a5fe161a422a0356110b47b808899f2d8)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** add non-generic EnumValueFormatter overloads ([bfb237b](https://github.com/senrecep/CSharpEssentials/commit/bfb237bb0f6d771e041c1a934558868c88d3ae32)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** generate enum metadata, registration and wire-name helpers ([80c0689](https://github.com/senrecep/CSharpEssentials/commit/80c06897b6ddbd5b5e52e0b58053659da690951d)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** report AlterColumn next to ConvertEnumColumn for the same column (CSE0014) ([0dc2562](https://github.com/senrecep/CSharpEssentials/commit/0dc256249e2004a412f8e7a82eb7135b4219b531))
* **enums:** warn with CSE0015 when a [StringEnum] enum gets no generated metadata ([b13a2e1](https://github.com/senrecep/CSharpEssentials/commit/b13a2e169fb0c88cb2550edea273d19b410f4d44)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **http:** format enums in query and route through EnumValueFormatter ([462ef1d](https://github.com/senrecep/CSharpEssentials/commit/462ef1da70821f9b2a0e761a683a85dc18d6ea47))
* **json:** add EnumConverterFactory and AddEnumConventions ([4b8a7f6](https://github.com/senrecep/CSharpEssentials/commit/4b8a7f66a05884a72fb978eb08f75c2f379cdd47)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **json:** make reflection enum metadata an explicit opt-in and fail loud for [StringEnum] without metadata ([a32e6f3](https://github.com/senrecep/CSharpEssentials/commit/a32e6f3bf7fc3fcdb6ecd1278303bc18f04d5c87)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **openapi:** warn once per enum when a document mixes string and number formats ([74ca8f3](https://github.com/senrecep/CSharpEssentials/commit/74ca8f3367cfa73c5668a82c483bade79dffb06e))
* **swashbuckle:** describe enums by the enum conventions ([7e1258e](https://github.com/senrecep/CSharpEssentials/commit/7e1258e352fbadf705ab458ffe22a892f09f66c1)), closes [#63](https://github.com/senrecep/CSharpEssentials/issues/63)
* **validation:** add enum rules that reuse the binding error text ([cb0233e](https://github.com/senrecep/CSharpEssentials/commit/cb0233e3102d1f23bae17090ba557cbf364c4a3a))


### Fixed

* **aspnetcore:** bind whitespace-only enum values to null for nullable targets ([13806b3](https://github.com/senrecep/CSharpEssentials/commit/13806b30cef79d48592a3c44ea8100dc6bc9354b))
* **aspnetcore:** keep the host output formatters when applying an enum wire format override ([eded825](https://github.com/senrecep/CSharpEssentials/commit/eded82558f07bed26464128d04f5a7be12697b94))
* **aspnetcore:** trim surrounding whitespace of enum binding values ([733f389](https://github.com/senrecep/CSharpEssentials/commit/733f38967d26a6f12ba12ae74fc0310439c83c53))
* **aspnetcore:** validate only the first value source of an unannotated MVC enum parameter ([212d2ea](https://github.com/senrecep/CSharpEssentials/commit/212d2eabe4ce8cf94c7f22c85eeb5f6b099f0685))
* **efcore:** compare text[] flags columns as enum values on PostgreSQL ([41b521b](https://github.com/senrecep/CSharpEssentials/commit/41b521bf05ae3ef84633900d65a72952436bfdcb)), closes [#64](https://github.com/senrecep/CSharpEssentials/issues/64)
* **efcore:** keep SQLite check constraints enforced when an enum conversion fails ([b6b0d4b](https://github.com/senrecep/CSharpEssentials/commit/b6b0d4bc96ab30f8b2ab6140d9957d0616c43ded))
* **efcore:** match a missing migration schema against any schema in CSE0014 ([53bf32a](https://github.com/senrecep/CSharpEssentials/commit/53bf32a324012c6488b7759ea010d9a40743696e))
* **efcore:** name the PostgreSQL flags conversion column from a short hash of the column name ([cb76c18](https://github.com/senrecep/CSharpEssentials/commit/cb76c183a9e7d2476ca567e52f7c821e03038e29))
* **efcore:** resolve ConfigureEnumConventions(null) ambiguity with OverloadResolutionPriority ([0dc031e](https://github.com/senrecep/CSharpEssentials/commit/0dc031ebaa971a37919d91deebbc95093ce1593e))
* **efcore:** split SQLite flags text with a recursive CTE instead of json_each ([b4d9f51](https://github.com/senrecep/CSharpEssentials/commit/b4d9f510fda32d08ecbc667b65acebb7100ab827))
* **efcore:** throw when an obsolete CanConvert predicate selects an enum without metadata ([5795fd0](https://github.com/senrecep/CSharpEssentials/commit/5795fd06b2f8053ebe7574911e35d5582ad8b8eb))
* **efcore:** write enum wire names in JSON columns on EF Core 9 ([44ccbac](https://github.com/senrecep/CSharpEssentials/commit/44ccbac3031855d489c23773c95117df75a23cca)), closes [#64](https://github.com/senrecep/CSharpEssentials/issues/64)
* **enums:** escape keyword member names in generated value expressions ([a468a28](https://github.com/senrecep/CSharpEssentials/commit/a468a28f252b76ff34b61c850c4800ebaa975104)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** format a flags zero without a zero member as "0" ([fac8d4a](https://github.com/senrecep/CSharpEssentials/commit/fac8d4a8eeeb335c077c34df464766d6144bd884)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** forward IsExternalInit on .NET 5+ targets ([b4591c3](https://github.com/senrecep/CSharpEssentials/commit/b4591c3898c15c2e7165095e01d4db354274d68b)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** register metadata lazily and rerun module initializers on every miss ([af0fa89](https://github.com/senrecep/CSharpEssentials/commit/af0fa896264cba285f4330886878fe002c18a9eb)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **enums:** reject numeric text longer than 20 characters in the parser as the JSON converter does ([16eefbd](https://github.com/senrecep/CSharpEssentials/commit/16eefbd78f152bf84311bc086af0f6e8210755d1))
* **enums:** throw from EnumValueFormatter when a handled enum has no generated metadata ([68c8aa8](https://github.com/senrecep/CSharpEssentials/commit/68c8aa87107f7d78417626e86d0831fb07e6ec25))
* **enums:** use unique hint names and report colliding extensions classes with CSE0016 ([d8a5aa4](https://github.com/senrecep/CSharpEssentials/commit/d8a5aa4748318f99dedcd3add598259c4b203473)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **examples:** read the sample database password from the environment instead of appsettings ([8d504b2](https://github.com/senrecep/CSharpEssentials/commit/8d504b2fa4eb79ccb3228db730d5c3c5d8c2cf29))
* **http:** annotate reflection based ToQueryString(object) with trimming attributes ([e399d2e](https://github.com/senrecep/CSharpEssentials/commit/e399d2e07cd86d08f5752c18b8d4761a8cbf1c9b))
* **http:** annotate the remaining reflection based query string overloads for trimming ([de2395c](https://github.com/senrecep/CSharpEssentials/commit/de2395cb637534e4695851636a0e5c0873d67365))
* **http:** cache builder JSON options per EnumConventions instance with a weak table ([5a2c88b](https://github.com/senrecep/CSharpEssentials/commit/5a2c88b797cc0c947c246e483b06c592f8b3be34))
* **http:** format non-enum collection items per item with invariant culture in query strings ([a7717bb](https://github.com/senrecep/CSharpEssentials/commit/a7717bb8fb19b27afef73f01f1a27460199a8769))
* **http:** serialize WithJsonContent payload at Build so later WithEnumConventions applies ([e1f15d0](https://github.com/senrecep/CSharpEssentials/commit/e1f15d05d6d15300533ad0dfb35c20236db8b4b2))
* **json:** do not claim a StringEnum enum without metadata that CanHandle excludes ([4e67b63](https://github.com/senrecep/CSharpEssentials/commit/4e67b6379ed142f11c4b35154839689a1c66b5d8))
* **json:** keep existing JsonStringEnumConverter when adding enum conventions ([47fe470](https://github.com/senrecep/CSharpEssentials/commit/47fe4700e93169c0625c22ecc3faf0d03d240f56))
* **json:** reject over-long enum strings before unescaping and truncate error values ([282f244](https://github.com/senrecep/CSharpEssentials/commit/282f244e1bc5e4a28edf3a0704f402bf532d6401)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **json:** reject the JSON number -0 for enums and bound number error values ([025c426](https://github.com/senrecep/CSharpEssentials/commit/025c4269317a96a6683aae7faafe74c510b02fad)), closes [#61](https://github.com/senrecep/CSharpEssentials/issues/61)
* **json:** stop writing parse failures to the console from ConvertToJsonDocument ([4c9c9ba](https://github.com/senrecep/CSharpEssentials/commit/4c9c9ba51e885ad937868761fe611bfbc3f52f7e))
* **openapi:** classify MVC enum parameters by their model type ([9f8c603](https://github.com/senrecep/CSharpEssentials/commit/9f8c60385d037b63f05d95b03b6322306b411c32))
* **openapi:** describe enum properties declared on polymorphic derived types ([0b5effa](https://github.com/senrecep/CSharpEssentials/commit/0b5effab939b7ace29acdda1abdfa70480310335))
* **openapi:** fall back to WriteAs when disposing the selector scope throws ([fdd6694](https://github.com/senrecep/CSharpEssentials/commit/fdd6694ddfcddce700d10570aeb89f5f0009c071))
* **openapi:** run header format selectors in a scope and fall back to WriteAs when they throw ([cdad36b](https://github.com/senrecep/CSharpEssentials/commit/cdad36bdcfd80323e1897de0a058a42e870f60b7))
* **openapi:** write ulong enum values above long.MaxValue exactly in both packages ([45267a4](https://github.com/senrecep/CSharpEssentials/commit/45267a48b84bb6af32bde447980cbb15e26a9c48))
* **validation:** use the {Prop}.{Rule} format for enum validator error codes ([89521bf](https://github.com/senrecep/CSharpEssentials/commit/89521bf873a9c815d114f1d5602c00f3c867de09))


### Changed

* **aspnetcore:** copy request query and form only when an enum binding value changes ([611e8ae](https://github.com/senrecep/CSharpEssentials/commit/611e8ae4bd93ea5dec4a90484c5cc4fa8a1cad81))
* **efcore:** drop the per-write closure in EnumColumnCodec ([4ff6b20](https://github.com/senrecep/CSharpEssentials/commit/4ff6b20f0fda51a671f9df59a054ada103b86de6))
* **json:** drop the per-call delegate array in ConvertToJsonDocument ([fe2c5a6](https://github.com/senrecep/CSharpEssentials/commit/fe2c5a62e3c30c572afd7c23383aa1d8677f5760))
* **json:** parse a number token once in the enum converter ([4823407](https://github.com/senrecep/CSharpEssentials/commit/48234078cc07f06e3db818c85e65a46724a7a541))
* **json:** read an over-long unknown enum value as the fallback without allocating its string ([70ea029](https://github.com/senrecep/CSharpEssentials/commit/70ea02931e4948e2dbe4ea835869e39bfc404883))


### Build

* **enums:** let every csharpessentials package bring the enums generator and analyzers ([0fff7c3](https://github.com/senrecep/CSharpEssentials/commit/0fff7c305a1b94047ebf1df26374e9388f750e19))
* **enums:** let the enums generator and analyzers flow through errors, results, openapi and swashbuckle ([0d4a4e3](https://github.com/senrecep/CSharpEssentials/commit/0d4a4e376cd4c7a428faf5df1abd957c0b3f0a46))
* **enums:** let the enums generator and analyzers flow through json, efcore, aspnetcore, http and the meta-package ([49890fc](https://github.com/senrecep/CSharpEssentials/commit/49890fc6a098014e3409eb731e7032b1b452e78e))

## [4.1.0](https://github.com/senrecep/CSharpEssentials/compare/v4.0.0...v4.1.0) (2026-10-06)

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

## [4.0.0](https://github.com/senrecep/CSharpEssentials/compare/v3.2.3...v4.0.0) (2026-10-06)

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

## [3.2.3](https://github.com/senrecep/CSharpEssentials/compare/v3.2.2...v3.2.3) (2026-05-31)

### Added

- `CSharpEssentials.These` is part of the meta-package.

## [3.2.2](https://github.com/senrecep/CSharpEssentials/compare/v3.2.1...v3.2.2) (2026-05-31)

### Added

- New package `CSharpEssentials.These` with JSON serialization.
- More `Maybe` and `Result` extensions, `RuleEngine.FromPredicate` and `FakeDateTimeProvider`.

## [3.2.1](https://github.com/senrecep/CSharpEssentials/compare/v3.2.0...v3.2.1) (2026-05-31)

### Added

- `ExceptionHandlingBehavior` pipeline behavior in `CSharpEssentials.Mediator`.

## [3.2.0](https://github.com/senrecep/CSharpEssentials/compare/v3.1.0...v3.2.0) (2026-05-31)

### Added

- New package `CSharpEssentials.Resilience`.

### Fixed

- `CSharpEssentials.Http` restores the default 30 second timeout.
- The `[StringEnum]` generator skips nested enums instead of producing invalid code.

## [3.1.0](https://github.com/senrecep/CSharpEssentials/compare/v3.0.8...v3.1.0) (2026-05-27)

### Added

- Batch APIs for `Result`, `Maybe` and `Any`.
- Railway validation bindings on `Result`, `Task` and `ValueTask`.

### Changed

- Failure paths of `Result` pipelines no longer allocate.

## [3.0.8](https://github.com/senrecep/CSharpEssentials/compare/v3.0.7...v3.0.8) (2026-05-20)

### Changed

- `CSharpEssentials.Validation` allocates less on the valid path.

## [3.0.7](https://github.com/senrecep/CSharpEssentials/compare/v3.0.6...v3.0.7) (2026-05-20)

### Fixed

- Validators support nullable types and concrete collections.

## [3.0.6](https://github.com/senrecep/CSharpEssentials/compare/v3.0.5...v3.0.6) (2026-05-20)

### Added

- New package `CSharpEssentials.Validation`. `CSharpEssentials.Mediator` uses it and surfaces validation errors based on `TResponse`.

## [3.0.5](https://github.com/senrecep/CSharpEssentials/compare/v3.0.4...v3.0.5) (2026-05-07)

### Fixed

- Static analysis warnings across the packages.

## [3.0.4](https://github.com/senrecep/CSharpEssentials/compare/v3.0.3...v3.0.4) (2026-05-06)

### Added

- GitHub Pages landing page and per-package AI agent skills under `.well-known/agent-skills`.

## [3.0.3](https://github.com/senrecep/CSharpEssentials/compare/v3.0.2...v3.0.3) (2026-05-06)

### Added

- New package `CSharpEssentials.Mediator` with pipeline behaviors.
- CQRS `DbContext` registration helpers and SQL connection factory abstractions in `CSharpEssentials.EntityFrameworkCore`.
- Endpoint route builder versioning extensions in `CSharpEssentials.AspNetCore`.
- Redirect following in `CSharpEssentials.Http`.
- `Result.Try` and `Result.TryAsync` overloads.

## [3.0.2](https://github.com/senrecep/CSharpEssentials/compare/v3.0.1...v3.0.2) (2026-05-05)

### Added

- New package `CSharpEssentials.Http`.
- `[StringEnum]` incremental source generator.
- `ResultEndpointFilter` for Minimal APIs.
- `Combine`, `Recover` and `Unwrap` for `Result`.

## [3.0.1](https://github.com/senrecep/CSharpEssentials/compare/v3.0.0...v3.0.1) (2026-05-04)

### Fixed

- Package metadata.

## [3.0.0](https://github.com/senrecep/CSharpEssentials/releases/tag/v3.0.0) (2026-05-04)

### Added

- `Result`: `Ensure`, `MapError`, `TapError`, `ElseDo`, `Compensate`, `Deconstruct`, `Try`, `SuccessIf`, `FailureIf`, `ThenEnsure`, `Select`/`SelectMany`.
- `Maybe`: `Tap`, `TapIf`, `BindIf`, `MapIf`, `ToResult`, `AsMaybe`.
- `Any`: `Deconstruct`, `ToTuple`, `Is<T>`, `As<T>`, `TryAs<T>`.
- `Error`: implicit array conversion and an error combination operator.
- SourceLink and symbol packages.
