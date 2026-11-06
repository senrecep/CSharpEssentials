```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **22.5530 ns** | **0.1104 ns** | **0.1033 ns** |  **1.00** |    **0.01** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  0.9960 ns | 0.0060 ns | 0.0056 ns |  0.04 |    0.00 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 79.3068 ns | 0.2383 ns | 0.2229 ns |  3.52 |    0.02 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 13.2294 ns | 0.0800 ns | 0.0748 ns |  0.59 |    0.00 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 57.9610 ns | 0.1911 ns | 0.1596 ns |  2.57 |    0.01 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |         |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  |  **8.6618 ns** | **0.0258 ns** | **0.0215 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  0.6411 ns | 0.0072 ns | 0.0064 ns |  0.07 |    0.00 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 45.5131 ns | 0.2225 ns | 0.2081 ns |  5.25 |    0.03 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 18.3746 ns | 0.0280 ns | 0.0262 ns |  2.12 |    0.01 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 28.8248 ns | 0.0091 ns | 0.0076 ns |  3.33 |    0.01 |      - |         - |          NA |
|                                |               |       |            |           |           |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** |  **8.1523 ns** | **0.0447 ns** | **0.0419 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 76.9977 ns | 0.0275 ns | 0.0243 ns |  9.45 |    0.05 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 15.1186 ns | 0.0352 ns | 0.0329 ns |  1.85 |    0.01 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 58.2002 ns | 0.0067 ns | 0.0059 ns |  7.14 |    0.04 |      - |         - |          NA |
|                                |               |       |            |           |           |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **27.5424 ns** | **0.0377 ns** | **0.0353 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 53.4897 ns | 0.0267 ns | 0.0250 ns |  1.94 |    0.00 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 39.2357 ns | 0.0209 ns | 0.0185 ns |  1.42 |    0.00 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 45.0380 ns | 0.0345 ns | 0.0323 ns |  1.64 |    0.00 |      - |         - |          NA |
