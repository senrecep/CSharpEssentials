```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **20.72 ns** |  **0.361 ns** |  **0.338 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    30.89 ns |  0.176 ns |  0.165 ns |  1.49 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    25.45 ns |  0.122 ns |  0.114 ns |  1.23 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapTaskHandler          | False   | False |    26.57 ns |  0.270 ns |  0.226 ns |  1.28 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    71.47 ns |  0.079 ns |  0.070 ns |  3.45 |    0.05 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    28.03 ns |  0.172 ns |  0.161 ns |  1.35 |    0.02 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |    30.88 ns |  0.028 ns |  0.026 ns |  1.49 |    0.02 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **847.42 ns** |  **2.970 ns** |  **2.480 ns** |  **1.00** |    **0.00** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   855.87 ns |  5.577 ns |  5.217 ns |  1.01 |    0.01 | 0.0134 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   844.77 ns |  5.801 ns |  5.426 ns |  1.00 |    0.01 | 0.0143 |     255 B |        1.14 |
| TaskSource_MapTaskHandler          | False   | True  |   848.14 ns |  5.897 ns |  5.516 ns |  1.00 |    0.01 | 0.0153 |     255 B |        1.14 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   893.63 ns |  7.485 ns |  6.635 ns |  1.05 |    0.01 | 0.0200 |     343 B |        1.53 |
| Instance_TaskHandler               | False   | True  |    28.86 ns |  0.127 ns |  0.113 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |    30.98 ns |  0.021 ns |  0.018 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **28.57 ns** |  **0.267 ns** |  **0.249 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    24.93 ns |  0.117 ns |  0.104 ns |  0.87 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    36.99 ns |  0.124 ns |  0.110 ns |  1.29 |    0.01 | 0.0095 |     160 B |        1.00 |
| TaskSource_MapTaskHandler          | True    | False |    35.76 ns |  0.105 ns |  0.093 ns |  1.25 |    0.01 | 0.0091 |     152 B |        0.95 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    76.30 ns |  0.182 ns |  0.170 ns |  2.67 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    37.69 ns |  0.183 ns |  0.172 ns |  1.32 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    34.85 ns |  0.033 ns |  0.029 ns |  1.22 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,385.78 ns** |  **5.307 ns** |  **4.705 ns** |  **1.00** |    **0.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   833.06 ns |  4.530 ns |  4.237 ns |  0.60 |    0.00 | 0.0143 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,423.81 ns |  4.994 ns |  4.427 ns |  1.03 |    0.00 | 0.0210 |     360 B |        1.10 |
| TaskSource_MapTaskHandler          | True    | True  | 1,410.75 ns |  3.677 ns |  3.259 ns |  1.02 |    0.00 | 0.0210 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,596.99 ns | 15.394 ns | 14.399 ns |  1.15 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  |   838.18 ns |  8.197 ns |  7.667 ns |  0.60 |    0.01 | 0.0143 |     239 B |        0.73 |
| Instance_ValueTaskHandler          | True    | True  |   857.77 ns |  5.588 ns |  5.227 ns |  0.62 |    0.00 | 0.0172 |     288 B |        0.88 |
