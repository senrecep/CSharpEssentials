```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **17.52 ns** | **0.249 ns** | **0.233 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    17.79 ns | 0.102 ns | 0.095 ns |  1.02 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    17.42 ns | 0.161 ns | 0.142 ns |  0.99 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |    45.25 ns | 0.240 ns | 0.225 ns |  2.58 |    0.04 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    39.46 ns | 0.207 ns | 0.173 ns |  2.25 |    0.03 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    22.93 ns | 0.157 ns | 0.139 ns |  1.31 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_ValueTaskHandler          | False   | False |    15.82 ns | 0.096 ns | 0.090 ns |  0.90 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **855.87 ns** | **3.841 ns** | **3.593 ns** |  **1.00** |    **0.01** | **0.0172** |     **295 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   837.87 ns | 5.180 ns | 4.846 ns |  0.98 |    0.01 | 0.0172 |     288 B |        0.98 |
| TaskSource_TaskHandler             | False   | True  |   854.76 ns | 4.191 ns | 3.920 ns |  1.00 |    0.01 | 0.0181 |     311 B |        1.05 |
| ValueTaskSource_SyncHandler        | False   | True  |   856.22 ns | 4.212 ns | 3.940 ns |  1.00 |    0.01 | 0.0219 |     367 B |        1.24 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   871.19 ns | 4.381 ns | 4.098 ns |  1.02 |    0.01 | 0.0229 |     383 B |        1.30 |
| Instance_TaskHandler               | False   | True  |    22.59 ns | 0.217 ns | 0.203 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
| Instance_ValueTaskHandler          | False   | True  |    17.77 ns | 0.133 ns | 0.118 ns |  0.02 |    0.00 | 0.0033 |      56 B |        0.19 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **23.60 ns** | **0.292 ns** | **0.274 ns** |  **1.00** |    **0.02** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.87 ns | 0.178 ns | 0.158 ns |  0.71 |    0.01 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |    26.67 ns | 0.288 ns | 0.240 ns |  1.13 |    0.02 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |    17.55 ns | 0.007 ns | 0.006 ns |  0.74 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    21.66 ns | 0.027 ns | 0.024 ns |  0.92 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    22.59 ns | 0.286 ns | 0.267 ns |  0.96 |    0.02 | 0.0091 |     152 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    12.56 ns | 0.014 ns | 0.012 ns |  0.53 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,361.96 ns** | **4.337 ns** | **3.844 ns** |  **1.00** |    **0.00** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   847.20 ns | 4.904 ns | 4.587 ns |  0.62 |    0.00 | 0.0134 |     231 B |        0.69 |
| TaskSource_TaskHandler             | True    | True  | 1,383.30 ns | 5.937 ns | 5.263 ns |  1.02 |    0.00 | 0.0210 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  |   824.56 ns | 3.474 ns | 2.901 ns |  0.61 |    0.00 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,439.09 ns | 8.538 ns | 7.987 ns |  1.06 |    0.01 | 0.0248 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  |   842.87 ns | 4.708 ns | 3.676 ns |  0.62 |    0.00 | 0.0124 |     223 B |        0.66 |
| Instance_ValueTaskHandler          | True    | True  |   834.61 ns | 6.690 ns | 6.258 ns |  0.61 |    0.00 | 0.0143 |     247 B |        0.74 |
