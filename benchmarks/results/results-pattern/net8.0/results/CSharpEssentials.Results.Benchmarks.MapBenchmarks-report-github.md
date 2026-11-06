```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **21.75 ns** | **0.155 ns** | **0.145 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    21.18 ns | 0.148 ns | 0.139 ns |  0.97 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    23.14 ns | 0.179 ns | 0.167 ns |  1.06 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |    60.15 ns | 0.099 ns | 0.088 ns |  2.77 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    50.20 ns | 0.386 ns | 0.361 ns |  2.31 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    29.28 ns | 0.363 ns | 0.339 ns |  1.35 |    0.02 | 0.0081 |     136 B |        1.70 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **852.95 ns** | **6.013 ns** | **5.624 ns** |  **1.00** |    **0.01** | **0.0172** |     **296 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   870.38 ns | 5.234 ns | 4.895 ns |  1.02 |    0.01 | 0.0172 |     288 B |        0.97 |
| TaskSource_TaskHandler             | False   | True  |   860.58 ns | 5.844 ns | 4.880 ns |  1.01 |    0.01 | 0.0181 |     311 B |        1.05 |
| ValueTaskSource_SyncHandler        | False   | True  |   896.94 ns | 6.143 ns | 5.746 ns |  1.05 |    0.01 | 0.0219 |     367 B |        1.24 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   893.69 ns | 6.846 ns | 6.404 ns |  1.05 |    0.01 | 0.0229 |     383 B |        1.29 |
| Instance_TaskHandler               | False   | True  |    28.82 ns | 0.277 ns | 0.259 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.46 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **27.87 ns** | **0.188 ns** | **0.176 ns** |  **1.00** |    **0.01** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    19.13 ns | 0.168 ns | 0.157 ns |  0.69 |    0.01 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |    30.23 ns | 0.313 ns | 0.292 ns |  1.08 |    0.01 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |    36.13 ns | 0.081 ns | 0.076 ns |  1.30 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    33.01 ns | 0.011 ns | 0.008 ns |  1.18 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    25.90 ns | 0.372 ns | 0.348 ns |  0.93 |    0.01 | 0.0091 |     152 B |        1.00 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,418.86 ns** | **9.773 ns** | **9.142 ns** |  **1.00** |    **0.01** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   844.29 ns | 3.271 ns | 2.732 ns |  0.60 |    0.00 | 0.0134 |     231 B |        0.69 |
| TaskSource_TaskHandler             | True    | True  | 1,404.32 ns | 4.357 ns | 4.075 ns |  0.99 |    0.01 | 0.0210 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  |   878.75 ns | 4.417 ns | 4.132 ns |  0.62 |    0.00 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,539.77 ns | 9.267 ns | 8.215 ns |  1.09 |    0.01 | 0.0229 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  |   849.21 ns | 4.413 ns | 4.128 ns |  0.60 |    0.00 | 0.0134 |     223 B |        0.66 |
