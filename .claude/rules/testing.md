---
paths: ["**/*.Tests/**", "**/*Tests.cs", "**/*Test.cs", "**/Tests/**"]
---

## Testing Conventions

- Framework: xUnit. Arrange/Act/Assert pattern with a blank line between each section.
- Test method naming: `MethodName_Should_ExpectedBehavior_When_Condition` or `MethodName_Given_Condition_Returns_Expected`.
- One assertion concept per test. Use multiple `Assert` calls only when they verify the same behavior.
- Test both `.IsSuccess` and `.IsFailure` paths for Result types; test `.HasValue` and `.HasNoValue` for Maybe.
- Never use `it.Skip` or conditional skips in committed tests.
- Tests must not depend on execution order.
- Use `FluentAssertions` if already present in the test project; do not add it if absent.
