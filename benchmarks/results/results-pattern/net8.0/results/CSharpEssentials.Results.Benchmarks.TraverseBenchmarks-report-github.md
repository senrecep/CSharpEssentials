```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Count | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |------ |-------- |------ |-------------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **HandWritten_SequentialLoop** | **16**    | **False**   | **False** |     **184.8 ns** |   **1.53 ns** |   **1.36 ns** |  **1.00** |    **0.01** | **0.0563** |      **-** |     **944 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | False   | False |     389.9 ns |   3.79 ns |   3.55 ns |  2.11 |    0.02 | 0.1078 |      - |    1808 B |        1.92 |
| SequenceAsync_Tasks        | 16    | False   | False |     432.0 ns |   3.38 ns |   3.16 ns |  2.34 |    0.02 | 0.1168 | 0.0005 |    1960 B |        2.08 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **16**    | **False**   | **True**  |   **4,879.1 ns** |  **18.22 ns** |  **15.22 ns** |  **1.00** |    **0.00** | **0.0687** |      **-** |    **1184 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | False   | True  |   9,066.8 ns |  44.94 ns |  42.04 ns |  1.86 |    0.01 | 0.1221 |      - |    2248 B |        1.90 |
| SequenceAsync_Tasks        | 16    | False   | True  |   5,406.4 ns |  47.95 ns |  44.85 ns |  1.11 |    0.01 | 0.1373 |      - |    2349 B |        1.98 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **16**    | **True**    | **False** |     **285.9 ns** |   **3.48 ns** |   **3.25 ns** |  **1.00** |    **0.02** | **0.0863** |      **-** |    **1448 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | True    | False |     297.6 ns |   3.74 ns |   3.49 ns |  1.04 |    0.02 | 0.0954 |      - |    1600 B |        1.10 |
| SequenceAsync_Tasks        | 16    | True    | False |     359.0 ns |   5.72 ns |   5.35 ns |  1.26 |    0.02 | 0.1044 | 0.0005 |    1752 B |        1.21 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **16**    | **True**    | **True**  |   **9,257.1 ns** |  **48.17 ns** |  **42.70 ns** |  **1.00** |    **0.01** | **0.1068** |      **-** |    **1880 B** |        **1.00** |
| TraverseAsync_TaskSelector | 16    | True    | True  |   9,340.6 ns |  65.76 ns |  61.51 ns |  1.01 |    0.01 | 0.1221 |      - |    2064 B |        1.10 |
| SequenceAsync_Tasks        | 16    | True    | True  |   5,742.3 ns |  27.69 ns |  25.90 ns |  0.62 |    0.00 | 0.1297 |      - |    2176 B |        1.16 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **False**   | **False** |   **2,229.9 ns** |  **21.57 ns** |  **20.17 ns** |  **1.00** |    **0.01** | **0.6866** |      **-** |   **11504 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | False   | False |   3,625.9 ns |  36.56 ns |  34.20 ns |  1.63 |    0.02 | 1.3123 |      - |   21968 B |        1.91 |
| SequenceAsync_Tasks        | 256   | False   | False |   4,738.7 ns |  52.58 ns |  49.18 ns |  2.13 |    0.03 | 1.4343 | 0.1144 |   24040 B |        2.09 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **False**   | **True**  |  **77,577.0 ns** | **238.03 ns** | **222.65 ns** |  **1.00** |    **0.00** | **0.8545** |      **-** |   **14687 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | False   | True  | 146,550.0 ns | 460.44 ns | 408.17 ns |  1.89 |    0.01 | 1.7090 |      - |   28232 B |        1.92 |
| SequenceAsync_Tasks        | 256   | False   | True  |  91,170.2 ns | 663.65 ns | 620.77 ns |  1.18 |    0.01 | 1.7090 |      - |   30163 B |        2.05 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **True**    | **False** |   **4,278.2 ns** |  **39.61 ns** |  **37.06 ns** |  **1.00** |    **0.01** | **1.2894** |      **-** |   **21608 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | True    | False |   3,614.1 ns |  47.37 ns |  44.31 ns |  0.84 |    0.01 | 1.3580 | 0.0038 |   22720 B |        1.05 |
| SequenceAsync_Tasks        | 256   | True    | False |   4,670.3 ns |  31.97 ns |  29.91 ns |  1.09 |    0.01 | 1.4801 | 0.1144 |   24792 B |        1.15 |
|                            |       |         |       |              |           |           |       |         |        |        |           |             |
| **HandWritten_SequentialLoop** | **256**   | **True**    | **True**  | **146,493.5 ns** | **614.43 ns** | **574.74 ns** |  **1.00** |    **0.01** | **1.4648** |      **-** |   **27864 B** |        **1.00** |
| TraverseAsync_TaskSelector | 256   | True    | True  | 147,799.4 ns | 536.61 ns | 501.95 ns |  1.01 |    0.01 | 1.7090 |      - |   29008 B |        1.04 |
| SequenceAsync_Tasks        | 256   | True    | True  |  88,296.4 ns | 338.22 ns | 316.37 ns |  0.60 |    0.00 | 1.8311 | 0.1221 |   30940 B |        1.11 |
