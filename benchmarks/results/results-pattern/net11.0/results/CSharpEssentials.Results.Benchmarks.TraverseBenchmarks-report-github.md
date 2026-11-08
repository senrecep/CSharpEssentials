```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                          | Count | Succeed | Yield | Mean        | Error       | StdDev      | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |------ |------------:|------------:|------------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **False** |    **169.4 ns** |     **1.39 ns** |     **1.30 ns** |  **1.00** |    **0.01** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | False |    290.4 ns |     1.32 ns |     1.10 ns |  1.71 |    0.01 | 0.1078 |      - |    1808 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 16    | False   | False |    234.6 ns |     0.71 ns |     0.66 ns |  1.39 |    0.01 | 0.0267 |      - |     448 B |        0.47 |
| SequenceAsync_Tasks             | 16    | False   | False |    345.5 ns |     3.35 ns |     3.13 ns |  2.04 |    0.02 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **False**   | **True**  |  **3,083.9 ns** |    **34.65 ns** |    **32.41 ns** |  **1.00** |    **0.01** | **0.0610** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | False   | True  |  5,473.1 ns |    59.87 ns |    50.00 ns |  1.77 |    0.02 | 0.1221 |      - |    2248 B |        1.90 |
| TraverseAsync_ValueTaskSelector | 16    | False   | True  |  6,075.1 ns |    48.19 ns |    45.08 ns |  1.97 |    0.02 | 0.1450 |      - |    2440 B |        2.06 |
| SequenceAsync_Tasks             | 16    | False   | True  |  3,783.0 ns |    52.81 ns |    49.39 ns |  1.23 |    0.02 | 0.1373 |      - |    2328 B |        1.97 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **False** |    **267.5 ns** |     **2.75 ns** |     **2.58 ns** |  **1.00** |    **0.01** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | False |    248.4 ns |     2.86 ns |     2.68 ns |  0.93 |    0.01 | 0.0954 |      - |    1600 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | False |    195.8 ns |     1.08 ns |     1.01 ns |  0.73 |    0.01 | 0.0143 |      - |     240 B |        0.17 |
| SequenceAsync_Tasks             | 16    | True    | False |    300.8 ns |     1.79 ns |     1.59 ns |  1.12 |    0.01 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **16**    | **True**    | **True**  |  **5,661.8 ns** |    **43.15 ns** |    **36.03 ns** |  **1.00** |    **0.01** | **0.0916** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 16    | True    | True  |  6,095.2 ns |    39.68 ns |    37.11 ns |  1.08 |    0.01 | 0.1221 |      - |    2064 B |        1.10 |
| TraverseAsync_ValueTaskSelector | 16    | True    | True  |  6,600.9 ns |   131.63 ns |   171.16 ns |  1.17 |    0.03 | 0.1373 |      - |    2352 B |        1.25 |
| SequenceAsync_Tasks             | 16    | True    | True  |  4,099.8 ns |    51.27 ns |    47.95 ns |  0.72 |    0.01 | 0.1221 |      - |    2149 B |        1.14 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **False** |  **2,141.0 ns** |    **20.53 ns** |    **19.20 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | False |  3,418.9 ns |    14.08 ns |    12.48 ns |  1.60 |    0.02 | 1.3123 | 0.0038 |   21968 B |        1.91 |
| TraverseAsync_ValueTaskSelector | 256   | False   | False |  2,575.4 ns |     7.58 ns |     6.72 ns |  1.20 |    0.01 | 0.0839 |      - |    1408 B |        0.12 |
| SequenceAsync_Tasks             | 256   | False   | False |  4,107.2 ns |    49.45 ns |    41.29 ns |  1.92 |    0.03 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **False**   | **True**  | **47,056.1 ns** |   **252.83 ns** |   **224.13 ns** |  **1.00** |    **0.01** | **0.8545** |      **-** |   **14752 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | False   | True  | 90,937.8 ns |   651.06 ns |   609.00 ns |  1.93 |    0.02 | 1.7090 |      - |   28296 B |        1.92 |
| TraverseAsync_ValueTaskSelector | 256   | False   | True  | 97,316.4 ns | 1,271.87 ns | 1,249.14 ns |  2.07 |    0.03 | 1.8311 |      - |   32200 B |        2.18 |
| SequenceAsync_Tasks             | 256   | False   | True  | 49,766.0 ns |   462.33 ns |   432.46 ns |  1.06 |    0.01 | 1.7700 | 0.1221 |   30206 B |        2.05 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **False** |  **4,101.2 ns** |    **30.75 ns** |    **28.77 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | False |  3,384.0 ns |    52.70 ns |    46.71 ns |  0.83 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| TraverseAsync_ValueTaskSelector | 256   | True    | False |  2,527.5 ns |     5.91 ns |     5.24 ns |  0.62 |    0.00 | 0.1259 |      - |    2160 B |        0.10 |
| SequenceAsync_Tasks             | 256   | True    | False |  3,932.0 ns |    43.59 ns |    40.77 ns |  0.96 |    0.01 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                                 |       |         |       |             |             |             |       |         |        |        |           |             |
| **HandWritten_SequentialLoop**      | **256**   | **True**    | **True**  | **87,344.4 ns** |   **869.53 ns** |   **813.36 ns** |  **1.00** |    **0.01** | **1.4648** |      **-** |   **27928 B** |        **1.00** |
| TraverseAsync_TaskSelector      | 256   | True    | True  | 92,885.7 ns | 1,839.36 ns | 1,888.89 ns |  1.06 |    0.02 | 1.7090 |      - |   29072 B |        1.04 |
| TraverseAsync_ValueTaskSelector | 256   | True    | True  | 93,974.1 ns |   926.96 ns |   821.73 ns |  1.08 |    0.01 | 1.9531 |      - |   33072 B |        1.18 |
| SequenceAsync_Tasks             | 256   | True    | True  | 50,621.7 ns |   469.22 ns |   438.91 ns |  0.58 |    0.01 | 1.8311 | 0.1221 |   30979 B |        1.11 |
