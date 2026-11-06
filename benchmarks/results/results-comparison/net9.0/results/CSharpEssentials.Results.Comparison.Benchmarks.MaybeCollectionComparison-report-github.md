```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean         | Error       | StdDev      | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |-------------:|------------:|------------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **4,992.979 ns** |  **25.2913 ns** |  **22.4200 ns** |   **1.00** |    **0.01** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 6,036.722 ns |   2.1977 ns |   1.8351 ns |   1.21 |    0.01 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   458.721 ns |   1.1466 ns |   1.0726 ns |   0.09 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **5,379.125 ns** | **106.2611 ns** | **118.1089 ns** |   **1.00** |    **0.03** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 6,170.203 ns |   7.7129 ns |   7.2146 ns |   1.15 |    0.02 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   457.189 ns |   1.9011 ns |   1.7783 ns |   0.09 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **37.516 ns** |   **0.2226 ns** |   **0.2082 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    36.937 ns |   0.0777 ns |   0.0689 ns |   0.98 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    16.970 ns |   0.0671 ns |   0.0628 ns |   0.45 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **38.149 ns** |   **0.3019 ns** |   **0.2676 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    37.409 ns |   0.0680 ns |   0.0568 ns |   0.98 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    16.832 ns |   0.0346 ns |   0.0289 ns |   0.44 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |    **11.657 ns** |   **0.0583 ns** |   **0.0545 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    11.670 ns |   0.0427 ns |   0.0378 ns |   1.00 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |     9.701 ns |   0.0110 ns |   0.0092 ns |   0.83 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |    **11.844 ns** |   **0.0630 ns** |   **0.0589 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |    11.735 ns |   0.0213 ns |   0.0199 ns |   0.99 |    0.01 |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     9.833 ns |   0.0828 ns |   0.0774 ns |   0.83 |    0.01 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |    **15.320 ns** |   **0.0901 ns** |   **0.0843 ns** |   **1.00** |    **0.01** | **0.0024** |      **40 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |    14.726 ns |   0.0040 ns |   0.0031 ns |   0.96 |    0.01 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |    **16.287 ns** |   **0.1166 ns** |   **0.1091 ns** |   **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 8,581.240 ns |  47.3369 ns |  44.2790 ns | 526.90 |    4.31 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **6,075.384 ns** |  **45.9151 ns** |  **42.9490 ns** |   **1.00** |    **0.01** |      **-** |      **40 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 3,546.207 ns |  23.1385 ns |  20.5117 ns |   0.58 |    0.01 | 0.0076 |     160 B |        4.00 |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,456.776 ns |   0.3349 ns |   0.2797 ns |   0.24 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **5,229.714 ns** |   **9.2935 ns** |   **7.2557 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 5,836.557 ns |  40.0125 ns |  37.4277 ns |   1.12 |    0.01 | 0.0076 |     192 B |        4.00 |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 3,832.063 ns |  18.3835 ns |  17.1959 ns |   0.73 |    0.00 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **3,552.999 ns** |  **12.5202 ns** |  **11.0988 ns** |   **1.00** |    **0.00** |      **-** |      **40 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 2,456.392 ns |  12.1026 ns |  11.3208 ns |   0.69 |    0.00 | 0.0114 |     192 B |        4.80 |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 1,622.883 ns |   4.1573 ns |   3.8888 ns |   0.46 |    0.00 |      - |         - |        0.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **3,114.837 ns** |  **13.4467 ns** |  **12.5781 ns** |   **1.00** |    **0.01** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 3,442.192 ns |  54.0563 ns |  50.5643 ns |   1.11 |    0.02 | 0.0114 |     224 B |        4.67 |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 2,642.237 ns |   8.7475 ns |   8.1824 ns |   0.85 |    0.00 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |    **11.153 ns** |   **0.0078 ns** |   **0.0069 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |    14.706 ns |   0.0079 ns |   0.0061 ns |   1.32 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** | **4,951.827 ns** |   **5.7764 ns** |   **5.1206 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 8,690.226 ns |  79.6444 ns |  74.4995 ns |   1.75 |    0.01 | 0.9918 |   16648 B |      346.83 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **2,897.326 ns** |   **0.5296 ns** |   **0.4695 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 3,223.150 ns |  11.6517 ns |   9.7297 ns |   1.11 |    0.00 |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,475.216 ns |   8.7744 ns |   8.2076 ns |   0.51 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **5,237.182 ns** |   **5.1368 ns** |   **4.2894 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 5,533.070 ns |   8.1965 ns |   6.3993 ns |   1.06 |    0.00 |      - |      48 B |        1.00 |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 3,502.743 ns |   3.2830 ns |   2.5632 ns |   0.67 |    0.00 |      - |      48 B |        1.00 |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **2,329.476 ns** |   **0.6911 ns** |   **0.6126 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 2,331.367 ns |   1.3799 ns |   1.2233 ns |   1.00 |    0.00 |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 1,604.034 ns |   0.2713 ns |   0.2266 ns |   0.69 |    0.00 |      - |         - |          NA |
|                                          |               |          |              |             |             |        |         |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **6,711.837 ns** |   **6.2134 ns** |   **5.8120 ns** |   **1.00** |    **0.00** |      **-** |      **48 B** |        **1.00** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 6,571.505 ns |  25.6168 ns |  22.7087 ns |   0.98 |    0.00 |      - |      48 B |        1.00 |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 5,253.496 ns |   2.8166 ns |   2.4968 ns |   0.78 |    0.00 |      - |      48 B |        1.00 |
