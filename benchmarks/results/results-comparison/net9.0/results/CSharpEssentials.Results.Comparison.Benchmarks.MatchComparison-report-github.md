```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                              | Categories | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_ToInt**              | **ToInt**      | **False**   |  **2.2059 ns** | **0.0016 ns** | **0.0014 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | False   |  2.3109 ns | 0.0057 ns | 0.0053 ns |  1.05 |    0.00 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | False   |  0.9943 ns | 0.0018 ns | 0.0017 ns |  0.45 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | False   | 31.9340 ns | 0.1576 ns | 0.1474 ns | 14.48 |    0.07 | 0.0052 |      88 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToInt**              | **ToInt**      | **True**    |  **1.0956 ns** | **0.0024 ns** | **0.0021 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToInt    | ToInt      | True    |  2.3063 ns | 0.0044 ns | 0.0041 ns |  2.10 |    0.01 |      - |         - |          NA |
| ErrorOr_ToInt                       | ToInt      | True    |  1.0955 ns | 0.0034 ns | 0.0032 ns |  1.00 |    0.00 |      - |         - |          NA |
| FluentResults_ToInt                 | ToInt      | True    | 40.6340 ns | 0.3074 ns | 0.2725 ns | 37.09 |    0.25 | 0.0057 |      96 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **False**   |  **2.0642 ns** | **0.0037 ns** | **0.0035 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | False   |  1.3714 ns | 0.0012 ns | 0.0010 ns |  0.66 |    0.00 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | False   |  0.9061 ns | 0.0057 ns | 0.0051 ns |  0.44 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | False   | 97.5171 ns | 0.3404 ns | 0.3184 ns | 47.24 |    0.17 | 0.0157 |     264 B |          NA |
|                                     |            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_ToString**           | **ToString**   | **True**    |  **1.5495 ns** | **0.0049 ns** | **0.0043 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_ToString | ToString   | True    |  2.6917 ns | 0.0087 ns | 0.0082 ns |  1.74 |    0.01 |      - |         - |          NA |
| ErrorOr_ToString                    | ToString   | True    |  1.4798 ns | 0.0013 ns | 0.0010 ns |  0.95 |    0.00 |      - |         - |          NA |
| FluentResults_ToString              | ToString   | True    | 41.7926 ns | 0.2438 ns | 0.2280 ns | 26.97 |    0.16 | 0.0057 |      96 B |          NA |
