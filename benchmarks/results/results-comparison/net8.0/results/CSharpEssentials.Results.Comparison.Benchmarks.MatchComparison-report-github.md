```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |   **1.0745 ns** | **0.0098 ns** | **0.0092 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |   2.3153 ns | 0.0079 ns | 0.0074 ns |   2.16 |    0.02 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |   1.0693 ns | 0.0101 ns | 0.0089 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   |  45.6624 ns | 0.1712 ns | 0.1429 ns |  42.50 |    0.37 | 0.0057 |      96 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |   **1.0647 ns** | **0.0005 ns** | **0.0004 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |   2.3065 ns | 0.0037 ns | 0.0033 ns |   2.17 |    0.00 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |   1.0664 ns | 0.0038 ns | 0.0034 ns |   1.00 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    |  56.5816 ns | 0.1606 ns | 0.1341 ns |  53.15 |    0.12 | 0.0067 |     112 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |   **1.0937 ns** | **0.0083 ns** | **0.0078 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |   1.2994 ns | 0.0016 ns | 0.0015 ns |   1.19 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |   0.5514 ns | 0.0154 ns | 0.0136 ns |   0.50 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 132.4267 ns | 0.7465 ns | 0.6983 ns | 121.08 |    1.04 | 0.0181 |     304 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |   **1.6781 ns** | **0.0044 ns** | **0.0037 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |   2.7712 ns | 0.0122 ns | 0.0108 ns |   1.65 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |   1.7048 ns | 0.0012 ns | 0.0010 ns |   1.02 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    |  58.3472 ns | 0.4002 ns | 0.3743 ns |  34.77 |    0.23 | 0.0067 |     112 B |          NA |
