```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **21.8815 ns** | **0.2044 ns** | **0.1812 ns** |  **1.00** |    **0.01** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  0.9498 ns | 0.0086 ns | 0.0077 ns |  0.04 |    0.00 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 18.7848 ns | 0.1016 ns | 0.0848 ns |  0.86 |    0.01 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 12.6380 ns | 0.0251 ns | 0.0235 ns |  0.58 |    0.00 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 57.8586 ns | 0.1951 ns | 0.1825 ns |  2.64 |    0.02 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |         |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  |  **8.9246 ns** | **0.0257 ns** | **0.0240 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  0.6382 ns | 0.0003 ns | 0.0002 ns |  0.07 |    0.00 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 14.0907 ns | 0.0029 ns | 0.0022 ns |  1.58 |    0.00 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 19.9345 ns | 0.2071 ns | 0.1836 ns |  2.23 |    0.02 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 29.0920 ns | 0.0023 ns | 0.0018 ns |  3.26 |    0.01 |      - |         - |          NA |
|                                |               |       |            |           |           |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** |  **8.1444 ns** | **0.0261 ns** | **0.0244 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 23.1647 ns | 0.1455 ns | 0.1361 ns |  2.84 |    0.02 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 17.0603 ns | 0.1630 ns | 0.1524 ns |  2.09 |    0.02 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 57.8152 ns | 0.2922 ns | 0.2733 ns |  7.10 |    0.04 |      - |         - |          NA |
|                                |               |       |            |           |           |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **13.3809 ns** | **0.0309 ns** | **0.0274 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 17.9229 ns | 0.1197 ns | 0.1120 ns |  1.34 |    0.01 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 27.6431 ns | 0.1081 ns | 0.1011 ns |  2.07 |    0.01 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 33.7045 ns | 0.0231 ns | 0.0193 ns |  2.52 |    0.01 |      - |         - |          NA |
