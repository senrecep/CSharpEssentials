```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                          | Count | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **False** |     **173.5 ns** |   **1.01 ns** |   **0.94 ns** |  **1.00** |    **0.01** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | False |     300.1 ns |   1.85 ns |   1.64 ns |  1.73 |    0.01 | 0.1078 |      - |    1808 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 16    | False   | False |     245.9 ns |   0.94 ns |   0.88 ns |  1.42 |    0.01 | 0.0267 |      - |     448 B |        0.47 |
| SequenceAsync_Tasks             | 16    | False   | False |     384.6 ns |   2.23 ns |   1.86 ns |  2.22 |    0.02 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **True**  |   **4,596.8 ns** |  **22.02 ns** |  **20.60 ns** |  **1.00** |    **0.01** | **0.0687** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | True  |   8,183.9 ns |  30.90 ns |  27.39 ns |  1.78 |    0.01 | 0.1221 |      - |    2248 B |        1.90 |
| TraverseAsync_ValueTaskSelector | 16    | False   | True  |   9,009.4 ns |  40.06 ns |  35.51 ns |  1.96 |    0.01 | 0.1373 |      - |    2440 B |        2.06 |
| SequenceAsync_Tasks             | 16    | False   | True  |   5,384.6 ns |  21.05 ns |  19.69 ns |  1.17 |    0.01 | 0.1373 |      - |    2354 B |        1.99 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **False** |     **272.4 ns** |   **3.16 ns** |   **2.47 ns** |  **1.00** |    **0.01** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | False |     257.7 ns |   2.00 ns |   1.87 ns |  0.95 |    0.01 | 0.0954 |      - |    1600 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | False |     201.8 ns |   0.68 ns |   0.63 ns |  0.74 |    0.01 | 0.0143 |      - |     240 B |        0.17 |
| SequenceAsync_Tasks             | 16    | True    | False |     313.3 ns |   2.12 ns |   1.99 ns |  1.15 |    0.01 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **True**  |   **8,734.3 ns** |  **22.89 ns** |  **21.41 ns** |  **1.00** |    **0.00** | **0.1068** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | True  |   8,672.5 ns |  33.59 ns |  29.78 ns |  0.99 |    0.00 | 0.1221 |      - |    2064 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | True  |   9,384.1 ns |  48.13 ns |  45.02 ns |  1.07 |    0.01 | 0.1373 |      - |    2352 B |        1.25 |
| SequenceAsync_Tasks             | 16    | True    | True  |   5,751.0 ns |  18.20 ns |  17.02 ns |  0.66 |    0.00 | 0.1297 |      - |    2178 B |        1.16 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **False** |   **2,148.5 ns** |  **13.01 ns** |  **12.17 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | False |   3,451.9 ns |  19.80 ns |  18.52 ns |  1.61 |    0.01 | 1.3123 | 0.0038 |   21968 B |        1.91 |
| TraverseAsync_ValueTaskSelector | 256   | False   | False |   2,622.5 ns |   7.67 ns |   7.18 ns |  1.22 |    0.01 | 0.0839 |      - |    1408 B |        0.12 |
| SequenceAsync_Tasks             | 256   | False   | False |   4,570.4 ns |  71.23 ns |  66.63 ns |  2.13 |    0.03 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **True**  |  **70,743.0 ns** | **167.84 ns** | **148.79 ns** |  **1.00** |    **0.00** | **0.8545** |      **-** |   **14687 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | True  | 134,163.2 ns | 915.97 ns | 856.80 ns |  1.90 |    0.01 | 1.7090 |      - |   28232 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 256   | False   | True  | 145,722.0 ns | 496.35 ns | 464.28 ns |  2.06 |    0.01 | 1.7090 |      - |   32200 B |        2.19 |
| SequenceAsync_Tasks             | 256   | False   | True  |  87,170.5 ns | 405.61 ns | 379.40 ns |  1.23 |    0.01 | 1.7090 | 0.1221 |   30164 B |        2.05 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **False** |   **4,183.0 ns** |  **32.34 ns** |  **30.25 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | False |   3,440.5 ns |  27.28 ns |  25.52 ns |  0.82 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| TraverseAsync_ValueTaskSelector | 256   | True    | False |   2,612.7 ns |   8.66 ns |   8.10 ns |  0.62 |    0.00 | 0.1259 |      - |    2160 B |        0.10 |
| SequenceAsync_Tasks             | 256   | True    | False |   5,029.4 ns | 100.40 ns | 156.32 ns |  1.20 |    0.04 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **True**  | **140,920.7 ns** | **387.97 ns** | **343.93 ns** |  **1.00** |    **0.00** | **1.4648** |      **-** |   **27864 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | True  | 136,371.2 ns | 316.04 ns | 295.63 ns |  0.97 |    0.00 | 1.7090 |      - |   29008 B |        1.04 |
| TraverseAsync_ValueTaskSelector | 256   | True    | True  | 152,914.1 ns | 560.60 ns | 524.39 ns |  1.09 |    0.00 | 1.9531 |      - |   33072 B |        1.19 |
| SequenceAsync_Tasks             | 256   | True    | True  |  87,617.9 ns | 246.83 ns | 218.81 ns |  0.62 |    0.00 | 1.8311 | 0.1221 |   30940 B |        1.11 |
