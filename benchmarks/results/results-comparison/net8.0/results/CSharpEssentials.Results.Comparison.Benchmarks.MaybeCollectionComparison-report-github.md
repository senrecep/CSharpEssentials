```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean        | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **6,285.35 ns** |  **73.663 ns** |  **68.904 ns** |   **1.00** |    **0.02** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 6,660.65 ns | 109.227 ns | 102.171 ns |   1.06 |    0.02 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   547.66 ns |   8.667 ns |   8.107 ns |   0.09 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **6,322.58 ns** |  **47.495 ns** |  **42.103 ns** |   **1.00** |    **0.01** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 6,646.99 ns |  37.211 ns |  31.073 ns |   1.05 |    0.01 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   551.56 ns |   9.623 ns |   9.001 ns |   0.09 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **43.44 ns** |   **0.355 ns** |   **0.332 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    39.61 ns |   0.334 ns |   0.312 ns |   0.91 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    18.65 ns |   0.235 ns |   0.220 ns |   0.43 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **39.33 ns** |   **0.425 ns** |   **0.376 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    40.92 ns |   0.574 ns |   0.537 ns |   1.04 |    0.02 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    18.55 ns |   0.264 ns |   0.247 ns |   0.47 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |    **12.66 ns** |   **0.132 ns** |   **0.123 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    12.96 ns |   0.096 ns |   0.085 ns |   1.02 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |    10.68 ns |   0.114 ns |   0.106 ns |   0.84 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |    **12.58 ns** |   **0.117 ns** |   **0.104 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |    12.78 ns |   0.105 ns |   0.087 ns |   1.02 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |    10.56 ns |   0.073 ns |   0.065 ns |   0.84 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |    **14.89 ns** |   **0.102 ns** |   **0.090 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |    19.56 ns |   0.311 ns |   0.291 ns |   1.31 |    0.02 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |    **27.36 ns** |   **0.289 ns** |   **0.270 ns** |   **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 8,659.06 ns | 162.337 ns | 159.437 ns | 316.47 |    6.41 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **1,887.61 ns** |  **11.236 ns** |   **9.960 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 4,646.53 ns |  54.403 ns |  50.889 ns |   2.46 |    0.03 | 0.0076 |     160 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,311.59 ns |  12.138 ns |  10.760 ns |   0.69 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **5,706.86 ns** |  **76.664 ns** |  **71.711 ns** |   **1.00** |    **0.02** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 5,510.63 ns |  26.410 ns |  20.619 ns |   0.97 |    0.01 | 0.0076 |     192 B |        4.00 |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 3,443.03 ns |  67.998 ns |  69.829 ns |   0.60 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **2,828.73 ns** |  **44.411 ns** |  **41.542 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 3,753.73 ns |  53.361 ns |  49.914 ns |   1.33 |    0.03 | 0.0114 |     216 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 2,148.24 ns |  19.173 ns |  17.935 ns |   0.76 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **4,353.14 ns** |  **40.232 ns** |  **35.664 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 4,272.85 ns |  62.722 ns |  55.602 ns |   0.98 |    0.01 | 0.0076 |     248 B |        5.17 |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 3,032.81 ns |  42.824 ns |  40.057 ns |   0.70 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |    **13.71 ns** |   **0.086 ns** |   **0.076 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |    21.02 ns |   0.224 ns |   0.210 ns |   1.53 |    0.02 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** | **7,595.14 ns** | **107.574 ns** | **100.625 ns** |   **1.00** |    **0.02** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 8,632.45 ns | 114.852 ns | 101.813 ns |   1.14 |    0.02 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **2,824.72 ns** |  **27.808 ns** |  **26.012 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 2,746.78 ns |  11.744 ns |  10.985 ns |   0.97 |    0.01 |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,298.87 ns |  24.270 ns |  22.702 ns |   0.46 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **5,593.46 ns** |  **59.641 ns** |  **55.788 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 5,024.79 ns |  60.518 ns |  53.648 ns |   0.90 |    0.01 |      - |      48 B |        1.00 |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 3,506.99 ns |  46.539 ns |  41.256 ns |   0.63 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **2,925.39 ns** |  **13.944 ns** |  **11.644 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 2,921.56 ns |  32.506 ns |  28.816 ns |   1.00 |    0.01 |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 2,176.32 ns |  28.459 ns |  26.621 ns |   0.74 |    0.01 |      - |         - |          NA |
|                                          |               |          |             |            |            |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **8,404.63 ns** |  **41.294 ns** |  **34.483 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 7,784.34 ns |  65.754 ns |  58.289 ns |   0.93 |    0.01 |      - |      48 B |        1.00 |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 7,107.47 ns |  65.467 ns |  61.238 ns |   0.85 |    0.01 |      - |      48 B |        1.00 |
