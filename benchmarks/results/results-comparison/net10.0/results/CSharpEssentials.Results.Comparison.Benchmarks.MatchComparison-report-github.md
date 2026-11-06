```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.0570 ns** | **0.0018 ns** | **0.0017 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3593 ns | 0.0048 ns | 0.0045 ns |   2.23 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  1.0181 ns | 0.0040 ns | 0.0038 ns |   0.96 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 21.7977 ns | 0.1225 ns | 0.1146 ns |  20.62 |    0.11 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.2063 ns** | **0.0027 ns** | **0.0023 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.3368 ns | 0.0038 ns | 0.0034 ns |   1.94 |    0.00 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.1083 ns | 0.0049 ns | 0.0043 ns |   0.92 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 24.6687 ns | 0.1335 ns | 0.1248 ns |  20.45 |    0.11 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **0.7223 ns** | **0.0012 ns** | **0.0011 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.5866 ns | 0.0011 ns | 0.0009 ns |   2.20 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  1.0124 ns | 0.0015 ns | 0.0013 ns |   1.40 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 75.4939 ns | 0.3767 ns | 0.3340 ns | 104.51 |    0.47 | 0.0143 |     240 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **2.6708 ns** | **0.0016 ns** | **0.0013 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.8218 ns | 0.0019 ns | 0.0017 ns |   1.06 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  2.6557 ns | 0.0019 ns | 0.0016 ns |   0.99 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 23.2173 ns | 0.1605 ns | 0.1501 ns |   8.69 |    0.05 | 0.0057 |      96 B |          NA |
