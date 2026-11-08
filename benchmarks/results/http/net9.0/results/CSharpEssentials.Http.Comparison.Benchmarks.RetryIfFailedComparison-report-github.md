```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     | **293.6 ns** | **0.66 ns** | **0.62 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 351.2 ns | 0.13 ns | 0.12 ns |  1.20 |      - |         - |        0.00 |
| Polly_Cached               | False     | 245.2 ns | 0.15 ns | 0.13 ns |  0.84 |      - |         - |        0.00 |
|                            |           |          |         |         |       |        |           |             |
| **CSharpEssentials_Cached**    | **True**      | **456.6 ns** | **0.37 ns** | **0.35 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 566.6 ns | 0.34 ns | 0.30 ns |  1.24 |      - |         - |        0.00 |
| Polly_Cached               | True      | 400.0 ns | 0.23 ns | 0.21 ns |  0.88 |      - |         - |        0.00 |
