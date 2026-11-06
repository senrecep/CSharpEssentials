```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     |   **306.1 ns** |  **1.66 ns** |  **1.55 ns** |  **1.00** |    **0.01** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 3,794.6 ns | 23.70 ns | 22.17 ns | 12.40 |    0.09 | 0.4425 |    7432 B |       84.45 |
| Polly_Cached               | False     |   225.6 ns |  0.14 ns |  0.12 ns |  0.74 |    0.00 |      - |         - |        0.00 |
|                            |           |            |          |          |       |         |        |           |             |
| **CSharpEssentials_Cached**    | **True**      |   **473.1 ns** |  **0.36 ns** |  **0.32 ns** |  **1.00** |    **0.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 4,055.5 ns | 23.97 ns | 22.43 ns |  8.57 |    0.05 | 0.4425 |    7432 B |       84.45 |
| Polly_Cached               | True      |   371.1 ns |  0.21 ns |  0.19 ns |  0.78 |    0.00 |      - |         - |        0.00 |
