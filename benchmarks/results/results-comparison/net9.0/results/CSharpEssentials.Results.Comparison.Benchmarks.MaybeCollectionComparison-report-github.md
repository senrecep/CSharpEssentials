```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean         | Error       | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |-------------:|------------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **5,108.465 ns** |  **59.4907 ns** | **55.6477 ns** |   **1.00** |    **0.01** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 6,122.335 ns |  22.3967 ns | 19.8541 ns |   1.20 |    0.01 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   487.383 ns |   4.5162 ns |  4.2245 ns |   0.10 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **5,482.921 ns** | **100.5271 ns** | **98.7311 ns** |   **1.00** |    **0.02** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 6,230.033 ns |  27.0726 ns | 23.9992 ns |   1.14 |    0.02 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   473.814 ns |   4.6214 ns |  4.3228 ns |   0.09 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **37.223 ns** |   **0.0954 ns** |  **0.0846 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    37.519 ns |   0.1286 ns |  0.1140 ns |   1.01 |    0.00 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    17.454 ns |   0.0840 ns |  0.0786 ns |   0.47 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **39.956 ns** |   **0.1422 ns** |  **0.1330 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    37.156 ns |   0.3822 ns |  0.3575 ns |   0.93 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    17.154 ns |   0.0536 ns |  0.0502 ns |   0.43 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |    **11.718 ns** |   **0.0264 ns** |  **0.0247 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    11.652 ns |   0.0141 ns |  0.0110 ns |   0.99 |    0.00 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |    10.145 ns |   0.0629 ns |  0.0558 ns |   0.87 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |    **11.758 ns** |   **0.0626 ns** |  **0.0586 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |    11.826 ns |   0.1257 ns |  0.1115 ns |   1.01 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     9.862 ns |   0.0498 ns |  0.0465 ns |   0.84 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |    **11.054 ns** |   **0.0391 ns** |  **0.0347 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |    15.528 ns |   0.2441 ns |  0.2283 ns |   1.40 |    0.02 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |    **22.896 ns** |   **0.1545 ns** |  **0.1445 ns** |   **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 8,411.376 ns |  49.8214 ns | 44.1654 ns | 367.38 |    2.92 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **1,465.363 ns** |   **2.7445 ns** |  **2.2918 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 3,821.654 ns |  22.6769 ns | 20.1025 ns |   2.61 |    0.01 | 0.0076 |     160 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,473.769 ns |  18.6021 ns | 15.5336 ns |   1.01 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **5,528.776 ns** |   **4.9746 ns** |  **4.1540 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 5,745.564 ns |  65.3003 ns | 61.0819 ns |   1.04 |    0.01 | 0.0076 |     192 B |        4.00 |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 4,097.459 ns |  24.4650 ns | 21.6876 ns |   0.74 |    0.00 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **1,778.366 ns** |  **12.6274 ns** | **11.1938 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 2,428.312 ns |   3.6431 ns |  3.0421 ns |   1.37 |    0.01 | 0.0114 |     192 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 1,606.592 ns |   0.3455 ns |  0.2885 ns |   0.90 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **3,288.580 ns** |  **59.4201 ns** | **55.5816 ns** |   **1.00** |    **0.02** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 3,415.353 ns |  40.5290 ns | 35.9279 ns |   1.04 |    0.02 | 0.0114 |     224 B |        4.67 |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 2,656.436 ns |  21.5150 ns | 19.0725 ns |   0.81 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |    **11.371 ns** |   **0.0765 ns** |  **0.0715 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |    15.350 ns |   0.2710 ns |  0.2263 ns |   1.35 |    0.02 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** | **4,588.100 ns** |  **80.3348 ns** | **67.0832 ns** |   **1.00** |    **0.02** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 8,948.013 ns |  70.6968 ns | 59.0350 ns |   1.95 |    0.03 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **3,000.827 ns** |  **33.8467 ns** | **31.6602 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 2,964.086 ns |  10.6989 ns |  9.4843 ns |   0.99 |    0.01 |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,503.495 ns |  17.6326 ns | 16.4936 ns |   0.50 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **5,986.563 ns** |  **58.7911 ns** | **54.9932 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 5,675.622 ns |  51.3150 ns | 45.4894 ns |   0.95 |    0.01 |      - |      48 B |        1.00 |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 4,150.107 ns |  59.4321 ns | 52.6850 ns |   0.69 |    0.01 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **2,423.949 ns** |  **41.6487 ns** | **38.9582 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 2,410.142 ns |  41.0486 ns | 36.3885 ns |   0.99 |    0.02 |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 1,690.248 ns |  11.5384 ns | 10.7930 ns |   0.70 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |            |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **6,947.844 ns** |  **57.9320 ns** | **54.1897 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 6,581.671 ns |  59.2226 ns | 52.4993 ns |   0.95 |    0.01 |      - |      48 B |        1.00 |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 5,774.985 ns |  73.0184 ns | 68.3014 ns |   0.83 |    0.01 |      - |      48 B |        1.00 |
