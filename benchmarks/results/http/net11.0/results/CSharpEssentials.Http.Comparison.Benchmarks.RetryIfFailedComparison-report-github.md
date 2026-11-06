```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     |   **295.1 ns** |  **0.27 ns** |  **0.24 ns** |  **1.00** |    **0.00** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 3,887.9 ns | 24.10 ns | 22.54 ns | 13.18 |    0.07 | 0.4349 |    7280 B |       91.00 |
| Polly_Cached               | False     |   229.8 ns |  0.23 ns |  0.21 ns |  0.78 |    0.00 |      - |         - |        0.00 |
|                            |           |            |          |          |       |         |        |           |             |
| **CSharpEssentials_Cached**    | **True**      |   **451.7 ns** |  **0.33 ns** |  **0.31 ns** |  **1.00** |    **0.00** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 4,071.4 ns | 16.39 ns | 14.53 ns |  9.01 |    0.03 | 0.4349 |    7280 B |       91.00 |
| Polly_Cached               | True      |   372.1 ns |  0.24 ns |  0.21 ns |  0.82 |    0.00 |      - |         - |        0.00 |
