```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     | **310.1 ns** | **0.46 ns** | **0.41 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 326.6 ns | 0.31 ns | 0.29 ns |  1.05 |      - |         - |        0.00 |
| Polly_Cached               | False     | 228.9 ns | 0.11 ns | 0.09 ns |  0.74 |      - |         - |        0.00 |
|                            |           |          |         |         |       |        |           |             |
| **CSharpEssentials_Cached**    | **True**      | **458.5 ns** | **0.89 ns** | **0.83 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 535.6 ns | 0.61 ns | 0.57 ns |  1.17 |      - |         - |        0.00 |
| Polly_Cached               | True      | 371.4 ns | 0.08 ns | 0.07 ns |  0.81 |      - |         - |        0.00 |
