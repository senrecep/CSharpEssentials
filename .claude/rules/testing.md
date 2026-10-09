---
paths: ["**/*.Tests/**", "**/*Tests.cs", "**/*Test.cs", "**/Tests/**", "tests/**"]
---

## Testing Conventions

- Framework: xUnit. Arrange/Act/Assert pattern with a blank line between each section.
- Test method naming: `MethodName_Should_ExpectedBehavior_When_Condition` or `MethodName_Given_Condition_Returns_Expected`.
- One assertion concept per test. Use multiple `Assert` calls only when they verify the same behavior.
- Test both `.IsSuccess` and `.IsFailure` paths for Result types; test `.HasValue` and `.HasNoValue` for Maybe.
- Never commit skipped tests (`[Fact(Skip = ...)]`) or conditional skips.
- Tests must not depend on execution order.
- Use `FluentAssertions` if already present in the test project (`CSharpEssentials.Tests` references it); do not add it if absent.
