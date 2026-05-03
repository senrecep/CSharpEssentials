---
paths: ["**/*.cs"]
---

## C# Conventions

- Nullable reference types are enabled globally. Every public API must have explicit nullability (`string?` vs `string`).
- Private fields: `_camelCase`. Properties and methods: `PascalCase`. Locals and parameters: `camelCase`.
- One public type per file; filename must match the type name exactly.
- Prefer `sealed` on concrete classes that are not designed for inheritance.
- Use `readonly` on fields that are set only in constructors.
- Extension methods live in a static class named `<Subject>Extensions` in the appropriate package.
- No `dynamic`. No `object` as a catch-all return type when generics work.
- Functional types (Result, Maybe, Any) use implicit operators for ergonomic construction — maintain this pattern in new types.
- Record types are preferred over classes for immutable value objects.
- `TreatWarningsAsErrors=true` is global — never suppress a warning without understanding why it fires.
