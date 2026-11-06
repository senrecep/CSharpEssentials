```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                          | Count | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **False** |     **190.8 ns** |   **3.47 ns** |   **3.25 ns** |  **1.00** |    **0.02** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | False |     348.7 ns |   2.31 ns |   2.05 ns |  1.83 |    0.03 | 0.1078 |      - |    1808 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 16    | False   | False |     276.4 ns |   0.97 ns |   0.91 ns |  1.45 |    0.02 | 0.0267 |      - |     448 B |        0.47 |
| SequenceAsync_Tasks             | 16    | False   | False |     433.7 ns |   5.50 ns |   4.59 ns |  2.27 |    0.04 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **True**  |   **4,846.6 ns** |  **21.35 ns** |  **19.97 ns** |  **1.00** |    **0.01** | **0.0687** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | True  |   8,544.9 ns |  34.19 ns |  28.55 ns |  1.76 |    0.01 | 0.1221 |      - |    2248 B |        1.90 |
| TraverseAsync_ValueTaskSelector | 16    | False   | True  |   9,225.4 ns |  30.66 ns |  27.18 ns |  1.90 |    0.01 | 0.1373 |      - |    2440 B |        2.06 |
| SequenceAsync_Tasks             | 16    | False   | True  |   5,445.2 ns |  30.74 ns |  28.75 ns |  1.12 |    0.01 | 0.1373 |      - |    2349 B |        1.98 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **False** |     **287.9 ns** |   **2.85 ns** |   **2.67 ns** |  **1.00** |    **0.01** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | False |     277.6 ns |   4.05 ns |   3.59 ns |  0.96 |    0.01 | 0.0954 |      - |    1600 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | False |     213.6 ns |   0.66 ns |   0.55 ns |  0.74 |    0.01 | 0.0143 |      - |     240 B |        0.17 |
| SequenceAsync_Tasks             | 16    | True    | False |     405.0 ns |   3.07 ns |   2.87 ns |  1.41 |    0.02 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **True**  |   **9,056.6 ns** |  **32.36 ns** |  **28.69 ns** |  **1.00** |    **0.00** | **0.1068** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | True  |   9,112.2 ns |  53.33 ns |  49.88 ns |  1.01 |    0.01 | 0.1221 |      - |    2064 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | True  |   9,868.1 ns |  55.44 ns |  51.86 ns |  1.09 |    0.01 | 0.1373 |      - |    2352 B |        1.25 |
| SequenceAsync_Tasks             | 16    | True    | True  |   6,002.4 ns |  36.51 ns |  34.15 ns |  0.66 |    0.00 | 0.1297 |      - |    2175 B |        1.16 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **False** |   **2,213.5 ns** |  **18.87 ns** |  **17.65 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | False |   3,666.8 ns |  36.77 ns |  32.59 ns |  1.66 |    0.02 | 1.3123 | 0.0038 |   21968 B |        1.91 |
| TraverseAsync_ValueTaskSelector | 256   | False   | False |   2,536.8 ns |   3.60 ns |   3.19 ns |  1.15 |    0.01 | 0.0839 |      - |    1408 B |        0.12 |
| SequenceAsync_Tasks             | 256   | False   | False |   5,513.6 ns |  48.52 ns |  45.39 ns |  2.49 |    0.03 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **True**  |  **75,397.8 ns** | **356.80 ns** | **333.75 ns** |  **1.00** |    **0.01** | **0.8545** |      **-** |   **14685 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | True  | 140,392.4 ns | 363.64 ns | 322.36 ns |  1.86 |    0.01 | 1.7090 |      - |   28232 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 256   | False   | True  | 154,114.8 ns | 663.61 ns | 588.28 ns |  2.04 |    0.01 | 1.7090 |      - |   32200 B |        2.19 |
| SequenceAsync_Tasks             | 256   | False   | True  |  88,462.4 ns | 414.68 ns | 387.89 ns |  1.17 |    0.01 | 1.7090 | 0.1221 |   30163 B |        2.05 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **False** |   **4,212.4 ns** |  **39.23 ns** |  **36.70 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | False |   3,580.5 ns |  59.53 ns |  55.69 ns |  0.85 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| TraverseAsync_ValueTaskSelector | 256   | True    | False |   2,580.0 ns |   5.49 ns |   5.13 ns |  0.61 |    0.01 | 0.1259 |      - |    2160 B |        0.10 |
| SequenceAsync_Tasks             | 256   | True    | False |   4,680.8 ns |  35.95 ns |  33.63 ns |  1.11 |    0.01 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                                 |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **True**  | **147,527.8 ns** | **702.24 ns** | **656.87 ns** |  **1.00** |    **0.01** | **1.4648** |      **-** |   **27864 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | True  | 143,922.3 ns | 529.51 ns | 469.39 ns |  0.98 |    0.01 | 1.7090 |      - |   29008 B |        1.04 |
| TraverseAsync_ValueTaskSelector | 256   | True    | True  | 154,550.3 ns | 478.28 ns | 423.98 ns |  1.05 |    0.01 | 1.9531 |      - |   33072 B |        1.19 |
| SequenceAsync_Tasks             | 256   | True    | True  |  88,887.6 ns | 378.90 ns | 335.89 ns |  0.60 |    0.00 | 1.8311 | 0.1221 |   30939 B |        1.11 |
