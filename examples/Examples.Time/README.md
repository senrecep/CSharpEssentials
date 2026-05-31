# CSharpEssentials.Time Example

This console application demonstrates date/time utilities from `CSharpEssentials.Time`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **DateTimeProvider** | Abstraction over `DateTime.UtcNow` for testability |
| **DateTime Extensions** | `ToDateOnly()` (NET6+), `ToTimeOnly()` (NET6+) |
| **Custom Provider** | Fixed provider for unit testing |

## Running

```bash
cd examples/Examples.Time
dotnet run
```
