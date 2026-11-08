---
name: testing-conventions
description: Unit testing conventions for CSharpEssentials. Use when writing, reviewing, or modifying tests.
---

## Testing conventions

- Framework: xUnit. Arrange/Act/Assert pattern with a blank line between each section.
- Test method naming: `MethodName_Should_ExpectedBehavior_When_Condition` or `MethodName_WithCondition_ShouldExpectedBehavior`.
- One assertion concept per test. Use multiple `Assert` calls only when they verify the same behavior.
- Test both `.IsSuccess` and `.IsFailure` paths for Result types; test `.HasValue` and `.HasNoValue` for Maybe.
- Never commit skipped tests (`[Fact(Skip = ...)]`) or conditional skips.
- Tests must not depend on execution order.
- Use `FluentAssertions` if the test project already references it (`CSharpEssentials.Tests` does); do not add it where it is absent.
