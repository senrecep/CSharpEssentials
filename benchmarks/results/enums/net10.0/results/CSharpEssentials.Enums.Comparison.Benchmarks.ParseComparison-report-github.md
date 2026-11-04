```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **39.8918 ns** | **0.1920 ns** | **0.1796 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  0.9333 ns | 0.0026 ns | 0.0023 ns |  0.02 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 70.4925 ns | 0.0149 ns | 0.0125 ns |  1.77 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 13.3405 ns | 0.0351 ns | 0.0311 ns |  0.33 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 57.3845 ns | 0.0244 ns | 0.0228 ns |  1.44 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  | **22.1890 ns** | **0.0514 ns** | **0.0481 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  0.7398 ns | 0.0026 ns | 0.0023 ns |  0.03 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 43.0744 ns | 0.0413 ns | 0.0366 ns |  1.94 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 17.8628 ns | 0.0443 ns | 0.0415 ns |  0.81 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 28.2404 ns | 0.0220 ns | 0.0205 ns |  1.27 |      - |         - |          NA |
|                                |               |       |            |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** | **60.5683 ns** | **0.2606 ns** | **0.2437 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 77.5124 ns | 0.0156 ns | 0.0130 ns |  1.28 |      - |         - |        0.00 |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 14.6183 ns | 0.0365 ns | 0.0342 ns |  0.24 |      - |         - |        0.00 |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 58.1841 ns | 0.0089 ns | 0.0074 ns |  0.96 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **51.9075 ns** | **0.0681 ns** | **0.0637 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 53.6330 ns | 0.0462 ns | 0.0386 ns |  1.03 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 35.9320 ns | 0.0390 ns | 0.0364 ns |  0.69 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 44.7521 ns | 0.0310 ns | 0.0274 ns |  0.86 |      - |         - |          NA |
