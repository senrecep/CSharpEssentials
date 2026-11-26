```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **20.63 ns** |  **0.108 ns** |  **0.095 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    24.55 ns |  0.130 ns |  0.121 ns |  1.19 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    50.71 ns |  0.180 ns |  0.150 ns |  2.46 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |   105.43 ns |  0.084 ns |  0.079 ns |  5.11 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    28.37 ns |  0.168 ns |  0.157 ns |  1.38 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |    31.01 ns |  0.043 ns |  0.038 ns |  1.50 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **831.84 ns** |  **8.607 ns** |  **8.051 ns** |  **1.00** |    **0.01** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   833.37 ns |  4.441 ns |  3.937 ns |  1.00 |    0.01 | 0.0143 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   856.61 ns | 12.760 ns | 12.532 ns |  1.03 |    0.02 | 0.0200 |     335 B |        1.50 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   893.99 ns |  7.090 ns |  6.285 ns |  1.07 |    0.01 | 0.0200 |     343 B |        1.53 |
| Instance_TaskHandler               | False   | True  |    33.53 ns |  0.207 ns |  0.173 ns |  0.04 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |    30.70 ns |  0.025 ns |  0.021 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **28.32 ns** |  **0.155 ns** |  **0.137 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    25.33 ns |  0.078 ns |  0.073 ns |  0.89 |    0.00 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    61.47 ns |  0.228 ns |  0.203 ns |  2.17 |    0.01 | 0.0143 |     240 B |        1.50 |
| ValueTaskSource_ValueTaskHandler   | True    | False |   108.80 ns |  0.160 ns |  0.149 ns |  3.84 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    36.67 ns |  0.072 ns |  0.056 ns |  1.29 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    35.11 ns |  0.040 ns |  0.034 ns |  1.24 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,381.31 ns** |  **5.283 ns** |  **4.942 ns** |  **1.00** |    **0.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   836.42 ns |  6.409 ns |  5.995 ns |  0.61 |    0.00 | 0.0143 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,661.24 ns |  7.232 ns |  6.765 ns |  1.20 |    0.01 | 0.0286 |     495 B |        1.51 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,794.67 ns | 13.653 ns | 12.771 ns |  1.30 |    0.01 | 0.0362 |     629 B |        1.92 |
| Instance_TaskHandler               | True    | True  |   831.81 ns |  5.986 ns |  5.599 ns |  0.60 |    0.00 | 0.0143 |     240 B |        0.73 |
| Instance_ValueTaskHandler          | True    | True  |   863.55 ns |  7.783 ns |  7.280 ns |  0.63 |    0.01 | 0.0172 |     288 B |        0.88 |
