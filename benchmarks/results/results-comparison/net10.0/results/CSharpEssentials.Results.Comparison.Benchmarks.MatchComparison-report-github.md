```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **2.241 ns** | **0.0010 ns** | **0.0009 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.328 ns | 0.0049 ns | 0.0046 ns |  1.04 |    0.00 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  1.151 ns | 0.0028 ns | 0.0026 ns |  0.51 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 20.944 ns | 0.1867 ns | 0.1747 ns |  9.34 |    0.08 | 0.0052 |      88 B |          NA |
|                                     |            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.119 ns** | **0.0071 ns** | **0.0067 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.325 ns | 0.0043 ns | 0.0039 ns |  2.08 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.122 ns | 0.0131 ns | 0.0123 ns |  1.00 |    0.01 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 20.782 ns | 0.1619 ns | 0.1515 ns | 18.58 |    0.17 | 0.0057 |      96 B |          NA |
|                                     |            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **1.989 ns** | **0.0009 ns** | **0.0007 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.588 ns | 0.0023 ns | 0.0019 ns |  0.80 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  1.010 ns | 0.0014 ns | 0.0012 ns |  0.51 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 76.271 ns | 0.4356 ns | 0.3862 ns | 38.34 |    0.19 | 0.0143 |     240 B |          NA |
|                                     |            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **2.030 ns** | **0.0072 ns** | **0.0067 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.748 ns | 0.0008 ns | 0.0006 ns |  1.35 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  2.604 ns | 0.0081 ns | 0.0075 ns |  1.28 |    0.01 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 22.401 ns | 0.1883 ns | 0.1762 ns | 11.03 |    0.09 | 0.0057 |      96 B |          NA |
