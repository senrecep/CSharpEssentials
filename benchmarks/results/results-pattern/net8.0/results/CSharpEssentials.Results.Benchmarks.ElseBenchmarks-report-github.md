```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **22.06 ns** |  **0.102 ns** |  **0.095 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    20.70 ns |  0.063 ns |  0.056 ns |  0.94 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    43.78 ns |  0.181 ns |  0.160 ns |  1.98 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | False   | False |    38.64 ns |  0.027 ns |  0.025 ns |  1.75 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | False   | False |    47.93 ns |  0.168 ns |  0.149 ns |  2.17 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    20.88 ns |  0.084 ns |  0.074 ns |  0.95 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,379.44 ns** |  **6.301 ns** |  **5.586 ns** |  **1.00** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   840.17 ns |  5.145 ns |  4.813 ns |  0.61 | 0.0134 |     239 B |        0.71 |
| TaskSource_TaskHandler             | False   | True  | 1,614.24 ns | 12.561 ns | 11.750 ns |  1.17 | 0.0286 |     478 B |        1.42 |
| ValueTaskSource_SyncHandler        | False   | True  |   876.85 ns |  6.888 ns |  6.443 ns |  0.64 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,693.47 ns | 15.488 ns | 14.488 ns |  1.23 | 0.0324 |     566 B |        1.68 |
| Instance_TaskHandler               | False   | True  |   842.98 ns |  5.854 ns |  5.476 ns |  0.61 | 0.0134 |     239 B |        0.71 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **18.48 ns** |  **0.095 ns** |  **0.089 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    19.11 ns |  0.090 ns |  0.084 ns |  1.03 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    38.71 ns |  0.164 ns |  0.146 ns |  2.09 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | True    | False |    39.19 ns |  0.046 ns |  0.043 ns |  2.12 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | True    | False |    45.31 ns |  0.194 ns |  0.181 ns |  2.45 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    18.96 ns |  0.215 ns |  0.201 ns |  1.03 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **826.00 ns** |  **5.726 ns** |  **5.357 ns** |  **1.00** | **0.0143** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   836.61 ns |  5.700 ns |  5.332 ns |  1.01 | 0.0134 |     239 B |        1.00 |
| TaskSource_TaskHandler             | True    | True  |   857.07 ns |  6.555 ns |  5.811 ns |  1.04 | 0.0191 |     319 B |        1.33 |
| ValueTaskSource_SyncHandler        | True    | True  |   873.63 ns |  7.829 ns |  7.323 ns |  1.06 | 0.0181 |     310 B |        1.30 |
| ValueTaskSource_TaskHandler        | True    | True  |   887.08 ns |  6.005 ns |  5.617 ns |  1.07 | 0.0238 |     406 B |        1.70 |
| Instance_TaskHandler               | True    | True  |    18.11 ns |  0.246 ns |  0.230 ns |  0.02 | 0.0048 |      80 B |        0.33 |
