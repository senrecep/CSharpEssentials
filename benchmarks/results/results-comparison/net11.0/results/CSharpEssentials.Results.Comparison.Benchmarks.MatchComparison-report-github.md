```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.3282 ns** | **0.0068 ns** | **0.0060 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |  1.6092 ns | 0.0019 ns | 0.0016 ns |   1.21 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3428 ns | 0.0222 ns | 0.0208 ns |   1.76 |    0.02 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9798 ns | 0.0034 ns | 0.0032 ns |   0.74 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 17.8536 ns | 0.1792 ns | 0.1676 ns |  13.44 |    0.14 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.2056 ns** | **0.0077 ns** | **0.0072 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |  1.2215 ns | 0.0042 ns | 0.0037 ns |   1.01 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.2931 ns | 0.0085 ns | 0.0080 ns |   1.90 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0896 ns | 0.0069 ns | 0.0062 ns |   0.90 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 19.2725 ns | 0.0942 ns | 0.0881 ns |  15.99 |    0.12 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **0.6857 ns** | **0.0011 ns** | **0.0009 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |  1.1307 ns | 0.0025 ns | 0.0022 ns |   1.65 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.5611 ns | 0.0103 ns | 0.0096 ns |   2.28 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  0.9973 ns | 0.0292 ns | 0.0259 ns |   1.45 |    0.04 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 69.7286 ns | 0.3618 ns | 0.3385 ns | 101.69 |    0.50 | 0.0143 |     240 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **1.7509 ns** | **0.0041 ns** | **0.0039 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |  1.7500 ns | 0.0043 ns | 0.0038 ns |   1.00 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.7662 ns | 0.0048 ns | 0.0043 ns |   1.58 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  1.8467 ns | 0.0092 ns | 0.0077 ns |   1.05 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 18.8836 ns | 0.1344 ns | 0.1257 ns |  10.79 |    0.07 | 0.0057 |      96 B |          NA |
