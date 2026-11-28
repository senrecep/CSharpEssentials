# CSharpEssentials.Clone example

A console app for [CSharpEssentials.Clone](../../CSharpEssentials.Clone/Readme.MD): a typed `Clone()` on domain objects and the collection extensions.

## Run

```bash
dotnet run --project examples/Examples.Clone
```

## What each section shows

| Section | Shows |
|---|---|
| `ICloneable<T>` on one object | `Person.Clone()` copies the child `Address` and the `Tags` list, so changing the copy leaves the original alone |
| `Clone()` on `IEnumerable<T>` | The extension is lazy: no `Clone()` call happens until `ToList()` |
| Arrays | `array.Clone()` is the shallow `Array.Clone()`; cast to `IEnumerable<T>` to clone the items |
| `Clone()` on `IQueryable<T>` | The query runs first, then the items are cloned in memory into an `EnumerableQuery<T>` |
