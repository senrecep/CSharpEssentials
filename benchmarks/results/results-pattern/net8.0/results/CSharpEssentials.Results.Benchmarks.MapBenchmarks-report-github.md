```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **20.81 ns** |  **0.145 ns** |  **0.136 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    20.18 ns |  0.089 ns |  0.083 ns |  0.97 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    23.68 ns |  0.082 ns |  0.077 ns |  1.14 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |    41.13 ns |  0.015 ns |  0.013 ns |  1.98 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    31.65 ns |  0.017 ns |  0.015 ns |  1.52 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    15.16 ns |  0.169 ns |  0.158 ns |  0.73 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **837.05 ns** |  **8.346 ns** |  **7.807 ns** |  **1.00** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   851.77 ns |  4.406 ns |  3.905 ns |  1.02 | 0.0134 |     231 B |        0.97 |
| TaskSource_TaskHandler             | False   | True  |   845.70 ns |  5.513 ns |  5.157 ns |  1.01 | 0.0143 |     255 B |        1.07 |
| ValueTaskSource_SyncHandler        | False   | True  |   868.13 ns |  6.623 ns |  6.195 ns |  1.04 | 0.0181 |     311 B |        1.30 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   872.05 ns |  3.775 ns |  3.531 ns |  1.04 | 0.0191 |     327 B |        1.37 |
| Instance_TaskHandler               | False   | True  |    15.24 ns |  0.139 ns |  0.116 ns |  0.02 | 0.0048 |      80 B |        0.33 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **27.26 ns** |  **0.145 ns** |  **0.136 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    18.36 ns |  0.089 ns |  0.084 ns |  0.67 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |    28.25 ns |  0.170 ns |  0.151 ns |  1.04 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |    34.70 ns |  0.119 ns |  0.111 ns |  1.27 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    32.97 ns |  0.030 ns |  0.026 ns |  1.21 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    24.45 ns |  0.138 ns |  0.129 ns |  0.90 | 0.0091 |     152 B |        1.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,401.94 ns** |  **7.729 ns** |  **7.230 ns** |  **1.00** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   843.21 ns |  6.352 ns |  5.942 ns |  0.60 | 0.0134 |     231 B |        0.69 |
| TaskSource_TaskHandler             | True    | True  | 1,396.95 ns |  8.806 ns |  8.237 ns |  1.00 | 0.0210 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  |   863.20 ns |  7.238 ns |  6.770 ns |  0.62 | 0.0181 |     311 B |        0.93 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,502.50 ns | 13.269 ns | 12.412 ns |  1.07 | 0.0229 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  |   833.24 ns |  3.914 ns |  3.661 ns |  0.59 | 0.0124 |     223 B |        0.66 |
