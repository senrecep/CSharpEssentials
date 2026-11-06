```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                          | Count | Succeed | Yield | Mean        | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |------ |------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **False** |    **179.4 ns** |     **2.27 ns** |     **2.13 ns** |  **1.00** |    **0.02** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | False |    319.4 ns |     4.87 ns |     4.55 ns |  1.78 |    0.03 | 0.1078 |      - |    1808 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 16    | False   | False |    260.3 ns |     2.30 ns |     2.15 ns |  1.45 |    0.02 | 0.0267 |      - |     448 B |        0.47 |
| SequenceAsync_Tasks             | 16    | False   | False |    364.4 ns |     3.39 ns |     3.17 ns |  2.03 |    0.03 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **True**  |  **4,151.4 ns** |    **81.88 ns** |   **120.02 ns** |  **1.00** |    **0.04** | **0.0610** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | True  |  6,862.3 ns |   134.66 ns |   242.83 ns |  1.65 |    0.07 | 0.1221 |      - |    2249 B |        1.90 |
| TraverseAsync_ValueTaskSelector | 16    | False   | True  |  6,880.9 ns |   107.99 ns |    90.18 ns |  1.66 |    0.05 | 0.1450 |      - |    2440 B |        2.06 |
| SequenceAsync_Tasks             | 16    | False   | True  |  5,724.5 ns |    89.03 ns |    83.28 ns |  1.38 |    0.04 | 0.1373 |      - |    2396 B |        2.02 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **False** |    **278.9 ns** |     **3.69 ns** |     **3.45 ns** |  **1.00** |    **0.02** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | False |    261.0 ns |     2.49 ns |     2.21 ns |  0.94 |    0.01 | 0.0954 |      - |    1600 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | False |    196.6 ns |     1.22 ns |     1.14 ns |  0.70 |    0.01 | 0.0143 |      - |     240 B |        0.17 |
| SequenceAsync_Tasks             | 16    | True    | False |    311.4 ns |     2.69 ns |     2.52 ns |  1.12 |    0.02 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **True**  |  **6,229.0 ns** |   **120.97 ns** |   **118.80 ns** |  **1.00** |    **0.03** | **0.0610** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | True  |  6,511.7 ns |   125.47 ns |   117.36 ns |  1.05 |    0.03 | 0.1221 |      - |    2065 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | True  |  6,688.3 ns |   125.06 ns |   116.98 ns |  1.07 |    0.03 | 0.1373 |      - |    2352 B |        1.25 |
| SequenceAsync_Tasks             | 16    | True    | True  |  5,419.0 ns |    47.30 ns |    39.50 ns |  0.87 |    0.02 | 0.1297 |      - |    2212 B |        1.18 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **False** |  **2,175.3 ns** |    **20.80 ns** |    **19.45 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | False |  3,503.4 ns |    68.64 ns |    86.81 ns |  1.61 |    0.04 | 1.3123 | 0.0038 |   21968 B |        1.91 |
| TraverseAsync_ValueTaskSelector | 256   | False   | False |  2,572.2 ns |     2.28 ns |     1.91 ns |  1.18 |    0.01 | 0.0839 |      - |    1408 B |        0.12 |
| SequenceAsync_Tasks             | 256   | False   | False |  4,340.8 ns |    82.76 ns |    81.28 ns |  2.00 |    0.04 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **True**  | **29,768.6 ns** |   **511.40 ns** |   **453.35 ns** |  **1.00** |    **0.02** | **0.7324** |      **-** |   **14751 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | True  | 52,157.4 ns |   463.81 ns |   433.85 ns |  1.75 |    0.03 | 1.6479 |      - |   28296 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 256   | False   | True  | 57,205.2 ns |   448.12 ns |   419.18 ns |  1.92 |    0.03 | 1.8921 |      - |   32200 B |        2.18 |
| SequenceAsync_Tasks             | 256   | False   | True  | 30,532.7 ns |   471.52 ns |   441.06 ns |  1.03 |    0.02 | 1.7090 |      - |   30199 B |        2.05 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **False** |  **4,267.4 ns** |    **49.67 ns** |    **46.46 ns** |  **1.00** |    **0.02** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | False |  3,479.4 ns |    46.61 ns |    41.32 ns |  0.82 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| TraverseAsync_ValueTaskSelector | 256   | True    | False |  2,544.5 ns |     4.09 ns |     3.83 ns |  0.60 |    0.01 | 0.1259 |      - |    2160 B |        0.10 |
| SequenceAsync_Tasks             | 256   | True    | False |  4,315.3 ns |    65.99 ns |    58.49 ns |  1.01 |    0.02 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **True**  | **50,046.9 ns** |   **606.48 ns** |   **537.63 ns** |  **1.00** |    **0.01** | **1.4648** |      **-** |   **27928 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | True  | 52,940.7 ns |   537.24 ns |   502.53 ns |  1.06 |    0.01 | 1.7090 |      - |   29072 B |        1.04 |
| TraverseAsync_ValueTaskSelector | 256   | True    | True  | 55,831.2 ns | 1,111.28 ns | 1,557.87 ns |  1.12 |    0.03 | 1.9531 |      - |   33073 B |        1.18 |
| SequenceAsync_Tasks             | 256   | True    | True  | 30,245.6 ns |   407.91 ns |   361.60 ns |  0.60 |    0.01 | 1.8311 | 0.1221 |   30984 B |        1.11 |
