```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                          | Count | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **False** |     **180.3 ns** |   **1.97 ns** |   **1.84 ns** |  **1.00** |    **0.01** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | False |     321.4 ns |   3.74 ns |   3.50 ns |  1.78 |    0.03 | 0.1078 |      - |    1808 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 16    | False   | False |     249.6 ns |   1.22 ns |   1.14 ns |  1.38 |    0.02 | 0.0267 |      - |     448 B |        0.47 |
| SequenceAsync_Tasks             | 16    | False   | False |     378.2 ns |   4.43 ns |   4.14 ns |  2.10 |    0.03 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **True**  |   **4,640.8 ns** |  **17.16 ns** |  **16.05 ns** |  **1.00** |    **0.00** | **0.0687** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | True  |   8,282.2 ns |  31.62 ns |  29.57 ns |  1.78 |    0.01 | 0.1221 |      - |    2248 B |        1.90 |
| TraverseAsync_ValueTaskSelector | 16    | False   | True  |   9,035.3 ns |  33.87 ns |  31.68 ns |  1.95 |    0.01 | 0.1373 |      - |    2440 B |        2.06 |
| SequenceAsync_Tasks             | 16    | False   | True  |   5,431.3 ns |  20.40 ns |  19.08 ns |  1.17 |    0.01 | 0.1373 |      - |    2354 B |        1.99 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **False** |     **282.9 ns** |   **2.38 ns** |   **2.22 ns** |  **1.00** |    **0.01** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | False |     273.2 ns |   3.63 ns |   3.39 ns |  0.97 |    0.01 | 0.0954 |      - |    1600 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | False |     203.2 ns |   0.36 ns |   0.32 ns |  0.72 |    0.01 | 0.0143 |      - |     240 B |        0.17 |
| SequenceAsync_Tasks             | 16    | True    | False |     333.0 ns |   4.45 ns |   4.16 ns |  1.18 |    0.02 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **True**  |   **8,700.9 ns** |  **40.91 ns** |  **36.27 ns** |  **1.00** |    **0.01** | **0.1068** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | True  |   8,809.7 ns | 100.68 ns |  89.25 ns |  1.01 |    0.01 | 0.1221 |      - |    2064 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | True  |   9,639.4 ns |  34.04 ns |  31.85 ns |  1.11 |    0.01 | 0.1373 |      - |    2352 B |        1.25 |
| SequenceAsync_Tasks             | 16    | True    | True  |   5,656.2 ns |  20.48 ns |  19.16 ns |  0.65 |    0.00 | 0.1297 |      - |    2178 B |        1.16 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **False** |   **2,239.4 ns** |  **20.89 ns** |  **19.54 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | False |   3,659.3 ns |  56.00 ns |  46.77 ns |  1.63 |    0.02 | 1.3123 | 0.0038 |   21968 B |        1.91 |
| TraverseAsync_ValueTaskSelector | 256   | False   | False |   2,627.8 ns |   2.52 ns |   2.36 ns |  1.17 |    0.01 | 0.0839 |      - |    1408 B |        0.12 |
| SequenceAsync_Tasks             | 256   | False   | False |   4,983.9 ns |  97.81 ns | 127.18 ns |  2.23 |    0.06 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **True**  |  **73,943.8 ns** | **367.42 ns** | **343.68 ns** |  **1.00** |    **0.01** | **0.7324** |      **-** |   **14687 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | True  | 140,139.7 ns | 465.47 ns | 412.63 ns |  1.90 |    0.01 | 1.7090 |      - |   28232 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 256   | False   | True  | 148,059.0 ns | 732.62 ns | 685.29 ns |  2.00 |    0.01 | 1.7090 |      - |   32200 B |        2.19 |
| SequenceAsync_Tasks             | 256   | False   | True  |  87,802.7 ns | 464.50 ns | 434.49 ns |  1.19 |    0.01 | 1.7090 | 0.1221 |   30163 B |        2.05 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **False** |   **4,368.4 ns** |  **40.93 ns** |  **38.28 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | False |   3,545.6 ns |  51.78 ns |  48.44 ns |  0.81 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| TraverseAsync_ValueTaskSelector | 256   | True    | False |   2,630.7 ns |   5.01 ns |   4.44 ns |  0.60 |    0.01 | 0.1259 |      - |    2160 B |        0.10 |
| SequenceAsync_Tasks             | 256   | True    | False |   4,613.2 ns |  71.53 ns |  66.91 ns |  1.06 |    0.02 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **True**  | **139,510.5 ns** | **525.69 ns** | **466.01 ns** |  **1.00** |    **0.00** | **1.4648** |      **-** |   **27864 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | True  | 142,568.6 ns | 717.82 ns | 671.45 ns |  1.02 |    0.01 | 1.7090 |      - |   29008 B |        1.04 |
| TraverseAsync_ValueTaskSelector | 256   | True    | True  | 150,662.1 ns | 882.66 ns | 825.64 ns |  1.08 |    0.01 | 1.9531 |      - |   33072 B |        1.19 |
| SequenceAsync_Tasks             | 256   | True    | True  |  98,648.2 ns | 494.24 ns | 462.31 ns |  0.71 |    0.00 | 1.8311 | 0.1221 |   30941 B |        1.11 |
