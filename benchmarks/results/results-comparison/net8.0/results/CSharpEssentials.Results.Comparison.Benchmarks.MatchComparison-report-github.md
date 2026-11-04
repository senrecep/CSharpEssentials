```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |   **2.6965 ns** | **0.0213 ns** | **0.0189 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |   2.3575 ns | 0.0029 ns | 0.0025 ns |  0.87 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |   1.0825 ns | 0.0011 ns | 0.0009 ns |  0.40 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   |  42.4105 ns | 0.6953 ns | 0.6164 ns | 15.73 |    0.25 | 0.0057 |      96 B |          NA |
|                                     |            |         |             |           |           |       |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |   **1.1213 ns** | **0.0034 ns** | **0.0032 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |   2.3589 ns | 0.0071 ns | 0.0066 ns |  2.10 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |   1.0825 ns | 0.0005 ns | 0.0004 ns |  0.97 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    |  57.6725 ns | 0.2879 ns | 0.2552 ns | 51.44 |    0.26 | 0.0067 |     112 B |          NA |
|                                     |            |         |             |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |   **2.3524 ns** | **0.0030 ns** | **0.0027 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |   1.3717 ns | 0.0020 ns | 0.0016 ns |  0.58 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |   0.9022 ns | 0.0017 ns | 0.0014 ns |  0.38 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 125.3772 ns | 0.6853 ns | 0.6410 ns | 53.30 |    0.27 | 0.0181 |     304 B |          NA |
|                                     |            |         |             |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |   **1.9639 ns** | **0.0219 ns** | **0.0194 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |   2.7354 ns | 0.0236 ns | 0.0221 ns |  1.39 |    0.02 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |   1.7679 ns | 0.0003 ns | 0.0002 ns |  0.90 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    |  56.8889 ns | 0.3392 ns | 0.3173 ns | 28.97 |    0.32 | 0.0067 |     112 B |          NA |
