```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean         | Error       | StdDev      | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |-------------:|------------:|------------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **5,850.007 ns** |   **1.8730 ns** |   **1.5640 ns** |   **1.00** |    **0.00** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 6,220.321 ns |   3.5098 ns |   3.1114 ns |   1.06 |    0.00 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   480.250 ns |   7.0537 ns |   6.5981 ns |   0.08 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **5,909.346 ns** |  **15.8290 ns** |  **14.8064 ns** |   **1.00** |    **0.00** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 6,263.867 ns |   5.7927 ns |   4.8372 ns |   1.06 |    0.00 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   478.274 ns |   9.5512 ns |  11.3700 ns |   0.08 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **37.471 ns** |   **0.0305 ns** |   **0.0254 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    40.233 ns |   0.1163 ns |   0.0908 ns |   1.07 |    0.00 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    17.454 ns |   0.0112 ns |   0.0105 ns |   0.47 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **37.348 ns** |   **0.1303 ns** |   **0.1218 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    36.355 ns |   0.0305 ns |   0.0255 ns |   0.97 |    0.00 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    17.234 ns |   0.0138 ns |   0.0129 ns |   0.46 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |    **11.755 ns** |   **0.0334 ns** |   **0.0296 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    12.152 ns |   0.0519 ns |   0.0486 ns |   1.03 |    0.00 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |    10.038 ns |   0.0666 ns |   0.0590 ns |   0.85 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |    **11.761 ns** |   **0.0694 ns** |   **0.0649 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |    11.725 ns |   0.0331 ns |   0.0277 ns |   1.00 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     9.897 ns |   0.0220 ns |   0.0195 ns |   0.84 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |    **17.453 ns** |   **0.0545 ns** |   **0.0509 ns** |   **1.00** |    **0.00** | **0.0024** |      **40 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |    18.119 ns |   0.0067 ns |   0.0056 ns |   1.04 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |    **17.586 ns** |   **0.0441 ns** |   **0.0368 ns** |   **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 7,840.432 ns |  35.3396 ns |  31.3276 ns | 445.83 |    1.94 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **6,043.668 ns** | **103.2450 ns** |  **96.5754 ns** |   **1.00** |    **0.02** |      **-** |      **40 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 4,346.232 ns |  81.8343 ns |  84.0378 ns |   0.72 |    0.02 | 0.0076 |     160 B |        4.00 |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,228.470 ns |   4.5683 ns |   4.0497 ns |   0.20 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **4,930.078 ns** |  **20.5142 ns** |  **19.1890 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 5,422.476 ns | 105.0268 ns | 103.1504 ns |   1.10 |    0.02 | 0.0076 |     192 B |        4.00 |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 3,226.448 ns |   3.9171 ns |   3.4724 ns |   0.65 |    0.00 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **4,352.433 ns** |  **48.7302 ns** |  **40.6919 ns** |   **1.00** |    **0.01** |      **-** |      **40 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 3,641.372 ns |   1.5376 ns |   1.3631 ns |   0.84 |    0.01 | 0.0114 |     216 B |        5.40 |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 2,038.723 ns |   0.4637 ns |   0.4111 ns |   0.47 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **3,797.088 ns** |   **2.7137 ns** |   **2.2661 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 4,030.446 ns |  38.4582 ns |  35.9739 ns |   1.06 |    0.01 | 0.0076 |     248 B |        5.17 |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 2,880.642 ns |  47.1572 ns |  41.8037 ns |   0.76 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |    **13.047 ns** |   **0.0219 ns** |   **0.0183 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |    19.879 ns |   0.2074 ns |   0.1940 ns |   1.52 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** | **7,118.724 ns** |   **2.9313 ns** |   **2.4477 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 7,832.359 ns |  27.1818 ns |  25.4259 ns |   1.10 |    0.00 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **2,613.203 ns** |   **0.5200 ns** |   **0.4610 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 2,613.721 ns |   0.5689 ns |   0.5043 ns |   1.00 |    0.00 |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,192.683 ns |   6.0327 ns |   5.3478 ns |   0.46 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **5,234.965 ns** |  **33.9036 ns** |  **31.7135 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 4,753.006 ns |  93.8395 ns |  96.3662 ns |   0.91 |    0.02 |      - |      48 B |        1.00 |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 3,443.629 ns |  61.7024 ns |  57.7164 ns |   0.66 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **2,762.695 ns** |   **0.7134 ns** |   **0.6673 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 2,760.766 ns |   0.5093 ns |   0.3976 ns |   1.00 |    0.00 |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 2,039.198 ns |   9.1456 ns |   8.5548 ns |   0.74 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **7,864.059 ns** |   **3.3182 ns** |   **2.5906 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 7,294.826 ns |  26.9212 ns |  23.8649 ns |   0.93 |    0.00 |      - |      48 B |        1.00 |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 6,693.462 ns |   1.7746 ns |   1.3855 ns |   0.85 |    0.00 |      - |      48 B |        1.00 |
