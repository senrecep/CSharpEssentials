```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |   **1.4124 ns** | **0.0045 ns** | **0.0042 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |   1.6261 ns | 0.0028 ns | 0.0026 ns |   1.15 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |   2.3869 ns | 0.0062 ns | 0.0058 ns |   1.69 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |   1.0525 ns | 0.0052 ns | 0.0048 ns |   0.75 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   |  46.0545 ns | 0.2015 ns | 0.1786 ns |  32.61 |    0.15 | 0.0057 |      96 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |   **1.0589 ns** | **0.0037 ns** | **0.0034 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |   1.2260 ns | 0.0050 ns | 0.0045 ns |   1.16 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |   2.2904 ns | 0.0079 ns | 0.0074 ns |   2.16 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |   1.0474 ns | 0.0012 ns | 0.0010 ns |   0.99 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    |  57.3241 ns | 0.2393 ns | 0.2122 ns |  54.13 |    0.26 | 0.0067 |     112 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |   **0.8550 ns** | **0.0076 ns** | **0.0071 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |   1.1410 ns | 0.0074 ns | 0.0066 ns |   1.33 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |   1.0005 ns | 0.0096 ns | 0.0089 ns |   1.17 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |   0.5562 ns | 0.0041 ns | 0.0035 ns |   0.65 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 138.7162 ns | 0.6750 ns | 0.6314 ns | 162.25 |    1.48 | 0.0181 |     304 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |   **2.0615 ns** | **0.0198 ns** | **0.0185 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |   1.7582 ns | 0.0077 ns | 0.0072 ns |   0.85 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |   2.2672 ns | 0.0022 ns | 0.0019 ns |   1.10 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |   1.7503 ns | 0.0124 ns | 0.0116 ns |   0.85 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    |  56.5977 ns | 0.1839 ns | 0.1630 ns |  27.46 |    0.25 | 0.0067 |     112 B |          NA |
