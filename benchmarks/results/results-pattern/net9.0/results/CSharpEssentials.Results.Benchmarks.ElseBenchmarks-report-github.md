```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **22.18 ns** | **0.114 ns** | **0.107 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    20.99 ns | 0.148 ns | 0.139 ns |  0.95 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    42.78 ns | 0.161 ns | 0.143 ns |  1.93 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | False   | False |    52.64 ns | 0.119 ns | 0.111 ns |  2.37 |    0.01 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_TaskHandler        | False   | False |    73.39 ns | 0.218 ns | 0.194 ns |  3.31 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_TaskHandler               | False   | False |    37.83 ns | 0.209 ns | 0.195 ns |  1.71 |    0.01 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    62.64 ns | 0.343 ns | 0.304 ns |  2.82 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_ValueTaskHandler          | False   | False |    43.53 ns | 0.148 ns | 0.123 ns |  1.96 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,419.26 ns** | **6.419 ns** | **5.691 ns** |  **1.00** |    **0.01** | **0.0229** |     **392 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   869.13 ns | 2.764 ns | 2.450 ns |  0.61 |    0.00 | 0.0172 |     295 B |        0.75 |
| TaskSource_TaskHandler             | False   | True  | 1,635.38 ns | 8.362 ns | 7.413 ns |  1.15 |    0.01 | 0.0305 |     534 B |        1.36 |
| ValueTaskSource_SyncHandler        | False   | True  |   891.05 ns | 8.844 ns | 8.273 ns |  0.63 |    0.01 | 0.0219 |     367 B |        0.94 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,687.13 ns | 9.948 ns | 9.305 ns |  1.19 |    0.01 | 0.0362 |     623 B |        1.59 |
| Instance_TaskHandler               | False   | True  |   849.71 ns | 5.391 ns | 5.043 ns |  0.60 |    0.00 | 0.0172 |     295 B |        0.75 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,706.36 ns | 9.131 ns | 8.541 ns |  1.20 |    0.01 | 0.0362 |     629 B |        1.60 |
| Instance_ValueTaskHandler          | False   | True  |   866.39 ns | 5.273 ns | 4.933 ns |  0.61 |    0.00 | 0.0191 |     319 B |        0.81 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **18.24 ns** | **0.184 ns** | **0.173 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    19.83 ns | 0.153 ns | 0.143 ns |  1.09 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    37.60 ns | 0.289 ns | 0.270 ns |  2.06 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | True    | False |    37.53 ns | 0.025 ns | 0.023 ns |  2.06 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | True    | False |    52.14 ns | 0.166 ns | 0.155 ns |  2.86 |    0.03 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    18.81 ns | 0.127 ns | 0.119 ns |  1.03 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    44.54 ns | 0.027 ns | 0.026 ns |  2.44 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    27.21 ns | 0.010 ns | 0.010 ns |  1.49 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **849.35 ns** | **5.063 ns** | **4.736 ns** |  **1.00** |    **0.01** | **0.0143** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   848.72 ns | 5.256 ns | 4.917 ns |  1.00 |    0.01 | 0.0134 |     239 B |        1.00 |
| TaskSource_TaskHandler             | True    | True  |   872.89 ns | 3.692 ns | 3.454 ns |  1.03 |    0.01 | 0.0191 |     319 B |        1.33 |
| ValueTaskSource_SyncHandler        | True    | True  |   871.33 ns | 4.457 ns | 4.169 ns |  1.03 |    0.01 | 0.0181 |     310 B |        1.30 |
| ValueTaskSource_TaskHandler        | True    | True  |   887.94 ns | 6.766 ns | 6.328 ns |  1.05 |    0.01 | 0.0238 |     406 B |        1.70 |
| Instance_TaskHandler               | True    | True  |    21.43 ns | 0.147 ns | 0.138 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.33 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   870.61 ns | 3.923 ns | 3.670 ns |  1.03 |    0.01 | 0.0181 |     310 B |        1.30 |
| Instance_ValueTaskHandler          | True    | True  |    27.15 ns | 0.023 ns | 0.022 ns |  0.03 |    0.00 |      - |         - |        0.00 |
