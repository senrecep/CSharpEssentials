```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |   **1.0030 ns** | **0.0070 ns** | **0.0065 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |   2.3396 ns | 0.0065 ns | 0.0060 ns |   2.33 |    0.02 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |   1.0049 ns | 0.0119 ns | 0.0099 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   |  33.5167 ns | 0.3022 ns | 0.2827 ns |  33.42 |    0.34 | 0.0052 |      88 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |   **1.0990 ns** | **0.0065 ns** | **0.0061 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |   2.3327 ns | 0.0139 ns | 0.0130 ns |   2.12 |    0.02 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |   1.0788 ns | 0.0030 ns | 0.0025 ns |   0.98 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    |  41.3295 ns | 0.4941 ns | 0.4380 ns |  37.61 |    0.43 | 0.0057 |      96 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |   **0.9161 ns** | **0.0191 ns** | **0.0169 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |   1.3738 ns | 0.0108 ns | 0.0090 ns |   1.50 |    0.03 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |   0.6149 ns | 0.0063 ns | 0.0053 ns |   0.67 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 101.3691 ns | 0.7157 ns | 0.6695 ns | 110.69 |    2.06 | 0.0157 |     264 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |   **1.5051 ns** | **0.0316 ns** | **0.0295 ns** |   **1.00** |    **0.03** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |   2.7035 ns | 0.0295 ns | 0.0276 ns |   1.80 |    0.04 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |   1.4569 ns | 0.0038 ns | 0.0029 ns |   0.97 |    0.02 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    |  42.4279 ns | 0.0852 ns | 0.0711 ns |  28.20 |    0.54 | 0.0057 |      96 B |          NA |
