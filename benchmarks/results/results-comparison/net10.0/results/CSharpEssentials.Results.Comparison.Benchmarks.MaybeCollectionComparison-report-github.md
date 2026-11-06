```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean         | Error      | StdDev     | Ratio    | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |-------------:|-----------:|-----------:|---------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **3,467.147 ns** | **22.3428 ns** | **20.8995 ns** |     **1.00** |    **0.01** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 3,429.457 ns | 24.4767 ns | 22.8955 ns |     0.99 |    0.01 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   449.589 ns |  0.9394 ns |  0.8787 ns |     0.13 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **3,408.543 ns** | **20.2182 ns** | **18.9121 ns** |     **1.00** |    **0.01** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 3,428.677 ns | 37.2433 ns | 34.8374 ns |     1.01 |    0.01 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   452.139 ns |  1.5478 ns |  1.3721 ns |     0.13 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **21.515 ns** |  **0.0568 ns** |  **0.0503 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    21.306 ns |  0.0366 ns |  0.0343 ns |     0.99 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    11.004 ns |  0.0209 ns |  0.0186 ns |     0.51 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **21.504 ns** |  **0.0315 ns** |  **0.0295 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    21.830 ns |  0.0281 ns |  0.0249 ns |     1.02 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    12.567 ns |  0.0127 ns |  0.0119 ns |     0.58 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |     **9.682 ns** |  **0.0166 ns** |  **0.0155 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |     9.761 ns |  0.0628 ns |  0.0588 ns |     1.01 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |     9.065 ns |  0.0217 ns |  0.0193 ns |     0.94 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |     **9.674 ns** |  **0.0064 ns** |  **0.0057 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |     9.686 ns |  0.0087 ns |  0.0081 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     9.216 ns |  0.0756 ns |  0.0707 ns |     0.95 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |     **1.101 ns** |  **0.0070 ns** |  **0.0065 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |     7.664 ns |  0.0601 ns |  0.0562 ns |     6.96 |    0.06 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |     **1.150 ns** |  **0.0107 ns** |  **0.0094 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 4,360.931 ns | 34.0537 ns | 30.1877 ns | 3,791.70 |   39.12 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **1,243.765 ns** |  **8.2624 ns** |  **7.3244 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 3,006.975 ns | 11.8182 ns | 11.0548 ns |     2.42 |    0.02 | 0.0076 |      - |     160 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,457.217 ns |  2.1478 ns |  1.6768 ns |     1.17 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **1,458.198 ns** |  **0.8019 ns** |  **0.6696 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 3,864.882 ns | 41.7457 ns | 39.0490 ns |     2.65 |    0.03 | 0.0114 |      - |     192 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 1,463.751 ns |  7.0910 ns |  6.6330 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **1,466.973 ns** |  **0.5278 ns** |  **0.4121 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 2,144.331 ns |  1.0525 ns |  0.8789 ns |     1.46 |    0.00 | 0.0114 |      - |     192 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 1,469.969 ns |  1.3749 ns |  1.2188 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **1,669.227 ns** |  **1.2846 ns** |  **1.0727 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 2,707.672 ns |  8.1483 ns |  7.6220 ns |     1.62 |    0.00 | 0.0114 |      - |     224 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 1,606.616 ns |  0.6829 ns |  0.5703 ns |     0.96 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |     **2.995 ns** |  **0.0102 ns** |  **0.0095 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |     7.518 ns |  0.0610 ns |  0.0571 ns |     2.51 |    0.02 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** |   **733.584 ns** |  **0.1667 ns** |  **0.1302 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 4,335.030 ns | 36.1224 ns | 33.7890 ns |     5.91 |    0.04 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **1,228.511 ns** |  **2.1853 ns** |  **2.0441 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 1,456.500 ns |  0.4445 ns |  0.3940 ns |     1.19 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,204.289 ns |  4.4568 ns |  4.1689 ns |     0.98 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **1,463.018 ns** |  **1.0261 ns** |  **0.9598 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 1,459.865 ns |  0.9991 ns |  0.8343 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 1,456.570 ns |  0.3727 ns |  0.3112 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **1,460.609 ns** |  **0.3383 ns** |  **0.2825 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 1,604.279 ns |  0.3912 ns |  0.3468 ns |     1.10 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 1,607.465 ns |  0.4780 ns |  0.4471 ns |     1.10 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |          |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **3,495.891 ns** |  **0.8264 ns** |  **0.7731 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 4,392.084 ns |  2.1623 ns |  1.8056 ns |     1.26 |    0.00 |      - |      - |      48 B |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 3,493.867 ns |  3.3552 ns |  2.8018 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
