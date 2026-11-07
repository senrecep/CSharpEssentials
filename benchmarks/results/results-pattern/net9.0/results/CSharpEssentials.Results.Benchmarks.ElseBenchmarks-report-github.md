```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **21.94 ns** |  **0.109 ns** |  **0.102 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    20.34 ns |  0.185 ns |  0.173 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    42.03 ns |  0.289 ns |  0.256 ns |  1.92 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | False   | False |    40.61 ns |  0.046 ns |  0.041 ns |  1.85 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | False   | False |    54.64 ns |  0.246 ns |  0.218 ns |  2.49 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    20.57 ns |  0.079 ns |  0.074 ns |  0.94 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    50.03 ns |  0.030 ns |  0.028 ns |  2.28 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | False   | False |    29.58 ns |  0.032 ns |  0.030 ns |  1.35 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,357.16 ns** |  **8.711 ns** |  **8.148 ns** |  **1.00** |    **0.01** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   841.86 ns |  5.995 ns |  5.608 ns |  0.62 |    0.01 | 0.0134 |     239 B |        0.71 |
| TaskSource_TaskHandler             | False   | True  | 1,604.24 ns | 11.409 ns | 10.672 ns |  1.18 |    0.01 | 0.0286 |     478 B |        1.42 |
| ValueTaskSource_SyncHandler        | False   | True  |   843.58 ns |  6.216 ns |  5.815 ns |  0.62 |    0.01 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,636.11 ns | 12.530 ns | 11.721 ns |  1.21 |    0.01 | 0.0324 |     566 B |        1.68 |
| Instance_TaskHandler               | False   | True  |   849.91 ns |  4.268 ns |  3.992 ns |  0.63 |    0.00 | 0.0134 |     239 B |        0.71 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,653.75 ns |  5.056 ns |  4.482 ns |  1.22 |    0.01 | 0.0343 |     571 B |        1.70 |
| Instance_ValueTaskHandler          | False   | True  |   853.64 ns |  5.943 ns |  5.559 ns |  0.63 |    0.01 | 0.0153 |     263 B |        0.78 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **17.74 ns** |  **0.109 ns** |  **0.102 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    18.72 ns |  0.074 ns |  0.061 ns |  1.05 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    36.08 ns |  0.210 ns |  0.186 ns |  2.03 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | True    | False |    37.99 ns |  0.012 ns |  0.012 ns |  2.14 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | True    | False |    51.26 ns |  0.167 ns |  0.148 ns |  2.89 |    0.02 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    18.15 ns |  0.085 ns |  0.075 ns |  1.02 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    45.38 ns |  0.020 ns |  0.017 ns |  2.56 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    27.36 ns |  0.006 ns |  0.005 ns |  1.54 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **837.92 ns** |  **3.034 ns** |  **2.838 ns** |  **1.00** |    **0.00** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   849.54 ns |  4.815 ns |  4.504 ns |  1.01 |    0.01 | 0.0134 |     239 B |        1.00 |
| TaskSource_TaskHandler             | True    | True  |   847.24 ns |  4.471 ns |  4.183 ns |  1.01 |    0.01 | 0.0191 |     319 B |        1.33 |
| ValueTaskSource_SyncHandler        | True    | True  |   850.25 ns |  5.011 ns |  4.442 ns |  1.01 |    0.01 | 0.0181 |     310 B |        1.30 |
| ValueTaskSource_TaskHandler        | True    | True  |   878.89 ns |  4.533 ns |  4.240 ns |  1.05 |    0.01 | 0.0238 |     406 B |        1.70 |
| Instance_TaskHandler               | True    | True  |    18.10 ns |  0.118 ns |  0.111 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.33 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   865.88 ns |  6.400 ns |  5.986 ns |  1.03 |    0.01 | 0.0181 |     310 B |        1.30 |
| Instance_ValueTaskHandler          | True    | True  |    26.88 ns |  0.008 ns |  0.008 ns |  0.03 |    0.00 |      - |         - |        0.00 |
