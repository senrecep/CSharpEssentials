```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                   | Categories    | Source   | Mean          | Error      | StdDev      | Ratio    | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------------------------------------- |-------------- |--------- |--------------:|-----------:|------------:|---------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Choose**                  | **Choose**        | **List**     | **3,403.2886 ns** | **25.1209 ns** |  **23.4981 ns** |     **1.00** |    **0.01** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | List     | 3,676.2386 ns | 30.3170 ns |  28.3585 ns |     1.08 |    0.01 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | List     |   449.3803 ns |  0.0526 ns |   0.0492 ns |     0.13 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_Choose**                  | **Choose**        | **Sequence** | **3,283.4757 ns** | **29.3244 ns** |  **27.4300 ns** |     **1.00** |    **0.01** | **0.0038** |      **-** |      **88 B** |        **1.00** |
| CSharpFunctionalExtensions_Choose        | Choose        | Sequence | 3,520.4162 ns | 69.4688 ns | 128.7650 ns |     1.07 |    0.04 | 0.0038 |      - |      88 B |        1.00 |
| HandWritten_Choose                       | Choose        | Sequence |   451.3236 ns |  0.1722 ns |   0.1438 ns |     0.14 |    0.00 |      - |      - |         - |        0.00 |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **List**     |    **21.9958 ns** |  **0.3658 ns** |   **0.3243 ns** |     **1.00** |    **0.02** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | List     |    21.3125 ns |  0.0341 ns |   0.0302 ns |     0.97 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | List     |    10.5059 ns |  0.0085 ns |   0.0075 ns |     0.48 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFindHit**              | **TryFindHit**    | **Sequence** |    **21.3798 ns** |  **0.2867 ns** |   **0.2682 ns** |     **1.00** |    **0.02** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindHit    | TryFindHit    | Sequence |    21.4046 ns |  0.0081 ns |   0.0075 ns |     1.00 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryFindHit                   | TryFindHit    | Sequence |    10.5974 ns |  0.0381 ns |   0.0356 ns |     0.50 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **List**     |    **11.7932 ns** |  **0.0050 ns** |   **0.0047 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | List     |    11.8222 ns |  0.0067 ns |   0.0063 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | List     |     8.9343 ns |  0.0190 ns |   0.0168 ns |     0.76 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFindMiss**             | **TryFindMiss**   | **Sequence** |     **9.6708 ns** |  **0.0201 ns** |   **0.0188 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFindMiss   | TryFindMiss   | Sequence |     9.6309 ns |  0.0169 ns |   0.0158 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryFindMiss                  | TryFindMiss   | Sequence |     8.8845 ns |  0.0188 ns |   0.0176 ns |     0.92 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **List**     |     **1.0858 ns** |  **0.0027 ns** |   **0.0024 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | List     |     7.9071 ns |  0.0513 ns |   0.0428 ns |     7.28 |    0.04 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFirst**                | **TryFirst**      | **Sequence** |     **0.8327 ns** |  **0.0037 ns** |   **0.0033 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirst      | TryFirst      | Sequence | 4,282.8529 ns | 35.3851 ns |  33.0992 ns | 5,143.52 |   43.18 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **List**     | **1,234.2120 ns** |  **2.7403 ns** |   **2.5633 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | List     | 3,546.2452 ns | 40.4917 ns |  31.6133 ns |     2.87 |    0.03 | 0.0076 |      - |     160 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | List     | 1,214.1907 ns |  0.4081 ns |   0.3407 ns |     0.98 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstMiss**            | **TryFirstMiss**  | **Sequence** | **1,222.0318 ns** |  **0.4290 ns** |   **0.4013 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstMiss  | TryFirstMiss  | Sequence | 3,953.8047 ns | 30.2234 ns |  28.2710 ns |     3.24 |    0.02 | 0.0076 |      - |     192 B |          NA |
| HandWritten_TryFirstMiss                 | TryFirstMiss  | Sequence | 1,452.9360 ns |  0.7205 ns |   0.6740 ns |     1.19 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **List**     | **1,463.7464 ns** |  **0.3582 ns** |   **0.2796 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | List     | 2,416.3632 ns |  2.9321 ns |   2.7427 ns |     1.65 |    0.00 | 0.0114 |      - |     192 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | List     | 1,463.1569 ns |  0.6366 ns |   0.5643 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryFirstWhere**           | **TryFirstWhere** | **Sequence** | **1,477.6782 ns** |  **2.8788 ns** |   **2.5520 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryFirstWhere | TryFirstWhere | Sequence | 2,617.6812 ns | 12.4972 ns |  11.6899 ns |     1.77 |    0.01 | 0.0114 |      - |     224 B |          NA |
| HandWritten_TryFirstWhere                | TryFirstWhere | Sequence | 1,625.9871 ns |  8.2652 ns |   7.7313 ns |     1.10 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **List**     |     **2.5696 ns** |  **0.0182 ns** |   **0.0152 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | List     |     8.7540 ns |  0.1271 ns |   0.1127 ns |     3.41 |    0.05 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryLast**                 | **TryLast**       | **Sequence** |   **591.6519 ns** |  **4.3293 ns** |   **3.6152 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLast       | TryLast       | Sequence | 4,355.1432 ns | 55.9140 ns |  52.3020 ns |     7.36 |    0.10 | 0.9918 | 0.0305 |   16600 B |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **List**     | **1,191.4421 ns** |  **1.7111 ns** |   **1.5168 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | List     | 1,194.8978 ns |  2.4872 ns |   2.3265 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | List     | 1,200.3192 ns |  7.5527 ns |   7.0648 ns |     1.01 |    0.01 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryLastMiss**             | **TryLastMiss**   | **Sequence** | **1,227.9459 ns** |  **6.5719 ns** |   **5.4878 ns** |     **1.00** |    **0.01** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastMiss   | TryLastMiss   | Sequence | 1,488.1080 ns |  3.6435 ns |   2.8446 ns |     1.21 |    0.01 |      - |      - |         - |          NA |
| HandWritten_TryLastMiss                  | TryLastMiss   | Sequence | 1,231.5654 ns |  3.1241 ns |   2.7694 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **List**     | **1,478.8776 ns** |  **4.7669 ns** |   **4.4589 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | List     | 1,480.5328 ns |  5.5675 ns |   5.2079 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | List     | 1,478.2216 ns |  6.4086 ns |   5.6811 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
|                                          |               |          |               |            |             |          |         |        |        |           |             |
| **CSharpEssentials_TryLastWhere**            | **TryLastWhere**  | **Sequence** | **2,948.9238 ns** |  **8.6788 ns** |   **7.6936 ns** |     **1.00** |    **0.00** |      **-** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_TryLastWhere  | TryLastWhere  | Sequence | 4,157.7061 ns | 14.2452 ns |  12.6280 ns |     1.41 |    0.01 |      - |      - |      48 B |          NA |
| HandWritten_TryLastWhere                 | TryLastWhere  | Sequence | 2,943.0152 ns |  9.0253 ns |   8.4423 ns |     1.00 |    0.00 |      - |      - |         - |          NA |
