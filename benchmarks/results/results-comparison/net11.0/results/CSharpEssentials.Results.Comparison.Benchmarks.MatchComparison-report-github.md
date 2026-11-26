```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.3195 ns** | **0.0039 ns** | **0.0036 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |  1.6078 ns | 0.0026 ns | 0.0021 ns |  1.22 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3315 ns | 0.0055 ns | 0.0049 ns |  1.77 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9809 ns | 0.0029 ns | 0.0027 ns |  0.74 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 17.6349 ns | 0.1206 ns | 0.1128 ns | 13.36 |    0.09 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.1992 ns** | **0.0048 ns** | **0.0040 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |  1.2247 ns | 0.0063 ns | 0.0059 ns |  1.02 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.2917 ns | 0.0051 ns | 0.0048 ns |  1.91 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0825 ns | 0.0054 ns | 0.0050 ns |  0.90 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 18.8436 ns | 0.0821 ns | 0.0728 ns | 15.71 |    0.08 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **0.9778 ns** | **0.0059 ns** | **0.0049 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |  1.1355 ns | 0.0013 ns | 0.0011 ns |  1.16 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.5517 ns | 0.0026 ns | 0.0023 ns |  1.59 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  0.6883 ns | 0.0062 ns | 0.0058 ns |  0.70 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 69.8311 ns | 0.3062 ns | 0.2556 ns | 71.42 |    0.43 | 0.0143 |     240 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **1.6064 ns** | **0.0034 ns** | **0.0032 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |  1.7525 ns | 0.0035 ns | 0.0031 ns |  1.09 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.7626 ns | 0.0023 ns | 0.0019 ns |  1.72 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  1.8409 ns | 0.0043 ns | 0.0040 ns |  1.15 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 18.7367 ns | 0.1572 ns | 0.1471 ns | 11.66 |    0.09 | 0.0057 |      96 B |          NA |
