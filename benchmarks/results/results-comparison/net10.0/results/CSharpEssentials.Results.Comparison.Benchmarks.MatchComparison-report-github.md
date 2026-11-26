```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **1.3206 ns** | **0.0030 ns** | **0.0026 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | False   |  1.8412 ns | 0.0217 ns | 0.0203 ns |  1.39 |    0.02 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3646 ns | 0.0111 ns | 0.0103 ns |  1.79 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9854 ns | 0.0053 ns | 0.0049 ns |  0.75 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 20.8902 ns | 0.0710 ns | 0.0665 ns | 15.82 |    0.06 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.2037 ns** | **0.0058 ns** | **0.0054 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToInt        | ToInt      | True    |  1.2270 ns | 0.0034 ns | 0.0032 ns |  1.02 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.3437 ns | 0.0044 ns | 0.0041 ns |  1.95 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0917 ns | 0.0073 ns | 0.0068 ns |  0.91 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 20.7559 ns | 0.1231 ns | 0.1152 ns | 17.24 |    0.12 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **1.0312 ns** | **0.0054 ns** | **0.0048 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | False   |  1.1931 ns | 0.0071 ns | 0.0066 ns |  1.16 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.6065 ns | 0.0028 ns | 0.0024 ns |  1.56 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  1.0323 ns | 0.0051 ns | 0.0045 ns |  1.00 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 78.7860 ns | 0.2557 ns | 0.2135 ns | 76.40 |    0.40 | 0.0143 |     240 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **2.6181 ns** | **0.0030 ns** | **0.0027 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed_ToString     | ToString   | True    |  2.4725 ns | 0.0018 ns | 0.0016 ns |  0.94 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.7688 ns | 0.0019 ns | 0.0016 ns |  1.06 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  2.6193 ns | 0.0050 ns | 0.0042 ns |  1.00 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 22.4659 ns | 0.1010 ns | 0.0945 ns |  8.58 |    0.04 | 0.0057 |      96 B |          NA |
