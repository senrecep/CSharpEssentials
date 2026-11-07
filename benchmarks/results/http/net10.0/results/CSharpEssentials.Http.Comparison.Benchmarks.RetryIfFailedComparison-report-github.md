```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean     | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |---------:|--------:|--------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     | **296.0 ns** | **0.43 ns** | **0.38 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 352.3 ns | 0.14 ns | 0.12 ns |  1.19 |      - |         - |        0.00 |
| Polly_Cached               | False     | 227.1 ns | 0.13 ns | 0.11 ns |  0.77 |      - |         - |        0.00 |
|                            |           |          |         |         |       |        |           |             |
| **CSharpEssentials_Cached**    | **True**      | **472.8 ns** | **0.59 ns** | **0.55 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 546.7 ns | 0.36 ns | 0.31 ns |  1.16 |      - |         - |        0.00 |
| Polly_Cached               | True      | 368.2 ns | 0.80 ns | 0.75 ns |  0.78 |      - |         - |        0.00 |
