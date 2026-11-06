```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **26.283 ns** | **0.0618 ns** | **0.0548 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  1.248 ns | 0.0111 ns | 0.0104 ns |  0.05 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 42.218 ns | 0.0208 ns | 0.0174 ns |  1.61 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 19.544 ns | 0.0048 ns | 0.0040 ns |  0.74 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 58.813 ns | 0.0083 ns | 0.0069 ns |  2.24 |      - |         - |        0.00 |
|                                |               |       |           |           |           |       |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  | **11.235 ns** | **0.0205 ns** | **0.0182 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  1.576 ns | 0.0153 ns | 0.0143 ns |  0.14 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 28.457 ns | 0.0190 ns | 0.0178 ns |  2.53 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 20.967 ns | 0.0385 ns | 0.0360 ns |  1.87 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 28.512 ns | 0.0115 ns | 0.0096 ns |  2.54 |      - |         - |          NA |
|                                |               |       |           |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** | **11.264 ns** | **0.0109 ns** | **0.0096 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 38.001 ns | 0.0104 ns | 0.0087 ns |  3.37 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 20.696 ns | 0.0049 ns | 0.0043 ns |  1.84 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 59.474 ns | 0.0154 ns | 0.0136 ns |  5.28 |      - |         - |          NA |
|                                |               |       |           |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **17.280 ns** | **0.0110 ns** | **0.0092 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 32.534 ns | 0.0193 ns | 0.0161 ns |  1.88 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 31.352 ns | 0.0739 ns | 0.0691 ns |  1.81 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 36.311 ns | 0.0089 ns | 0.0069 ns |  2.10 |      - |         - |          NA |
