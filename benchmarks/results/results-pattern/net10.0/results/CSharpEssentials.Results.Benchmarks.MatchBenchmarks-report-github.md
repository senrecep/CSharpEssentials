```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **13.93 ns** |  **0.014 ns** |  **0.013 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    11.20 ns |  0.016 ns |  0.015 ns |  0.80 |    0.00 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    26.68 ns |  0.016 ns |  0.015 ns |  1.91 |    0.00 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    22.69 ns |  0.023 ns |  0.021 ns |  1.63 |    0.00 |      - |         - |          NA |
| Instance_TaskHandler               | False   | False |    12.90 ns |  0.009 ns |  0.009 ns |  0.93 |    0.00 |      - |         - |          NA |
| ValueTaskSource_ValueTaskHandler   | False   | False |    31.45 ns |  0.030 ns |  0.028 ns |  2.26 |    0.00 |      - |         - |          NA |
| Instance_ValueTaskHandler          | False   | False |    15.13 ns |  0.042 ns |  0.039 ns |  1.09 |    0.00 |      - |         - |          NA |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,368.56 ns** | **12.588 ns** | **10.511 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   828.82 ns |  4.666 ns |  4.365 ns |  0.61 |    0.01 | 0.0134 |     238 B |        0.73 |
| TaskSource_TaskHandler             | False   | True  | 1,578.59 ns | 13.233 ns | 12.378 ns |  1.15 |    0.01 | 0.0267 |     491 B |        1.50 |
| ValueTaskSource_SyncHandler        | False   | True  |   837.50 ns |  5.656 ns |  5.291 ns |  0.61 |    0.01 | 0.0172 |     302 B |        0.92 |
| Instance_TaskHandler               | False   | True  |   823.08 ns |  5.557 ns |  5.198 ns |  0.60 |    0.01 | 0.0134 |     238 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,650.94 ns | 11.843 ns | 11.078 ns |  1.21 |    0.01 | 0.0305 |     570 B |        1.74 |
| Instance_ValueTaskHandler          | False   | True  |   853.62 ns |  4.779 ns |  4.470 ns |  0.62 |    0.01 | 0.0143 |     255 B |        0.78 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **22.23 ns** |  **0.220 ns** |  **0.206 ns** |  **1.00** |    **0.01** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.20 ns |  0.114 ns |  0.107 ns |  0.73 |    0.01 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    37.11 ns |  0.199 ns |  0.166 ns |  1.67 |    0.02 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    18.35 ns |  0.024 ns |  0.022 ns |  0.83 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    20.71 ns |  0.357 ns |  0.316 ns |  0.93 |    0.02 | 0.0086 |     144 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    33.95 ns |  0.045 ns |  0.040 ns |  1.53 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    18.46 ns |  0.362 ns |  0.847 ns |  0.83 |    0.04 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,372.82 ns** |  **8.736 ns** |  **8.172 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   848.23 ns |  4.382 ns |  4.099 ns |  0.62 |    0.00 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,640.94 ns | 11.857 ns |  9.901 ns |  1.20 |    0.01 | 0.0267 |     494 B |        1.51 |
| ValueTaskSource_SyncHandler        | True    | True  |   841.16 ns |  3.407 ns |  3.020 ns |  0.61 |    0.00 | 0.0172 |     303 B |        0.92 |
| Instance_TaskHandler               | True    | True  |   837.31 ns |  6.481 ns |  6.062 ns |  0.61 |    0.01 | 0.0134 |     239 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,666.30 ns | 11.384 ns | 10.649 ns |  1.21 |    0.01 | 0.0343 |     572 B |        1.74 |
| Instance_ValueTaskHandler          | True    | True  |   844.57 ns |  7.962 ns |  7.448 ns |  0.62 |    0.01 | 0.0153 |     255 B |        0.78 |
