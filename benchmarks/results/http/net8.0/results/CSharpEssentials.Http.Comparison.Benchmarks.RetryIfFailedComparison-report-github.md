```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     |   **313.8 ns** |  **0.29 ns** |  **0.24 ns** |  **1.00** |    **0.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 5,137.6 ns | 44.20 ns | 41.34 ns | 16.37 |    0.13 | 0.5112 |    8584 B |       97.55 |
| Polly_Cached               | False     |   259.4 ns |  0.17 ns |  0.16 ns |  0.83 |    0.00 |      - |         - |        0.00 |
|                            |           |            |          |          |       |         |        |           |             |
| **CSharpEssentials_Cached**    | **True**      |   **501.5 ns** |  **1.56 ns** |  **1.45 ns** |  **1.00** |    **0.00** | **0.0048** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 5,783.6 ns | 78.48 ns | 69.57 ns | 11.53 |    0.14 | 0.5112 |    8584 B |       97.55 |
| Polly_Cached               | True      |   411.6 ns |  0.21 ns |  0.17 ns |  0.82 |    0.00 |      - |         - |        0.00 |
