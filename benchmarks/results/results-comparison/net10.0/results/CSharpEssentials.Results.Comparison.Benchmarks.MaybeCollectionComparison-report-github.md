```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **3,319.996 ns** | **40.8307 ns** | **38.1931 ns** |   **1.00** |    **0.02** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 3,397.408 ns | 22.7749 ns | 21.3036 ns |   1.02 |    0.01 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   475.180 ns |  6.5653 ns |  7.0248 ns |   0.14 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **3,273.527 ns** | **28.2913 ns** | **26.4637 ns** |   **1.00** |    **0.01** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 3,327.794 ns | 42.5070 ns | 39.7611 ns |   1.02 |    0.01 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   466.653 ns |  2.9608 ns |  2.4724 ns |   0.14 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **22.916 ns** |  **0.1985 ns** |  **0.1857 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    22.038 ns |  0.1076 ns |  0.1006 ns |   0.96 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    11.405 ns |  0.0489 ns |  0.0409 ns |   0.50 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **24.853 ns** |  **0.0753 ns** |  **0.0704 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    23.488 ns |  0.2238 ns |  0.1984 ns |   0.95 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    11.549 ns |  0.1111 ns |  0.1040 ns |   0.46 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |     **9.931 ns** |  **0.0981 ns** |  **0.0917 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    10.054 ns |  0.1275 ns |  0.1193 ns |   1.01 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |     9.526 ns |  0.1001 ns |  0.0887 ns |   0.96 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |    **10.469 ns** |  **0.0749 ns** |  **0.0664 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |    10.593 ns |  0.1328 ns |  0.1177 ns |   1.01 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     9.464 ns |  0.1421 ns |  0.1329 ns |   0.90 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |     **2.687 ns** |  **0.0688 ns** |  **0.0644 ns** |   **1.00** |    **0.03** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |     8.070 ns |  0.1198 ns |  0.1062 ns |   3.01 |    0.08 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |     **5.898 ns** |  **0.1371 ns** |  **0.1283 ns** |   **1.00** |    **0.03** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 4,423.863 ns | 82.5379 ns | 77.2060 ns | 750.37 |   20.19 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **1,314.010 ns** | **20.6847 ns** | **19.3485 ns** |   **1.00** |    **0.02** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 3,083.930 ns | 23.2444 ns | 21.7428 ns |   2.35 |    0.04 | 0.0076 |      - |     160 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,509.095 ns |  8.7614 ns |  8.1954 ns |   1.15 |    0.02 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **1,516.854 ns** | **14.8908 ns** | **13.2003 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 3,830.247 ns | 34.5533 ns | 30.6306 ns |   2.53 |    0.03 | 0.0076 |      - |     192 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 1,779.556 ns | 16.6830 ns | 15.6053 ns |   1.17 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **1,511.619 ns** | **12.1802 ns** | **10.7974 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 2,381.262 ns | 31.6988 ns | 28.1001 ns |   1.58 |    0.02 | 0.0114 |      - |     192 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 1,549.859 ns | 17.1677 ns | 15.2187 ns |   1.03 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **1,842.250 ns** | **21.8019 ns** | **20.3935 ns** |   **1.00** |    **0.02** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 2,815.999 ns |  9.6399 ns |  9.0171 ns |   1.53 |    0.02 | 0.0114 |      - |     224 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 1,815.016 ns |  8.1455 ns |  7.6193 ns |   0.99 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |     **2.637 ns** |  **0.0277 ns** |  **0.0259 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |     7.646 ns |  0.1229 ns |  0.1150 ns |   2.90 |    0.05 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** |   **909.606 ns** | **10.1027 ns** |  **8.9558 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 4,102.166 ns | 78.7349 ns | 80.8549 ns |   4.51 |    0.10 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **1,264.376 ns** | **24.9151 ns** | **25.5860 ns** |   **1.00** |    **0.03** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 1,291.321 ns | 21.5585 ns | 20.1659 ns |   1.02 |    0.03 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,247.162 ns |  9.7753 ns |  9.1439 ns |   0.99 |    0.02 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **1,534.901 ns** | **30.0688 ns** | **30.8784 ns** |   **1.00** |    **0.03** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 1,517.922 ns | 29.1215 ns | 31.1596 ns |   0.99 |    0.03 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 1,546.054 ns | 30.9165 ns | 28.9193 ns |   1.01 |    0.03 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **1,520.830 ns** | **11.8044 ns** | **11.0419 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 1,527.976 ns | 13.2744 ns | 11.7674 ns |   1.00 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 1,663.832 ns |  9.5997 ns |  8.5098 ns |   1.09 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **3,580.297 ns** |  **7.1364 ns** |  **6.3262 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 4,368.203 ns | 16.3741 ns | 15.3163 ns |   1.22 |    0.00 |      - |      - |      48 B |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 3,264.382 ns |  6.9659 ns |  6.5159 ns |   0.91 |    0.00 |      - |      - |         - |          NA |
