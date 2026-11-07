```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **3,545.760 ns** | **51.4454 ns** | **48.1221 ns** |   **1.00** |    **0.02** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 3,224.333 ns | 19.0314 ns | 17.8020 ns |   0.91 |    0.01 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   450.640 ns |  0.3206 ns |  0.2677 ns |   0.13 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **3,550.725 ns** | **22.5585 ns** | **21.1012 ns** |   **1.00** |    **0.01** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 3,631.362 ns | 64.9725 ns | 60.7753 ns |   1.02 |    0.02 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   451.183 ns |  0.7437 ns |  0.6957 ns |   0.13 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **21.044 ns** |  **0.1083 ns** |  **0.0846 ns** |   **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    21.834 ns |  0.0211 ns |  0.0198 ns |   1.04 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    10.937 ns |  0.0127 ns |  0.0112 ns |   0.52 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **21.391 ns** |  **0.0647 ns** |  **0.0573 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    21.221 ns |  0.0108 ns |  0.0101 ns |   0.99 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    10.548 ns |  0.0077 ns |  0.0072 ns |   0.49 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |     **9.865 ns** |  **0.0171 ns** |  **0.0160 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    11.811 ns |  0.0172 ns |  0.0153 ns |   1.20 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |     8.949 ns |  0.0101 ns |  0.0089 ns |   0.91 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |    **11.811 ns** |  **0.0238 ns** |  **0.0223 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |    10.051 ns |  0.0408 ns |  0.0382 ns |   0.85 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     8.945 ns |  0.0284 ns |  0.0266 ns |   0.76 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |     **2.542 ns** |  **0.0044 ns** |  **0.0041 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |     7.946 ns |  0.0627 ns |  0.0586 ns |   3.13 |    0.02 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |     **4.847 ns** |  **0.0021 ns** |  **0.0018 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 4,064.984 ns | 21.2356 ns | 19.8638 ns | 838.68 |    3.98 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **1,212.333 ns** |  **0.6046 ns** |  **0.5656 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 3,260.476 ns |  8.0135 ns |  7.4958 ns |   2.69 |    0.01 | 0.0076 |      - |     160 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,229.866 ns |  2.6728 ns |  2.2319 ns |   1.01 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **1,474.651 ns** |  **1.2143 ns** |  **1.0765 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 3,852.894 ns | 20.0779 ns | 18.7809 ns |   2.61 |    0.01 | 0.0114 |      - |     192 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 1,454.889 ns |  0.2718 ns |  0.2410 ns |   0.99 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **1,470.585 ns** |  **0.3783 ns** |  **0.3354 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 2,405.983 ns |  3.0808 ns |  2.8818 ns |   1.64 |    0.00 | 0.0114 |      - |     192 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 1,463.430 ns |  0.4053 ns |  0.3593 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **1,617.460 ns** |  **0.3208 ns** |  **0.3000 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 2,598.692 ns |  9.6377 ns |  9.0152 ns |   1.61 |    0.01 | 0.0114 |      - |     224 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 1,469.123 ns |  0.4370 ns |  0.3874 ns |   0.91 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |     **2.540 ns** |  **0.0026 ns** |  **0.0024 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |     7.629 ns |  0.1382 ns |  0.1292 ns |   3.00 |    0.05 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** |   **590.278 ns** |  **0.1525 ns** |  **0.1352 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 4,076.349 ns | 26.3264 ns | 24.6258 ns |   6.91 |    0.04 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **1,210.729 ns** |  **2.0195 ns** |  **1.8890 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 1,184.469 ns |  0.3673 ns |  0.3256 ns |   0.98 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,194.491 ns |  1.5110 ns |  1.4134 ns |   0.99 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **1,228.849 ns** |  **0.6148 ns** |  **0.5751 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 1,463.571 ns |  0.4904 ns |  0.4587 ns |   1.19 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 1,229.839 ns |  1.3574 ns |  1.2697 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **1,463.008 ns** |  **0.4406 ns** |  **0.4122 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 1,466.591 ns |  0.5099 ns |  0.4520 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 1,463.536 ns |  0.3381 ns |  0.2997 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |              |            |            |        |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **2,919.927 ns** |  **1.1040 ns** |  **1.0327 ns** |   **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 4,104.591 ns |  1.5969 ns |  1.4156 ns |   1.41 |    0.00 |      - |      - |      48 B |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 2,914.465 ns |  0.4729 ns |  0.4192 ns |   1.00 |    0.00 |      - |      - |         - |          NA |
