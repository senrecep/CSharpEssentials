```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.0104 ns** | **0.0050 ns** | **0.0042 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3318 ns | 0.0069 ns | 0.0065 ns |   2.31 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  1.0169 ns | 0.0059 ns | 0.0055 ns |   1.01 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 18.8536 ns | 0.2294 ns | 0.2146 ns |  18.66 |    0.22 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.1006 ns** | **0.0098 ns** | **0.0092 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.3115 ns | 0.0073 ns | 0.0068 ns |   2.10 |    0.02 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.1277 ns | 0.0150 ns | 0.0140 ns |   1.02 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 19.3693 ns | 0.1906 ns | 0.1783 ns |  17.60 |    0.21 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **0.6090 ns** | **0.0024 ns** | **0.0020 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.3085 ns | 0.0136 ns | 0.0127 ns |   2.15 |    0.02 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  0.8132 ns | 0.0141 ns | 0.0117 ns |   1.34 |    0.02 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 74.7315 ns | 0.9247 ns | 0.8649 ns | 122.71 |    1.43 | 0.0143 |     240 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **1.6031 ns** | **0.0342 ns** | **0.0303 ns** |   **1.00** |    **0.03** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.7434 ns | 0.0016 ns | 0.0013 ns |   1.71 |    0.03 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  1.5903 ns | 0.0052 ns | 0.0040 ns |   0.99 |    0.02 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 20.0319 ns | 0.2450 ns | 0.2291 ns |  12.50 |    0.27 | 0.0057 |      96 B |          NA |
