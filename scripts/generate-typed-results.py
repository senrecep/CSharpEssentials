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
    lines += [f"        where {c}" if " : " in c else f"        where {c} : notnull" for c in constraints]
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

    def __init__(self, family: str, name: str, generics: list[str], out: str, param: str, arg: str, ret: str | None, extras: list[tuple[str, str]], doc: list[str], body, checked_tag: str | None = None,
                 leading: list[tuple[str, str]] | None = None, constraints: list[str] | None = None, cells: list[tuple[str, str, bool, bool]] | None = None, lift: int = 0):
        self.family, self.name, self.generics, self.out = family, name, generics, out
        # ret None: the handler returns nothing (Action, Func<.., Task>, Func<.., ValueTask>).
        self.param, self.arg, self.ret, self.extras, self.doc, self.body = param, arg, ret, extras, doc, body
        # Result<TValue>.MapAsync and MapErrorAsync check the token before they call the handler; the tag on which it is called.
        self.checked_tag = checked_tag
        self.leading = leading or []
        self.constraints = constraints or generics
        self.cells = cells or CELLS[family]
        self.lift = lift

    def type_args(self) -> str:
        return f"<{', '.join(self.generics)}>" if self.generics else ""

    def handler(self, delegate: str) -> str:
        if self.ret is None:
            return f"Action<{self.arg}>" if delegate == "sync" else f"Func<{self.arg}, {delegate}>"
        return f"Func<{self.arg}, {self.ret if delegate == 'sync' else f'{delegate}<{self.ret}>'}>"

    def parameters(self, delegate: str) -> str:
        return ", ".join([*[f"{t} {p}" for t, p in self.leading], f"{self.handler(delegate)} {self.param}", *[f"{t} {p}" for t, p in self.extras]])

    def arguments(self) -> str:
        return ", ".join([*[p for _, p in self.leading], self.param, *[p for _, p in self.extras]])

    def sync(self) -> list[str]:
        return method(
            self.doc,
            f"public {self.out} {self.name}{self.type_args()}({self.parameters('sync')})",
            self.constraints,
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
# Result<TValue>.MapErrorAsync checks the token before the mapper and then awaits it without the token, so a running mapper finishes.
UNABANDONED = {"MapError"}
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


def token_doc(op: Op, awaited: str | None) -> str:
    check = f'Checked before <paramref name="{op.param}"/> is called. ' if op.checked_tag else ""
    if op.family in UNABANDONED:
        observed = f"Observed while awaiting {awaited}; a" if awaited else "A"
        return f'{check}{observed} running <paramref name="{op.param}"/> is not abandoned.'
    return f"{check}Observed while awaiting {awaited}."


def instance_cell(op: Op, delegate: str, priority: bool) -> list[str]:
    token = op.family not in NO_INSTANCE_TOKEN
    awaiter = AWAIT_TOKEN if token and op.family not in UNABANDONED else ".ConfigureAwait(false)"
    params = op.parameters(delegate) + (f", {CT_PARAM}" if token else "")
    body = op.body(lambda a: f"await {op.param}({a}){awaiter}", "")
    return method(
        async_doc(op.doc, f"<paramref name=\"{op.param}\"/> returns a {'task' if delegate == 'Task' else 'value task'}, which is awaited.",
                  token_doc(op, None if op.family in UNABANDONED else f'<paramref name="{op.param}"/>') if token else None),
        f"public async {delegate}<{op.out}> {op.name}Async{op.type_args()}({params})",
        op.constraints,
        checked(op, "", body) if token else body,
        (PRIORITY,) if priority else (),
    )


def extension_cell(op: Op, n: int, receiver: str, delegate: str, priority: bool, map_inline: bool) -> list[str]:
    errors = slots(n)
    source = result("T", errors)
    generics = ["T", *errors, *op.generics]
    call_args = op.arguments()
    if delegate == "sync":
        tail = [f"return result.{op.name}{op.type_args()}({call_args});"]
        sentence = f"Awaits <paramref name=\"task\"/>, then calls <c>{op.name}</c>."
        observed = 'Observed while awaiting <paramref name="task"/>.'
    else:
        sentence = f"Awaits <paramref name=\"task\"/>, then awaits the {'task' if delegate == 'Task' else 'value task'} that <paramref name=\"{op.param}\"/> returns."
        observed = token_doc(op, '<paramref name="task"/>' if op.family in UNABANDONED else f'<paramref name="task"/> and <paramref name="{op.param}"/>')
        if map_inline:
            tail = checked(op, "result.", op.body(lambda a: f"await {op.param}({a}){AWAIT_TOKEN}", "result."))
        elif op.family in NO_INSTANCE_TOKEN:
            tail = [f"return await result.{op.name}Async{op.type_args()}({call_args}){AWAIT_TOKEN};"]
        else:
            tail = [f"return await result.{op.name}Async{op.type_args()}({call_args}, cancellationToken).ConfigureAwait(false);"]
    level = (1 if priority else 0) + (SAME_SLOT_LIFT if op.family == "Ensure" and not op.generics and n < MAX_SLOTS else 0) + op.lift
    typeparams = ['<typeparam name="T">The success value type.</typeparam>',
                  *[f'<typeparam name="{e}">The {ORDINALS[i]} error type.</typeparam>' for i, e in enumerate(errors, 1)]]
    return method(
        async_doc(op.doc, sentence, observed, tuple(typeparams), ('<param name="task">The pending result.</param>',)),
        f"public static async {receiver}<{op.out}> {op.name}Async<{', '.join(generics)}>(this {receiver}<{source}> task, {op.parameters(delegate)}, {CT_PARAM})",
        ["T", *errors, *op.constraints],
        [f"{source} result = await task{AWAIT_TOKEN};", *tail],
        (SLOT_NAMES, f"[OverloadResolutionPriority({level})]") if level else (SLOT_NAMES,),
    )


def gated(members: list[list[str]]) -> list[list[str]]:
    return [[f"#if {GATE}", *join(members), "#endif"]] if members else []


def emit(n: int, family: str, ops: list[Op]) -> tuple[list[str], list[str]]:
    instance, instance_gated, extensions, extensions_gated = [], [], [], []
    for op in ops:
        for receiver, delegate, net9, priority in op.cells:
            if receiver == "this":
                (instance_gated if net9 else instance).append(instance_cell(op, delegate, priority))
            else:
                # Inline the handler call when no instance member with this handler exists on every target the cell exists on.
                twin = [gate for r, d, gate, _ in op.cells if r == "this" and d == delegate]
                inline = delegate != "sync" and not (twin and (not twin[0] or net9))
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


# ---------------------------------------------------------------------------------------------------------------- remaining families

# Cells of the families added after the first six; the same tuples as CELLS. Tap and TapIf copy Result<TValue>.TapAsync/TapIfAsync,
# TapError copies TapErrorAsync, Finally copies FinallyAsync (one table per delegate shape), Select copies SelectAsync and TryCatch
# copies TryCatchAsync, which on Result<TValue> exist only as extensions with a synchronous handler.
TAP_CELLS = [("this", "Task", False, True), ("this", "ValueTask", True, False),
             ("Task", "sync", False, False), ("Task", "Task", False, False),
             ("ValueTask", "sync", False, False), ("ValueTask", "ValueTask", False, False)]
TAP_ERROR_CELLS = [("this", "Task", False, True), ("this", "ValueTask", True, False),
                   ("Task", "Task", False, False),
                   ("ValueTask", "Task", False, True), ("ValueTask", "ValueTask", True, False)]
FINALLY_CELLS = [("Task", "sync", False, False), ("Task", "Task", False, False),
                 ("ValueTask", "sync", False, False), ("ValueTask", "ValueTask", True, False)]
FINALLY_ACTION_CELLS = [("Task", "sync", False, False), ("Task", "Task", False, False),
                        ("ValueTask", "sync", False, False), ("ValueTask", "Task", False, True), ("ValueTask", "ValueTask", True, False)]
SELECT_CELLS = [("Task", "sync", False, False), ("Task", "Task", False, False), ("ValueTask", "sync", False, False)]
TRY_CATCH_CELLS = [("Task", "sync", False, False), ("ValueTask", "sync", False, False)]
CELLS |= {"Tap": TAP_CELLS, "TapError": TAP_ERROR_CELLS, "TryCatch": TRY_CATCH_CELLS, "Finally": FINALLY_CELLS, "Select": SELECT_CELLS}
# The per-slot TapErrorAsync extensions sit 2 above the "any error" ones, so an untyped lambda keeps binding the slot overload.
ANY_ERROR_LIFT = 2
COUNT_WORDS = ["first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth"]
IERROR_REMARK = "It exists only when every error type implements <see cref=\"IError\"/>; the error is converted as by <c>ToResult</c>."


def self_of(r: str) -> str:
    return r[:-1] if r else "this"


def ext_typeparams(errors: list[str]) -> list[str]:
    return ['<typeparam name="T">The success value type.</typeparam>',
            *[f'<typeparam name="{e}">The {ORDINALS[i]} error type.</typeparam>' for i, e in enumerate(errors, 1)]]


def tap(n: int) -> list[Op]:
    out = result("T", slots(n))

    def body(condition: str):
        return lambda call, r: [
            f"TypedResultLogic.EnsureInitialized({r}Tag);",
            f"if ({r}Tag == {SUCCESS}{condition.format(r=r)})",
            f"    {call(f'{r}RawValue')};",
            "",
            f"return {self_of(r)};",
        ]

    common = ['<returns>This result.</returns>', DEFAULT_DOC]
    return [
        Op("Tap", "Tap", [], out, "action", "T", None, [], [
            "<summary>Calls <paramref name=\"action\"/> with the success value. An error passes through and <paramref name=\"action\"/> is not called.</summary>",
            '<param name="action">Called with the success value.</param>', *common,
        ], body(""), SUCCESS),
        Op("Tap", "TapIf", [], out, "action", "T", None, [], [
            "<summary>Calls <paramref name=\"action\"/> with the success value when <paramref name=\"predicate\"/> holds. An error passes",
            "through and neither delegate is called.</summary>",
            '<param name="predicate">Called with the success value.</param>',
            '<param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>', *common,
        ], body(" && predicate({r}RawValue)"), leading=[("Func<T, bool>", "predicate")]),
    ]


def any_error_members(n: int, name: str, summary: list[str], returns: str, out: str, handler: str, param: str, body_of, cells: list[tuple[str, str, bool, bool]], extra_doc: list[str]) -> list[str]:
    """The "any error" members: extensions constrained to IError slots, a sync one on the result and async cells on the result and
    on Task and ValueTask sources. body_of(call) is the body after the source is in `result`."""
    errors = slots(n)
    source = result("T", errors)
    constraints = ["T", *[f"{e} : notnull, IError" for e in errors]]
    generics = ", ".join(["T", *errors])
    doc_tail = [*extra_doc, f'<returns>{returns}</returns>', DEFAULT_DOC]
    members = [method(
        [*summary, *ext_typeparams(errors), '<param name="result">The result.</param>', f'<param name="{param}">{handler[1]}</param>', *doc_tail],
        f"public static {out} {name}<{generics}>(this {source} result, {handler[0].format(ret=handler[2] or '')} {param})",
        constraints, body_of(lambda a: f"{param}({a})"), (SLOT_NAMES,))]
    plain, gated_members = [], []
    for receiver, delegate, net9, priority in cells:
        typed = handler[0].format(ret=f"{delegate}<{handler[2]}>" if handler[2] else delegate) if delegate != "sync" else handler[0].format(ret=handler[2] or "")
        if handler[2] is None and delegate != "sync":
            typed = f"Func<Error, {delegate}>"
        awaited = f"await {param}({{}}){AWAIT_TOKEN}" if delegate != "sync" else f"{param}({{}})"
        sentence = (f"<paramref name=\"{param}\"/> returns a {'task' if delegate == 'Task' else 'value task'}, which is awaited." if receiver == "this"
                    else f"Awaits <paramref name=\"task\"/>{', then awaits the task that' if delegate != 'sync' else ''}{f' <paramref name=\"{param}\"/> returns' if delegate != 'sync' else ''}.")
        first = '<param name="result">The result.</param>' if receiver == "this" else '<param name="task">The pending result.</param>'
        this = source if receiver == "this" else f"{receiver}<{source}>"
        task = "Task" if receiver != "ValueTask" and (receiver != "this" or delegate == "Task") else "ValueTask"
        body = body_of(lambda a, aw=awaited: aw.format(a))
        if receiver != "this":
            body = [f"{source} result = await task{AWAIT_TOKEN};", *body]
        attributes = (SLOT_NAMES, PRIORITY) if priority else (SLOT_NAMES,)
        member = method(
            [*summary, *ext_typeparams(errors), first, f'<param name="{param}">{handler[1]}</param>',
             f'<param name="cancellationToken">Observed while awaiting{" <paramref name=\"task\"/> and" if receiver != "this" else ""} <paramref name="{param}"/>.</param>' if delegate != "sync"
             else '<param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>',
             *doc_tail],
            f"public static async {task}<{out}> {name}Async<{generics}>(this {this} {'result' if receiver == 'this' else 'task'}, {typed} {param}, {CT_PARAM})",
            constraints, body, attributes)
        member = fix_summary(member, sentence)
        (gated_members if net9 else plain).append(member)
    return join([*[members[0]], *plain, *gated(gated_members)])


def fix_summary(member: list[str], sentence: str) -> list[str]:
    """Puts the async sentence at the end of the summary of a member written by any_error_members."""
    out = []
    for line in member:
        if line.strip() == "/// </summary>":
            out += [line.replace("</summary>", sentence), line]
        elif line.rstrip().endswith("</summary>"):
            out.append(line[: -len("</summary>")].rstrip() + " " + sentence + "</summary>")
        else:
            out.append(line)
    return out


def tap_error(n: int) -> tuple[list[Op], list[str], list[str]]:
    errors = slots(n)
    out = result("T", errors)
    ops = []
    for i, e in enumerate(errors, 1):
        ops.append(Op("TapError", "TapError", [], out, "action", e, None, [], [
            f"<summary>Calls <paramref name=\"action\"/> with the error in the {ref(e)} slot. Every other case passes through.</summary>",
            f'<param name="action">Called with the error of type {ref(e)}. Give its parameter type to choose the slot.</param>',
            '<returns>This result.</returns>', DEFAULT_DOC,
        ], lambda call, r, i=i, e=e: [
            f"TypedResultLogic.EnsureInitialized({r}Tag);",
            f"if ({r}Tag == {tag(i)})",
            f"    {call(f'({e}){r}RawError!')};",
            "",
            f"return {self_of(r)};",
        ], lift=ANY_ERROR_LIFT))
    any_error = any_error_members(
        n, "TapError",
        ["<summary>Calls <paramref name=\"action\"/> with the error converted to <see cref=\"Error\"/>, whatever slot holds it. A success",
         "passes through.</summary>"],
        "The result.", out, ("Action<Error>", "Called with the error.", None), "action",
        lambda call: ["TypedResultLogic.EnsureInitialized(result.Tag);", f"if (result.Tag != {SUCCESS})", f"    {call('result.CurrentError()')};", "", "return result;"],
        TAP_ERROR_CELLS, [f"<remarks>{IERROR_REMARK} Give the lambda parameter type <see cref=\"Error\"/> on a result whose slots are typed errors.</remarks>"])
    return ops, [], any_error


def try_catch(n: int) -> list[Op]:
    if n == MAX_SLOTS:
        return []
    errors = slots(n)
    return [Op(
        "TryCatch", "TryCatch", ["TOut", "TException"], result("TOut", errors + ["TException"]), "func", "T", "TOut", [],
        [
            "<summary>",
            "Calls <paramref name=\"func\"/> with the success value and returns its return value. An exception of type",
            "<typeparamref name=\"TException\"/> that <paramref name=\"func\"/> throws becomes an error in a new last slot; any other",
            "exception propagates. An error passes through in its slot and <paramref name=\"func\"/> is not called. Give both type",
            "arguments: <typeparamref name=\"TException\"/> cannot be inferred.",
            "</summary>",
            '<typeparam name="TOut">The new success value type.</typeparam>',
            '<typeparam name="TException">The exception type that is caught and added as the last slot.</typeparam>',
            '<param name="func">Called with the success value.</param>',
            "<returns>A result with <typeparamref name=\"TException\"/> added as the last slot.</returns>",
            '<exception cref="ArgumentNullException"><paramref name="func"/> returns <see langword="null"/>.</exception>',
            DEFAULT_DOC,
        ],
        lambda call, r: [
            f"if ({r}Tag != {SUCCESS})",
            f"    return new(TypedResultLogic.ErrorTag({r}Tag), default!, {r}RawError);",
            "",
            "TOut value;",
            "try",
            "{",
            f"    value = {call(f'{r}RawValue')};",
            "}",
            "catch (TException exception)",
            "{",
            f"    return new({tag(n + 1)}, default!, exception);",
            "}",
            "",
            f"return new({SUCCESS}, TypedResultLogic.NotNull(value, nameof(func)), null);",
        ],
        constraints=["TOut", "TException : Exception"],
    )]


def finally_(n: int) -> list[Op]:
    out = result("T", slots(n))
    note = "Unlike the other combinators it does not throw on <see langword=\"default\"/>."
    return [
        Op("Finally", "Finally", ["TOut"], "TOut", "func", out, "TOut", [], [
            f"<summary>Calls <paramref name=\"func\"/> with this result, whatever case it holds, and returns its return value. {note}</summary>",
            '<typeparam name="TOut">The return type.</typeparam>',
            '<param name="func">Called with this result.</param>',
            '<returns>The return value of <paramref name="func"/>.</returns>',
        ], lambda call, r: [f"return {call(self_of(r))};"]),
        Op("Finally", "Finally", [], out, "action", out, None, [], [
            f"<summary>Calls <paramref name=\"action\"/> with this result, whatever case it holds. {note}</summary>",
            '<param name="action">Called with this result.</param>',
            "<returns>This result.</returns>",
        ], lambda call, r: [f"{call(self_of(r))};", "", f"return {self_of(r)};"], cells=FINALLY_ACTION_CELLS),
    ]


def select(n: int) -> tuple[list[Op], list[str], list[str]]:
    errors = slots(n)
    ops = [Op(
        "Select", "Select", ["TOut"], result("TOut", errors), "selector", "T", "TOut", [],
        [
            "<summary>Replaces the success value with the return value of <paramref name=\"selector\"/>, as <c>Map</c> does. It makes",
            "<c>select</c> work in query syntax. An error passes through in its slot.</summary>",
            '<typeparam name="TOut">The new success value type.</typeparam>',
            '<param name="selector">Called with the success value.</param>',
            "<returns>A result that holds the mapped value or the same error.</returns>",
            '<exception cref="ArgumentNullException"><paramref name="selector"/> returns <see langword="null"/>.</exception>',
            DEFAULT_DOC,
        ],
        lambda call, r: switch([
            f"{SUCCESS} => new({SUCCESS}, TypedResultLogic.NotNull({call(f'{r}RawValue')}, nameof(selector)), null),",
            f"{error_tags(n)} => new({r}Tag, default!, {r}RawError),",
            DEFAULT_ARM,
        ], r),
    )]
    members = []
    for m in range(0, MAX_SLOTS - n + 1):
        added = slots(m, "TError")
        inner = result("TMiddle", errors if m == 0 else added)
        targets = ", ".join(str(tag(n + j)) for j in range(1, m + 1))
        fail = "TypedResultLogic.ErrorTag(inner.Tag)" if m == 0 else f"TypedResultLogic.Remap(TypedResultLogic.ErrorTag(inner.Tag), {targets})"
        summary = ([
            "<summary>",
            "Calls <paramref name=\"selector\"/> with the success value, then <paramref name=\"resultSelector\"/> with both values.",
            "It makes a second <c>from</c> work in query syntax when the inner result has the same error types. An error passes",
            "through in its slot.",
            "</summary>",
        ] if m == 0 else [
            "<summary>",
            "Calls <paramref name=\"selector\"/> with the success value, then <paramref name=\"resultSelector\"/> with both values,",
            "and adds the error types of the inner result after this result's, as <c>BindWiden</c> does. Query syntax picks it for an",
            "inner result with other error types; one whose error types are a subset of this result's also lands here and repeats",
            "a slot, so call <c>Bind</c> for that case. An error passes through in its slot.",
            "</summary>",
        ])
        members.append(method(
            [
                *summary,
                '<typeparam name="TMiddle">The success value type of the inner result.</typeparam>',
                '<typeparam name="TOut">The new success value type.</typeparam>',
                *[f'<typeparam name="{a}">The {ORDINALS[j]} error type that <paramref name="selector"/> adds.</typeparam>' for j, a in enumerate(added, 1)],
                '<param name="selector">Called with the success value.</param>',
                '<param name="resultSelector">Called with the success value and the inner success value.</param>',
                "<returns>A result that holds the projected value or the first error.</returns>",
                '<exception cref="ArgumentNullException"><paramref name="resultSelector"/> returns <see langword="null"/>.</exception>',
                DEFAULT_DOC,
            ],
            f"public {result('TOut', errors + added)} SelectMany<{', '.join(['TMiddle', 'TOut', *added])}>(Func<T, {inner}> selector, Func<T, TMiddle, TOut> resultSelector)",
            ["TMiddle", "TOut", *added],
            [
                f"if (Tag != {SUCCESS})",
                "    return new(TypedResultLogic.ErrorTag(Tag), default!, RawError);",
                "",
                f"{inner} inner = selector(RawValue);",
                f"if (inner.Tag != {SUCCESS})",
                f"    return new({fail}, default!, inner.RawError);",
                "",
                f"return new({SUCCESS}, TypedResultLogic.NotNull(resultSelector(RawValue, inner.RawValue), nameof(resultSelector)), null);",
            ],
            (PRIORITY,) if m == 0 else (),
        ))
    return ops, join(members), []


def combine(n: int) -> tuple[list[Op], list[str], list[str]]:
    errors = slots(n)
    members = []
    for k in range(2, 9):
        values = ["T", *[f"T{j}" for j in range(2, k + 1)]]
        names = COUNT_WORDS[:k]
        members.append(method(
            [
                f"<summary>Combines {k} results with the same error types into one result that holds a tuple of their values. The results",
                "are checked in order and the first error is returned; the later results are not looked at.</summary>",
                *[f'<typeparam name="T{j}">The success value type of the {COUNT_WORDS[j - 1]} result.</typeparam>' for j in range(2, k + 1)],
                *[f'<param name="{w}">The {w} result.</param>' for w in names],
                "<returns>A result that holds every value, or the first error.</returns>",
                '<exception cref="InvalidOperationException">A result checked before the first error is <see langword="default"/>.</exception>',
            ],
            f"public static {result('(' + ', '.join(values) + ')', errors)} Combine<{', '.join(values[1:])}>({', '.join(f'{result(v, errors)} {w}' for v, w in zip(values, names))})",
            values[1:],
            [
                *[line for w in names for line in (f"if ({w}.Tag != {SUCCESS})", f"    return new(TypedResultLogic.ErrorTag({w}.Tag), default!, {w}.RawError);", "")],
                f"return new({SUCCESS}, ({', '.join(f'{w}.RawValue' for w in names)}), null);",
            ],
        ))
    return [], join(members), []


def get_value(n: int) -> tuple[list[Op], list[str], list[str]]:
    errors = slots(n)
    source = result("T", errors)
    struct = join([
        method(
            ["<summary>Returns the success value, or <see langword=\"default\"/> of <typeparamref name=\"T\"/> when the result holds an",
             "error or is <see langword=\"default\"/>. It never throws.</summary>",
             "<returns>The success value or <see langword=\"default\"/>.</returns>"],
            "public T? GetValueOrDefault()", [], [f"return Tag == {SUCCESS} ? RawValue : default;"]),
        method(
            ["<summary>Returns the success value, or <paramref name=\"fallback\"/> when the result holds an error or is",
             "<see langword=\"default\"/>. It never throws.</summary>",
             '<param name="fallback">The value to return when the result holds no success value.</param>',
             "<returns>The success value or <paramref name=\"fallback\"/>.</returns>"],
            "public T GetValueOr(T fallback)", [], [f"return Tag == {SUCCESS} ? RawValue : fallback;"]),
        method(
            ["<summary>Returns the success value, or throws when the result holds an error.</summary>",
             '<param name="errorMessage">The message of the exception; a default message when <see langword="null"/>.</param>',
             "<returns>The success value.</returns>",
             '<exception cref="InvalidOperationException">The result holds an error or is <see langword="default"/>.</exception>'],
            "public T GetValueOrThrow(string? errorMessage = null)", [],
            switch([f"{SUCCESS} => RawValue,", "TypedResultLogic.DefaultTag => throw TypedResultLogic.DefaultInitialized(),",
                    '_ => throw new InvalidOperationException(errorMessage ?? "Result has no value."),'])),
    ])
    ext = []
    for receiver in ("Task", "ValueTask"):
        for name, out, extra, call, doc in (
            ("GetValueOrDefault", "T?", "", "GetValueOrDefault()", []),
            ("GetValueOr", "T", "T fallback, ", "GetValueOr(fallback)", ['<param name="fallback">The value to return when the result holds no success value.</param>']),
        ):
            ext.append(method(
                [f"<summary>Awaits <paramref name=\"task\"/>, then calls <c>{name}</c>. It throws only when the token is canceled.</summary>",
                 *ext_typeparams(errors), '<param name="task">The pending result.</param>', *doc,
                 '<param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>',
                 "<returns>The success value or the fallback.</returns>"],
                f"public static async {receiver}<{out}> {name}Async<{', '.join(['T', *errors])}>(this {receiver}<{source}> task, {extra}{CT_PARAM})",
                ["T", *errors],
                [f"{source} result = await task{AWAIT_TOKEN};", f"return result.{call};"],
                (SLOT_NAMES,)))
    any_error = any_error_members(
        n, "GetValueOr",
        ["<summary>Returns the success value, or the return value of <paramref name=\"fallback\"/> called with the error converted to",
         "<see cref=\"Error\"/>.</summary>"],
        "The success value or the return value of <paramref name=\"fallback\"/>.", "T", ("Func<Error, {ret}>", "Called with the error.", "T"), "fallback",
        lambda call: ["TypedResultLogic.EnsureInitialized(result.Tag);",
                      f"return result.Tag == {SUCCESS} ? result.RawValue : TypedResultLogic.NotNull({call('result.CurrentError()')}, nameof(fallback));"],
        [("Task", "sync", False, False), ("ValueTask", "sync", False, False)],
        [f"<remarks>{IERROR_REMARK}</remarks>", '<exception cref="ArgumentNullException"><paramref name="fallback"/> returns <see langword="null"/>.</exception>'])
    return [], struct, join([*ext, any_error])


def collections(n: int) -> tuple[list[Op], list[str], list[str]]:
    errors = slots(n)
    item = result("T", errors)
    out = result("T[]", errors)
    generics = ", ".join(["T", *errors])
    tp = ext_typeparams(errors)
    stop = ["if (result.Tag != " + SUCCESS + ")", "    return new(TypedResultLogic.ErrorTag(result.Tag), default!, result.RawError);", "", "values.Add(result.RawValue);"]
    default_doc = '<exception cref="InvalidOperationException">A result looked at before the first error is <see langword="default"/>.</exception>'
    members = [
        method(
            ["<summary>Collects the success values of <paramref name=\"source\"/> in order. It stops at the first error and returns it;",
             "the later results are not looked at.</summary>", *tp, '<param name="source">The results.</param>',
             "<returns>A result that holds every value, or the first error.</returns>", default_doc],
            f"public static {out} Sequence<{generics}>(this IEnumerable<{item}> source)", ["T", *errors],
            ["source = EnsureSource(source);", "List<T> values = CreateList<T>(CountHint(source));", f"foreach ({item} result in source)", "{",
             *[f"    {x}" if x else "" for x in stop], "}", "", f"return new({SUCCESS}, values.ToArray(), null);"], (SLOT_NAMES,)),
        method(
            ["<summary>Calls <paramref name=\"selector\"/> on each item in order and collects the success values. It stops at the first",
             "error and returns it; <paramref name=\"selector\"/> is not called for the later items.</summary>",
             '<typeparam name="TSource">The item type.</typeparam>', *tp, '<param name="source">The items.</param>',
             '<param name="selector">Called with each item.</param>',
             "<returns>A result that holds every value, or the first error.</returns>", default_doc],
            f"public static {out} Traverse<{', '.join(['TSource', 'T', *errors])}>(this IEnumerable<TSource> source, Func<TSource, {item}> selector)", ["T", *errors],
            ["source = EnsureSource(source);", "selector = EnsureSelector(selector);", "List<T> values = CreateList<T>(CountHint(source));",
             "foreach (TSource element in source)", "{", f"    {item} result = selector(element);",
             *[f"    {x}" if x else "" for x in stop], "}", "", f"return new({SUCCESS}, values.ToArray(), null);"], (SLOT_NAMES,)),
        method(
            ["<summary>Splits <paramref name=\"source\"/> into its success values and its errors, both in order. Each error is converted",
             "to <see cref=\"Error\"/> as by <c>ToResult</c>.</summary>", *tp, '<param name="source">The results.</param>',
             "<returns>The success values and the errors.</returns>",
             '<exception cref="InvalidOperationException">A result is <see langword="default"/>.</exception>'],
            f"public static (T[] Successes, Error[] Errors) Partition<{generics}>(this IEnumerable<{item}> source)", ["T", *errors],
            ["List<T> successes = [];", "List<Error> errors = [];", f"foreach ({item} result in EnsureSource(source))", "{",
             "    TypedResultLogic.EnsureInitialized(result.Tag);", f"    if (result.Tag == {SUCCESS})", "        successes.Add(result.RawValue);",
             "    else", "        errors.Add(result.CurrentError());", "}", "", "return (successes.ToArray(), errors.ToArray());"], (SLOT_NAMES,)),
    ]
    plain, net9 = [], []
    for task, token, priority in (("Task", False, True), ("Task", True, True), ("ValueTask", False, False), ("ValueTask", True, False)):
        selector = f"Func<TSource, {'CancellationToken, ' if token else ''}{task}<{item}>>"
        (net9 if task == "ValueTask" else plain).append(method(
            ["<summary>Calls <paramref name=\"selector\"/> on each item in order and awaits it before the next one. It stops at the first",
             "error and returns it; <paramref name=\"selector\"/> is not called for the later items.</summary>",
             '<typeparam name="TSource">The item type.</typeparam>', *tp, '<param name="source">The items.</param>',
             f'<param name="selector">Called with each item{" and the token" if token else ""}.</param>',
             '<param name="cancellationToken">Checked before each item.</param>',
             "<returns>A result that holds every value, or the first error.</returns>", default_doc],
            f"public static async {task}<{out}> TraverseAsync<{', '.join(['TSource', 'T', *errors])}>(this IEnumerable<TSource> source, {selector} selector, {CT_PARAM})",
            ["T", *errors],
            ["source = EnsureSource(source);", "selector = EnsureDelegate(selector);", "List<T> values = CreateList<T>(CountHint(source));",
             "foreach (TSource element in source)", "{", "    cancellationToken.ThrowIfCancellationRequested();",
             f"    {item} result = await selector(element{', cancellationToken' if token else ''}).ConfigureAwait(false);",
             *[f"    {x}" if x else "" for x in stop], "}", "", f"return new({SUCCESS}, values.ToArray(), null);"],
            (SLOT_NAMES, PRIORITY) if priority else (SLOT_NAMES,)))
    for task in ("Task", "ValueTask"):
        plain.append(method(
            ["<summary>Awaits the results of <paramref name=\"source\"/> in order. It stops at the first error and returns it; the tasks",
             "after it are not awaited, and a fault of theirs is observed so it does not surface as an unobserved exception.</summary>",
             *tp, '<param name="source">The pending results.</param>',
             '<param name="cancellationToken">Checked before each result and observed while awaiting it.</param>',
             "<returns>A result that holds every value, or the first error.</returns>", default_doc],
            f"public static async {task}<{out}> SequenceAsync<{generics}>(this IEnumerable<{task}<{item}>> source, {CT_PARAM})",
            ["T", *errors],
            ["source = EnsureSource(source);", "List<T> values = CreateList<T>(CountHint(source));", "int started = 0;", "bool drained = false;",
             "try", "{", f"    foreach ({task}<{item}> pending in source)", "    {", "        cancellationToken.ThrowIfCancellationRequested();",
             "        started++;", f"        {item} result = await pending{AWAIT_TOKEN};",
             *[f"        {x}" if x else "" for x in stop], "    }", "", "    drained = true;", "}", "finally", "{", "    if (!drained)",
             f"        ObserveRemaining(source, started, static t => t{'.AsTask()' if task == 'ValueTask' else ''});", "}", "",
             f"return new({SUCCESS}, values.ToArray(), null);"],
            (SLOT_NAMES,)))
    return [], [], join([*members, *plain, *gated(net9)])


FAMILIES = {"Bind": bind, "Map": map_, "Ensure": ensure, "MapError": map_error, "Recover": recover, "Compensate": compensate,
            "Tap": tap, "TapError": tap_error, "TryCatch": try_catch, "Finally": finally_, "Select": select, "Combine": combine,
            "GetValue": get_value, "Collections": collections}
ERRORS_USING = {"TapError", "GetValue", "Collections"}


def library_file(n: int, family: str) -> str | None:
    produced = FAMILIES[family](n)
    if family == "Recover":
        struct, extensions = produced, []
    else:
        ops, more_struct, more_extensions = produced if isinstance(produced, tuple) else (produced, [], [])
        struct, extensions = emit(n, family, ops)
        struct = join([part for part in (struct, more_struct) if part])
        extensions = join([part for part in (extensions, more_extensions) if part])
    if not struct and not extensions:
        return None
    text = "\n".join([*struct, *extensions])
    usings = [u for u, used in (
        ("using System.Diagnostics.CodeAnalysis;", "[SuppressMessage(" in text),
        ("using System.Runtime.CompilerServices;", "[OverloadResolutionPriority(" in text),
        ("using CSharpEssentials.Core;", ".WithCancellation(" in text),
        ("using CSharpEssentials.Errors;", family in ERRORS_USING),
    ) if used]
    head = [*usings, ""] if usings else []
    body = ["", f"public readonly partial struct {result('T', slots(n))}", "{", *struct, "}"] if struct else []
    tail = ["", "public static partial class ResultExtensions", "{", *extensions, "}"] if extensions else []
    return "\n".join([
        HEADER,
        *head,
        "namespace CSharpEssentials.ResultPattern;",
        *body,
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


class RawTest:
    """A test whose body does not fit Test: query syntax, static calls, collections, awaited calls. net9: the member is net9.0-only."""

    def __init__(self, name: str, body: list[str], is_async: bool = False):
        self.name, self.body, self.is_async, self.net9 = name, body, is_async, False

    def lines(self) -> list[str]:
        signature = f"public async Task {self.name}()" if self.is_async else f"public void {self.name}()"
        return ["    [Fact]", f"    {signature}", "    {", *[f"        {b}" if b else "" for b in self.body], "    }"]


def code_of(error: str) -> str:
    return f"{FIXTURE[error]}.Code"


def tap_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests: list = [Test("Tap_Should_CallTheAction_When_TheResultHoldsAValue", src, OK, "Tap(_ => calls++)", src, OK, 1)]
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"Tap_Should_PassTheErrorThrough_When_{case}", src, value, "Tap(_ => calls++)", src, value, 0, e))
    tests.append(Test("Tap_Should_Throw_When_TheResultIsDefault", src, "default", "Tap(_ => calls++)", None, None, 0, throws="InvalidOperationException"))
    tap_if = "TapIf(v => { calls++; return v > 0; }, _ => calls++)"
    tests.append(Test("TapIf_Should_CallTheAction_When_ThePredicateAllowsTheValue", src, OK, tap_if, src, OK, 2))
    tests.append(Test("TapIf_Should_SkipTheAction_When_ThePredicateRejectsTheValue", src, FAILING_OK, tap_if, src, FAILING_OK, 1))
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"TapIf_Should_CallNeitherDelegate_When_{case}", src, value, tap_if, src, value, 0, e))
    tests.append(Test("TapIf_Should_Throw_When_TheResultIsDefault", src, "default", tap_if, None, None, 0, throws="InvalidOperationException"))
    return tests


def tap_error_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests: list = []
    for e in errors:
        call = f"TapError(({e} _) => calls++)"
        prefix = f"TapError{e}"
        tests.append(Test(f"{prefix}_Should_SkipTheAction_When_TheResultHoldsAValue", src, OK, call, src, OK, 0))
        for (case, value), x in zip(source_cases(errors), errors):
            tests.append(Test(f"{prefix}_Should_{'CallTheAction' if x == e else 'PassTheErrorThrough'}_When_{case}", src, value, call, src, value, 1 if x == e else 0, x))
        tests.append(Test(f"{prefix}_Should_Throw_When_TheResultIsDefault", src, "default", call, None, None, 0, throws="InvalidOperationException"))
    any_call = "TapError((Error _) => calls++)"
    tests.append(Test("TapErrorAny_Should_SkipTheAction_When_TheResultHoldsAValue", src, OK, any_call, src, OK, 0))
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(RawTest(f"TapErrorAny_Should_PassTheConvertedError_When_{case}", [
            f"{src} source = {value};", "string? code = null;", "",
            f"{src} result = source.TapError((Error error) => code = error.Code);", "",
            f"code.Should().Be({code_of(e)});", f"result.Should().Be(source);",
        ]))
    tests.append(Test("TapErrorAny_Should_Throw_When_TheResultIsDefault", src, "default", any_call, None, None, 0, throws="InvalidOperationException"))
    e = errors[-1]
    for kind, delegate, net9, _ in TAP_ERROR_CELLS:
        wrap = {"this": "source", "Task": "Task.FromResult(source)", "ValueTask": f"new ValueTask<{src}>(source)"}[kind]
        for case, value, error in (("TheResultHoldsAValue", OK, None), (case_name(e), FIXTURE[e], e)):
            test = RawTest(f"TapErrorAnyAsync_{RECEIVER_NAMES[kind]}{HANDLER_NAMES[delegate]}_Should_{'PassTheConvertedError' if error else 'SkipTheAction'}_When_{case}", [
                f"{src} source = {value};", "string? code = null;", "",
                f"{src} result = await {wrap}.TapErrorAsync((Error error) => {{ code = error.Code; return {delegate}.CompletedTask; }});", "",
                f"code.Should().Be({code_of(error) if error else 'null'});", "result.Should().Be(source);",
            ], True)
            test.net9 = net9
            tests.append(test)
    return tests


def try_catch_tests(n: int) -> list:
    if n == MAX_SLOTS:
        return []
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    out = result("string", errors + ["FormatException"])
    call = lambda body: f"TryCatch<string, FormatException>(v => {{ calls++; {body}; }})"  # noqa: E731
    tests: list = [
        Test("TryCatch_Should_ReturnTheValue_When_TheFunctionReturns", src, OK, call("return TypedFixture.Text(v)"), out, OTHER, 1),
        Test("TryCatch_Should_ReturnTheException_When_TheFunctionThrowsTheCaughtType", src, OK, call("throw TypedFixture.Format"), out, "TypedFixture.Format", 1),
        Test("TryCatch_Should_Throw_When_TheFunctionThrowsAnotherType", src, OK, call("throw new InvalidOperationException()"), None, None, 1, throws="InvalidOperationException"),
        Test("TryCatch_Should_Throw_When_TheFunctionReturnsNull", src, OK, call("return null!"), None, None, 1, throws="ArgumentNullException"),
    ]
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"TryCatch_Should_PassTheErrorThrough_When_{case}", src, value, call("return TypedFixture.Text(v)"), out, value, 0, e))
    tests.append(Test("TryCatch_Should_Throw_When_TheResultIsDefault", src, "default", call("return TypedFixture.Text(v)"), None, None, 0, throws="InvalidOperationException"))
    return tests


def finally_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    func = "Finally(r => { calls++; return r.IsSuccess; })"
    action = "Finally(_ => { calls++; })"
    tests: list = [Test("Finally_Should_ReturnTheFunctionValue_When_TheResultHoldsAValue", src, OK, func, "bool", "true", 1)]
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"Finally_Should_ReturnTheFunctionValue_When_{case}", src, value, func, "bool", "false", 1))
    tests.append(Test("Finally_Should_PassTheDefaultResult_When_TheResultIsDefault", src, "default", "Finally(r => { calls++; return r.HasValue; })", "bool", "false", 1))
    tests.append(Test("FinallyAction_Should_CallTheAction_When_TheResultHoldsAValue", src, OK, action, src, OK, 1))
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"FinallyAction_Should_CallTheAction_When_{case}", src, value, action, src, value, 1, e))
    tests.append(Test("FinallyAction_Should_CallTheAction_When_TheResultIsDefault", src, "default", action, src, "default", 1))
    return tests


def select_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    out = result("string", errors)
    call = f"Select(v => {counted('TypedFixture.Text(v)')})"
    tests: list = [Test("Select_Should_MapTheValue_When_TheResultHoldsAValue", src, OK, call, out, OTHER, 1)]
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(Test(f"Select_Should_PassTheErrorThrough_When_{case}", src, value, call, out, value, 0, e))
    tests.append(Test("Select_Should_Throw_When_TheResultIsDefault", src, "default", call, None, None, 0, throws="InvalidOperationException"))
    tests.append(Test("Select_Should_Throw_When_TheSelectorReturnsNull", src, OK, f"Select<string>(v => {counted('null!')})", None, None, 1, throws="ArgumentNullException"))

    def query(name: str, outer: str, inner_type: str, inner: str, out_type: str, expected: str, held: str | None = None) -> RawTest:
        body = [f"{src} source = {outer};", f"{inner_type} other = {inner};", "",
                f"{out_type} result = from a in source from b in other select TypedFixture.Text(a + b);", "",
                f"{out_type} expected = {expected};", "result.Should().Be(expected);"]
        if held:
            body.append(f"result.Value.Should().{'Be' if held in STRUCTS else 'BeSameAs'}({FIXTURE[held]});")
        return RawTest(name, body)

    tests.append(query("SelectMany_Should_ProjectBothValues_When_BothResultsHoldAValue", OK, src, "7", out, "TypedFixture.Text(12)"))
    tests.append(query("SelectMany_Should_ReturnTheInnerError_When_TheInnerResultFails", OK, src, FIXTURE[errors[-1]], out, FIXTURE[errors[-1]], errors[-1]))
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(query(f"SelectMany_Should_PassTheErrorThrough_When_{case}", value, src, "7", out, value, e))
    tests.append(RawTest("SelectMany_Should_Throw_When_TheResultIsDefault", [
        f"{src} source = default;", f"{src} other = 7;", "",
        f"Action act = () => _ = from a in source from b in other select TypedFixture.Text(a + b);", "",
        "act.Should().Throw<InvalidOperationException>();",
    ]))
    for m in range(1, MAX_SLOTS - n + 1):
        added = ADDED_TYPES[:m]
        inner_type = result("int", added)
        widened = result("string", errors + added)
        prefix = f"SelectManyWiden{m}"
        tests.append(query(f"{prefix}_Should_ProjectBothValues_When_BothResultsHoldAValue", OK, inner_type, "7", widened, "TypedFixture.Text(12)"))
        for a in added:
            tests.append(query(f"{prefix}_Should_MoveTheInnerErrorToItsAddedSlot_When_TheInnerResultHolds{a}", OK, inner_type, FIXTURE[a], widened, FIXTURE[a], a))
        for (case, value), e in zip(source_cases(errors), errors):
            tests.append(query(f"{prefix}_Should_PassTheErrorThrough_When_{case}", value, inner_type, "7", widened, value, e))
    return tests


def combine_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests: list = []
    for k in (2, 8):
        values = [str(v) for v in range(1, k + 1)]
        tuple_type = "(" + ", ".join(["int"] * k) + ")"
        out = result(tuple_type, errors)
        tests.append(RawTest(f"Combine{k}_Should_ReturnEveryValue_When_EveryResultHoldsAValue", [
            *[f"{src} {COUNT_WORDS[i]} = {v};" for i, v in enumerate(values)], "",
            f"{out} result = {src}.Combine({', '.join(COUNT_WORDS[:k])});", "",
            f"{out} expected = ({', '.join(values)});", "result.Should().Be(expected);",
        ]))
    out = result("(int, int, int)", errors)
    for (case, value), e in zip(source_cases(errors), errors):
        tests.append(RawTest(f"Combine_Should_ReturnTheFirstErrorWithoutLookingFurther_When_{case}", [
            f"{src} first = 1;", f"{src} second = {value};", f"{src} third = default;", "",
            f"{out} result = {src}.Combine(first, second, third);", "",
            f"{out} expected = {value};", "result.Should().Be(expected);",
        ]))
    tests.append(RawTest("Combine_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault", [
        f"{src} first = default;", f"{src} second = {FIXTURE[errors[0]]};", "",
        f"Action act = () => _ = {src}.Combine(first, second);", "",
        "act.Should().Throw<InvalidOperationException>();",
    ]))
    return tests


def get_value_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    tests: list = [
        Test("GetValueOrDefault_Should_ReturnTheValue_When_TheResultHoldsAValue", src, OK, "GetValueOrDefault()", "int", OK, 0),
        Test("GetValueOrDefault_Should_ReturnDefault_When_TheResultIsDefault", src, "default", "GetValueOrDefault()", "int", "0", 0),
        Test("GetValueOr_Should_ReturnTheValue_When_TheResultHoldsAValue", src, OK, f"GetValueOr({RECOVERED})", "int", OK, 0),
        Test("GetValueOr_Should_ReturnTheFallback_When_TheResultIsDefault", src, "default", f"GetValueOr({RECOVERED})", "int", RECOVERED, 0),
        Test("GetValueOrThrow_Should_ReturnTheValue_When_TheResultHoldsAValue", src, OK, "GetValueOrThrow()", "int", OK, 0),
        Test("GetValueOrThrow_Should_Throw_When_TheResultIsDefault", src, "default", "GetValueOrThrow()", None, None, 0, throws="InvalidOperationException"),
        Test("GetValueOrAny_Should_ReturnTheValue_When_TheResultHoldsAValue", src, OK, "GetValueOr((Error e) => { calls++; return e.Code.Length; })", "int", OK, 0),
        Test("GetValueOrAny_Should_Throw_When_TheResultIsDefault", src, "default", "GetValueOr((Error e) => { calls++; return e.Code.Length; })", None, None, 0, throws="InvalidOperationException"),
    ]
    for (case, value), e in zip(source_cases(errors), errors):
        tests += [
            Test(f"GetValueOrDefault_Should_ReturnDefault_When_{case}", src, value, "GetValueOrDefault()", "int", "0", 0),
            Test(f"GetValueOr_Should_ReturnTheFallback_When_{case}", src, value, f"GetValueOr({RECOVERED})", "int", RECOVERED, 0),
            Test(f"GetValueOrThrow_Should_Throw_When_{case}", src, value, "GetValueOrThrow()", None, None, 0, throws="InvalidOperationException"),
            Test(f"GetValueOrAny_Should_CallTheFallbackWithTheConvertedError_When_{case}", src, value, "GetValueOr((Error e) => { calls++; return e.Code.Length; })", "int", f"{code_of(e)}.Length", 1),
        ]
    for kind in ("Task", "ValueTask"):
        wrap = "Task.FromResult(source)" if kind == "Task" else f"new ValueTask<{src}>(source)"
        e = errors[-1]
        for name, value, call, expected in (
            ("GetValueOrDefaultAsync_{k}Source_Should_ReturnTheValue_When_TheResultHoldsAValue", OK, "GetValueOrDefaultAsync()", OK),
            (f"GetValueOrDefaultAsync_{{k}}Source_Should_ReturnDefault_When_{case_name(e)}", FIXTURE[e], "GetValueOrDefaultAsync()", "0"),
            (f"GetValueOrAsync_{{k}}Source_Should_ReturnTheFallback_When_{case_name(e)}", FIXTURE[e], f"GetValueOrAsync({RECOVERED})", RECOVERED),
            (f"GetValueOrAnyAsync_{{k}}Source_Should_CallTheFallbackWithTheConvertedError_When_{case_name(e)}", FIXTURE[e], "GetValueOrAsync((Error e) => e.Code.Length)", f"{code_of(e)}.Length"),
        ):
            tests.append(RawTest(name.format(k=kind), [f"{src} source = {value};", "", f"int result = await {wrap}.{call};", "", f"result.Should().Be({expected});"], True))
    return tests


def collection_tests(n: int) -> list:
    errors = SLOT_TYPES[:n]
    src = result("int", errors)
    out = result("int[]", errors)
    first, last = errors[0], errors[-1]
    tests: list = [
        RawTest("Sequence_Should_ReturnEveryValue_When_EveryResultHoldsAValue", [
            f"List<{src}> source = [5, 7];", "", f"{out} result = source.Sequence();", "",
            "result.TryGetValue(out int[] values).Should().BeTrue();", "values.Should().Equal(5, 7);"]),
        RawTest(f"Sequence_Should_ReturnTheFirstErrorWithoutLookingFurther_When_{case_name(first)}", [
            f"List<{src}> source = [5, {FIXTURE[first]}, default];", "", f"{out} result = source.Sequence();", "",
            f"{out} expected = {FIXTURE[first]};", "result.Should().Be(expected);"]),
        RawTest("Sequence_Should_Throw_When_AResultBeforeTheFirstErrorIsDefault", [
            f"List<{src}> source = [5, default, {FIXTURE[first]}];", "", "Action act = () => source.Sequence();", "",
            "act.Should().Throw<InvalidOperationException>();"]),
        RawTest(f"Traverse_Should_StopCallingTheSelector_When_ItReturns{last}", [
            "int[] items = [1, 2, 3];", "int calls = 0;", "", f"{out} result = items.Traverse(v => {{ calls++; return v == 2 ? ({src}){FIXTURE[last]} : ({src})v; }});", "",
            "calls.Should().Be(2);", f"{out} expected = {FIXTURE[last]};", "result.Should().Be(expected);"]),
        RawTest("Traverse_Should_ReturnEveryValue_When_TheSelectorAlwaysSucceeds", [
            "int[] items = [1, 2];", "", f"{out} result = items.Traverse(v => ({src})(v * 10));", "",
            "result.TryGetValue(out int[] values).Should().BeTrue();", "values.Should().Equal(10, 20);"]),
        RawTest("Partition_Should_SplitTheValuesAndTheConvertedErrors_When_TheResultsAreMixed", [
            f"List<{src}> source = [5, {', '.join(FIXTURE[e] for e in errors)}, 7];", "",
            "(int[] successes, Error[] failures) = source.Partition();", "",
            "successes.Should().Equal(5, 7);", f"failures.Select(e => e.Code).Should().Equal({', '.join(code_of(e) for e in errors)});"]),
        RawTest("Partition_Should_Throw_When_AResultIsDefault", [
            f"List<{src}> source = [5, default];", "", "Action act = () => source.Partition();", "",
            "act.Should().Throw<InvalidOperationException>();"]),
    ]
    for kind in ("Task", "ValueTask"):
        made = (lambda v: f"Task.FromResult<{src}>({v})") if kind == "Task" else (lambda v: f"new ValueTask<{src}>({v})")
        tests += [
            RawTest(f"SequenceAsync_{kind}_Should_ReturnEveryValue_When_EveryResultHoldsAValue", [
                f"List<{kind}<{src}>> source = [{made('5')}, {made('7')}];", "", f"{out} result = await source.SequenceAsync();", "",
                "result.TryGetValue(out int[] values).Should().BeTrue();", "values.Should().Equal(5, 7);"], True),
            RawTest(f"SequenceAsync_{kind}_Should_ReturnTheFirstErrorWithoutAwaitingTheRest_When_{case_name(first)}", [
                f"TaskCompletionSource<{src}> never = new();",
                f"List<{kind}<{src}>> source = [{made(FIXTURE[first])}, {'never.Task' if kind == 'Task' else f'new ValueTask<{src}>(never.Task)'}];", "",
                f"{out} result = await source.SequenceAsync();", "", f"{out} expected = {FIXTURE[first]};", "result.Should().Be(expected);"], True),
        ]
        handler_return = (lambda v: f"Task.FromResult<{src}>({v})") if kind == "Task" else (lambda v: f"new ValueTask<{src}>({v})")
        gate = kind == "ValueTask"
        for token in (False, True):
            params = "(v, _)" if token else "v"
            test = RawTest(f"TraverseAsync_{kind}{'WithToken' if token else ''}_Should_StopCallingTheSelector_When_ItReturns{last}", [
                "int[] items = [1, 2, 3];", "int calls = 0;", "",
                f"{out} result = await items.TraverseAsync({params} => {{ calls++; return {handler_return(f'v == 2 ? ({src}){FIXTURE[last]} : ({src})v')}; }});", "",
                "calls.Should().Be(2);", f"{out} expected = {FIXTURE[last]};", "result.Should().Be(expected);"], True)
            test.net9 = gate
            tests.append(test)
        tests.append(RawTest(f"TraverseAsync_{kind}_Should_Throw_When_TheTokenIsCanceled", [
            "int[] items = [1];", "using CancellationTokenSource cancellation = new();", "await cancellation.CancelAsync();", "",
            f"Func<Task> act = async () => await items.TraverseAsync(v => {handler_return(f'({src})v')}, cancellation.Token);", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();"], True))
        tests[-1].net9 = gate
    return tests


TEST_FAMILIES = {"Bind": bind_tests, "Map": map_tests, "Ensure": ensure_tests, "MapError": map_error_tests, "Recover": recover_tests, "Compensate": compensate_tests,
                 "Tap": tap_tests, "TapError": tap_error_tests, "TryCatch": try_catch_tests, "Finally": finally_tests, "Select": select_tests,
                 "Combine": combine_tests, "GetValue": get_value_tests, "Collections": collection_tests}


def test_file(n: int, family: str, tests: list) -> str | None:
    if not tests:
        return None
    plain = join([t.lines() for t in tests if not getattr(t, "net9", False)])
    net9 = join([t.lines() for t in tests if getattr(t, "net9", False)])
    body = join([part for part in (plain, [f"#if {NET9_ONLY}", *net9, "#endif"] if net9 else []) if part])
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
    def __init__(self, family: str, name: str, prefix: str, src: str, arg: str, ret: str | None, out: str, extras: str, lam: str, cases: list[AsyncCase],
                 leading: str = "", type_args: str = "", cells: list[tuple[str, str, bool, bool]] | None = None, checks_token: bool | None = None, throws_on_default: bool = True):
        self.family, self.name, self.prefix, self.src, self.arg, self.ret, self.out = family, name, prefix, src, arg, ret, out
        self.extras, self.lam, self.cases = extras, lam, cases
        self.checks_token = family in ("Map", "MapError") if checks_token is None else checks_token
        # leading: arguments before the handler; type_args: explicit type arguments of the extension call.
        self.leading, self.type_args, self.cells, self.throws_on_default = leading, type_args, cells or CELLS[family], throws_on_default

    def call(self, target: str, token: str = "") -> str:
        return f"{target}.{self.name}Async{self.type_args}({self.leading}handler{self.extras}{token})"


def handler(op: AsyncOp, delegate: str, body: str) -> str:
    if op.ret is None:
        if delegate == "sync":
            return f"Action<{op.arg}> handler = _ => calls++;"
        return f"Func<{op.arg}, {delegate}> handler = _ => {{ calls++; return {delegate}.CompletedTask; }};"
    lam = op.lam if re.search(rf"\b{op.lam}\b", body) else "_"
    if delegate == "sync":
        return f"Func<{op.arg}, {op.ret}> handler = {lam} => {{ calls++; return {body}; }};"
    wrapped = f"Task.FromResult<{op.ret}>({body})" if delegate == "Task" else f"new ValueTask<{op.ret}>({body})"
    return f"Func<{op.arg}, {delegate}<{op.ret}>> handler = {lam} => {{ calls++; return {wrapped}; }};"


def pending_handler(op: AsyncOp, delegate: str) -> str:
    if op.ret is None:
        pending = "new TaskCompletionSource().Task"
        wrapped = pending if delegate == "Task" else f"new ValueTask({pending})"
        return f"Func<{op.arg}, {delegate}> handler = _ => {{ calls++; return {wrapped}; }};"
    pending = f"new TaskCompletionSource<{op.ret}>().Task"
    wrapped = pending if delegate == "Task" else f"new ValueTask<{op.ret}>({pending})"
    return f"Func<{op.arg}, {delegate}<{op.ret}>> handler = _ => {{ calls++; return {wrapped}; }};"


def receiver(op: AsyncOp, kind: str, value: str = "source") -> str:
    return {"this": value, "Task": f"Task.FromResult({value})", "ValueTask": f"new ValueTask<{op.src}>({value})"}[kind]


def fact(name: str, body: list[str]) -> list[str]:
    return ["    [Fact]", f"    public async Task {name}()", "    {", *[f"        {b}" if b else "" for b in body], "    }"]


def async_tests(op: AsyncOp, kind: str, delegate: str, token: bool) -> list[list[str]]:
    cell = f"{op.prefix}Async_{RECEIVER_NAMES[kind]}{HANDLER_NAMES[delegate]}"
    call = op.call(receiver(op, kind))
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
    tests.append([] if not op.throws_on_default else fact(f"{cell}_Should_Throw_When_TheResultIsDefault", [
        f"{op.src} source = default;", "int calls = 0;", handler(op, delegate, first.body), "",
        f"Func<Task> act = async () => await {call};", "",
        "await act.Should().ThrowAsync<InvalidOperationException>();", "calls.Should().Be(0);",
    ]))
    if kind != "this":
        tests.append(fact(f"{cell}_Should_Throw_When_TheTokenIsCanceledWhileTheSourceIsPending", [
            f"TaskCompletionSource<{op.src}> pending = new();", "using CancellationTokenSource cancellation = new();",
            "await cancellation.CancelAsync();", "int calls = 0;", handler(op, delegate, first.body), "",
            f"Func<Task> act = async () => await {op.call('pending.Task' if kind == 'Task' else f'new ValueTask<{op.src}>(pending.Task)', ', cancellation.Token')};", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();", "calls.Should().Be(0);",
        ]))
    if op.checks_token and delegate != "sync":
        tests.append(fact(f"{cell}_Should_Throw_When_TheTokenIsCanceledBeforeTheHandlerIsCalled", [
            f"{op.src} source = {first.source};", "using CancellationTokenSource cancellation = new();",
            "await cancellation.CancelAsync();", "int calls = 0;", handler(op, delegate, first.body), "",
            f"Func<Task> act = async () => await {op.call(receiver(op, kind), ', cancellation.Token')};", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();", "calls.Should().Be(0);",
        ]))
    elif kind == "this" and token:
        tests.append(fact(f"{cell}_Should_Throw_When_TheTokenIsCanceledWhileTheHandlerIsPending", [
            f"{op.src} source = {first.source};", "using CancellationTokenSource cancellation = new();",
            "await cancellation.CancelAsync();", "int calls = 0;", pending_handler(op, delegate), "",
            f"Func<Task> act = async () => await {op.call('source', ', cancellation.Token')};", "",
            "await act.Should().ThrowAsync<OperationCanceledException>();", "calls.Should().Be(1);",
        ]))
    if op.family in UNABANDONED and delegate != "sync":
        returned = delegate if kind == "this" else kind
        tests.append(fact(f"{cell}_Should_FinishTheRunningHandler_When_TheTokenIsCanceledWhileItRuns", [
            f"{op.src} source = {first.source};", "using CancellationTokenSource cancellation = new();",
            f"TaskCompletionSource<{op.ret}> running = new();", "int calls = 0;",
            f"Func<{op.arg}, {delegate}<{op.ret}>> handler = _ => {{ calls++; return {'running.Task' if delegate == 'Task' else f'new ValueTask<{op.ret}>(running.Task)'}; }};", "",
            f"{returned}<{op.out}> call = {op.call(receiver(op, kind), ', cancellation.Token')};",
            "await cancellation.CancelAsync();", f"running.SetResult({first.body});", f"{op.out} result = await call;", "",
            "calls.Should().Be(1);", f"{op.out} expected = {first.expected};", "result.Should().Be(expected);",
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
    elif family == "Tap":
        ops.append(AsyncOp(family, "Tap", "Tap", src, "int", None, src, "", "v", [
            AsyncCase("CallTheAction_When_TheResultHoldsAValue", OK, "", OK, 1),
            AsyncCase(f"PassTheErrorThrough_When_{case_name(errors[-1])}", FIXTURE[errors[-1]], "", FIXTURE[errors[-1]], 0, errors[-1]),
        ], checks_token=True))
        ops.append(AsyncOp(family, "TapIf", "TapIf", src, "int", None, src, "", "v", [
            AsyncCase("CallTheAction_When_ThePredicateAllowsTheValue", OK, "", OK, 1),
            AsyncCase("SkipTheAction_When_ThePredicateRejectsTheValue", FAILING_OK, "", FAILING_OK, 0),
            AsyncCase(f"PassTheErrorThrough_When_{case_name(errors[-1])}", FIXTURE[errors[-1]], "", FIXTURE[errors[-1]], 0, errors[-1]),
        ], leading="v => v > 0, "))
    elif family == "TapError":
        for e in errors:
            ops.append(AsyncOp(family, "TapError", f"TapError{e}", src, e, None, src, "", "e", [
                AsyncCase(f"CallTheAction_When_{case_name(e)}", FIXTURE[e], "", FIXTURE[e], 1, e),
                AsyncCase("SkipTheAction_When_TheResultHoldsAValue", OK, "", OK, 0),
            ], cells=TAP_ERROR_CELLS))
    elif family == "TryCatch" and n < MAX_SLOTS:
        out = result("string", errors + ["FormatException"])
        ops.append(AsyncOp(family, "TryCatch", "TryCatch", src, "int", "string", out, "", "v", [
            AsyncCase("ReturnTheValue_When_TheFunctionReturns", OK, "TypedFixture.Text(v)", OTHER, 1),
            passthrough(errors, "TypedFixture.Text(v)"),
        ], type_args=f"<{', '.join(['int', *errors, 'string', 'FormatException'])}>"))
    elif family == "Finally":
        ops.append(AsyncOp(family, "Finally", "Finally", src, src, "bool", "bool", "", "r", [
            AsyncCase("ReturnTheFunctionValue_When_TheResultHoldsAValue", OK, "r.IsSuccess", "true", 1),
            AsyncCase(f"ReturnTheFunctionValue_When_{case_name(errors[-1])}", FIXTURE[errors[-1]], "r.IsSuccess", "false", 1),
        ], throws_on_default=False))
        ops.append(AsyncOp(family, "Finally", "FinallyAction", src, src, None, src, "", "r", [
            AsyncCase("CallTheAction_When_TheResultHoldsAValue", OK, "", OK, 1),
            AsyncCase(f"CallTheAction_When_{case_name(errors[-1])}", FIXTURE[errors[-1]], "", FIXTURE[errors[-1]], 1, errors[-1]),
        ], cells=FINALLY_ACTION_CELLS, throws_on_default=False))
    elif family == "Select":
        ops.append(AsyncOp(family, "Select", "Select", src, "int", "string", result("string", errors), "", "v", [
            AsyncCase("MapTheValue_When_TheResultHoldsAValue", OK, "TypedFixture.Text(v)", OTHER, 1),
            passthrough(errors, "TypedFixture.Text(v)"),
        ]))
    return ops


def async_test_file(n: int, family: str) -> str | None:
    plain: list[list[str]] = []
    net9: list[list[str]] = []
    ops = async_ops(n, family)
    if not ops:
        return None
    for op in ops:
        for kind, delegate, gate, _ in op.cells:
            token = family not in NO_INSTANCE_TOKEN
            (net9 if gate else plain).extend(t for t in async_tests(op, kind, delegate, token) if t)
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

    library = {LIBRARY / f"{type_name(n)}.{family}.cs": text for n in range(1, MAX_SLOTS + 1) for family in FAMILIES if (text := library_file(n, family))}
    tests = {TESTS / f"{type_name(n)}{family}Tests.cs": text for n in range(1, MAX_SLOTS + 1) for family, emit in TEST_FAMILIES.items() if (text := test_file(n, family, emit(n)))}
    tests |= {TESTS / f"{type_name(n)}{family}AsyncTests.cs": text for n in range(1, MAX_SLOTS + 1) for family in CELLS if (text := async_test_file(n, family))}
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
