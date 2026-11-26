```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.3184 ns** | **0.0053 ns** | **0.0050 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |  1.3231 ns | 0.0026 ns | 0.0023 ns |   1.00 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.2918 ns | 0.0100 ns | 0.0094 ns |   1.74 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9735 ns | 0.0038 ns | 0.0032 ns |   0.74 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 35.1365 ns | 0.2089 ns | 0.1954 ns |  26.65 |    0.17 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.2190 ns** | **0.0082 ns** | **0.0077 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |  1.2232 ns | 0.0048 ns | 0.0043 ns |   1.00 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.2878 ns | 0.0029 ns | 0.0025 ns |   1.88 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0878 ns | 0.0063 ns | 0.0059 ns |   0.89 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 40.3945 ns | 0.2023 ns | 0.1893 ns |  33.14 |    0.25 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **0.8802 ns** | **0.0042 ns** | **0.0035 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |  0.6512 ns | 0.0060 ns | 0.0050 ns |   0.74 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.3157 ns | 0.0022 ns | 0.0017 ns |   1.49 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  0.8879 ns | 0.0090 ns | 0.0080 ns |   1.01 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 98.7657 ns | 0.2443 ns | 0.2165 ns | 112.21 |    0.49 | 0.0157 |     264 B |          NA |
|                                     |            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **1.7388 ns** | **0.0084 ns** | **0.0078 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |  1.7363 ns | 0.0033 ns | 0.0031 ns |   1.00 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.3201 ns | 0.0022 ns | 0.0020 ns |   1.33 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  1.4467 ns | 0.0125 ns | 0.0117 ns |   0.83 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 41.7408 ns | 0.3437 ns | 0.3215 ns |  24.01 |    0.21 | 0.0057 |      96 B |          NA |
