# Changelog

All notable changes to the CSharpEssentials packages are listed here. All packages share one version number.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.0.0](https://github.com/senrecep/CSharpEssentials/compare/v7.0.0...v8.0.0) (2026-11-06)


### ⚠ BREAKING CHANGES

* **core:** On netstandard2.0 and netstandard2.1, including .NET 8 and .NET Framework apps, Guider.NewGuid returns version 7 GUIDs instead of random version 4 GUIDs. The first 6 bytes now hold the creation time.
* **json:** DefaultOptions and StrictOptions now set TypeInfoResolver and AllowOutOfOrderMetadataProperties = true, and ApplyTo applies them to the target options. A resolver the target already has is kept and gets the polymorphism modifier.
* **logging:** projects that compiled against Microsoft.IO types through CSharpEssentials.RequestResponseLogging must add Microsoft.IO.RecyclableMemoryStream themselves.
* **efcore:** CSharpEssentials.EntityFrameworkCore no longer brings in EFCore.NamingConventions. Projects that call UseSnakeCaseNamingConvention or the other naming convention methods must add EFCore.NamingConventions themselves, with the major that matches their EF Core version.
* **json:** a $type that names a framework type (for example System.IO.MemoryStream on an IDisposable member) now throws JsonException.
* **endpoints:** a non-partial class that implements IEndpoint only by inheriting Map from a base class is no longer added to the generated registry. With an abstract or generic base those routes are no longer mapped. Declare static Map on the derived class, or make it partial, to keep mapping it.
* **logging:** the first write or flush to Response.Body now starts the response. Set the status code, headers and cookies before it; changing them afterwards throws InvalidOperationException, and Response.OnStarting callbacks run at the first write instead of when the endpoint returns.
* **logging:** a failed write to the client now throws inside the endpoint and is logged as one exception entry instead of a body entry followed by a copy-failure entry. Partial responses are sent before a later exception, and synchronous writes reach the server, which rejects them unless AllowSynchronousIO is set.
* **logging:** when no logged field needs the request body and no handler is set, Request.Body is no longer replaced by a seekable buffered stream. Code that rewinds the request body must call EnableBuffering.
* **logging:** LoggingFields is read once when the middleware is registered; changing the list afterwards no longer changes what is logged.
* **entity:** EntityBase.DomainEvents is a live view instead of a snapshot. A reference kept before ClearDomainEvents() no longer holds the old events, and raising or clearing events inside a foreach over DomainEvents throws InvalidOperationException. Copy the events first, for example [.. entity.DomainEvents].
* **core:** on net9.0, net10.0 and net11.0, WithCancellation throws TaskCanceledException (a subclass of OperationCanceledException) carrying the token when the token is cancelled. The Results, Maybe and Validation async operations that wait through WithCancellation inherit the change. Catch blocks for OperationCanceledException still work; exact type checks do not.
* **maybe:** on an IList<T> or IReadOnlyList<T>, TryLast(predicate) calls the predicate from the end and stops at the first match, so a predicate with side effects runs fewer times and in reverse order.
* **errors:** exceptionStackTrace is now null for an exception that was never thrown. Call AddStackTrace() to record the current call stack.
* **any:** with a source-generated JsonSerializerContext, register each branch type of Any<...> with [JsonSerializable]. An unregistered branch type now throws NotSupportedException on deserialization instead of falling back to reflection with EnhancedJsonSerializerOptions.DefaultOptions.
* **any:** Any<...>.ToString() no longer returns JSON. A string value comes back without quotes, an object returns its own ToString() and a null value returns an empty string instead of "null". Serialize Value with JsonSerializer when the JSON text is needed.
* **any:** the value inside Any<...> is now deserialized with the JsonSerializerOptions passed to JsonSerializer instead of always with EnhancedJsonSerializerOptions.DefaultOptions. Pass those options to keep the old camelCase, case-insensitive and enum handling.
* **http:** a literal null or default passed as the options argument is now ambiguous (CS0121) for GetFromJsonAsResultAsync<T>(uri, null), SendAsResultAsync<T>(request, null), ReadFromJsonAsResultAsync<T>(null), HttpRequestBuilder.AsResultAsync<T>(client, null) and SendWithRedirectsAsResultAsync<T>(request, null). Drop the argument or cast it to (JsonSerializerOptions?).
* **json:** serializing an undefined enum value with AddEnumConventions no longer throws EnumValueException; it writes the underlying number. Check EnumInfo<TEnum>.IsDefined before serializing if such a value must still fail.
* **resilience:** a TimeoutRejectedException or BrokenCircuitException thrown by the WithFallback delegate now yields the code Resilience.Timeout or Resilience.CircuitBroken instead of the exception type name.
* **resilience:** an exception thrown by the WithFallback delegate is now returned as an ErrorType.Unexpected error instead of ErrorType.Failure.
* **logging:** IgnorePaths("/") now skips only the root path instead of every non-root path, and null or empty entries are skipped instead of throwing or matching all requests.
* **efcore:** AddAuditInterceptor no longer replaces a TimeProvider registered before it with TimeProvider.System, and repeated calls register AuditInterceptor only once.
* **logging:** a failure while copying the buffered response to the client now propagates from the middleware instead of being swallowed, and it is logged as a second "Exception: <type>, Message: <message>" entry after the normal entry with the captured response body.
* **logging:** the UseHandler handler is now called for requests whose pipeline throws, and the logged RequestBody on that path is the captured body instead of "Error occurred during request processing". Handlers that assumed a completed request should check ResponseBody for the
* **errors:** on the netstandard2.1 build, used by net8.0 apps, ErrorMetadata.ToString() formats numbers and dates with the invariant culture instead of the current culture.
* **errors:** setting Code or Description to null in a with expression or object initializer throws ArgumentNullException instead of creating an Error with a null value.
* **swashbuckle:** AddSwagger and new SwashbuckleSchemaIdFactory instances write namespace-qualified schema ids (MyApp.Contracts.Order instead of Order), so every $ref and generated client type name changes. Regenerate clients, or call options.CustomSchemaIds(new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false }.GetSchemaId) after AddSwagger to keep the 7.x ids.
* **efcore:** a cursor of 0 or Guid.Empty is now a real position instead of "first page", and ICursorPaginationRequest<TCursor> has a new HasCursor member. Leave Cursor unset to request the first page, and implement HasCursor in custom ICursorPaginationRequest<TCursor> types (false when the request has no cursor).
* **core:** the case converters, EF Core LegacySnakeCase enum storage and the generated {Member}SnakeCase/{Member}KebabCase constants keep letters without case, titlecase letters and combining marks. "user用户Name".ToSnakeCase() was "user_name" and is now "user用户_name". Output for Latin-only names does not change. Rewrite stored values produced from non-Latin names with the new output.
* **core:** ToStringFromGuid and ToGuidFromString use the RFC 4648 base64url alphabet ('+' as '-', '/' as '_'). A short ID stored by 7.x that contains '-' or '_' now decodes to a different GUID. Convert stored IDs once by swapping every '-' with '_' and every '_' with '-'.

### Added

* **build:** ship each package's agent skill inside the NuGet package ([7eb31d9](https://github.com/senrecep/CSharpEssentials/commit/7eb31d9c77240daa455d83c89f4f4e6c7e03a1ac))
* **core:** add a 6-bit rand_b counter to NewMonotonicGuid ([f3d04b1](https://github.com/senrecep/CSharpEssentials/commit/f3d04b175205fd1d52cfccc1b45a210be7f9f677))
* **core:** add Guider.NewMonotonicGuid for strictly increasing version 7 GUIDs ([40b270d](https://github.com/senrecep/CSharpEssentials/commit/40b270da16329ab0dad5a6f877fd7ce9be1875f6))
* **core:** add Guider.NewSqlServerGuid for SQL Server uniqueidentifier order ([734dbbd](https://github.com/senrecep/CSharpEssentials/commit/734dbbd044212a312758509da47038da14b142d4))
* **core:** return version 7 GUIDs from Guider.NewGuid on netstandard targets ([179fd1a](https://github.com/senrecep/CSharpEssentials/commit/179fd1ae85c3928375d9dccd81393944d206eab3))
* **efcore:** add opt-in, provider-aware ordered GUID key generation ([efeb712](https://github.com/senrecep/CSharpEssentials/commit/efeb7121c367e2f85a185e060a5a571aee4907a4))
* **endpoints:** warn when a non-partial endpoint only inherits Map ([175f67a](https://github.com/senrecep/CSharpEssentials/commit/175f67af994454425a8105cdfd1999935bcdb44b))
* **json:** add CSE4012 and report CSE4005 as a warning only in AOT projects ([c59c03a](https://github.com/senrecep/CSharpEssentials/commit/c59c03aa39974ef2a3d9c90162de83e59f086255))
* **json:** add JsonTypeDiscriminator and AddPolymorphism ([823dcfb](https://github.com/senrecep/CSharpEssentials/commit/823dcfb4308c7798a33678ca9067e1ea0694ce10))
* **json:** add polymorphism source generator, analyzers and code fix ([7fb7ab9](https://github.com/senrecep/CSharpEssentials/commit/7fb7ab9d3e58d4c468066109e9b6fa622e6ca630))
* **json:** turn on polymorphism in DefaultOptions and StrictOptions ([56a03c9](https://github.com/senrecep/CSharpEssentials/commit/56a03c960ea09e574b651fad787ccdd78f456ec7))


### Fixed

* **any:** deserialize the value with the caller's JSON options ([4271e67](https://github.com/senrecep/CSharpEssentials/commit/4271e672eb119e867544c077c3ea10e2a80b13c5))
* **any:** hide AnyValueJsonConverter from IntelliSense and document its only use ([c471255](https://github.com/senrecep/CSharpEssentials/commit/c4712550e91a5d8d95922d6069a655e764e1dfc3))
* **any:** return the value's own ToString without reflection ([7674feb](https://github.com/senrecep/CSharpEssentials/commit/7674feb46649d1798c3925abe08ea3a2adb4f13b))
* **aspnetcore:** add the missing RFC 9110 problem type links ([bd7c1f3](https://github.com/senrecep/CSharpEssentials/commit/bd7c1f36b501eeae5cf9044686549e003402c628))
* **aspnetcore:** title problems for undefined error types as Unknown Error ([b9f0efd](https://github.com/senrecep/CSharpEssentials/commit/b9f0efdd125b1f601d98a289685df5e7b69ee931))
* **bench:** batch the sub-nanosecond Any and These rows so they can be measured ([fc7e557](https://github.com/senrecep/CSharpEssentials/commit/fc7e5572a7ff59860907fed9e68fd827abc8b00d))
* **bench:** expect the plain value from Any ToString ([ea45ab6](https://github.com/senrecep/CSharpEssentials/commit/ea45ab68230601b9c4425d5461332c688d92c7fc))
* **bench:** fail run-benchmarks.sh when a benchmark row is NA ([035f109](https://github.com/senrecep/CSharpEssentials/commit/035f109b53606d7a795841eeeae7296471d974e4))
* **bench:** match enum components by the last segment of the schema id ([54ea718](https://github.com/senrecep/CSharpEssentials/commit/54ea71823361395bcb093ad20b0d7810d3bd9cc0))
* **bench:** return the M.E.Validation errors dictionary so BDN's generated project builds without ASP0029 ([c1e5c4d](https://github.com/senrecep/CSharpEssentials/commit/c1e5c4d1ebd9205d215a1f5aec284f29b721da43))
* **bench:** reuse one failed Result in the results async matrix ([2d238ec](https://github.com/senrecep/CSharpEssentials/commit/2d238ec1ca97bee1b5fad306d13460a1cada53cc))
* **clone:** mark package AOT compatible ([eba73f8](https://github.com/senrecep/CSharpEssentials/commit/eba73f88c490eda653c3dff844e9974d6ed98b6c))
* **core:** decode non-canonical Guider strings the same way on every runtime ([#256](https://github.com/senrecep/CSharpEssentials/issues/256)) ([f10e3aa](https://github.com/senrecep/CSharpEssentials/commit/f10e3aab7b9b9585059874966fe090bd4e5994e6))
* **core:** keep letters without case and combining marks in case converters ([5e0cc2e](https://github.com/senrecep/CSharpEssentials/commit/5e0cc2e867e20d1ebd1559fba13b50b802ea384e))
* **core:** take version 7 random bits from Guid.NewGuid instead of a per-thread buffer ([24ae37a](https://github.com/senrecep/CSharpEssentials/commit/24ae37a77479bea781b752bc35f2aeb8791ea0c1))
* **core:** use the RFC 4648 base64url alphabet in Guider ([38f2d9e](https://github.com/senrecep/CSharpEssentials/commit/38f2d9edcf6a8753940a2442bfcdc4fd5a61cc0d))
* **efcore:** drop the unused EFCore.NamingConventions dependency ([6344875](https://github.com/senrecep/CSharpEssentials/commit/6344875608168cebd8405b30db01d2eaa87c6a02))
* **efcore:** honour a cursor equal to default in obsolete cursor PaginateAsync ([a405345](https://github.com/senrecep/CSharpEssentials/commit/a405345dd6992c71b0aa8e61851df4480a7104f7))
* **efcore:** keep after-save events in DomainEventInterceptor when the before-save publish fails ([5c570a3](https://github.com/senrecep/CSharpEssentials/commit/5c570a330a57a812d25dce453ff6756bbe308739))
* **efcore:** keep an earlier TimeProvider and add AuditInterceptor once in AddAuditInterceptor ([f920bb3](https://github.com/senrecep/CSharpEssentials/commit/f920bb3e1e001451e73878f866b99d5a91379de2))
* **efcore:** keep the outer AfterSave restore when the outbox saves the same BaseDbContext ([ee0b8df](https://github.com/senrecep/CSharpEssentials/commit/ee0b8df053eddab9c625ca744a11b2494b2eb740))
* **efcore:** leave Pomelo GUID key generation to the provider ([ce629ca](https://github.com/senrecep/CSharpEssentials/commit/ce629ca3c0b0ca331b46d40c0ea3533d3a405bc4))
* **efcore:** put AfterSave events back in BaseDbContext when the outbox write fails ([8b510fc](https://github.com/senrecep/CSharpEssentials/commit/8b510fcf048b68cd294fc0662ddd52ee4abccc3f))
* **efcore:** put AfterSave events back in DomainEventInterceptor when the outbox write fails ([062b557](https://github.com/senrecep/CSharpEssentials/commit/062b557f0a2206795b92a5d75dc0da715d7acc09))
* **efcore:** skip GUID keys with a value conversion in UseOrderedGuidKeys ([efe99e9](https://github.com/senrecep/CSharpEssentials/commit/efe99e9248024c8cdb7c18981e7a393bf940d50c))
* **efcore:** skip the BaseDbContext finalizer log when the constructor threw before Logger was set ([172545e](https://github.com/senrecep/CSharpEssentials/commit/172545e3fbade6e3ebfc66a86840414df7c15f16))
* **entity:** mark package AOT compatible ([0278f9f](https://github.com/senrecep/CSharpEssentials/commit/0278f9f81b3cdf9fda42bd8ef97fb609d11f0e1f))
* **enums:** emit the ModuleInitializerAttribute polyfill as a bare partial class ([d2bed63](https://github.com/senrecep/CSharpEssentials/commit/d2bed6344cec581b068de4bd66405956ecfbf123))
* **enums:** keep WireNames and allowed values read-only and fast to index ([b46149c](https://github.com/senrecep/CSharpEssentials/commit/b46149c8f67dddbc927351250fbba805cd93130c))
* **enums:** stage TryFormatMany results unless the caller passes a List&lt;string&gt; ([d87da47](https://github.com/senrecep/CSharpEssentials/commit/d87da47979e968a33d0bb75927c1574fee6c3b75))
* **errors:** format ErrorMetadata.ToString invariantly on netstandard2.1 ([f08467f](https://github.com/senrecep/CSharpEssentials/commit/f08467f5ca152767887d343873393b36d2f35f07))
* **errors:** reject null Code and Description in with expressions ([c78a03e](https://github.com/senrecep/CSharpEssentials/commit/c78a03efcff5aeb937abd88b9017eeeedc2fcb79))
* **gcp-secret-manager:** reject a null or empty credentials path in CreateWithRegion ([820526b](https://github.com/senrecep/CSharpEssentials/commit/820526bde9e5b4defb29b54572a6fbe76941878d))
* **http:** add JsonTypeInfo overloads and mark package AOT compatible ([99f5caf](https://github.com/senrecep/CSharpEssentials/commit/99f5caf80a624c50d4994ea61ab33a233d52a7b0))
* **http:** dispose the response in the result extensions ([3825188](https://github.com/senrecep/CSharpEssentials/commit/3825188d83fb219b04691f8c47ec8f6ef964c39a))
* **http:** generate idempotency keys as random UUIDv4 ([5c41161](https://github.com/senrecep/CSharpEssentials/commit/5c41161fbaebdf5fddb8a6a61de94380b5851bfd))
* **http:** rename the builder JsonTypeInfo overload and test the JsonTypeInfo paths ([6b584eb](https://github.com/senrecep/CSharpEssentials/commit/6b584ebb031450c6291bd6ea3579a98e06b09a7f))
* **http:** wait for cancellation once around the GET request and body read ([a9b1bce](https://github.com/senrecep/CSharpEssentials/commit/a9b1bce26dc84df21f7f1c914e9591a9f13b5586))
* **idempotency-testing:** drop the AOT compatible flag ([94f930f](https://github.com/senrecep/CSharpEssentials/commit/94f930ff4551f38b7a169c07661b0e994cd23629))
* **idempotency-testing:** mark package AOT compatible ([d049673](https://github.com/senrecep/CSharpEssentials/commit/d049673463fada8ec75426a5be7a4b54ef87024b))
* **idempotency:** mark package AOT compatible ([c6b7195](https://github.com/senrecep/CSharpEssentials/commit/c6b7195654861f30cec63c0656464a7340c3a4fb))
* **json:** add a hash of the assembly name to the registry class name ([e1c87a5](https://github.com/senrecep/CSharpEssentials/commit/e1c87a5ab8f3e66b6907e4e19bf1d85171f0cc85))
* **json:** copy a chain-built resolver before wrapping it ([1607afb](https://github.com/senrecep/CSharpEssentials/commit/1607afba4dced6391a0331a16950a10ad1be603b))
* **json:** copy AllowOutOfOrderMetadataProperties in ApplyTo on netstandard2.1 ([a820ecf](https://github.com/senrecep/CSharpEssentials/commit/a820ecf0dff266147d268e18487966fc4aa4b8f8))
* **json:** emit the ModuleInitializerAttribute polyfill as a bare partial class ([8e26195](https://github.com/senrecep/CSharpEssentials/commit/8e261951db8381e2cd226cbaea481b9ce4476c99))
* **json:** keep the target's resolver in ApplyTo and add polymorphism to it ([62af21b](https://github.com/senrecep/CSharpEssentials/commit/62af21be99a952c0e638a1aad58b1b0e531d245f))
* **json:** read old CLR names nested in a base and from $type under another discriminator name ([4e4ff2d](https://github.com/senrecep/CSharpEssentials/commit/4e4ff2d0a926a8f93b35f9378d13097a2f0e6a4b))
* **json:** report CSE4005 as a warning in every project ([02532e2](https://github.com/senrecep/CSharpEssentials/commit/02532e2677d47641cebb605c86090be1fec438b5))
* **json:** report value as paramName for unsupported GetName value types ([41308b3](https://github.com/senrecep/CSharpEssentials/commit/41308b32b0cef3e7eebf644c5c6e2d639454060a))
* **json:** require digits after the dot in the ISO fast path ([6414de2](https://github.com/senrecep/CSharpEssentials/commit/6414de2af10d1ad135a7a1437f59db108b7e54fa))
* **json:** skip framework types in the polymorphic type scan ([6763bd9](https://github.com/senrecep/CSharpEssentials/commit/6763bd917ee6563e0b14d292348610cd36790f82))
* **json:** throw when polymorphic property or CLR-name converters meet preserved references ([f033314](https://github.com/senrecep/CSharpEssentials/commit/f033314c74330254bd48f336801ba7288f52b9ed))
* **json:** write undefined enum values as their number ([75c3b22](https://github.com/senrecep/CSharpEssentials/commit/75c3b22b94c438ef2c81bc2557e4559484223b81))
* **logging:** await the log writer so its failures reach the error path ([a26eba9](https://github.com/senrecep/CSharpEssentials/commit/a26eba9c84f0441649e1e87c12754f79964035f3))
* **logging:** hide Microsoft.IO.RecyclableMemoryStream from consumers' compile ([e97e7f8](https://github.com/senrecep/CSharpEssentials/commit/e97e7f850e3e639202b03d44833af71df27ac2b4))
* **logging:** log a handler failure on the error path instead of hiding it ([309e974](https://github.com/senrecep/CSharpEssentials/commit/309e97486cbd56bf0d04c57cd6ae3fb0b116ebbe))
* **logging:** log the captured response and rethrow when the client copy fails ([8b825af](https://github.com/senrecep/CSharpEssentials/commit/8b825af4e57d82f19dd3600c5174268a99f0b8d7))
* **logging:** make IgnorePaths("/") skip only the root path ([183b1ca](https://github.com/senrecep/CSharpEssentials/commit/183b1ca07798f743ee607b529a712b1107a7df4e))
* **logging:** run the UseHandler handler when the pipeline throws ([0862280](https://github.com/senrecep/CSharpEssentials/commit/08622806625af2def14176d53c4bd7ca6ff20c32))
* **logging:** stream the response to the client while capturing it ([275e572](https://github.com/senrecep/CSharpEssentials/commit/275e5722f7dedfe47ee972c543632372b686d6ed))
* **maybe:** observe the token in two ValueTask-source async overloads ([33b4ea5](https://github.com/senrecep/CSharpEssentials/commit/33b4ea59656694215427b581abacecd346a3e7d1))
* **mediator:** apply ForRequest settings changed after the first request ([293bebc](https://github.com/senrecep/CSharpEssentials/commit/293bebc3d2788d621582832dabb6728fff07dba8))
* **mediator:** read CacheKey once per request in CachingBehavior ([ea7e674](https://github.com/senrecep/CSharpEssentials/commit/ea7e674bef86ecd2f32ff51c9d56c4a0679fb154))
* **request-response-logging:** mark package AOT compatible ([6e01937](https://github.com/senrecep/CSharpEssentials/commit/6e019379fa2cc4e1b7555c8ce5806e0469f9c0af))
* **resilience:** annotate retry pipeline result type for trimming and mark package AOT compatible ([7bb3b55](https://github.com/senrecep/CSharpEssentials/commit/7bb3b5550a88fc78d49e3aba35926800ab8f985a))
* **resilience:** build retry pipelines without DynamicallyAccessedMembers.All ([e032593](https://github.com/senrecep/CSharpEssentials/commit/e032593f3b9d1655c0aafa97eab87891c6462f0f))
* **resilience:** classify a throwing fallback delegate as Unexpected ([506b455](https://github.com/senrecep/CSharpEssentials/commit/506b455826816aafff4502cd5209e044d62a5268))
* **rules:** mark package AOT compatible ([9ece118](https://github.com/senrecep/CSharpEssentials/commit/9ece1184b6d3e15c84b400e9ca9fdd12754f0a32))
* **swashbuckle:** call AddSwaggerGen from AddSwaggerEnumConventions only when it is missing ([2afa634](https://github.com/senrecep/CSharpEssentials/commit/2afa63486236c1ada72d76f971ff1ad6ef9798fc))
* **swashbuckle:** keep an object, array or dictionary contract of an enum unchanged ([b8ff7d7](https://github.com/senrecep/CSharpEssentials/commit/b8ff7d70e61bc6a479e7dd7bd0ee6182b2a803f7))
* **swashbuckle:** omit the default of an undefined enum value instead of failing the document ([56d0aab](https://github.com/senrecep/CSharpEssentials/commit/56d0aab470d62aeb04a85d998f37bd70e8319fab))
* **swashbuckle:** qualify schema ids with the namespace by default ([4b712b6](https://github.com/senrecep/CSharpEssentials/commit/4b712b6e927cc0a3f316842dd8169119e0fa130f))
* **these:** mark package AOT compatible ([c7130a8](https://github.com/senrecep/CSharpEssentials/commit/c7130a86939b8edb7b3f958130c0466a5222126b))
* **time:** mark package AOT compatible ([c1302d5](https://github.com/senrecep/CSharpEssentials/commit/c1302d56a642683fdae8f7375009df47879a6a80))


### Changed

* **any:** create Partition delegates once per call instead of per item ([3cc2030](https://github.com/senrecep/CSharpEssentials/commit/3cc20307153b55741ad40896c5c41cf441878ace))
* **aspnetcore:** read Result&lt;T&gt; in ResultEndpointFilter without reflection ([aaf2b4c](https://github.com/senrecep/CSharpEssentials/commit/aaf2b4cce041b0a2e3ff0baf9778e71b0dd2b07e))
* **aspnetcore:** stream the body-hash ETag into SHA-256 ([a9dfa0a](https://github.com/senrecep/CSharpEssentials/commit/a9dfa0a905538d57122b6b51f720688c67283966))
* **aspnetcore:** unwrap Result&lt;T&gt; in conditional GET without reflection ([391a4a3](https://github.com/senrecep/CSharpEssentials/commit/391a4a3482d3e170b94ef8be820254f9360e93ec))
* **bench:** add a Native AOT check for generated JSON polymorphism ([9086dcb](https://github.com/senrecep/CSharpEssentials/commit/9086dcb8aa3dbeb5b44848f003642e009bc6c746))
* **bench:** add any suite vs OneOf, Dunet and hand-written These baseline ([37e747c](https://github.com/senrecep/CSharpEssentials/commit/37e747c0fad48d7ff5a486073b30c37cb79bcf4e))
* **bench:** add aspnetcore suite vs Hellang.Middleware.ProblemDetails, Marvin.Cache.Headers, SharpGrip, IdempotentAPI, HttpLogging and Serilog.AspNetCore ([36042f1](https://github.com/senrecep/CSharpEssentials/commit/36042f12dac061a7247eb0823b69506b0f2d253c))
* **bench:** add baselines suite with Time clock and conversion classes and Clone collection class ([f395d35](https://github.com/senrecep/CSharpEssentials/commit/f395d358102339716cc112d3e6cdc398b9781417))
* **bench:** add BoxedFormatComparison and ReflectionInfoComparison to enums suite ([a2bf803](https://github.com/senrecep/CSharpEssentials/commit/a2bf8032ff31e79593c1ce9ce1dbdbd6f204dea1))
* **bench:** add Core GUID benchmarks for Guider.NewGuid and NewMonotonicGuid ([a9b25a6](https://github.com/senrecep/CSharpEssentials/commit/a9b25a60fcc3f7f69991f109a2040dc45847290f))
* **bench:** add efcore suite vs MR.EntityFrameworkCore.KeysetPagination, X.PagedList.EF and EntityFrameworkCore.Exceptions.Sqlite ([21e6414](https://github.com/senrecep/CSharpEssentials/commit/21e64142ccc6c953c0576294e7b0f85a114525f5))
* **bench:** add Error, Sequence, Try and Maybe collection classes to results-comparison suite ([ad7199c](https://github.com/senrecep/CSharpEssentials/commit/ad7199c3b5a672d32409d558c692c10067ef1b7a))
* **bench:** add generator and analyzer design-time classes to generators suite with parity self-check ([48857a6](https://github.com/senrecep/CSharpEssentials/commit/48857a68f43ec572cea612ee97c1adf3a5dff56a))
* **bench:** add http suite vs Refit, Flurl.Http, RestSharp, Polly.Core and Microsoft.Extensions.Http.Resilience ([06599cf](https://github.com/senrecep/CSharpEssentials/commit/06599cfa57ebbfa47aaccf73bdb9abc5d9dd994f))
* **bench:** add json suite vs System.Text.Json, Macross.Json.Extensions, Humanizer and ShortGuid ([8267cc4](https://github.com/senrecep/CSharpEssentials/commit/8267cc40b19467491d30a9177ea5f1d588f99e2a))
* **bench:** add OpenApi and Swashbuckle document generation suites with parity self-check ([954d563](https://github.com/senrecep/CSharpEssentials/commit/954d563c2548fa920b72f2f203641eeb9309ae2b))
* **bench:** add operation naming and empty-prefix group classes to endpoints suite ([31f1696](https://github.com/senrecep/CSharpEssentials/commit/31f1696b2c316a10cc8c320bb1cd6255ac2cfa66))
* **bench:** add rules suite vs NRules and RulesEngine ([47bfe8f](https://github.com/senrecep/CSharpEssentials/commit/47bfe8fcc9ccef3a302fcf64789e67136572145d))
* **bench:** add TryAdd and transient decorator classes to di suite ([679cd77](https://github.com/senrecep/CSharpEssentials/commit/679cd77262bdda0fd8dd12655999655b04751418))
* **bench:** add validation suite vs FluentValidation, DataAnnotations and Microsoft.Extensions.Validation ([c0436fb](https://github.com/senrecep/CSharpEssentials/commit/c0436fba0c6a9c9d4c4a21056bd54b0aa6e79b64))
* **bench:** add validation, idempotency, lock, caching and logging behavior classes to mediator suite ([60135d6](https://github.com/senrecep/CSharpEssentials/commit/60135d6a29f5fc7b6f5f2899c544d270ea99c389))
* **bench:** move the JSON AOT sample events into their own library ([8d91144](https://github.com/senrecep/CSharpEssentials/commit/8d91144bd973a76987bc57f919c7e2aacc7f8b2c))
* **clone:** call Clone through a static lambda instead of a generic method group ([df266a2](https://github.com/senrecep/CSharpEssentials/commit/df266a237ff030179ef2ee89ff3de3b1ccbbd39b))
* **core:** build version 7 GUIDs from a per-thread random pool on every target ([30e32e0](https://github.com/senrecep/CSharpEssentials/commit/30e32e0bbe02f31265f8b135407e785396d45de2))
* **core:** convert case in one pass with an ASCII fast path ([f756bd0](https://github.com/senrecep/CSharpEssentials/commit/f756bd053e24711894cb536e40ea5561b1e5017d))
* **core:** keep the case conversion loop free of calls ([2304aad](https://github.com/senrecep/CSharpEssentials/commit/2304aad64646dcb0d8424c9fa6ddae3e3e84d5ec))
* **core:** keep the NewMonotonicGuid shared value on its own cache line ([981a9b0](https://github.com/senrecep/CSharpEssentials/commit/981a9b0c697bb66fac0d9a6e95380e9ba2beb279))
* **core:** use a partial Fisher-Yates shuffle in GetRandomItems for large counts ([3c18fb2](https://github.com/senrecep/CSharpEssentials/commit/3c18fb2964c9b444ff816201c8f349f389dd7328))
* **core:** use Base64Url for Guider encode and decode on net9.0+ ([c24df81](https://github.com/senrecep/CSharpEssentials/commit/c24df81e01fa8779d043fe167379321dfb59235b))
* **core:** use Task.WaitAsync in WithCancellation on .NET 9 and later ([35e4548](https://github.com/senrecep/CSharpEssentials/commit/35e45481370028e7efab3108b3d2ebd4143d84dc))
* **di:** invoke runtime decorator constructors without an argument array ([2f86499](https://github.com/senrecep/CSharpEssentials/commit/2f8649986b243d86454adbe25b414914d2aed444))
* **di:** register the hidden inner of a scoped decoration as transient ([b7072f4](https://github.com/senrecep/CSharpEssentials/commit/b7072f442c3677a20477a85d4ab8e84a689fe39b))
* **di:** scan service interfaces without LINQ and hoist the TryAdd lookup ([75e18ab](https://github.com/senrecep/CSharpEssentials/commit/75e18ab84a3bac8d28c305be09eb2d422288ad6e))
* **di:** scan the collection by index when checking TryAdd and Skip duplicates ([b94d9a2](https://github.com/senrecep/CSharpEssentials/commit/b94d9a222a6792064af3da7d7e8050c1680fc602))
* **efcore:** cache stored names in LegacySnakeCaseEnumConverter ([35032db](https://github.com/senrecep/CSharpEssentials/commit/35032dbd2350e19f323f71b6f1b46d7535efe23d))
* **efcore:** compile MigrateDataOptions key selectors once per options instance ([63c7762](https://github.com/senrecep/CSharpEssentials/commit/63c7762b1c7a6efd2db9699f6be6312951dd2f48))
* **efcore:** create the AuditInterceptor scope only when a save has entries to stamp ([18744ea](https://github.com/senrecep/CSharpEssentials/commit/18744eaf2184b2d64fe8585dbbbc08b31b670979))
* **efcore:** diff MigrateDataAsync keys through hash sets ([b0bd735](https://github.com/senrecep/CSharpEssentials/commit/b0bd735ea647a0870ef0484bd5c12cf209d5ff3b))
* **efcore:** skip the BaseDbContext instance id when Information logging is off ([b8252e0](https://github.com/senrecep/CSharpEssentials/commit/b8252e01f0fe06939f5508f8c01d1e1cc0c684be))
* **efcore:** try an exact match first when reading legacy enum storage ([cd0c7ae](https://github.com/senrecep/CSharpEssentials/commit/cd0c7aef9d2bc353789ba35143cbd6d30314479d))
* **endpoints:** narrow generator discovery to types that declare static Map ([dc7e648](https://github.com/senrecep/CSharpEssentials/commit/dc7e64890fddacb532a1b2eec24d4189ab8c65dd))
* **endpoints:** read literal WithName arguments without the semantic model ([19e6007](https://github.com/senrecep/CSharpEssentials/commit/19e600702afaff64ab86d210fb56949a3c3be3c7))
* **endpoints:** read resolved operation names without taking the lock ([5a8f63b](https://github.com/senrecep/CSharpEssentials/commit/5a8f63b7efa68ab22ec5f4c3c37a8fdacd11ee7f))
* **entity:** return a cached read-only view from EntityBase.DomainEvents ([8ea5e32](https://github.com/senrecep/CSharpEssentials/commit/8ea5e323bf348b0576d7e098660188866071df96))
* **enums:** compare names ordinally before ignoring case ([515d48d](https://github.com/senrecep/CSharpEssentials/commit/515d48dc577687715bcfe42f2d730a73be41f91b))
* **enums:** drop per-call registry and array dispatch costs from metadata reads ([41fc378](https://github.com/senrecep/CSharpEssentials/commit/41fc3781c4e339741766de47ce23f7b9b49d57f6))
* **enums:** format boxed enum values without a visitor or a staging list ([cbac945](https://github.com/senrecep/CSharpEssentials/commit/cbac945a59c73e7314296d68cc70cb52be240342))
* **enums:** let WireNames and allowed values copy in bulk ([4fe8144](https://github.com/senrecep/CSharpEssentials/commit/4fe8144b56f4d0ec4682805f456b7ece695adffe))
* **enums:** look up the registry once when formatting with the default CanHandle ([97ae9c9](https://github.com/senrecep/CSharpEssentials/commit/97ae9c98a59bc990c328bb3ae0ef0e09d29497f2))
* **enums:** serve cached reflection metadata without probing the module initializer ([6caebe2](https://github.com/senrecep/CSharpEssentials/commit/6caebe2c96e50be693b23910bdee81fda2f86015))
* **errors:** enumerate ErrorMetadata once in ToString ([cf4778f](https://github.com/senrecep/CSharpEssentials/commit/cf4778fbe8efeacd8690cebf62d6222c99dcf800))
* **errors:** format ErrorMetadata.ToString straight into one builder ([36d39db](https://github.com/senrecep/CSharpEssentials/commit/36d39db4c6f9b3ad6e79aa116a835e1870787db3))
* **errors:** stop hashing Error's type twice and simplify the metadata hash ([9f31471](https://github.com/senrecep/CSharpEssentials/commit/9f31471c348fd084fddd8bd50aee276c562e1d14))
* **errors:** stop walking the call stack for unthrown exceptions ([bdf3bae](https://github.com/senrecep/CSharpEssentials/commit/bdf3bae3603b42c80b2159e8736470c3ddadb8ee))
* **http:** drop the TryAsync closure from the GET overloads ([cded5e3](https://github.com/senrecep/CSharpEssentials/commit/cded5e3042c9fc8d06a66884de4ef527c229c00d))
* **http:** parse the request URI once when routes and query are both set ([927ced7](https://github.com/senrecep/CSharpEssentials/commit/927ced715e59c46baeaadc690c8544fd3edd8d48))
* **idempotency:** look up the key before building a reservation in InMemoryIdempotencyRecordStore ([9505c72](https://github.com/senrecep/CSharpEssentials/commit/9505c72f19352956105a0148b54416589b4a8f70))
* **idempotency:** stop copying the response body on capture, store and replay ([3dbd314](https://github.com/senrecep/CSharpEssentials/commit/3dbd314747a1d3f330973402f785c4f6cda5a547))
* **json:** check for framework assemblies once per assembly in the scan ([ef19fbe](https://github.com/senrecep/CSharpEssentials/commit/ef19fbea88fbf37f234e5a1abb387d431c9700b2))
* **json:** look up a non-flags enum dictionary key once per write ([e13198a](https://github.com/senrecep/CSharpEssentials/commit/e13198ac46485c1315e53ff6b609f022108b8473))
* **json:** look up a non-flags enum member once per write ([efdabb6](https://github.com/senrecep/CSharpEssentials/commit/efdabb630118a597d8af75c8422f0fa71bd7e016))
* **json:** read ISO 8601 dates without allocating a string ([ff56896](https://github.com/senrecep/CSharpEssentials/commit/ff56896415fd4335b86fabf095674918de0576ad))
* **json:** read the polymorphic discriminator without a JsonDocument ([1ce2a21](https://github.com/senrecep/CSharpEssentials/commit/1ce2a212ad6a15f1cb3e2eb6821124e2da633aa3))
* **json:** write enum names from pre-encoded JSON text ([005e3b5](https://github.com/senrecep/CSharpEssentials/commit/005e3b561b2efbb6992d11807d6944e770d89f73))
* **logging:** build the separate-context message template once ([8229399](https://github.com/senrecep/CSharpEssentials/commit/8229399a78aa69df7b0696f13d023f7c704a78a6))
* **logging:** capture bodies only when a logged field needs them ([ea14454](https://github.com/senrecep/CSharpEssentials/commit/ea14454c987b8878896edef8c08f47f028ab4fa9))
* **logging:** check IsEnabled before building the log message ([d41a6db](https://github.com/senrecep/CSharpEssentials/commit/d41a6dbfaa546394024e4a59883d6c7c0432da50))
* **logging:** decode captured bodies straight from pooled bytes ([c8eb6ab](https://github.com/senrecep/CSharpEssentials/commit/c8eb6ab0b90d375b36233c7e2fc326b67618cbe3))
* **logging:** read the request body once and keep it in memory ([d878f8a](https://github.com/senrecep/CSharpEssentials/commit/d878f8aad77b3a18c990bbe4840941d58c1f772d))
* **logging:** read the request body with a 4 KB stream reader buffer ([a2cc1dd](https://github.com/senrecep/CSharpEssentials/commit/a2cc1dddbd1c6585fe378f6c6d0e6b344d41f3fc))
* **maybe:** read TryLast from the end of indexable lists ([850451a](https://github.com/senrecep/CSharpEssentials/commit/850451a8d5d58e75f36f1b011bfa6035fe9f61a9))
* **maybe:** skip the enumerator in TryFirst for arrays and lists ([79eb20a](https://github.com/senrecep/CSharpEssentials/commit/79eb20a80bb08f44f2f84a76fe5f874d65f6b4a8))
* **mediator:** drop per-request token, message and closure allocations from idempotency ([a6c9260](https://github.com/senrecep/CSharpEssentials/commit/a6c92604e4b7a8470b7b17ca047672cd129b936d))
* **mediator:** hash the default idempotency fingerprint without buffering the request JSON ([efa10fa](https://github.com/senrecep/CSharpEssentials/commit/efa10fa9872df9c7ab23c7cd173c8b6e0cf708cf))
* **mediator:** read request and response type names once per closed LoggingBehavior ([2c71220](https://github.com/senrecep/CSharpEssentials/commit/2c712200563a1c03d9b089699b2e85a3ec57b718))
* **mediator:** share the idempotency settings of requests without their own retention or lease ([1c60ea1](https://github.com/senrecep/CSharpEssentials/commit/1c60ea15a6d97352efe489d33e487ff9dea85477))
* **mediator:** skip LINQ grouping when all validators share one order ([b137c15](https://github.com/senrecep/CSharpEssentials/commit/b137c15193f2e2ac61ff9d3d23712d98805dfefe))
* **mediator:** validate several validators without async state machines when they complete synchronously ([624ff72](https://github.com/senrecep/CSharpEssentials/commit/624ff72226589409e652e10549d5fd89c0eb21b1))
* **resilience:** pass the callback as Polly state instead of capturing it ([ce98fc4](https://github.com/senrecep/CSharpEssentials/commit/ce98fc4098a77af2cd8e7507d32a1c3bd1cb666d))
* **resilience:** reuse cached retry pipelines for predicate RetryIfFailed ([034cdfc](https://github.com/senrecep/CSharpEssentials/commit/034cdfc1c27187ccc6998f108993be240a44cc0d))
* **resilience:** start ResiliencePolicy&lt;T&gt; from the empty pipeline ([6d206d5](https://github.com/senrecep/CSharpEssentials/commit/6d206d5f45e80b3d1926be32bb041211cfc017fc))
* **rules:** aggregate And/Or errors without lists or boxing ([1832bb8](https://github.com/senrecep/CSharpEssentials/commit/1832bb8319c327e8ee48ddb10f7ef95182d47e3e))
* **rules:** evaluate sync rules without closures or LINQ ([670e1bf](https://github.com/senrecep/CSharpEssentials/commit/670e1bfd3ab9d3d9aeeab57e64b82e6935909080))
* **these:** partition without per-item delegates ([979038c](https://github.com/senrecep/CSharpEssentials/commit/979038c9cdf3515225c0ac4566bc87e458b396d5))


### Documentation

* **any:** note that source-generated contexts must register each branch type ([c58fbd7](https://github.com/senrecep/CSharpEssentials/commit/c58fbd7db134e44b9b2742dac0f94485f3e1cc76))
* **logging:** note that the first response write now starts the response ([a5db1a7](https://github.com/senrecep/CSharpEssentials/commit/a5db1a7451b31c85893568ac83f0995ac1917e45))
* **resilience:** note fallback timeout and circuit codes in the 8.0 upgrade notes ([6bc693d](https://github.com/senrecep/CSharpEssentials/commit/6bc693dd9d0ae87a99a80674d2c1e2f0165e441c))

## [7.0.0](https://github.com/senrecep/CSharpEssentials/compare/v6.5.2...v7.0.0) (2026-10-20)


### ⚠ BREAKING CHANGES

* **idempotency:** idempotency moves to the new CSharpEssentials.Idempotency core; see [the migration guide](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v6-to-v7-idempotency.md).
    - `IIdempotencyStore`, `IdempotencyReservation`, `IdempotencyReservationStatus`, `InMemoryIdempotencyStore` and `DistributedCacheIdempotencyStore` are removed from `CSharpEssentials.AspNetCore`; use `IIdempotencyRecordStore`, `InMemoryIdempotencyRecordStore` and `DistributedCacheIdempotencyRecordStore`.
    - `IdempotencyOptions.KeyScope` and `DefaultKeyScope` are removed; use `ScopeResolver` with `IdempotencyScopes`. Scopes are built only through the `IdempotencyScope` factories, and `AllowUnscopedKeys` is replaced by `UnscopedKeyBehavior`.
    - `IdempotencyOptions.ShouldStore` is a `Func<IdempotencyOutcome, bool>` instead of a `Func<int, bool>`; `IdempotencyStorePolicies.SuccessOnly` restores the 6.x rule.
    - `IdempotencyOptions.RetryAfter` is a non-nullable `TimeSpan`; the 409 always carries `Retry-After`.
    - A 5xx after the handler started is stored for `ServerErrorRetention`; 503, 504, exceptions and aborts after the handler started keep the key in flight until `InFlightTimeout` instead of releasing it.
    - A key must be printable ASCII other than space, `"` and `\`; other keys get 400 `Idempotency.KeyInvalid`.
    - `IdempotencyOptions` is resolved as a transient `CurrentValue`, and records use the `idem:v3:` storage key format, so 6.x entries are not read.

### Added

* **idempotency:** production-grade idempotency across http, mediator, messaging and ef core ([42287de](https://github.com/senrecep/CSharpEssentials/commit/42287de0e8a0663cd72c2d203e16315d44a12643))


### Fixed

* **idempotency:** store observe results in the business transaction ([#158](https://github.com/senrecep/CSharpEssentials/issues/158)) ([bdcbdd1](https://github.com/senrecep/CSharpEssentials/commit/bdcbdd15cbedda81a95e4e81754c0c594bfade8e))

## [6.5.2](https://github.com/senrecep/CSharpEssentials/compare/v6.5.1...v6.5.2) (2026-10-10)


### Fixed

* **time:** return utc kind from datetimeprovider.utcnowdatetime ([#147](https://github.com/senrecep/CSharpEssentials/issues/147)) ([83f7ff3](https://github.com/senrecep/CSharpEssentials/commit/83f7ff393c71390543636d70ff22a20abbc5f36e)), closes [#146](https://github.com/senrecep/CSharpEssentials/issues/146)

## [6.5.1](https://github.com/senrecep/CSharpEssentials/compare/v6.5.0...v6.5.1) (2026-10-09)


### Fixed

* **core:** bound stackalloc and validate guider input ([#142](https://github.com/senrecep/CSharpEssentials/issues/142)) ([2eddfba](https://github.com/senrecep/CSharpEssentials/commit/2eddfba5a4220efc54aa46d8b734517bfe686cc8)), closes [#136](https://github.com/senrecep/CSharpEssentials/issues/136)
* **http:** append query to relative uris without throwing ([#140](https://github.com/senrecep/CSharpEssentials/issues/140)) ([04106ee](https://github.com/senrecep/CSharpEssentials/commit/04106ee6514579a1a97047847d78d0eac52df523)), closes [#136](https://github.com/senrecep/CSharpEssentials/issues/136)
* **json:** return utc from multiformatdatetimeconverter ([#141](https://github.com/senrecep/CSharpEssentials/issues/141)) ([f754e54](https://github.com/senrecep/CSharpEssentials/commit/f754e54163bd2e531a256a80993070c3cbd9beb9)), closes [#136](https://github.com/senrecep/CSharpEssentials/issues/136)
* **rules:** keep every rule when chaining next calls ([#138](https://github.com/senrecep/CSharpEssentials/issues/138)) ([7399565](https://github.com/senrecep/CSharpEssentials/commit/7399565e76f53ed52f740a2fe9f9b0569b23be15)), closes [#136](https://github.com/senrecep/CSharpEssentials/issues/136)
* **these:** keep the error of both in flatmap ([#143](https://github.com/senrecep/CSharpEssentials/issues/143)) ([79f2aac](https://github.com/senrecep/CSharpEssentials/commit/79f2aac6fd8eb2a5bfdb09ba955ca44a48723088)), closes [#136](https://github.com/senrecep/CSharpEssentials/issues/136)
* **validation:** normalise property paths for filtered collections and null operators ([#144](https://github.com/senrecep/CSharpEssentials/issues/144)) ([e2760e0](https://github.com/senrecep/CSharpEssentials/commit/e2760e090921e43dbc798d4246c0f048a4ee645b)), closes [#136](https://github.com/senrecep/CSharpEssentials/issues/136)

## [6.5.0](https://github.com/senrecep/CSharpEssentials/compare/v6.4.0...v6.5.0) (2026-10-09)


### Added

* **maybe:** `Async`-suffixed names for the task-returning members left from 6.4.0: `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync`, `MatchAsync`, `ToMaybeResultAsync`, `ToMaybeUnitResultAsync` and `Maybe.FromAsync` ([2ec0851](https://github.com/senrecep/CSharpEssentials/commit/2ec0851325a24f46694a6dc56440c9e406997a0b)), closes [#121](https://github.com/senrecep/CSharpEssentials/issues/121)
* **results:** `ValueTask` twins on .NET 9+ for `ElseAsync`, `FailIfAsync`, `SwitchFirstAsync`, `SwitchLastAsync`, `TapErrorAsync`, `TapErrorFirstAsync`, `ElseDoAsync`, `ElseDoFirstAsync`, `CompensateAsync`, `CompensateFirstAsync`, `ThenEnsureAsync`, `FinallyAsync` (on `ValueTask` sources) and `Result.TryAsync` ([5f1a0db](https://github.com/senrecep/CSharpEssentials/commit/5f1a0dbcfaec2a2ab17a34b5e3c94688df9ea6cb)), closes [#122](https://github.com/senrecep/CSharpEssentials/issues/122)


### Fixed

* **results:** the conditional `TapAsync(bool | Func<bool>, …)` on `Task` and `ValueTask` sources has awaited `Func<Task>`/`Func<ValueTask>` handlers, so an `async` lambda no longer runs as `async void` ([4724c1f](https://github.com/senrecep/CSharpEssentials/commit/4724c1f918ace9256ff9dc8f9d4a3946407927d0)), closes [#124](https://github.com/senrecep/CSharpEssentials/issues/124)
* **http:** remove the pre-6.3.0 cancellation workaround; only the caller's cancellation throws, an `HttpClient.Timeout` or any other cancellation becomes a failed `Result` ([6e212c7](https://github.com/senrecep/CSharpEssentials/commit/6e212c73feba3e23e9cb75b6f4e8ac145725e033)), closes [#127](https://github.com/senrecep/CSharpEssentials/issues/127)
* add `ConfigureAwait(false)` to about 800 awaits across the library packages and enforce CA2007 for them ([c54f124](https://github.com/senrecep/CSharpEssentials/commit/c54f1240c0a1bcb653f2ff23f17e3c0240126c7a), [8e4c149](https://github.com/senrecep/CSharpEssentials/commit/8e4c149273e24b9a8795adcb08218308f3aeaaf4)), closes [#123](https://github.com/senrecep/CSharpEssentials/issues/123)


### Changed

* **results:** add Else and TapError async matrix benchmarks ([ba2d839](https://github.com/senrecep/CSharpEssentials/commit/ba2d8394857f87b1cc907f38d51e03ba3e34e693))
* **maybe:** document the null equality rule: a typed `null` means absence, `Equals(object?)` follows the BCL contract ([8a387ef](https://github.com/senrecep/CSharpEssentials/commit/8a387ef)), closes [#125](https://github.com/senrecep/CSharpEssentials/issues/125)
* **results:** document that `Result.And` short-circuits on the first failure and that `CombineAll` collects every error ([beca7d4](https://github.com/senrecep/CSharpEssentials/commit/beca7d4)), closes [#126](https://github.com/senrecep/CSharpEssentials/issues/126)


### Behaviour changes

* **http:** `HttpClient.Timeout` and any cancellation that is not the caller's token now return a failed `Result` instead of throwing `OperationCanceledException`. A timeout gives `Http.Timeout` (`ErrorType.Unexpected`), any other cancellation uses the exception type name as the code. Cancelling the caller's own token still throws. Code that wraps these calls in `catch (OperationCanceledException)` to handle timeouts must check the `Result` instead. Timeouts raised by Polly resilience handlers keep `TimeoutRejectedException` as the code.
* **results:** an `async` lambda passed to the conditional `TapAsync` on a `Task` or `ValueTask` source is now awaited and its exception reaches the caller; it used to run as `async void`. A non-`async` lambda that returns the source's own flavour (`Task` on a `Task` source, `ValueTask` on a `ValueTask` source) is awaited too.
* **results:** on .NET 9+, `SwitchFirstAsync` and `SwitchLastAsync` on `ValueTask` sources await lambdas that return a `ValueTask`; they used to bind the `Action` pair and discard them. `FinallyAsync` on `ValueTask` sources now awaits a lambda that returns `ValueTask`, `ValueTask<T>` or `Task<T>` and returns `ValueTask<Result…>` or `ValueTask<T>`, where it used to return a nested task.
* the library no longer resumes continuations on the caller's `SynchronizationContext`, so sync-over-async callers on a single-threaded context no longer deadlock inside library code.


### Deprecations

* **maybe:** the unsuffixed task-returning `Execute`, `ExecuteNoValue`, `Or`, `Match`, `ToMaybeResult`, `ToMaybeUnitResult` and `Maybe.From(Task…)` overloads, and the matching async members of `IMaybe<T>`, are `[Obsolete]` forwarders to the `*Async` names and are removed in 7.0. Projects with `TreatWarningsAsErrors` will see CS0618 — see `docs/migration/v6-async-naming.md`.


### Source compatibility

* .NET 9+ consumers that pin `LangVersion` 12 get CS0121 for untyped async lambdas on the new `ValueTask` twins. The `Maybe` instance and key/value `ExecuteAsync`, `ExecuteNoValueAsync`, `OrAsync` and `MatchAsync` were already ambiguous on every language version; they now bind the `Task` overload on C# 13+ and still need a typed handler on C# 12, on .NET 8 as well. No binary breaks.

## [6.4.0](https://github.com/senrecep/CSharpEssentials/compare/v6.3.0...v6.4.0) (2026-10-08)


### Added

* **results:** add Async-suffixed names for task-returning members ([18e8b75](https://github.com/senrecep/CSharpEssentials/commit/18e8b75fda126cf3b8cc88b8b93fa26ff352abf0)), closes [#108](https://github.com/senrecep/CSharpEssentials/issues/108)
* **results:** add sequential TraverseAsync and SequenceAsync ([dc9fbcc](https://github.com/senrecep/CSharpEssentials/commit/dc9fbcc22666e08354927ae857fc556c95685bd2)), closes [#108](https://github.com/senrecep/CSharpEssentials/issues/108)
* **results:** fill flavour-matched async matrix ([e8f4f49](https://github.com/senrecep/CSharpEssentials/commit/e8f4f49ab413ba9cdf6a5f06e5fe455b6a6d29a3)), closes [#108](https://github.com/senrecep/CSharpEssentials/issues/108)


### Fixed

* **maybe:** strict equality with T, cancellation-safe FromTry, reflection-free ToString ([244d7fd](https://github.com/senrecep/CSharpEssentials/commit/244d7fd594fa2f3e3fead8e35ab4a906d3503180)), closes [#109](https://github.com/senrecep/CSharpEssentials/issues/109)
* **results:** cancel and observe pending tasks in SequenceAsync ([cf5b8da](https://github.com/senrecep/CSharpEssentials/commit/cf5b8da55cb20c74c5b9c0b23770731158ecc7ff)), closes [#108](https://github.com/senrecep/CSharpEssentials/issues/108)
* **results:** clarify async naming docs and deprecate interface Bind overloads ([ffbcae4](https://github.com/senrecep/CSharpEssentials/commit/ffbcae4c65a457b5ac8917aef862ff9c87e1eacd)), closes [#108](https://github.com/senrecep/CSharpEssentials/issues/108)


### Changed

* **results:** add async matrix benchmarks ([7a7a44b](https://github.com/senrecep/CSharpEssentials/commit/7a7a44b3d1c9e95a959c1904e5fafbf6f1c3a840))


### Behaviour changes

* **maybe:** equality with a raw value is strict — a non-null value never equals `None` (`Maybe<int>.None == 0` is now `false`); `None == null` is `true` in either operand order, `Some(x) == null` is `false`.
* **maybe:** `FromTry` no longer swallows `OperationCanceledException`; cancellation propagates to the caller.
* **maybe:** `ToString()` returns `Some(value)` / `None` (invariant culture, no reflection/JSON).
* **results:** `TapAsync` with an async lambda on a `Task`/`ValueTask` source result is now awaited (previously ran as `async void`, losing exceptions); a non-async Task-returning lambda on a `Task` source is now awaited too.
* **results:** `ValueTask<Result<T>>.MapAsync(async …)` now returns `ValueTask<Result<U>>` instead of `ValueTask<Result<Task<U>>>`.
* **results:** on .NET 9+, a `ValueTask`-returning lambda binds to the new `ValueTask` handler instead of the sync/`Action` overload.


### Deprecations

* **results:** unsuffixed task-returning members (`Bind`, `Else`, `FailIf`, `Match*`, `Switch*`, `Then`, `ThenDo`, Maybe `Where`) are `[Obsolete]` forwarders to their `*Async` names, and the Task/ValueTask `Bind` members of `IResult`/`IResult<T>` are obsolete; all are removed in 7.0. Projects with `TreatWarningsAsErrors` will see CS0618 — see `docs/migration/v6-async-naming.md`.
* **maybe:** `IsNone` is obsolete; use `HasNoValue`.


### Source compatibility

* .NET 9+ consumers that pin `LangVersion` 12 get CS0121 for untyped async lambdas on `Match*`, `Switch`, `Ensure`, `TapIf`, `Then`, `ThenDo` and `TraverseAsync` (C# 12 ignores `OverloadResolutionPriority`). Use C# 13+ (the .NET 9 default) or a typed handler. No binary breaks.

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
