```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **15.91 ns** |  **0.047 ns** |  **0.044 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    13.52 ns |  0.043 ns |  0.040 ns |  0.85 |    0.00 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    32.00 ns |  0.238 ns |  0.223 ns |  2.01 |    0.01 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    47.17 ns |  0.086 ns |  0.072 ns |  2.96 |    0.01 | 0.0033 |      56 B |          NA |
| Instance_TaskHandler               | False   | False |    29.58 ns |  0.618 ns |  0.712 ns |  1.86 |    0.04 | 0.0033 |      56 B |          NA |
| ValueTaskSource_ValueTaskHandler   | False   | False |    54.70 ns |  1.124 ns |  1.294 ns |  3.44 |    0.08 | 0.0033 |      56 B |          NA |
| Instance_ValueTaskHandler          | False   | False |    37.88 ns |  0.615 ns |  0.575 ns |  2.38 |    0.04 | 0.0033 |      56 B |          NA |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,418.83 ns** |  **8.189 ns** |  **7.259 ns** |  **1.00** |    **0.01** | **0.0229** |     **384 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   860.04 ns |  4.782 ns |  4.473 ns |  0.61 |    0.00 | 0.0172 |     295 B |        0.77 |
| TaskSource_TaskHandler             | False   | True  | 1,682.01 ns | 20.042 ns | 18.748 ns |  1.19 |    0.01 | 0.0305 |     549 B |        1.43 |
| ValueTaskSource_SyncHandler        | False   | True  |   879.73 ns |  6.885 ns |  6.440 ns |  0.62 |    0.01 | 0.0210 |     359 B |        0.93 |
| Instance_TaskHandler               | False   | True  |   852.56 ns |  6.573 ns |  6.148 ns |  0.60 |    0.01 | 0.0172 |     295 B |        0.77 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,750.96 ns | 16.765 ns | 14.862 ns |  1.23 |    0.01 | 0.0362 |     630 B |        1.64 |
| Instance_ValueTaskHandler          | False   | True  |   882.64 ns |  6.644 ns |  6.215 ns |  0.62 |    0.01 | 0.0181 |     312 B |        0.81 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **27.08 ns** |  **0.186 ns** |  **0.174 ns** |  **1.00** |    **0.01** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    19.19 ns |  0.119 ns |  0.111 ns |  0.71 |    0.01 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    45.01 ns |  0.490 ns |  0.458 ns |  1.66 |    0.02 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    26.70 ns |  0.044 ns |  0.039 ns |  0.99 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    26.03 ns |  0.321 ns |  0.285 ns |  0.96 |    0.01 | 0.0086 |     144 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    41.51 ns |  0.265 ns |  0.235 ns |  1.53 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    25.20 ns |  0.517 ns |  0.531 ns |  0.93 |    0.02 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,386.80 ns** |  **5.640 ns** |  **5.000 ns** |  **1.00** |    **0.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   846.42 ns |  5.600 ns |  5.238 ns |  0.61 |    0.00 | 0.0143 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,642.05 ns |  9.132 ns |  8.542 ns |  1.18 |    0.01 | 0.0286 |     494 B |        1.51 |
| ValueTaskSource_SyncHandler        | True    | True  |   866.58 ns |  5.207 ns |  4.870 ns |  0.62 |    0.00 | 0.0172 |     303 B |        0.92 |
| Instance_TaskHandler               | True    | True  |   844.50 ns | 14.081 ns | 12.483 ns |  0.61 |    0.01 | 0.0143 |     239 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,726.67 ns |  9.480 ns |  8.868 ns |  1.25 |    0.01 | 0.0343 |     573 B |        1.75 |
| Instance_ValueTaskHandler          | True    | True  |   859.59 ns |  6.257 ns |  5.853 ns |  0.62 |    0.00 | 0.0153 |     255 B |        0.78 |
