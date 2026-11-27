```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **18.72 ns** |  **0.109 ns** |  **0.097 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    24.30 ns |  0.122 ns |  0.114 ns |  1.30 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    22.79 ns |  0.121 ns |  0.113 ns |  1.22 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapTaskHandler          | False   | False |    27.12 ns |  0.129 ns |  0.121 ns |  1.45 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    38.04 ns |  0.067 ns |  0.060 ns |  2.03 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    32.48 ns |  0.107 ns |  0.095 ns |  1.74 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |    14.92 ns |  0.026 ns |  0.025 ns |  0.80 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **815.29 ns** |  **5.110 ns** |  **4.780 ns** |  **1.00** |    **0.01** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   817.61 ns |  8.738 ns |  8.174 ns |  1.00 |    0.01 | 0.0143 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   818.10 ns | 12.516 ns | 11.707 ns |  1.00 |    0.02 | 0.0143 |     255 B |        1.14 |
| TaskSource_MapTaskHandler          | False   | True  |   843.07 ns |  6.652 ns |  6.222 ns |  1.03 |    0.01 | 0.0143 |     255 B |        1.14 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   866.93 ns |  3.480 ns |  3.085 ns |  1.06 |    0.01 | 0.0200 |     343 B |        1.53 |
| Instance_TaskHandler               | False   | True  |    32.13 ns |  0.150 ns |  0.133 ns |  0.04 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |    15.09 ns |  0.004 ns |  0.003 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **25.74 ns** |  **0.138 ns** |  **0.130 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    23.25 ns |  0.177 ns |  0.165 ns |  0.90 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    30.50 ns |  0.161 ns |  0.150 ns |  1.18 |    0.01 | 0.0095 |     160 B |        1.00 |
| TaskSource_MapTaskHandler          | True    | False |    31.08 ns |  0.347 ns |  0.324 ns |  1.21 |    0.01 | 0.0091 |     152 B |        0.95 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    49.04 ns |  0.041 ns |  0.039 ns |  1.91 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    37.83 ns |  0.137 ns |  0.128 ns |  1.47 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    19.88 ns |  0.018 ns |  0.015 ns |  0.77 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,364.35 ns** |  **4.339 ns** |  **4.059 ns** |  **1.00** |    **0.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   839.20 ns |  4.173 ns |  3.903 ns |  0.62 |    0.00 | 0.0143 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,403.23 ns |  8.743 ns |  8.178 ns |  1.03 |    0.01 | 0.0210 |     360 B |        1.10 |
| TaskSource_MapTaskHandler          | True    | True  | 1,403.69 ns |  4.699 ns |  4.396 ns |  1.03 |    0.00 | 0.0210 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,555.52 ns | 14.641 ns | 13.695 ns |  1.14 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  |   838.97 ns |  7.259 ns |  6.790 ns |  0.61 |    0.01 | 0.0143 |     240 B |        0.73 |
| Instance_ValueTaskHandler          | True    | True  |   851.97 ns |  6.022 ns |  5.633 ns |  0.62 |    0.00 | 0.0172 |     288 B |        0.88 |
