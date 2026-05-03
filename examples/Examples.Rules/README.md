# CSharpEssentials.Rules Example

This console application demonstrates the rule engine from `CSharpEssentials.Rules`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Simple Rules** | Create individual validation rules with `Rule.Create<T>` |
| **Rule Engine** | Evaluate multiple rules where ALL must pass (AND) |
| **Or Rules** | Combine rules where AT LEAST ONE must pass |
| **Result Integration** | Rules return `Result` for composable validation |
| **Error Collection** | Collect all validation failures at once |

## Running

```bash
cd examples/Examples.Rules
dotnet run
```
