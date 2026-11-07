```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Count | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |------ |-------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop** | **16**    | **False**   | **False** |     **185.1 ns** |   **1.71 ns** |   **1.60 ns** |  **1.00** |    **0.01** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | False   | False |     349.9 ns |   1.55 ns |   1.37 ns |  1.89 |    0.02 | 0.1078 |      - |    1808 B |        1.92 |
| SequenceAsync_Tasks        | 16    | False   | False |     421.4 ns |   3.37 ns |   2.98 ns |  2.28 |    0.02 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **16**    | **False**   | **True**  |   **4,829.2 ns** |  **21.84 ns** |  **19.36 ns** |  **1.00** |    **0.01** | **0.0687** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | False   | True  |   8,597.6 ns |  49.85 ns |  46.63 ns |  1.78 |    0.01 | 0.1221 |      - |    2248 B |        1.90 |
| SequenceAsync_Tasks        | 16    | False   | True  |   5,478.5 ns |  37.46 ns |  35.04 ns |  1.13 |    0.01 | 0.1373 |      - |    2350 B |        1.98 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **16**    | **True**    | **False** |     **269.2 ns** |   **2.52 ns** |   **2.36 ns** |  **1.00** |    **0.01** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | True    | False |     279.8 ns |   3.09 ns |   2.74 ns |  1.04 |    0.01 | 0.0954 |      - |    1600 B |        1.10 |
| SequenceAsync_Tasks        | 16    | True    | False |     332.3 ns |   2.14 ns |   2.00 ns |  1.23 |    0.01 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **16**    | **True**    | **True**  |   **9,044.6 ns** |  **41.73 ns** |  **36.99 ns** |  **1.00** |    **0.01** | **0.1068** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | True    | True  |   9,255.7 ns |  42.80 ns |  40.04 ns |  1.02 |    0.01 | 0.1221 |      - |    2064 B |        1.10 |
| SequenceAsync_Tasks        | 16    | True    | True  |   5,804.9 ns |  17.80 ns |  16.65 ns |  0.64 |    0.00 | 0.1297 |      - |    2176 B |        1.16 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **False**   | **False** |   **2,130.6 ns** |   **8.89 ns** |   **8.32 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | False   | False |   3,489.5 ns |  29.17 ns |  27.29 ns |  1.64 |    0.01 | 1.3123 | 0.0038 |   21968 B |        1.91 |
| SequenceAsync_Tasks        | 256   | False   | False |   4,594.1 ns |  24.12 ns |  21.39 ns |  2.16 |    0.01 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **False**   | **True**  |  **73,023.5 ns** | **805.94 ns** | **672.99 ns** |  **1.00** |    **0.01** | **0.8545** |      **-** |   **14688 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | False   | True  | 138,520.2 ns | 623.41 ns | 583.14 ns |  1.90 |    0.02 | 1.7090 |      - |   28232 B |        1.92 |
| SequenceAsync_Tasks        | 256   | False   | True  |  90,688.9 ns | 258.96 ns | 229.56 ns |  1.24 |    0.01 | 1.7090 | 0.1221 |   30162 B |        2.05 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **True**    | **False** |   **4,035.3 ns** |  **29.34 ns** |  **27.44 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | True    | False |   3,404.6 ns |  11.80 ns |  10.46 ns |  0.84 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| SequenceAsync_Tasks        | 256   | True    | False |   4,431.8 ns |  28.77 ns |  26.91 ns |  1.10 |    0.01 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **True**    | **True**  | **145,529.1 ns** | **371.55 ns** | **347.55 ns** |  **1.00** |    **0.00** | **1.4648** |      **-** |   **27864 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | True    | True  | 143,399.6 ns | 396.78 ns | 371.14 ns |  0.99 |    0.00 | 1.4648 |      - |   29008 B |        1.04 |
| SequenceAsync_Tasks        | 256   | True    | True  |  87,928.6 ns | 217.22 ns | 203.19 ns |  0.60 |    0.00 | 1.8311 | 0.1221 |   30938 B |        1.11 |
