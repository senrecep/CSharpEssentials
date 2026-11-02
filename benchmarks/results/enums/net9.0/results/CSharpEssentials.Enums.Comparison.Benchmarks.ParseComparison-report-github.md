```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **55.568 ns** | **0.1819 ns** | **0.1701 ns** |  **1.00** |    **0.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  1.187 ns | 0.0016 ns | 0.0013 ns |  0.02 |    0.00 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 35.976 ns | 0.0079 ns | 0.0062 ns |  0.65 |    0.00 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 18.007 ns | 0.0551 ns | 0.0516 ns |  0.32 |    0.00 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 57.942 ns | 0.0435 ns | 0.0407 ns |  1.04 |    0.00 |      - |         - |        0.00 |
|                                |               |       |           |           |           |       |         |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  | **35.099 ns** | **0.0200 ns** | **0.0177 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  1.518 ns | 0.0038 ns | 0.0035 ns |  0.04 |    0.00 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 27.178 ns | 0.0187 ns | 0.0175 ns |  0.77 |    0.00 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 20.505 ns | 0.0268 ns | 0.0251 ns |  0.58 |    0.00 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 29.360 ns | 0.0184 ns | 0.0144 ns |  0.84 |    0.00 |      - |         - |          NA |
|                                |               |       |           |           |           |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** | **73.439 ns** | **1.0936 ns** | **1.0229 ns** |  **1.00** |    **0.02** | **0.0052** |      **88 B** |        **1.00** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 42.050 ns | 0.0376 ns | 0.0351 ns |  0.57 |    0.01 |      - |         - |        0.00 |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 17.530 ns | 0.0215 ns | 0.0201 ns |  0.24 |    0.00 |      - |         - |        0.00 |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 58.490 ns | 0.0065 ns | 0.0054 ns |  0.80 |    0.01 |      - |         - |        0.00 |
|                                |               |       |           |           |           |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **46.664 ns** | **0.0453 ns** | **0.0401 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 28.519 ns | 0.0234 ns | 0.0208 ns |  0.61 |    0.00 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 39.212 ns | 0.2968 ns | 0.2777 ns |  0.84 |    0.01 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 33.996 ns | 0.0127 ns | 0.0106 ns |  0.73 |    0.00 |      - |         - |          NA |
