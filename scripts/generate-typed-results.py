#!/usr/bin/env python3
"""Write the combinators of the typed Result<T, E1..E3> and their tests.

The hand-written core (ResultTE1.cs, ResultTE1E2.cs, ResultTE1E2E3.cs) holds the layout, the constructors, the conversions,
Match, Switch and ToResult. This script writes the rest of the member surface as partial struct files next to it, one file per
family and arity, and a sync and an async test class per family and arity:

    CSharpEssentials.Results/Modules/ResultTE1E2.Bind.cs
    tests/CSharpEssentials.Results.Typed.Tests/Typed/Emitted/ResultTE1E2BindTests.cs
    tests/CSharpEssentials.Results.Typed.Tests/Typed/Emitted/ResultTE1E2BindAsyncTests.cs

A family file holds the synchronous members, their *Async instance members and the *Async extensions on Task and ValueTask
sources (in ResultExtensions). The async cells copy the matrix of Result<TValue> for the same family (see CELLS).

The names avoid *.g.cs and *.Generated.cs and the files carry no auto-generated header, so Roslyn treats them as normal source
and every analyzer runs on them. After writing, the script runs `dotnet format whitespace` on the written files. CI runs the
script and fails when the checked-in files differ.

Usage, from anywhere:
    python3 scripts/generate-typed-results.py [--no-format]
"""

import argparse
import re
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


def method(doc: list[str], signature: str, constraints: list[str], body: list[str], attributes: tuple[str, ...] = ()) -> list[str]:
    lines = [f"    /// {line}" for line in doc]
    lines += [f"    {a}" for a in attributes]
    lines.append(f"    {signature}")
    lines += [f"        where {c} : notnull" for c in constraints]
    lines.append("    {")
    lines += [f"        {line}" if line else "" for line in body]
    lines.append("    }")
    return lines


def switch(arms: list[str], receiver: str = "") -> list[str]:
    return [f"return {receiver}Tag switch", "{", *[f"    {arm}" for arm in arms], "};"]


def error_tags(n: int, skip: int = 0) -> str:
    return " or ".join(str(tag(i)) for i in range(1, n + 1) if i != skip)


# ---------------------------------------------------------------------------------------------------------------- library


class Op:
    """One synchronous combinator: <name><generics>(Func<arg, ret> param, extras...) -> out, plus the async cells of its family.

    body(call, receiver) returns the method body; call(argument) is the expression that invokes the delegate, and receiver is ""
    inside the struct or "result." inside an extension method that has awaited the source.
    """

    def __init__(self, family: str, name: str, generics: list[str], out: str, param: str, arg: str, ret: str, extras: list[tuple[str, str]], doc: list[str], body, checked_tag: str | None = None):
        self.family, self.name, self.generics, self.out = family, name, generics, out
        self.param, self.arg, self.ret, self.extras, self.doc, self.body = param, arg, ret, extras, doc, body
        # Result<TValue>.MapAsync and MapErrorAsync check the token before they call the handler; the tag on which it is called.
        self.checked_tag = checked_tag

    def type_args(self) -> str:
        return f"<{', '.join(self.generics)}>" if self.generics else ""

    def parameters(self, delegate: str) -> str:
        return ", ".join([f"Func<{self.arg}, {delegate}> {self.param}", *[f"{t} {p}" for t, p in self.extras]])

    def arguments(self) -> str:
        return ", ".join([self.param, *[p for _, p in self.extras]])

    def sync(self) -> list[str]:
        return method(
            self.doc,
            f"public {self.out} {self.name}{self.type_args()}({self.parameters(self.ret)})",
            self.generics,
            self.body(lambda a: f"{self.param}({a})", ""),
        )


def bind(n: int) -> list[Op]:
    errors = slots(n)
    out = result("TOut", errors)
    ops = [Op(
        "Bind", "Bind", ["TOut"], out, "bind", "T", out, [],
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
        lambda call, r: switch([f"{SUCCESS} => {call(f'{r}RawValue')},", f"{error_tags(n)} => new({r}Tag, default!, {r}RawError),", DEFAULT_ARM], r),
    )]
    for m in range(1, MAX_SLOTS - n + 1):
        added = slots(m, "TError")
        inner = result("TOut", added)
        targets = ", ".join(str(tag(n + j)) for j in range(1, m + 1))
        ops.append(Op(
            "Bind", "BindWiden", ["TOut", *added], result("TOut", errors + added), "bind", "T", inner, [],
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
            lambda call, r, inner=inner, targets=targets: [
                f"if ({r}Tag == {SUCCESS})",
                "{",
                f"    {inner} inner = {call(f'{r}RawValue')};",
                f"    return new(TypedResultLogic.Remap(inner.Tag, {targets}), inner.RawValue, inner.RawError);",
                "}",
                "",
                *switch([f"{error_tags(n)} => new({r}Tag, default!, {r}RawError),", DEFAULT_ARM], r),
            ],
        ))
    return ops


def map_(n: int) -> list[Op]:
    errors = slots(n)
    return [Op(
        "Map", "Map", ["TOut"], result("TOut", errors), "map", "T", "TOut", [],
        [
            "<summary>Replaces the success value with the return value of <paramref name=\"map\"/>. An error passes through in its slot.</summary>",
            '<typeparam name="TOut">The new success value type.</typeparam>',
            '<param name="map">Called with the success value.</param>',
            "<returns>A result that holds the mapped value or the same error.</returns>",
            '<exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>',
            DEFAULT_DOC,
        ],
        lambda call, r: switch([
            f"{SUCCESS} => new({SUCCESS}, TypedResultLogic.NotNull({call(f'{r}RawValue')}, nameof(map)), null),",
            f"{error_tags(n)} => new({r}Tag, default!, {r}RawError),",
            DEFAULT_ARM,
        ], r),
        SUCCESS,
    )]


def ensure(n: int) -> list[Op]:
    errors = slots(n)
    self_type = result("T", errors)
    ops = []

    def choose(name: str, check: str, keep: str, fail: str) -> str:
        return f"{check} ? {keep} : {fail}" if name == "Ensure" else f"{check} ? {fail} : {keep}"

    for name, fails in (("Ensure", "false"), ("FailIf", "true")):
        verb = "does not hold" if name == "Ensure" else "holds"
        for i, e in enumerate(errors, 1):
            ops.append(Op(
                "Ensure", name, [], self_type, "predicate", "T", "bool", [(e, "error")],
                [
                    f"<summary>Turns a success into the error <paramref name=\"error\"/> in the {ref(e)} slot when <paramref name=\"predicate\"/> {verb}.",
                    "An error passes through and <paramref name=\"predicate\"/> is not called.</summary>",
                    '<param name="predicate">Called with the success value.</param>',
                    f'<param name="error">The error to return when <paramref name="predicate"/> returns <see langword="{fails}"/>.</param>',
                    "<returns>This result, or a result that holds <paramref name=\"error\"/>.</returns>",
                    f'<exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="{fails}"/>.</exception>',
                    DEFAULT_DOC,
                ],
                lambda call, r, name=name, i=i: switch([
                    f"{SUCCESS} => {choose(name, call('RawValue'), 'this', f'new({tag(i)}, default!, TypedResultLogic.NotNull(error, nameof(error)))')},",
                    f"{error_tags(n)} => this,",
                    DEFAULT_ARM,
                ]),
            ))
        if n < MAX_SLOTS:
            ops.append(Op(
                "Ensure", name, ["TError"], result("T", errors + ["TError"]), "predicate", "T", "bool", [("TError", "error")],
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
                lambda call, r, name=name: switch([
                    f"{SUCCESS} => {choose(name, call('RawValue'), f'new({SUCCESS}, RawValue, null)', f'new({tag(n + 1)}, default!, TypedResultLogic.NotNull(error, nameof(error)))')},",
                    f"{error_tags(n)} => new(Tag, default!, RawError),",
                    DEFAULT_ARM,
                ]),
            ))
    return ops


def map_error(n: int) -> list[Op]:
    errors = slots(n)
    ops = []
    for i, e in enumerate(errors, 1):
        replaced = ["TError" if x == e else x for x in errors]

        def body(call, r, i=i, e=e):
            arms = [f"{SUCCESS} => new(Tag, RawValue, null),", f"{tag(i)} => new(Tag, default!, TypedResultLogic.NotNull({call(f'({e})RawError!')}, nameof(map))),"]
            if n > 1:
                arms.append(f"{error_tags(n, i)} => new(Tag, default!, RawError),")
            return switch([*arms, DEFAULT_ARM])

        ops.append(Op(
            "MapError", "MapError", ["TError"], result("T", replaced), "map", e, "TError", [],
            [
                f"<summary>Replaces the error in the {ref(e)} slot with the return value of <paramref name=\"map\"/>. Every other case passes through.</summary>",
                f'<typeparam name="TError">The error type that replaces {ref(e)}.</typeparam>',
                f'<param name="map">Called with the error of type {ref(e)}. Give its parameter type to choose the slot.</param>',
                "<returns>A result whose slot holds the mapped error, or the same case.</returns>",
                '<exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>',
                DEFAULT_DOC,
            ],
            body,
            str(tag(i)),
        ))
    return ops


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


def compensate(n: int) -> list[Op]:
    errors = slots(n)
    ops = []
    for i, e in enumerate(errors, 1):
        rest = errors if n == 1 else without(errors, i)
        out = result("T", rest)
        if n == 1:
            summary = [
                "<summary>Replaces the error with the result of <paramref name=\"compensate\"/>, which can fail again with the same error type.",
                "A success passes through.</summary>",
            ]
        else:
            summary = [
                f"<summary>Replaces the error in the {ref(e)} slot with the result of <paramref name=\"compensate\"/> and removes the slot.",
                "Every other case passes through.</summary>",
            ]

        def body(call, r, i=i, e=e):
            handled = call(f"({e})RawError!")
            if n == 1:
                return switch([f"{SUCCESS} => this,", f"{tag(i)} => {handled},", DEFAULT_ARM])
            return switch([f"{SUCCESS} => new(Tag, RawValue, null),", *removing_arms(n, i, handled), DEFAULT_ARM])

        ops.append(Op(
            "Compensate", "Compensate", [], out, "compensate", e, out, [],
            [
                *summary,
                f'<param name="compensate">Called with the error of type {ref(e)}. Give its parameter type to choose the slot.</param>',
                f"<returns>The result of <paramref name=\"compensate\"/>, or the same case.</returns>",
                DEFAULT_DOC,
            ],
            body,
        ))
    return ops


# ---------------------------------------------------------------------------------------------------------------- async cells

# The async matrix of Result<TValue> per family (CSharpEssentials.Results/Modules/ResultT.<Family>.cs), restricted to the
# delegate shapes the typed surface has. A cell is (receiver, delegate, net9-only, priority 1): receiver "this" is an instance
# member, "Task"/"ValueTask" an extension on a pending result; delegate "sync" is the plain Func. A Task cell with a ValueTask
# twin on the same receiver carries [OverloadResolutionPriority(1)] so an untyped async lambda binds to it under C# 13+.
CELLS = {
    "Bind": [("this", "Task", False, True), ("this", "ValueTask", False, False),
             ("Task", "sync", False, False), ("Task", "Task", False, False),
             ("ValueTask", "sync", False, False), ("ValueTask", "ValueTask", False, False)],
    "Map": [("this", "Task", False, True), ("this", "ValueTask", True, False),
            ("Task", "sync", False, False), ("Task", "Task", False, False),
            ("ValueTask", "sync", False, False), ("ValueTask", "ValueTask", False, False)],
    "Ensure": [("this", "Task", False, True), ("this", "ValueTask", True, False),
               ("Task", "sync", False, False), ("Task", "Task", False, False),
               ("ValueTask", "sync", False, False), ("ValueTask", "Task", False, True), ("ValueTask", "ValueTask", True, False)],
    "MapError": [("this", "Task", False, True), ("this", "ValueTask", False, False),
                 ("Task", "sync", False, False), ("Task", "Task", False, False),
                 ("ValueTask", "sync", False, False), ("ValueTask", "ValueTask", False, False)],
    "Compensate": [("this", "Task", False, True), ("this", "ValueTask", True, False),
                   ("Task", "Task", False, False),
                   ("ValueTask", "Task", False, True), ("ValueTask", "ValueTask", True, False)],
}
NO_INSTANCE_TOKEN = {"Bind"}  # Result<TValue>.BindAsync takes no CancellationToken
GATE = "NET9_0_OR_GREATER"
PRIORITY = "[OverloadResolutionPriority(1)]"
# On ResultExtensions the same-slot Ensure/FailIf cell and the slot-adding one are both generic, so the "non-generic wins" tie-break
# of the instance members does not apply and an error of a slot type binds both. The same-slot cells sit 2 above the adding ones.
SAME_SLOT_LIFT = 2
CT_PARAM = "CancellationToken cancellationToken = default"
AWAIT_TOKEN = ".WithCancellation(cancellationToken).ConfigureAwait(false)"
SLOT_NAMES = '[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]'


def async_doc(doc: list[str], sentence: str, cancellation: str | None, typeparams: tuple[str, ...] = (), task_param: tuple[str, ...] = ()) -> list[str]:
    out: list[str] = []
    in_summary, params_seen = True, False
    for line in doc:
        if in_summary and line.strip() == "</summary>":
            out += [sentence, line, *typeparams]
            in_summary = False
            continue
        if in_summary and line.endswith("</summary>"):
            out += [line[: -len("</summary>")].rstrip(), sentence + "</summary>", *typeparams]
            in_summary = False
            continue
        if line.startswith("<param") and not params_seen:
            out += task_param
            params_seen = True
        if line.startswith("<returns>") and cancellation:
            out.append(f'<param name="cancellationToken">{cancellation}</param>')
        out.append(line)
    return out


def checked(op: Op, receiver: str, body: list[str]) -> list[str]:
    if op.checked_tag is None:
        return body
    return [f"if ({receiver}Tag == {op.checked_tag})", "    cancellationToken.ThrowIfCancellationRequested();", "", *body]


def token_doc(op: Op, awaited: str) -> str:
    check = f'Checked before <paramref name="{op.param}"/> is called. ' if op.checked_tag else ""
    return f"{check}Observed while awaiting {awaited}."


def instance_cell(op: Op, delegate: str, priority: bool) -> list[str]:
    token = op.family not in NO_INSTANCE_TOKEN
    awaiter = AWAIT_TOKEN if token else ".ConfigureAwait(false)"
    params = op.parameters(f"{delegate}<{op.ret}>") + (f", {CT_PARAM}" if token else "")
    body = op.body(lambda a: f"await {op.param}({a}){awaiter}", "")
    return method(
        async_doc(op.doc, f"<paramref name=\"{op.param}\"/> returns a {'task' if delegate == 'Task' else 'value task'}, which is awaited.",
                  token_doc(op, f'<paramref name="{op.param}"/>') if token else None),
        f"public async {delegate}<{op.out}> {op.name}Async{op.type_args()}({params})",
        op.generics,
        checked(op, "", body) if token else body,
        (PRIORITY,) if priority else (),
    )


def extension_cell(op: Op, n: int, receiver: str, delegate: str, priority: bool, map_inline: bool) -> list[str]:
    errors = slots(n)
    source = result("T", errors)
    generics = ["T", *errors, *op.generics]
    handler = op.ret if delegate == "sync" else f"{delegate}<{op.ret}>"
    call_args = op.arguments()
    if delegate == "sync":
        tail = [f"return result.{op.name}{op.type_args()}({call_args});"]
        sentence = f"Awaits <paramref name=\"task\"/>, then calls <c>{op.name}</c>."
        observed = 'Observed while awaiting <paramref name="task"/>.'
    else:
        sentence = f"Awaits <paramref name=\"task\"/>, then awaits the {'task' if delegate == 'Task' else 'value task'} that <paramref name=\"{op.param}\"/> returns."
        observed = token_doc(op, f'<paramref name="task"/> and <paramref name="{op.param}"/>')
        if map_inline:
            tail = checked(op, "result.", op.body(lambda a: f"await {op.param}({a}){AWAIT_TOKEN}", "result."))
        elif op.family in NO_INSTANCE_TOKEN:
            tail = [f"return await result.{op.name}Async{op.type_args()}({call_args}){AWAIT_TOKEN};"]
        else:
            tail = [f"return await result.{op.name}Async{op.type_args()}({call_args}, cancellationToken).ConfigureAwait(false);"]
    level = (1 if priority else 0) + (SAME_SLOT_LIFT if op.family == "Ensure" and not op.generics and n < MAX_SLOTS else 0)
    typeparams = ['<typeparam name="T">The success value type.</typeparam>',
                  *[f'<typeparam name="{e}">The {ORDINALS[i]} error type.</typeparam>' for i, e in enumerate(errors, 1)]]
    return method(
        async_doc(op.doc, sentence, observed, tuple(typeparams), ('<param name="task">The pending result.</param>',)),
        f"public static async {receiver}<{op.out}> {op.name}Async<{', '.join(generics)}>(this {receiver}<{source}> task, {op.parameters(handler)}, {CT_PARAM})",
        generics,
        [f"{source} result = await task{AWAIT_TOKEN};", *tail],
        (SLOT_NAMES, f"[OverloadResolutionPriority({level})]") if level else (SLOT_NAMES,),
    )


def gated(members: list[list[str]]) -> list[list[str]]:
    return [[f"#if {GATE}", *join(members), "#endif"]] if members else []


def emit(n: int, family: str, ops: list[Op]) -> tuple[list[str], list[str]]:
    instance, instance_gated, extensions, extensions_gated = [], [], [], []
    for op in ops:
        for receiver, delegate, net9, priority in CELLS[family]:
            if receiver == "this":
                (instance_gated if net9 else instance).append(instance_cell(op, delegate, priority))
            else:
                inline = family == "Map" and delegate == "ValueTask"  # the instance ValueTask MapAsync is net9-only, this cell is not
                (extensions_gated if net9 else extensions).append(extension_cell(op, n, receiver, delegate, priority, inline))
    struct = join([op.sync() for op in ops] + instance + gated(instance_gated))
    return struct, join(extensions + gated(extensions_gated))


def join(members: list[list[str]]) -> list[str]:
    lines: list[str] = []
    for member in members:
        if lines:
            lines.append("")
        lines += member
    return lines


FAMILIES = {"Bind": bind, "Map": map_, "Ensure": ensure, "MapError": map_error, "Recover": recover, "Compensate": compensate}


def library_file(n: int, family: str) -> str:
    produced = FAMILIES[family](n)
    if family == "Recover":
        struct, extensions = produced, []
    else:
        struct, extensions = emit(n, family, produced)
    usings = ["using System.Diagnostics.CodeAnalysis;", "using System.Runtime.CompilerServices;", "using CSharpEssentials.Core;", ""] if extensions else []
    tail = ["", "public static partial class ResultExtensions", "{", *extensions, "}"] if extensions else []
    return "\n".join([
        HEADER,
        *usings,
        "namespace CSharpEssentials.ResultPattern;",
        "",
        f"public readonly partial struct {result('T', slots(n))}",
        "{",
        *struct,
        "}",
        *tail,
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
            if error not in STRUCTS:  # a struct error cannot be null
                null_call = f"{name}(v => {counted(predicate)}, ({error})null!)"
                tests.append(Test(f"{prefix}_Should_Throw_When_TheErrorIsNullAndThePredicateRejectsTheValue", src, FAILING_OK, null_call, None, None, 1, throws="ArgumentNullException"))
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


# ---------------------------------------------------------------------------------------------------------------- async tests

RECEIVER_NAMES = {"this": "Instance", "Task": "TaskSource", "ValueTask": "ValueTaskSource"}
HANDLER_NAMES = {"sync": "SyncHandler", "Task": "TaskHandler", "ValueTask": "ValueTaskHandler"}
NET9_ONLY = "!TYPED_NETSTANDARD_ASSET"  # the net9.0 cells are missing from the netstandard2.1 asset


class AsyncCase:
    def __init__(self, label: str, source: str, body: str, expected: str | None, calls: int, held: str | None = None):
        self.label, self.source, self.body, self.expected, self.calls, self.held = label, source, body, expected, calls, held


class AsyncOp:
    def __init__(self, family: str, name: str, prefix: str, src: str, arg: str, ret: str, out: str, extras: str, lam: str, cases: list[AsyncCase]):
        self.family, self.name, self.prefix, self.src, self.arg, self.ret, self.out = family, name, prefix, src, arg, ret, out
        self.extras, self.lam, self.cases = extras, lam, cases
        self.checks_token = family in ("Map", "MapError")


def handler(op: AsyncOp, delegate: str, body: str) -> str:
    lam = op.lam if re.search(rf"\b{op.lam}\b", body) else "_"
    if delegate == "sync":
        return f"Func<{op.arg}, {op.ret}> handler = {lam} => {{ calls++; return {body}; }};"
    wrapped = f"Task.FromResult<{op.ret}>({body})" if delegate == "Task" else f"new ValueTask<{op.ret}>({body})"
    return f"Func<{op.arg}, {delegate}<{op.ret}>> handler = {lam} => {{ calls++; return {wrapped}; }};"


def pending_handler(op: AsyncOp, delegate: str) -> str:
    pending = f"new TaskCompletionSource<{op.ret}>().Task"
    wrapped = pending if delegate == "Task" else f"new ValueTask<{op.ret}>({pending})"
    return f"Func<{op.arg}, {delegate}<{op.ret}>> handler = _ => {{ calls++; return {wrapped}; }};"


def receiver(op: AsyncOp, kind: str, value: str = "source") -> str:
    return {"this": value, "Task": f"Task.FromResult({value})", "ValueTask": f"new ValueTask<{op.src}>({value})"}[kind]


def fact(name: str, body: list[str]) -> list[str]:
    return ["    [Fact]", f"    public async Task {name}()", "    {", *[f"        {b}" if b else "" for b in body], "    }"]


def async_tests(op: AsyncOp, kind: str, delegate: str, token: bool) -> list[list[str]]:
    cell = f"{op.prefix}Async_{RECEIVER_NAMES[kind]}{HANDLER_NAMES[delegate]}"
    call = f"{receiver(op, kind)}.{op.name}Async(handler{op.extras})"
    tests = []
    for case in op.cases:
        body = [f"{op.src} source = {case.source};", "int calls = 0;", handler(op, delegate, case.body), "",
                f"{op.out} result = await {call};", "", f"calls.Should().Be({case.calls});",
                f"{op.out} expected = {case.expected};", "result.Should().Be(expected);"]
        if case.held:
            check = "Be" if case.held in STRUCTS else "BeSameAs"
            body.append(f"result.Value.Should().{check}({FIXTURE[case.held]});")
        tests.append(fact(f"{cell}_Should_{case.label}", body))
    first = op.cases[0]
    tests.append(fact(f"{cell}_Should_Throw_When_TheResultIsDefault", [
        f"{op.src} source = default;", "int calls = 0;", handler(op, delegate, first.body), "",
        f"Func<Task> act = async () => await {call};", "",
        "await act.Should().ThrowAsync<InvalidOperationException>();", "calls.Should().Be(0);",
    ]))
    if kind != "this":
        tests.append(fact(f"{cell}_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending", [
            f"TaskCompletionSource<{op.src}> pending = new();", "using CancellationTokenSource cancellation = new();",
            "await cancellation.CancelAsync();", "int calls = 0;", handler(op, delegate, first.body), "",
            f"Func<Task> act = async () => await {'pending.Task' if kind == 'Task' else f'new ValueTask<{op.src}>(pending.Task)'}.{op.name}Async(handler{op.extras}, cancellation.Token);", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();", "calls.Should().Be(0);",
        ]))
    if op.checks_token and delegate != "sync":
        tests.append(fact(f"{cell}_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled", [
            f"{op.src} source = {first.source};", "using CancellationTokenSource cancellation = new();",
            "await cancellation.CancelAsync();", "int calls = 0;", handler(op, delegate, first.body), "",
            f"Func<Task> act = async () => await {receiver(op, kind)}.{op.name}Async(handler{op.extras}, cancellation.Token);", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();", "calls.Should().Be(0);",
        ]))
    elif kind == "this" and token:
        tests.append(fact(f"{cell}_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending", [
            f"{op.src} source = {first.source};", "using CancellationTokenSource cancellation = new();",
            "await cancellation.CancelAsync();", "int calls = 0;", pending_handler(op, delegate), "",
            f"Func<Task> act = async () => await source.{op.name}Async(handler{op.extras}, cancellation.Token);", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();", "calls.Should().Be(1);",
        ]))
    return tests


def passthrough(errors: list[str], body: str) -> AsyncCase:
    e = errors[-1]
    return AsyncCase(f"PassTheErrorThrough_When_{case_name(e)}", FIXTURE[e], body, FIXTURE[e], 0, e)


def async_ops(n: int, family: str) -> list[AsyncOp]:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    ops = []
    if family == "Bind":
        out = result("string", errors)
        ops.append(AsyncOp(family, "Bind", "Bind", src, "int", out, out, "", "v", [
            AsyncCase("ReturnTheInnerResult_When_TheResultHoldsAValue", OK, "TypedFixture.Text(v)", OTHER, 1),
            passthrough(errors, "TypedFixture.Text(v)"),
        ]))
        for m in range(1, MAX_SLOTS - n + 1):
            added = ADDED_TYPES[:m]
            inner = result("string", added)
            body = f"new {inner}({FIXTURE[added[-1]]})"
            ops.append(AsyncOp(family, "BindWiden", f"BindWiden{m}", src, "int", inner, result("string", errors + added), "", "v", [
                AsyncCase(f"MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHolds{added[-1]}", OK, body, FIXTURE[added[-1]], 1, added[-1]),
                passthrough(errors, body),
            ]))
    elif family == "Map":
        ops.append(AsyncOp(family, "Map", "Map", src, "int", "string", result("string", errors), "", "v", [
            AsyncCase("MapTheValue_When_TheResultHoldsAValue", OK, "TypedFixture.Text(v)", OTHER, 1),
            passthrough(errors, "TypedFixture.Text(v)"),
        ]))
    elif family == "Ensure":
        for name, predicate in (("Ensure", "v > 0"), ("FailIf", "v < 0")):
            targets = [(f"{name}{e}", e, src) for e in errors]
            if n < MAX_SLOTS:
                targets.append((f"{name}Added", "ValidationError", result("int", errors + ["ValidationError"])))
            for prefix, error, out in targets:
                ops.append(AsyncOp(family, name, prefix, src, "int", "bool", out, f", {FIXTURE[error]}", "v", [
                    AsyncCase("KeepTheValue_When_ThePredicateAllowsIt", OK, predicate, OK, 1),
                    AsyncCase("ReturnTheError_When_ThePredicateRejectsTheValue", FAILING_OK, predicate, FIXTURE[error], 1, error),
                    passthrough(errors, predicate),
                ]))
    elif family == "MapError":
        for e in errors:
            out = result("int", ["ValidationError" if x == e else x for x in errors])
            ops.append(AsyncOp(family, "MapError", f"MapError{e}", src, e, "ValidationError", out, "", "e", [
                AsyncCase(f"MapTheError_When_{case_name(e)}", FIXTURE[e], FIXTURE["ValidationError"], FIXTURE["ValidationError"], 1, "ValidationError"),
                AsyncCase("KeepTheValue_When_TheResultHoldsAValue", OK, FIXTURE["ValidationError"], OK, 0),
            ]))
    elif family == "Compensate":
        for e in errors:
            out = result("int", errors if n == 1 else [x for x in errors if x != e])
            ops.append(AsyncOp(family, "Compensate", f"Compensate{e}", src, e, out, out, "", "e", [
                AsyncCase(f"ReturnTheCompensatingValue_When_{case_name(e)}", FIXTURE[e], RECOVERED, RECOVERED, 1),
                AsyncCase("KeepTheValue_When_TheResultHoldsAValue", OK, RECOVERED, OK, 0),
            ]))
    return ops


def async_test_file(n: int, family: str) -> str:
    plain: list[list[str]] = []
    net9: list[list[str]] = []
    for op in async_ops(n, family):
        for kind, delegate, gate, _ in CELLS[family]:
            token = family not in NO_INSTANCE_TOKEN
            (net9 if gate else plain).extend(async_tests(op, kind, delegate, token))
    body = join(plain + ([[f"#if {NET9_ONLY}", *join(net9), "#endif"]] if net9 else []))
    return "\n".join([
        HEADER,
        "using CSharpEssentials.Errors;",
        "using CSharpEssentials.ResultPattern;",
        "using FluentAssertions;",
        "",
        "namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;",
        "",
        f"public sealed class {type_name(n)}{family}AsyncTests",
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

    library = {LIBRARY / f"{type_name(n)}.{family}.cs": library_file(n, family) for n in range(1, MAX_SLOTS + 1) for family in FAMILIES}
    tests = {TESTS / f"{type_name(n)}{family}Tests.cs": test_file(n, family, emit(n)) for n in range(1, MAX_SLOTS + 1) for family, emit in TEST_FAMILIES.items()}
    tests |= {TESTS / f"{type_name(n)}{family}AsyncTests.cs": async_test_file(n, family) for n in range(1, MAX_SLOTS + 1) for family in CELLS}
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
