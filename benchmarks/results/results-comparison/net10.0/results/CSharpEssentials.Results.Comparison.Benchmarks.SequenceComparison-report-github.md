```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                | Categories | N   | Succeed | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |---- |-------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_CombineAll**           | **CombineAll** | **10**  | **False**   |    **31.122 ns** |  **0.6453 ns** |  **0.8833 ns** |  **1.00** |    **0.04** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 10  | False   |   183.490 ns |  1.1332 ns |  1.0600 ns |  5.90 |    0.16 | 0.0319 |      - |     536 B |        9.57 |
| ErrorOr_CombineAll                    | CombineAll | 10  | False   |    32.546 ns |  0.1481 ns |  0.1313 ns |  1.05 |    0.03 | 0.0110 |      - |     184 B |        3.29 |
| FluentResults_CombineAll              | CombineAll | 10  | False   |   144.540 ns |  0.7222 ns |  0.6402 ns |  4.65 |    0.13 | 0.0148 |      - |     248 B |        4.43 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **10**  | **True**    |     **8.727 ns** |  **0.0062 ns** |  **0.0058 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 10  | True    |    29.557 ns |  0.1804 ns |  0.1687 ns |  3.39 |    0.02 | 0.0052 |      - |      88 B |          NA |
| ErrorOr_CombineAll                    | CombineAll | 10  | True    |    11.183 ns |  0.0388 ns |  0.0363 ns |  1.28 |    0.00 | 0.0019 |      - |      32 B |          NA |
| FluentResults_CombineAll              | CombineAll | 10  | True    |    99.212 ns |  0.1715 ns |  0.1521 ns | 11.37 |    0.02 | 0.0091 |      - |     152 B |          NA |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **100** | **False**   |   **137.997 ns** |  **0.1753 ns** |  **0.1464 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 100 | False   |   211.938 ns |  0.7306 ns |  0.6834 ns |  1.54 |    0.01 | 0.0319 |      - |     536 B |        9.57 |
| ErrorOr_CombineAll                    | CombineAll | 100 | False   |    69.222 ns |  0.2119 ns |  0.1982 ns |  0.50 |    0.00 | 0.0110 |      - |     184 B |        3.29 |
| FluentResults_CombineAll              | CombineAll | 100 | False   |   744.986 ns | 14.9138 ns | 17.1747 ns |  5.40 |    0.12 | 0.0143 |      - |     248 B |        4.43 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **100** | **True**    |    **67.625 ns** |  **0.0642 ns** |  **0.0569 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 100 | True    |    62.774 ns |  0.2810 ns |  0.2629 ns |  0.93 |    0.00 | 0.0052 |      - |      88 B |          NA |
| ErrorOr_CombineAll                    | CombineAll | 100 | True    |    37.141 ns |  0.6991 ns |  1.0026 ns |  0.55 |    0.01 | 0.0019 |      - |      32 B |          NA |
| FluentResults_CombineAll              | CombineAll | 100 | True    |   606.378 ns |  0.6428 ns |  0.6013 ns |  8.97 |    0.01 | 0.0086 |      - |     152 B |          NA |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **10**  | **False**   |   **118.384 ns** |  **0.5721 ns** |  **0.5351 ns** |  **1.00** |    **0.01** | **0.0291** |      **-** |     **488 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 10  | False   |   299.347 ns |  1.7491 ns |  1.6361 ns |  2.53 |    0.02 | 0.0505 |      - |     848 B |        1.74 |
| ErrorOr_Sequence                      | Sequence   | 10  | False   |    82.588 ns |  0.8932 ns |  0.8355 ns |  0.70 |    0.01 | 0.0238 |      - |     400 B |        0.82 |
| FluentResults_Sequence                | Sequence   | 10  | False   |   265.587 ns |  1.1796 ns |  1.1034 ns |  2.24 |    0.01 | 0.0339 |      - |     568 B |        1.16 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **10**  | **True**    |    **68.816 ns** |  **1.3752 ns** |  **1.3507 ns** |  **1.00** |    **0.03** | **0.0186** |      **-** |     **312 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 10  | True    |   161.258 ns |  0.9264 ns |  0.8665 ns |  2.34 |    0.05 | 0.0324 |      - |     544 B |        1.74 |
| ErrorOr_Sequence                      | Sequence   | 10  | True    |    54.330 ns |  0.3582 ns |  0.3176 ns |  0.79 |    0.02 | 0.0148 |      - |     248 B |        0.79 |
| FluentResults_Sequence                | Sequence   | 10  | True    |   386.860 ns |  2.5966 ns |  2.1683 ns |  5.62 |    0.11 | 0.0672 |      - |    1128 B |        3.62 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **100** | **False**   |   **484.463 ns** |  **2.0239 ns** |  **1.8932 ns** |  **1.00** |    **0.01** | **0.0868** |      **-** |    **1456 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 100 | False   |   868.817 ns |  4.0976 ns |  3.8329 ns |  1.79 |    0.01 | 0.1364 | 0.0010 |    2288 B |        1.57 |
| ErrorOr_Sequence                      | Sequence   | 100 | False   |   364.533 ns |  3.6646 ns |  3.0601 ns |  0.75 |    0.01 | 0.0815 |      - |    1368 B |        0.94 |
| FluentResults_Sequence                | Sequence   | 100 | False   |   945.368 ns |  9.7299 ns |  9.1013 ns |  1.95 |    0.02 | 0.0763 |      - |    1288 B |        0.88 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **100** | **True**    |   **268.480 ns** |  **3.0696 ns** |  **2.8713 ns** |  **1.00** |    **0.01** | **0.0978** |      **-** |    **1640 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 100 | True    |   854.432 ns |  5.6335 ns |  5.2695 ns |  3.18 |    0.04 | 0.1392 |      - |    2344 B |        1.43 |
| ErrorOr_Sequence                      | Sequence   | 100 | True    |   229.394 ns |  1.8199 ns |  1.7024 ns |  0.85 |    0.01 | 0.0725 |      - |    1216 B |        0.74 |
| FluentResults_Sequence                | Sequence   | 100 | True    | 2,311.158 ns | 18.4504 ns | 17.2585 ns |  8.61 |    0.11 | 0.3891 |      - |    6528 B |        3.98 |
