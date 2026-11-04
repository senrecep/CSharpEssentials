```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.5411 ns** | **0.0032 ns** | **0.0030 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |  1.9546 ns | 0.0036 ns | 0.0033 ns |   3.61 |    0.02 |      - |         - |          NA |
| ErrorOr                    | False   |  0.8614 ns | 0.0041 ns | 0.0038 ns |   1.59 |    0.01 |      - |         - |          NA |
| FluentResults              | False   | 43.7827 ns | 0.3738 ns | 0.3497 ns |  80.91 |    0.76 | 0.0124 |     208 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.5727 ns** | **0.0032 ns** | **0.0030 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  1.9656 ns | 0.0027 ns | 0.0025 ns |   3.43 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5827 ns | 0.0064 ns | 0.0053 ns |   1.02 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 69.6296 ns | 1.4515 ns | 1.3577 ns | 121.59 |    2.38 | 0.0191 |     320 B |          NA |
