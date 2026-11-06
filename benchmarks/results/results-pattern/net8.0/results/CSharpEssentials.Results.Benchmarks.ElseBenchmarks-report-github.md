```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **23.12 ns** |  **0.129 ns** |  **0.121 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    21.45 ns |  0.135 ns |  0.126 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    44.94 ns |  0.299 ns |  0.265 ns |  1.94 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | False   | False |    55.25 ns |  0.436 ns |  0.407 ns |  2.39 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_TaskHandler        | False   | False |    64.20 ns |  0.379 ns |  0.355 ns |  2.78 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_TaskHandler               | False   | False |    41.13 ns |  0.424 ns |  0.376 ns |  1.78 |    0.02 | 0.0081 |     136 B |        1.70 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,426.08 ns** |  **6.548 ns** |  **6.125 ns** |  **1.00** |    **0.01** | **0.0229** |     **392 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   876.96 ns |  7.300 ns |  6.471 ns |  0.61 |    0.01 | 0.0172 |     296 B |        0.76 |
| TaskSource_TaskHandler             | False   | True  | 1,662.55 ns | 14.222 ns | 13.303 ns |  1.17 |    0.01 | 0.0305 |     535 B |        1.36 |
| ValueTaskSource_SyncHandler        | False   | True  |   895.64 ns |  7.801 ns |  7.297 ns |  0.63 |    0.01 | 0.0219 |     367 B |        0.94 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,704.32 ns | 16.811 ns | 14.903 ns |  1.20 |    0.01 | 0.0362 |     623 B |        1.59 |
| Instance_TaskHandler               | False   | True  |   850.91 ns |  5.302 ns |  4.959 ns |  0.60 |    0.00 | 0.0172 |     296 B |        0.76 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **19.50 ns** |  **0.079 ns** |  **0.074 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    20.02 ns |  0.143 ns |  0.134 ns |  1.03 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    39.56 ns |  0.352 ns |  0.329 ns |  2.03 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | True    | False |    37.05 ns |  0.036 ns |  0.032 ns |  1.90 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | True    | False |    46.92 ns |  0.117 ns |  0.110 ns |  2.41 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    18.81 ns |  0.109 ns |  0.102 ns |  0.96 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **843.73 ns** |  **4.836 ns** |  **4.287 ns** |  **1.00** |    **0.01** | **0.0134** |     **240 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   859.21 ns |  6.746 ns |  6.310 ns |  1.02 |    0.01 | 0.0143 |     239 B |        1.00 |
| TaskSource_TaskHandler             | True    | True  |   874.47 ns | 12.080 ns | 11.300 ns |  1.04 |    0.01 | 0.0191 |     319 B |        1.33 |
| ValueTaskSource_SyncHandler        | True    | True  |   885.39 ns |  5.261 ns |  4.921 ns |  1.05 |    0.01 | 0.0181 |     311 B |        1.30 |
| ValueTaskSource_TaskHandler        | True    | True  |   911.59 ns |  6.784 ns |  6.346 ns |  1.08 |    0.01 | 0.0238 |     407 B |        1.70 |
| Instance_TaskHandler               | True    | True  |    19.90 ns |  0.179 ns |  0.159 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.33 |
