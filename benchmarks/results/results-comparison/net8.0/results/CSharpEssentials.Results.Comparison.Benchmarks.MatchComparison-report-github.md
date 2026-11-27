```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |   **1.4122 ns** | **0.0015 ns** | **0.0014 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |   1.6307 ns | 0.0075 ns | 0.0067 ns |   1.15 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |   2.1050 ns | 0.0091 ns | 0.0085 ns |   1.49 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |   1.0429 ns | 0.0017 ns | 0.0013 ns |   0.74 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   |  42.8618 ns | 0.1949 ns | 0.1823 ns |  30.35 |    0.13 | 0.0057 |      96 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |   **1.0516 ns** | **0.0076 ns** | **0.0063 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |   1.2211 ns | 0.0029 ns | 0.0026 ns |   1.16 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |   2.2880 ns | 0.0041 ns | 0.0034 ns |   2.18 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |   1.0482 ns | 0.0030 ns | 0.0025 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    |  57.3230 ns | 0.2143 ns | 0.2004 ns |  54.51 |    0.36 | 0.0067 |     112 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |   **0.5607 ns** | **0.0053 ns** | **0.0047 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |   1.1419 ns | 0.0053 ns | 0.0050 ns |   2.04 |    0.02 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |   1.2675 ns | 0.0088 ns | 0.0078 ns |   2.26 |    0.02 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |   0.8659 ns | 0.0105 ns | 0.0093 ns |   1.54 |    0.02 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 130.8356 ns | 0.8239 ns | 0.7706 ns | 233.36 |    2.31 | 0.0181 |     304 B |          NA |
|                                     |            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |   **2.0564 ns** | **0.0121 ns** | **0.0113 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |   1.7574 ns | 0.0082 ns | 0.0077 ns |   0.85 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |   2.2676 ns | 0.0053 ns | 0.0044 ns |   1.10 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |   1.8212 ns | 0.0151 ns | 0.0141 ns |   0.89 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    |  58.5890 ns | 0.2833 ns | 0.2365 ns |  28.49 |    0.19 | 0.0067 |     112 B |          NA |
