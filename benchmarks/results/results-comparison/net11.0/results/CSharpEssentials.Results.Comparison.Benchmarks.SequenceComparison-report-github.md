```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                | Categories | N   | Succeed | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |---- |-------- |-------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_CombineAll**           | **CombineAll** | **10**  | **False**   |    **38.115 ns** |  **0.1457 ns** |  **0.1217 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 10  | False   |   197.402 ns |  1.2039 ns |  1.1261 ns |  5.18 |    0.03 | 0.0319 |      - |     536 B |        9.57 |
| ErrorOr_CombineAll                    | CombineAll | 10  | False   |    26.990 ns |  0.1346 ns |  0.1259 ns |  0.71 |    0.00 | 0.0110 |      - |     184 B |        3.29 |
| FluentResults_CombineAll              | CombineAll | 10  | False   |   134.491 ns |  0.5142 ns |  0.4810 ns |  3.53 |    0.02 | 0.0148 |      - |     248 B |        4.43 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **10**  | **True**    |     **8.684 ns** |  **0.0129 ns** |  **0.0108 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 10  | True    |    30.701 ns |  0.1758 ns |  0.1645 ns |  3.54 |    0.02 | 0.0052 |      - |      88 B |          NA |
| ErrorOr_CombineAll                    | CombineAll | 10  | True    |    11.126 ns |  0.1351 ns |  0.1198 ns |  1.28 |    0.01 | 0.0019 |      - |      32 B |          NA |
| FluentResults_CombineAll              | CombineAll | 10  | True    |   102.030 ns |  0.4550 ns |  0.4256 ns | 11.75 |    0.05 | 0.0091 |      - |     152 B |          NA |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **100** | **False**   |   **141.678 ns** |  **0.2327 ns** |  **0.2176 ns** |  **1.00** |    **0.00** | **0.0033** |      **-** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 100 | False   |   203.137 ns |  2.9884 ns |  2.7954 ns |  1.43 |    0.02 | 0.0319 |      - |     536 B |        9.57 |
| ErrorOr_CombineAll                    | CombineAll | 100 | False   |    61.036 ns |  0.4164 ns |  0.3691 ns |  0.43 |    0.00 | 0.0110 |      - |     184 B |        3.29 |
| FluentResults_CombineAll              | CombineAll | 100 | False   |   705.157 ns |  4.4134 ns |  4.1283 ns |  4.98 |    0.03 | 0.0143 |      - |     248 B |        4.43 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **100** | **True**    |    **68.541 ns** |  **0.0362 ns** |  **0.0321 ns** |  **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 100 | True    |    64.439 ns |  0.8131 ns |  0.7208 ns |  0.94 |    0.01 | 0.0052 |      - |      88 B |          NA |
| ErrorOr_CombineAll                    | CombineAll | 100 | True    |    41.358 ns |  0.5897 ns |  0.5516 ns |  0.60 |    0.01 | 0.0019 |      - |      32 B |          NA |
| FluentResults_CombineAll              | CombineAll | 100 | True    |   617.955 ns |  6.2414 ns |  5.8382 ns |  9.02 |    0.08 | 0.0086 |      - |     152 B |          NA |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **10**  | **False**   |   **110.465 ns** |  **1.4168 ns** |  **1.2559 ns** |  **1.00** |    **0.02** | **0.0291** |      **-** |     **488 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 10  | False   |   288.923 ns |  2.4717 ns |  2.3120 ns |  2.62 |    0.04 | 0.0505 |      - |     848 B |        1.74 |
| ErrorOr_Sequence                      | Sequence   | 10  | False   |    73.678 ns |  0.4917 ns |  0.4600 ns |  0.67 |    0.01 | 0.0238 |      - |     400 B |        0.82 |
| FluentResults_Sequence                | Sequence   | 10  | False   |   250.575 ns |  0.5199 ns |  0.4342 ns |  2.27 |    0.03 | 0.0339 |      - |     568 B |        1.16 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **10**  | **True**    |    **63.664 ns** |  **0.6129 ns** |  **0.5118 ns** |  **1.00** |    **0.01** | **0.0186** |      **-** |     **312 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 10  | True    |   151.865 ns |  1.0732 ns |  1.0039 ns |  2.39 |    0.02 | 0.0324 |      - |     544 B |        1.74 |
| ErrorOr_Sequence                      | Sequence   | 10  | True    |    54.575 ns |  0.5752 ns |  0.5381 ns |  0.86 |    0.01 | 0.0148 |      - |     248 B |        0.79 |
| FluentResults_Sequence                | Sequence   | 10  | True    |   379.984 ns |  2.8566 ns |  2.6721 ns |  5.97 |    0.06 | 0.0672 |      - |    1128 B |        3.62 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **100** | **False**   |   **467.639 ns** |  **2.7863 ns** |  **2.6063 ns** |  **1.00** |    **0.01** | **0.0868** |      **-** |    **1456 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 100 | False   |   779.430 ns |  5.5663 ns |  5.2067 ns |  1.67 |    0.01 | 0.1364 | 0.0010 |    2288 B |        1.57 |
| ErrorOr_Sequence                      | Sequence   | 100 | False   |   383.771 ns |  2.3951 ns |  2.1232 ns |  0.82 |    0.01 | 0.0815 |      - |    1368 B |        0.94 |
| FluentResults_Sequence                | Sequence   | 100 | False   |   934.005 ns | 10.7240 ns | 10.0312 ns |  2.00 |    0.02 | 0.0763 |      - |    1288 B |        0.88 |
|                                       |            |     |         |              |            |            |       |         |        |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **100** | **True**    |   **270.811 ns** |  **3.5935 ns** |  **3.1855 ns** |  **1.00** |    **0.02** | **0.0978** |      **-** |    **1640 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 100 | True    |   794.306 ns |  4.9954 ns |  4.6727 ns |  2.93 |    0.04 | 0.1392 |      - |    2344 B |        1.43 |
| ErrorOr_Sequence                      | Sequence   | 100 | True    |   214.780 ns |  1.8599 ns |  1.4521 ns |  0.79 |    0.01 | 0.0725 |      - |    1216 B |        0.74 |
| FluentResults_Sequence                | Sequence   | 100 | True    | 2,128.751 ns | 16.7429 ns | 13.9811 ns |  7.86 |    0.10 | 0.3891 |      - |    6528 B |        3.98 |
