#!/usr/bin/env python3
"""Write the combinators of the typed Result<T, E1..E3> and their tests.

The hand-written core (ResultTE1.cs, ResultTE1E2.cs, ResultTE1E2E3.cs) holds the layout, the constructors, the conversions,
Match, Switch and ToResult. This script writes the rest of the member surface as partial struct files next to it, one file per
family and arity, and one test class per family and arity:

    CSharpEssentials.Results/Modules/ResultTE1E2.Bind.cs
    tests/CSharpEssentials.Results.Typed.Tests/Typed/Emitted/ResultTE1E2BindTests.cs

The names avoid *.g.cs and *.Generated.cs and the files carry no auto-generated header, so Roslyn treats them as normal source
and every analyzer runs on them. After writing, the script runs `dotnet format whitespace` on the written files. CI runs the
script and fails when the checked-in files differ.

Usage, from anywhere:
    python3 scripts/generate-typed-results.py [--no-format]
"""

import argparse
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
LIBRARY = ROOT / "CSharpEssentials.Results" / "Modules"
TESTS = ROOT / "tests" / "CSharpEssentials.Results.Typed.Tests" / "Typed" / "Emitted"
HEADER = "// Produced by scripts/generate-typed-results.py; do not edit."
MAX_SLOTS = 3
SUCCESS = "TypedResultLogic.SuccessTag"
DEFAULT_ARM = "_ => throw TypedResultLogic.DefaultInitialized(),"
DEFAULT_DOC = '<exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>'
ORDINALS = {1: "first", 2: "second", 3: "third"}


def slots(n: int, prefix: str = "E") -> list[str]:
    return [f"{prefix}{i}" for i in range(1, n + 1)]


def result(t: str, errors: list[str]) -> str:
    return f"Result<{t}, {', '.join(errors)}>"


def type_name(n: int) -> str:
    return "ResultT" + "".join(slots(n))


def tag(slot: int) -> int:
    return slot + 1


def without(errors: list[str], slot: int) -> list[str]:
    return [e for i, e in enumerate(errors, 1) if i != slot]


def removed_tag(slot: int, removed: int) -> int:
    return tag(slot) if slot < removed else tag(slot - 1)


def ref(name: str) -> str:
    return f'<typeparamref name="{name}"/>'


def method(doc: list[str], signature: str, constraints: list[str], body: list[str]) -> list[str]:
    lines = [f"    /// {line}" for line in doc]
    lines.append(f"    {signature}")
    lines += [f"        where {c} : notnull" for c in constraints]
    lines.append("    {")
    lines += [f"        {line}" if line else "" for line in body]
    lines.append("    }")
    return lines


def switch(arms: list[str]) -> list[str]:
    return ["return Tag switch", "{", *[f"    {arm}" for arm in arms], "};"]


def error_tags(n: int, skip: int = 0) -> str:
    return " or ".join(str(tag(i)) for i in range(1, n + 1) if i != skip)


# ---------------------------------------------------------------------------------------------------------------- library


def bind(n: int) -> list[str]:
    errors = slots(n)
    out = result("TOut", errors)
    members = [method(
        [
            "<summary>",
            "Calls <paramref name=\"bind\"/> with the success value and returns its result, which has the same error types.",
            "An error passes through in its slot and <paramref name=\"bind\"/> is not called.",
            "</summary>",
            '<typeparam name="TOut">The success value type of the returned result.</typeparam>',
            '<param name="bind">Called with the success value.</param>',
            '<returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>',
            DEFAULT_DOC,
        ],
        f"public {out} Bind<TOut>(Func<T, {out}> bind)",
        ["TOut"],
        switch([f"{SUCCESS} => bind(RawValue),", f"{error_tags(n)} => new(Tag, default!, RawError),", DEFAULT_ARM]),
    )]
    for m in range(1, MAX_SLOTS - n + 1):
        added = slots(m, "TError")
        inner = result("TOut", added)
        targets = ", ".join(str(tag(n + j)) for j in range(1, m + 1))
        members.append(method(
            [
                "<summary>",
                "Calls <paramref name=\"bind\"/> with the success value and returns its result with the error types of this result",
                "added in front: an error of <paramref name=\"bind\"/> moves to the matching added slot. An error of this result passes",
                "through in its slot and <paramref name=\"bind\"/> is not called. Use <c>Bind</c> when <paramref name=\"bind\"/> returns",
                "the same error types.",
                "</summary>",
                '<typeparam name="TOut">The success value type of the returned result.</typeparam>',
                *[f'<typeparam name="{a}">The {ORDINALS[j]} error type that <paramref name="bind"/> adds.</typeparam>' for j, a in enumerate(added, 1)],
                '<param name="bind">Called with the success value.</param>',
                '<returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>',
                DEFAULT_DOC,
            ],
            f"public {result('TOut', errors + added)} BindWiden<TOut, {', '.join(added)}>(Func<T, {inner}> bind)",
            ["TOut", *added],
            [
                f"if (Tag == {SUCCESS})",
                "{",
                f"    {inner} inner = bind(RawValue);",
                f"    return new(TypedResultLogic.Remap(inner.Tag, {targets}), inner.RawValue, inner.RawError);",
                "}",
                "",
                *switch([f"{error_tags(n)} => new(Tag, default!, RawError),", DEFAULT_ARM]),
            ],
        ))
    return join(members)


def map_(n: int) -> list[str]:
    errors = slots(n)
    return method(
        [
            "<summary>Replaces the success value with the return value of <paramref name=\"map\"/>. An error passes through in its slot.</summary>",
            '<typeparam name="TOut">The new success value type.</typeparam>',
            '<param name="map">Called with the success value.</param>',
            "<returns>A result that holds the mapped value or the same error.</returns>",
            '<exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>',
            DEFAULT_DOC,
        ],
        f"public {result('TOut', errors)} Map<TOut>(Func<T, TOut> map)",
        ["TOut"],
        switch([
            f"{SUCCESS} => new({SUCCESS}, TypedResultLogic.NotNull(map(RawValue), nameof(map)), null),",
            f"{error_tags(n)} => new(Tag, default!, RawError),",
            DEFAULT_ARM,
        ]),
    )


def ensure(n: int) -> list[str]:
    errors = slots(n)
    self_type = result("T", errors)
    members = []
    def choose(name: str, keep: str, fail: str) -> str:
        return f"predicate(RawValue) ? {keep} : {fail}" if name == "Ensure" else f"predicate(RawValue) ? {fail} : {keep}"

    for name, fails in (("Ensure", "false"), ("FailIf", "true")):
        verb = "does not hold" if name == "Ensure" else "holds"
        for i, e in enumerate(errors, 1):
            members.append(method(
                [
                    f"<summary>Turns a success into the error <paramref name=\"error\"/> in the {ref(e)} slot when <paramref name=\"predicate\"/> {verb}.",
                    "An error passes through and <paramref name=\"predicate\"/> is not called.</summary>",
                    '<param name="predicate">Called with the success value.</param>',
                    f'<param name="error">The error to return when <paramref name="predicate"/> returns <see langword="{fails}"/>.</param>',
                    "<returns>This result, or a result that holds <paramref name=\"error\"/>.</returns>",
                    f'<exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="{fails}"/>.</exception>',
                    DEFAULT_DOC,
                ],
                f"public {self_type} {name}(Func<T, bool> predicate, {e} error)",
                [],
                switch([
                    f"{SUCCESS} => {choose(name, 'this', f'new({tag(i)}, default!, TypedResultLogic.NotNull(error, nameof(error)))')},",
                    f"{error_tags(n)} => this,",
                    DEFAULT_ARM,
                ]),
            ))
        if n < MAX_SLOTS:
            members.append(method(
                [
                    "<summary>",
                    f"Turns a success into the error <paramref name=\"error\"/> in a new last slot when <paramref name=\"predicate\"/> {verb}.",
                    "An error passes through in its slot and <paramref name=\"predicate\"/> is not called. When <paramref name=\"error\"/> has",
                    "the exact type of an existing slot, the overload that keeps the error types is chosen instead.",
                    "</summary>",
                    '<typeparam name="TError">The added error type.</typeparam>',
                    '<param name="predicate">Called with the success value.</param>',
                    f'<param name="error">The error to return when <paramref name="predicate"/> returns <see langword="{fails}"/>.</param>',
                    "<returns>A result with <typeparamref name=\"TError\"/> added as the last slot.</returns>",
                    f'<exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="{fails}"/>.</exception>',
                    DEFAULT_DOC,
                ],
                f"public {result('T', errors + ['TError'])} {name}<TError>(Func<T, bool> predicate, TError error)",
                ["TError"],
                switch([
                    f"{SUCCESS} => {choose(name, f'new({SUCCESS}, RawValue, null)', f'new({tag(n + 1)}, default!, TypedResultLogic.NotNull(error, nameof(error)))')},",
                    f"{error_tags(n)} => new(Tag, default!, RawError),",
                    DEFAULT_ARM,
                ]),
            ))
    return join(members)


def map_error(n: int) -> list[str]:
    errors = slots(n)
    members = []
    for i, e in enumerate(errors, 1):
        replaced = ["TError" if x == e else x for x in errors]
        arms = [f"{SUCCESS} => new(Tag, RawValue, null),", f"{tag(i)} => new(Tag, default!, TypedResultLogic.NotNull(map(({e})RawError!), nameof(map))),"]
        if n > 1:
            arms.append(f"{error_tags(n, i)} => new(Tag, default!, RawError),")
        members.append(method(
            [
                f"<summary>Replaces the error in the {ref(e)} slot with the return value of <paramref name=\"map\"/>. Every other case passes through.</summary>",
                f'<typeparam name="TError">The error type that replaces {ref(e)}.</typeparam>',
                f'<param name="map">Called with the error of type {ref(e)}. Give its parameter type to choose the slot.</param>',
                "<returns>A result whose slot holds the mapped error, or the same case.</returns>",
                '<exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>',
                DEFAULT_DOC,
            ],
            f"public {result('T', replaced)} MapError<TError>(Func<{e}, TError> map)",
            ["TError"],
            switch([*arms, DEFAULT_ARM]),
        ))
    return join(members)


def removing_arms(n: int, removed: int, handled: str) -> list[str]:
    return [f"{tag(j)} => {handled if j == removed else f'new({removed_tag(j, removed)}, default!, RawError)'}," for j in range(1, n + 1)]


def recover(n: int) -> list[str]:
    errors = slots(n)
    members = []
    for i, e in enumerate(errors, 1):
        doc = [
            f"<summary>Turns the error in the {ref(e)} slot into a success value and removes the slot. Every other case passes through.</summary>",
            f'<param name="recover">Called with the error of type {ref(e)}. Give its parameter type to choose the slot.</param>',
        ]
        if n == 1:
            doc.append("<returns>The success value, or the return value of <paramref name=\"recover\"/>.</returns>")
            signature = f"public T Recover(Func<{e}, T> recover)"
            arms = [f"{SUCCESS} => RawValue,", f"{tag(i)} => TypedResultLogic.NotNull(recover(({e})RawError!), nameof(recover)),"]
        else:
            doc.append(f"<returns>A result without the {ref(e)} slot.</returns>")
            signature = f"public {result('T', without(errors, i))} Recover(Func<{e}, T> recover)"
            arms = [f"{SUCCESS} => new(Tag, RawValue, null),", *removing_arms(n, i, f"new({SUCCESS}, TypedResultLogic.NotNull(recover(({e})RawError!), nameof(recover)), null)")]
        doc += ['<exception cref="ArgumentNullException"><paramref name="recover"/> returns <see langword="null"/>.</exception>', DEFAULT_DOC]
        members.append(method(doc, signature, [], switch([*arms, DEFAULT_ARM])))
    return join(members)


def compensate(n: int) -> list[str]:
    errors = slots(n)
    members = []
    for i, e in enumerate(errors, 1):
        rest = errors if n == 1 else without(errors, i)
        out = result("T", rest)
        if n == 1:
            summary = [
                "<summary>Replaces the error with the result of <paramref name=\"compensate\"/>, which can fail again with the same error type.",
                "A success passes through.</summary>",
            ]
            arms = [f"{SUCCESS} => this,"]
        else:
            summary = [
                f"<summary>Replaces the error in the {ref(e)} slot with the result of <paramref name=\"compensate\"/> and removes the slot.",
                "Every other case passes through.</summary>",
            ]
            arms = [f"{SUCCESS} => new(Tag, RawValue, null),", *removing_arms(n, i, f"compensate(({e})RawError!)")]
        if n == 1:
            arms.append(f"{tag(i)} => compensate(({e})RawError!),")
        members.append(method(
            [
                *summary,
                f'<param name="compensate">Called with the error of type {ref(e)}. Give its parameter type to choose the slot.</param>',
                f"<returns>The result of <paramref name=\"compensate\"/>, or the same case.</returns>",
                DEFAULT_DOC,
            ],
            f"public {out} Compensate(Func<{e}, {out}> compensate)",
            [],
            switch([*arms, DEFAULT_ARM]),
        ))
    return join(members)


def join(members: list[list[str]]) -> list[str]:
    lines: list[str] = []
    for member in members:
        if lines:
            lines.append("")
        lines += member
    return lines


FAMILIES = {"Bind": bind, "Map": map_, "Ensure": ensure, "MapError": map_error, "Recover": recover, "Compensate": compensate}


def library_file(n: int, body: list[str]) -> str:
    return "\n".join([
        HEADER,
        "namespace CSharpEssentials.ResultPattern;",
        "",
        f"public readonly partial struct {result('T', slots(n))}",
        "{",
        *body,
        "}",
        "",
    ])


# ---------------------------------------------------------------------------------------------------------------- tests

SLOT_TYPES = ["NotFoundError", "ConflictError", "RateLimited"]
ADDED_TYPES = ["ValidationError", "PlainError"]
FIXTURE = {
    "NotFoundError": "TypedFixture.NotFound",
    "ConflictError": "TypedFixture.Conflict",
    "RateLimited": "TypedFixture.Limited",
    "ValidationError": "TypedFixture.Invalid",
    "PlainError": "TypedFixture.Plain",
}
STRUCTS = {"RateLimited"}
OK, FAILING_OK, OTHER, RECOVERED = "5", "-5", "TypedFixture.Text(5)", "9"


class Test:
    def __init__(self, name: str, source_type: str, source: str, call: str, result_type: str | None, expected: str | None, calls: int, held: str | None = None, throws: str | None = None):
        self.name, self.source_type, self.source, self.call = name, source_type, source, call
        self.result_type, self.expected, self.calls, self.held, self.throws = result_type, expected, calls, held, throws

    def lines(self) -> list[str]:
        body = [f"{self.source_type} source = {self.source};", "int calls = 0;", ""]
        if self.throws:
            body += [f"Action act = () => _ = source.{self.call};", "", f"act.Should().Throw<{self.throws}>();", f"calls.Should().Be({self.calls});"]
        else:
            body += [f"{self.result_type} result = source.{self.call};", "", f"calls.Should().Be({self.calls});"]
            if self.result_type == "int":
                body.append(f"result.Should().Be({self.expected});")
            else:
                body += [f"{self.result_type} expected = {self.expected};", "result.Should().Be(expected);"]
            if self.held:
                check = "Be" if self.held in STRUCTS else "BeSameAs"
                body.append(f"result.Value.Should().{check}({FIXTURE[self.held]});")
        return ["    [Fact]", f"    public void {self.name}()", "    {", *[f"        {b}" if b else "" for b in body], "    }"]


def case_name(error: str) -> str:
    return f"TheResultHolds{error}"


def source_cases(errors: list[str]) -> list[tuple[str, str]]:
    return [(case_name(e), FIXTURE[e]) for e in errors]


def counted(body: str) -> str:
    return f"{{ calls++; return {body}; }}"


def bind_tests(n: int) -> list[Test]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    out = result("string", errors)
    call = lambda inner: f"Bind<string>(v => {counted(inner)})"  # noqa: E731
    tests = [
        Test("Bind_Should_ReturnTheInnerResult_When_TheResultHoldsAValue", src, OK, call("TypedFixture.Text(v)"), out, OTHER, 1),
        Test("Bind_Should_ReturnTheInnerError_When_TheInnerResultFails", src, OK, call(FIXTURE[errors[-1]]), out, FIXTURE[errors[-1]], 1, errors[-1]),
    ]
    for (name, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"Bind_Should_PassTheErrorThrough_When_{name}", src, value, call("TypedFixture.Text(v)"), out, value, 0, e))
    tests.append(Test("Bind_Should_Throw_When_TheResultIsDefault", src, "default", call("TypedFixture.Text(v)"), None, None, 0, throws="InvalidOperationException"))
    for m in range(1, MAX_SLOTS - n + 1):
        added = ADDED_TYPES[:m]
        inner = result("string", added)
        widened = result("string", errors + added)
        args = ", ".join(["string", *added])
        widen = lambda body: f"BindWiden<{args}>(v => {counted(body)})"  # noqa: E731
        prefix = f"BindWiden{m}"
        tests.append(Test(f"{prefix}_Should_ReturnTheInnerValue_When_TheResultHoldsAValue", src, OK, widen("TypedFixture.Text(v)"), widened, OTHER, 1))
        for a in added:
            tests.append(Test(f"{prefix}_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHolds{a}", src, OK, widen(f"new {inner}({FIXTURE[a]})"), widened, FIXTURE[a], 1, a))
        for (name, value), e in zip(source_cases(errors), errors):
            tests.append(Test(f"{prefix}_Should_PassTheErrorThrough_When_{name}", src, value, widen("TypedFixture.Text(v)"), widened, value, 0, e))
        tests.append(Test(f"{prefix}_Should_Throw_When_TheResultIsDefault", src, "default", widen("TypedFixture.Text(v)"), None, None, 0, throws="InvalidOperationException"))
    return tests


def map_tests(n: int) -> list[Test]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    out = result("string", errors)
    call = f"Map(v => {counted('TypedFixture.Text(v)')})"
    tests = [Test("Map_Should_MapTheValue_When_TheResultHoldsAValue", src, OK, call, out, OTHER, 1)]
    for (name, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"Map_Should_PassTheErrorThrough_When_{name}", src, value, call, out, value, 0, e))
    tests.append(Test("Map_Should_Throw_When_TheResultIsDefault", src, "default", call, None, None, 0, throws="InvalidOperationException"))
    tests.append(Test("Map_Should_Throw_When_TheMapReturnsNull", src, OK, f"Map<string>(v => {counted('null!')})", None, None, 1, throws="ArgumentNullException"))
    return tests


def ensure_tests(n: int) -> list[Test]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests = []
    for name, predicate in (("Ensure", "v > 0"), ("FailIf", "v < 0")):
        targets = [(f"{name}{e}", e, src) for e in errors]
        if n < MAX_SLOTS:
            targets.append((f"{name}Added", "ValidationError", result("int", errors + ["ValidationError"])))
        for prefix, error, out in targets:
            call = f"{name}(v => {counted(predicate)}, {FIXTURE[error]})"
            tests.append(Test(f"{prefix}_Should_KeepTheValue_When_ThePredicateAllowsIt", src, OK, call, out, OK, 1))
            tests.append(Test(f"{prefix}_Should_ReturnTheError_When_ThePredicateRejectsTheValue", src, FAILING_OK, call, out, FIXTURE[error], 1, error))
            for (case, value), e in zip(source_cases(errors), errors):
                tests.append(Test(f"{prefix}_Should_PassTheErrorThrough_When_{case}", src, value, call, out, value, 0, e))
            tests.append(Test(f"{prefix}_Should_Throw_When_TheResultIsDefault", src, "default", call, None, None, 0, throws="InvalidOperationException"))
    return tests


def map_error_tests(n: int) -> list[Test]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests = []
    for e in errors:
        out = result("int", ["ValidationError" if x == e else x for x in errors])
        call = f"MapError(({e} e) => {counted(FIXTURE['ValidationError'])})"
        prefix = f"MapError{e}"
        tests.append(Test(f"{prefix}_Should_KeepTheValue_When_TheResultHoldsAValue", src, OK, call, out, OK, 0))
        for (case, value), x in zip(source_cases(errors), errors):
            if x == e:
                tests.append(Test(f"{prefix}_Should_MapTheError_When_{case}", src, value, call, out, FIXTURE["ValidationError"], 1, "ValidationError"))
            else:
                tests.append(Test(f"{prefix}_Should_PassTheErrorThrough_When_{case}", src, value, call, out, value, 0, x))
        tests.append(Test(f"{prefix}_Should_Throw_When_TheResultIsDefault", src, "default", call, None, None, 0, throws="InvalidOperationException"))
    tests.append(Test("MapError_Should_Throw_When_TheMapReturnsNull", src, FIXTURE[errors[0]], f"MapError(({errors[0]} e) => {counted('(ValidationError)null!')})", None, None, 1, throws="ArgumentNullException"))
    return tests


def recover_tests(n: int) -> list[Test]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests = []
    for e in errors:
        out = "int" if n == 1 else result("int", [x for x in errors if x != e])
        call = f"Recover(({e} e) => {counted(RECOVERED)})"
        prefix = f"Recover{e}"
        tests.append(Test(f"{prefix}_Should_KeepTheValue_When_TheResultHoldsAValue", src, OK, call, out, OK, 0))
        for (case, value), x in zip(source_cases(errors), errors):
            if x == e:
                tests.append(Test(f"{prefix}_Should_ReturnTheRecoveredValue_When_{case}", src, value, call, out, RECOVERED, 1))
            else:
                tests.append(Test(f"{prefix}_Should_MoveTheErrorToItsNewSlot_When_{case}", src, value, call, out, value, 0, x))
        tests.append(Test(f"{prefix}_Should_Throw_When_TheResultIsDefault", src, "default", call, None, None, 0, throws="InvalidOperationException"))
    tests.append(Test("Recover_Should_Throw_When_TheRecoveryReturnsNull", result("string", errors), FIXTURE[errors[0]], f"Recover(({errors[0]} e) => {counted('(string)null!')})", None, None, 1, throws="ArgumentNullException"))
    return tests


def compensate_tests(n: int) -> list[Test]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests = []
    for e in errors:
        rest = errors if n == 1 else [x for x in errors if x != e]
        out = result("int", rest)
        call = lambda body, e=e: f"Compensate(({e} e) => {counted(body)})"  # noqa: E731
        prefix = f"Compensate{e}"
        tests.append(Test(f"{prefix}_Should_KeepTheValue_When_TheResultHoldsAValue", src, OK, call(RECOVERED), out, OK, 0))
        for (case, value), x in zip(source_cases(errors), errors):
            if x == e:
                tests.append(Test(f"{prefix}_Should_ReturnTheCompensatingValue_When_{case}", src, value, call(RECOVERED), out, RECOVERED, 1))
                tests.append(Test(f"{prefix}_Should_ReturnTheCompensatingError_When_{case}AndTheCompensationFails", src, value, call(f"new {out}({FIXTURE[rest[-1]]})"), out, FIXTURE[rest[-1]], 1, rest[-1]))
            else:
                tests.append(Test(f"{prefix}_Should_MoveTheErrorToItsNewSlot_When_{case}", src, value, call(RECOVERED), out, value, 0, x))
        tests.append(Test(f"{prefix}_Should_Throw_When_TheResultIsDefault", src, "default", call(RECOVERED), None, None, 0, throws="InvalidOperationException"))
    return tests


TEST_FAMILIES = {"Bind": bind_tests, "Map": map_tests, "Ensure": ensure_tests, "MapError": map_error_tests, "Recover": recover_tests, "Compensate": compensate_tests}


def test_file(n: int, family: str, tests: list[Test]) -> str:
    body: list[str] = []
    for t in tests:
        if body:
            body.append("")
        body += t.lines()
    return "\n".join([
        HEADER,
        "using CSharpEssentials.Errors;",
        "using CSharpEssentials.ResultPattern;",
        "using FluentAssertions;",
        "",
        "namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;",
        "",
        f"public sealed class {type_name(n)}{family}Tests",
        "{",
        *body,
        "}",
        "",
    ])


# ---------------------------------------------------------------------------------------------------------------- output


def write(files: dict[Path, str], directory: Path, pattern: str) -> None:
    directory.mkdir(parents=True, exist_ok=True)
    for stale in directory.glob(pattern):
        if stale not in files and stale.read_text(encoding="utf-8").startswith(HEADER):
            stale.unlink()
    for path, text in files.items():
        with path.open("w", encoding="utf-8", newline="\n") as handle:
            handle.write(text)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--no-format", action="store_true", help="skip dotnet format whitespace")
    args = parser.parse_args()

    library = {LIBRARY / f"{type_name(n)}.{family}.cs": library_file(n, emit(n)) for n in range(1, MAX_SLOTS + 1) for family, emit in FAMILIES.items()}
    tests = {TESTS / f"{type_name(n)}{family}Tests.cs": test_file(n, family, emit(n)) for n in range(1, MAX_SLOTS + 1) for family, emit in TEST_FAMILIES.items()}
    write(library, LIBRARY, "ResultTE*.cs")
    write(tests, TESTS, "*.cs")

    if not args.no_format:
        paths = [str(p.relative_to(ROOT)) for p in [*library, *tests]]
        subprocess.run(["dotnet", "format", "whitespace", ".", "--folder", "--include", *paths], cwd=ROOT, check=True)

    count = sum(text.count("    public ") for text in library.values())
    facts = sum(text.count("[Fact]") for text in tests.values())
    print(f"wrote {len(library)} library files ({count} members) and {len(tests)} test files ({facts} tests)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
