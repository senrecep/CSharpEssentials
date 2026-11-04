```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean       | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |-----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** |  **75.539 ns** | **0.5613 ns** | **0.5250 ns** |  **1.00** | **0.0124** |     **208 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |   1.243 ns | 0.0013 ns | 0.0011 ns |  0.02 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False |  38.349 ns | 0.0579 ns | 0.0541 ns |  0.51 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False |  21.048 ns | 0.0088 ns | 0.0074 ns |  0.28 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False |  58.871 ns | 0.1272 ns | 0.1062 ns |  0.78 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  |  **62.067 ns** | **0.3440 ns** | **0.3218 ns** |  **1.00** | **0.0067** |     **112 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |   1.520 ns | 0.0047 ns | 0.0041 ns |  0.02 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | True  |  27.087 ns | 0.0151 ns | 0.0134 ns |  0.44 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | True  |  21.061 ns | 0.0478 ns | 0.0447 ns |  0.34 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | True  |  29.390 ns | 0.0071 ns | 0.0056 ns |  0.47 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** | **104.523 ns** | **0.6512 ns** | **0.5773 ns** |  **1.00** | **0.0148** |     **248 B** |        **1.00** |
| Bcl_IgnoreCase                 | IgnoreCase    | False |  48.218 ns | 0.0401 ns | 0.0313 ns |  0.46 |      - |         - |        0.00 |
| EnumsNet_IgnoreCase            | IgnoreCase    | False |  20.993 ns | 0.0173 ns | 0.0154 ns |  0.20 |      - |         - |        0.00 |
| NetEscapades_IgnoreCase        | IgnoreCase    | False |  58.850 ns | 0.0914 ns | 0.0855 ns |  0.56 |      - |         - |        0.00 |
|                                |               |       |            |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  |  **96.458 ns** | **1.0776 ns** | **1.0079 ns** |  **1.00** | **0.0100** |     **168 B** |        **1.00** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  |  32.012 ns | 0.0082 ns | 0.0068 ns |  0.33 |      - |         - |        0.00 |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  |  28.935 ns | 0.5651 ns | 0.7735 ns |  0.30 |      - |         - |        0.00 |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  |  36.372 ns | 0.3871 ns | 0.3621 ns |  0.38 |      - |         - |        0.00 |
