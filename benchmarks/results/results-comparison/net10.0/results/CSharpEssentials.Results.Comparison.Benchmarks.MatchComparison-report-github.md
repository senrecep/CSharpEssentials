```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.3235 ns** | **0.0051 ns** | **0.0043 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |  1.8476 ns | 0.0093 ns | 0.0087 ns |  1.40 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.2946 ns | 0.0052 ns | 0.0049 ns |  1.73 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9881 ns | 0.0116 ns | 0.0109 ns |  0.75 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 20.9174 ns | 0.1692 ns | 0.1582 ns | 15.80 |    0.13 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.2095 ns** | **0.0070 ns** | **0.0062 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |  1.2280 ns | 0.0031 ns | 0.0029 ns |  1.02 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.3277 ns | 0.0059 ns | 0.0052 ns |  1.92 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0859 ns | 0.0054 ns | 0.0048 ns |  0.90 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 21.0285 ns | 0.1283 ns | 0.1138 ns | 17.39 |    0.13 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **1.0313 ns** | **0.0032 ns** | **0.0028 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |  1.1882 ns | 0.0080 ns | 0.0075 ns |  1.15 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.6029 ns | 0.0028 ns | 0.0025 ns |  1.55 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  1.0287 ns | 0.0035 ns | 0.0032 ns |  1.00 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 80.0210 ns | 0.3454 ns | 0.3231 ns | 77.59 |    0.37 | 0.0143 |     240 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **2.6218 ns** | **0.0105 ns** | **0.0098 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |  2.4749 ns | 0.0035 ns | 0.0027 ns |  0.94 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.7763 ns | 0.0083 ns | 0.0074 ns |  1.06 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  2.6217 ns | 0.0061 ns | 0.0054 ns |  1.00 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 22.8536 ns | 0.1209 ns | 0.1131 ns |  8.72 |    0.05 | 0.0057 |      96 B |          NA |
