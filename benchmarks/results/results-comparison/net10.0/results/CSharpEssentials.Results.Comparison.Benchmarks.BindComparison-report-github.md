```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.8911 ns** | **0.0040 ns** | **0.0037 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |  1.9525 ns | 0.0016 ns | 0.0014 ns |   2.19 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |  0.8756 ns | 0.0026 ns | 0.0025 ns |   0.98 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 53.8809 ns | 0.3465 ns | 0.3072 ns |  60.47 |    0.41 | 0.0124 |     208 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.5758 ns** | **0.0064 ns** | **0.0053 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  1.8695 ns | 0.0055 ns | 0.0051 ns |   3.25 |    0.03 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5774 ns | 0.0042 ns | 0.0040 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 69.6682 ns | 0.6100 ns | 0.5706 ns | 121.00 |    1.44 | 0.0191 |     320 B |          NA |
