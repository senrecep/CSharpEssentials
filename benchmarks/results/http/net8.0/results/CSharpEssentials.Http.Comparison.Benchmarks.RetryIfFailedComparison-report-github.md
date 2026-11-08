```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     | **307.0 ns** | **0.52 ns** | **0.49 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 378.0 ns | 0.15 ns | 0.14 ns |  1.23 |      - |         - |        0.00 |
| Polly_Cached               | False     | 257.7 ns | 0.26 ns | 0.25 ns |  0.84 |      - |         - |        0.00 |
|                            |           |          |         |         |       |        |           |             |
| **CSharpEssentials_Cached**    | **True**      | **478.1 ns** | **0.63 ns** | **0.56 ns** |  **1.00** | **0.0048** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 630.2 ns | 0.70 ns | 0.62 ns |  1.32 |      - |         - |        0.00 |
| Polly_Cached               | True      | 413.6 ns | 0.31 ns | 0.28 ns |  0.86 |      - |         - |        0.00 |
