```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | FailFirst | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |---------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Cached**    | **False**     |   **296.7 ns** |  **0.32 ns** |  **0.28 ns** |  **1.00** |    **0.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | False     | 4,849.1 ns | 29.35 ns | 24.51 ns | 16.34 |    0.08 | 0.4501 |    7632 B |       86.73 |
| Polly_Cached               | False     |   257.7 ns |  0.20 ns |  0.19 ns |  0.87 |    0.00 |      - |         - |        0.00 |
|                            |           |            |          |          |       |         |        |           |             |
| **CSharpEssentials_Cached**    | **True**      |   **457.2 ns** |  **0.52 ns** |  **0.48 ns** |  **1.00** |    **0.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_Predicate | True      | 4,924.7 ns | 26.01 ns | 24.33 ns | 10.77 |    0.05 | 0.4501 |    7632 B |       86.73 |
| Polly_Cached               | True      |   398.7 ns |  0.30 ns |  0.25 ns |  0.87 |    0.00 |      - |         - |        0.00 |
