```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                          | Count | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **False** |     **183.1 ns** |   **0.97 ns** |   **0.90 ns** |  **1.00** |    **0.01** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | False |     335.4 ns |   1.75 ns |   1.46 ns |  1.83 |    0.01 | 0.1078 |      - |    1808 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 16    | False   | False |     270.1 ns |   1.68 ns |   1.57 ns |  1.47 |    0.01 | 0.0267 |      - |     448 B |        0.47 |
| SequenceAsync_Tasks             | 16    | False   | False |     406.0 ns |   2.73 ns |   2.55 ns |  2.22 |    0.02 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **True**  |   **4,596.6 ns** |  **22.93 ns** |  **21.45 ns** |  **1.00** |    **0.01** | **0.0687** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | True  |   8,311.1 ns |  40.79 ns |  38.16 ns |  1.81 |    0.01 | 0.1221 |      - |    2248 B |        1.90 |
| TraverseAsync_ValueTaskSelector | 16    | False   | True  |   9,046.2 ns |  53.50 ns |  50.04 ns |  1.97 |    0.01 | 0.1373 |      - |    2440 B |        2.06 |
| SequenceAsync_Tasks             | 16    | False   | True  |   5,452.6 ns |   9.64 ns |   9.02 ns |  1.19 |    0.01 | 0.1373 |      - |    2350 B |        1.98 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **False** |     **266.4 ns** |   **2.82 ns** |   **2.64 ns** |  **1.00** |    **0.01** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | False |     270.5 ns |   3.97 ns |   3.71 ns |  1.02 |    0.02 | 0.0954 |      - |    1600 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | False |     218.0 ns |   0.99 ns |   0.92 ns |  0.82 |    0.01 | 0.0143 |      - |     240 B |        0.17 |
| SequenceAsync_Tasks             | 16    | True    | False |     348.6 ns |   6.50 ns |   6.08 ns |  1.31 |    0.03 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **True**  |   **8,781.6 ns** |  **38.73 ns** |  **36.23 ns** |  **1.00** |    **0.01** | **0.1068** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | True  |   8,773.0 ns |  44.75 ns |  41.86 ns |  1.00 |    0.01 | 0.1221 |      - |    2064 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | True  |   9,947.3 ns |  56.46 ns |  52.81 ns |  1.13 |    0.01 | 0.1373 |      - |    2352 B |        1.25 |
| SequenceAsync_Tasks             | 16    | True    | True  |   5,734.6 ns |  10.20 ns |   9.04 ns |  0.65 |    0.00 | 0.1297 |      - |    2176 B |        1.16 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **False** |   **2,034.6 ns** |  **21.59 ns** |  **20.19 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | False |   4,048.4 ns |  47.35 ns |  44.29 ns |  1.99 |    0.03 | 1.3123 |      - |   21968 B |        1.91 |
| TraverseAsync_ValueTaskSelector | 256   | False   | False |   2,539.9 ns |   9.37 ns |   8.76 ns |  1.25 |    0.01 | 0.0839 |      - |    1408 B |        0.12 |
| SequenceAsync_Tasks             | 256   | False   | False |   4,545.7 ns |  44.79 ns |  41.90 ns |  2.23 |    0.03 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **True**  |  **73,222.3 ns** | **209.68 ns** | **196.13 ns** |  **1.00** |    **0.00** | **0.8545** |      **-** |   **14688 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | True  | 141,581.4 ns | 434.31 ns | 406.26 ns |  1.93 |    0.01 | 1.7090 |      - |   28232 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 256   | False   | True  | 148,845.6 ns | 549.68 ns | 459.01 ns |  2.03 |    0.01 | 1.7090 |      - |   32200 B |        2.19 |
| SequenceAsync_Tasks             | 256   | False   | True  |  89,024.2 ns | 414.82 ns | 388.02 ns |  1.22 |    0.01 | 1.7090 | 0.1221 |   30162 B |        2.05 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **False** |   **4,054.4 ns** |  **31.74 ns** |  **29.69 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | False |   3,415.2 ns |  26.44 ns |  24.73 ns |  0.84 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| TraverseAsync_ValueTaskSelector | 256   | True    | False |   2,623.4 ns |  51.99 ns |  51.06 ns |  0.65 |    0.01 | 0.1259 |      - |    2160 B |        0.10 |
| SequenceAsync_Tasks             | 256   | True    | False |   4,484.6 ns |  30.39 ns |  28.43 ns |  1.11 |    0.01 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **True**  | **139,669.2 ns** | **425.90 ns** | **398.39 ns** |  **1.00** |    **0.00** | **1.4648** |      **-** |   **27864 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | True  | 138,229.6 ns | 463.00 ns | 433.09 ns |  0.99 |    0.00 | 1.7090 |      - |   29008 B |        1.04 |
| TraverseAsync_ValueTaskSelector | 256   | True    | True  | 148,952.1 ns | 655.74 ns | 613.38 ns |  1.07 |    0.01 | 1.9531 |      - |   33072 B |        1.19 |
| SequenceAsync_Tasks             | 256   | True    | True  |  88,720.8 ns | 573.57 ns | 536.52 ns |  0.64 |    0.00 | 1.8311 | 0.1221 |   30940 B |        1.11 |
