```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean       | Error     | StdDev    | Median     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **40.7442 ns** | **0.8407 ns** | **1.6982 ns** | **40.7380 ns** |  **1.00** |    **0.06** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  0.9725 ns | 0.0320 ns | 0.0300 ns |  0.9600 ns |  0.02 |    0.00 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 18.2234 ns | 0.2332 ns | 0.2181 ns | 18.1587 ns |  0.45 |    0.02 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 12.1470 ns | 0.0513 ns | 0.0479 ns | 12.1433 ns |  0.30 |    0.01 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 57.7573 ns | 0.1177 ns | 0.1101 ns | 57.6899 ns |  1.42 |    0.06 |      - |         - |        0.00 |
|                                |               |       |            |           |           |            |       |         |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  | **20.6890 ns** | **0.1217 ns** | **0.1139 ns** | **20.7008 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  0.8836 ns | 0.0554 ns | 0.1616 ns |  0.8144 ns |  0.04 |    0.01 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 14.7735 ns | 0.3199 ns | 0.4981 ns | 14.7371 ns |  0.71 |    0.02 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 21.8771 ns | 0.4679 ns | 0.4805 ns | 21.6278 ns |  1.06 |    0.02 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 28.6613 ns | 0.2361 ns | 0.2208 ns | 28.5702 ns |  1.39 |    0.01 |      - |         - |          NA |
|                                |               |       |            |           |           |            |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** | **58.9579 ns** | **1.2176 ns** | **1.1389 ns** | **58.9716 ns** |  **1.00** |    **0.03** | **0.0052** |      **88 B** |        **1.00** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 24.9442 ns | 0.5084 ns | 0.5651 ns | 24.6356 ns |  0.42 |    0.01 |      - |         - |        0.00 |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 14.7859 ns | 0.0271 ns | 0.0226 ns | 14.7805 ns |  0.25 |    0.00 |      - |         - |        0.00 |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 58.3552 ns | 0.9716 ns | 0.8113 ns | 58.0455 ns |  0.99 |    0.02 |      - |         - |        0.00 |
|                                |               |       |            |           |           |            |       |         |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **33.1584 ns** | **0.0784 ns** | **0.0612 ns** | **33.1494 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 17.9225 ns | 0.0542 ns | 0.0423 ns | 17.9076 ns |  0.54 |    0.00 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 24.5205 ns | 0.0667 ns | 0.0624 ns | 24.5070 ns |  0.74 |    0.00 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 33.2026 ns | 0.1022 ns | 0.0906 ns | 33.1714 ns |  1.00 |    0.00 |      - |         - |          NA |
