```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                         | Categories    | Valid | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------------- |------ |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **False** | **25.743 ns** | **0.1476 ns** | **0.1380 ns** |  **1.00** | **0.0052** |      **88 B** |        **1.00** |
| CSharpEssentials_IsDefinedName | CaseSensitive | False |  1.199 ns | 0.0021 ns | 0.0016 ns |  0.05 |      - |         - |        0.00 |
| Bcl_CaseSensitive              | CaseSensitive | False | 36.244 ns | 0.0072 ns | 0.0056 ns |  1.41 |      - |         - |        0.00 |
| EnumsNet_CaseSensitive         | CaseSensitive | False | 16.057 ns | 0.0194 ns | 0.0172 ns |  0.62 |      - |         - |        0.00 |
| NetEscapades_CaseSensitive     | CaseSensitive | False | 57.316 ns | 0.0083 ns | 0.0069 ns |  2.23 |      - |         - |        0.00 |
|                                |               |       |           |           |           |       |        |           |             |
| **CSharpEssentials_CaseSensitive** | **CaseSensitive** | **True**  | **11.447 ns** | **0.0071 ns** | **0.0060 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_IsDefinedName | CaseSensitive | True  |  1.505 ns | 0.0010 ns | 0.0009 ns |  0.13 |      - |         - |          NA |
| Bcl_CaseSensitive              | CaseSensitive | True  | 26.225 ns | 0.0235 ns | 0.0219 ns |  2.29 |      - |         - |          NA |
| EnumsNet_CaseSensitive         | CaseSensitive | True  | 22.231 ns | 0.0455 ns | 0.0380 ns |  1.94 |      - |         - |          NA |
| NetEscapades_CaseSensitive     | CaseSensitive | True  | 29.354 ns | 0.0078 ns | 0.0069 ns |  2.56 |      - |         - |          NA |
|                                |               |       |           |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **False** | **10.964 ns** | **0.0195 ns** | **0.0183 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | False | 32.491 ns | 0.1147 ns | 0.1017 ns |  2.96 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | False | 17.505 ns | 0.0034 ns | 0.0029 ns |  1.60 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | False | 58.500 ns | 0.0183 ns | 0.0171 ns |  5.34 |      - |         - |          NA |
|                                |               |       |           |           |           |       |        |           |             |
| **CSharpEssentials_IgnoreCase**    | **IgnoreCase**    | **True**  | **15.772 ns** | **0.0276 ns** | **0.0258 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| Bcl_IgnoreCase                 | IgnoreCase    | True  | 28.506 ns | 0.0107 ns | 0.0095 ns |  1.81 |      - |         - |          NA |
| EnumsNet_IgnoreCase            | IgnoreCase    | True  | 27.324 ns | 0.0649 ns | 0.0607 ns |  1.73 |      - |         - |          NA |
| NetEscapades_IgnoreCase        | IgnoreCase    | True  | 34.295 ns | 0.0129 ns | 0.0115 ns |  2.17 |      - |         - |          NA |
