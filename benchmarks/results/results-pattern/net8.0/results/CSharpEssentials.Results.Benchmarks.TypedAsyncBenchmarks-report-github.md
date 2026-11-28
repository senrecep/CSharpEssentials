```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                                | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler**    | **False**   | **False** |    **20.43 ns** |  **0.067 ns** |  **0.062 ns** |  **1.00** |    **0.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler                | False   | False |    63.67 ns |  0.175 ns |  0.163 ns |  3.12 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler                | False   | False |    48.68 ns |  0.147 ns |  0.138 ns |  2.38 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapTaskHandler             | False   | False |    45.61 ns |  0.102 ns |  0.096 ns |  2.23 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_EnsureTaskHandler          | False   | False |    47.09 ns |  0.205 ns |  0.192 ns |  2.30 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TapTaskHandler             | False   | False |    47.37 ns |  0.174 ns |  0.163 ns |  2.32 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapErrorTaskHandler        | False   | False |    64.26 ns |  0.213 ns |  0.189 ns |  3.14 |    0.01 | 0.0091 |     152 B |        1.90 |
| TaskSource_CompensateTaskHandler      | False   | False |    64.48 ns |  0.409 ns |  0.382 ns |  3.16 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler           | False   | False |    93.89 ns |  0.107 ns |  0.100 ns |  4.60 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_MapSyncHandler        | False   | False |    95.70 ns |  0.118 ns |  0.110 ns |  4.68 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_EnsureTaskHandler     | False   | False |    88.96 ns |  0.198 ns |  0.185 ns |  4.35 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapErrorSyncHandler   | False   | False |    95.46 ns |  0.046 ns |  0.036 ns |  4.67 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_CompensateTaskHandler | False   | False |   102.28 ns |  0.308 ns |  0.288 ns |  5.01 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler      | False   | False |    85.17 ns |  0.057 ns |  0.054 ns |  4.17 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler                  | False   | False |    35.48 ns |  0.174 ns |  0.163 ns |  1.74 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler             | False   | False |    44.46 ns |  0.032 ns |  0.028 ns |  2.18 |    0.01 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **False**   | **True**  |   **845.53 ns** |  **6.240 ns** |  **5.837 ns** |  **1.00** |    **0.01** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler                | False   | True  |   847.80 ns |  4.767 ns |  4.459 ns |  1.00 |    0.01 | 0.0143 |     240 B |        1.07 |
| TaskSource_TaskHandler                | False   | True  |   865.69 ns |  4.067 ns |  3.804 ns |  1.02 |    0.01 | 0.0153 |     255 B |        1.14 |
| TaskSource_MapTaskHandler             | False   | True  |   853.95 ns |  6.666 ns |  5.909 ns |  1.01 |    0.01 | 0.0153 |     255 B |        1.14 |
| TaskSource_EnsureTaskHandler          | False   | True  |   875.01 ns |  5.301 ns |  4.958 ns |  1.03 |    0.01 | 0.0162 |     279 B |        1.25 |
| TaskSource_TapTaskHandler             | False   | True  |   858.50 ns |  6.658 ns |  6.228 ns |  1.02 |    0.01 | 0.0162 |     271 B |        1.21 |
| TaskSource_MapErrorTaskHandler        | False   | True  | 1,509.45 ns |  8.578 ns |  8.024 ns |  1.79 |    0.02 | 0.0210 |     368 B |        1.64 |
| TaskSource_CompensateTaskHandler      | False   | True  | 1,521.10 ns | 13.244 ns | 12.388 ns |  1.80 |    0.02 | 0.0191 |     376 B |        1.68 |
| ValueTaskSource_SyncHandler           | False   | True  |   962.21 ns | 19.205 ns | 19.722 ns |  1.14 |    0.02 | 0.0172 |     311 B |        1.39 |
| ValueTaskSource_MapSyncHandler        | False   | True  |   988.97 ns | 18.308 ns | 16.229 ns |  1.17 |    0.02 | 0.0181 |     311 B |        1.39 |
| ValueTaskSource_EnsureTaskHandler     | False   | True  |   996.58 ns | 19.932 ns | 45.797 ns |  1.18 |    0.05 | 0.0210 |     351 B |        1.57 |
| ValueTaskSource_MapErrorSyncHandler   | False   | True  |   957.92 ns | 18.766 ns | 24.401 ns |  1.13 |    0.03 | 0.0181 |     311 B |        1.39 |
| ValueTaskSource_CompensateTaskHandler | False   | True  | 1,639.63 ns | 28.134 ns | 26.316 ns |  1.94 |    0.03 | 0.0267 |     448 B |        2.00 |
| ValueTaskSource_ValueTaskHandler      | False   | True  |   976.39 ns | 19.347 ns | 46.355 ns |  1.15 |    0.05 | 0.0191 |     343 B |        1.53 |
| Instance_TaskHandler                  | False   | True  |    35.47 ns |  0.092 ns |  0.086 ns |  0.04 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler             | False   | True  |    40.27 ns |  0.035 ns |  0.029 ns |  0.05 |    0.00 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **True**    | **False** |    **29.08 ns** |  **0.205 ns** |  **0.191 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler                | True    | False |    49.50 ns |  0.140 ns |  0.124 ns |  1.70 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler                | True    | False |    60.12 ns |  0.073 ns |  0.057 ns |  2.07 |    0.01 | 0.0095 |     160 B |        1.00 |
| TaskSource_MapTaskHandler             | True    | False |    52.71 ns |  0.137 ns |  0.122 ns |  1.81 |    0.01 | 0.0091 |     152 B |        0.95 |
| TaskSource_EnsureTaskHandler          | True    | False |    48.85 ns |  0.167 ns |  0.156 ns |  1.68 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TapTaskHandler             | True    | False |    48.22 ns |  0.117 ns |  0.109 ns |  1.66 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_MapErrorTaskHandler        | True    | False |    47.05 ns |  0.127 ns |  0.119 ns |  1.62 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_CompensateTaskHandler      | True    | False |    47.04 ns |  0.129 ns |  0.120 ns |  1.62 |    0.01 | 0.0048 |      80 B |        0.50 |
| ValueTaskSource_SyncHandler           | True    | False |    95.13 ns |  0.214 ns |  0.201 ns |  3.27 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapSyncHandler        | True    | False |    95.66 ns |  0.023 ns |  0.019 ns |  3.29 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_EnsureTaskHandler     | True    | False |    89.60 ns |  0.219 ns |  0.194 ns |  3.08 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapErrorSyncHandler   | True    | False |    94.69 ns |  0.152 ns |  0.127 ns |  3.26 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_CompensateTaskHandler | True    | False |    82.90 ns |  0.181 ns |  0.161 ns |  2.85 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler      | True    | False |   123.14 ns |  0.166 ns |  0.147 ns |  4.24 |    0.03 |      - |         - |        0.00 |
| Instance_TaskHandler                  | True    | False |    49.07 ns |  0.287 ns |  0.268 ns |  1.69 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler             | True    | False |    65.13 ns |  0.068 ns |  0.064 ns |  2.24 |    0.01 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **True**    | **True**  | **1,427.12 ns** | **11.690 ns** | **10.363 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler                | True    | True  |   853.91 ns |  8.329 ns |  7.791 ns |  0.60 |    0.01 | 0.0143 |     240 B |        0.73 |
| TaskSource_TaskHandler                | True    | True  | 1,495.19 ns |  6.384 ns |  5.660 ns |  1.05 |    0.01 | 0.0191 |     360 B |        1.10 |
| TaskSource_MapTaskHandler             | True    | True  | 1,471.34 ns | 11.315 ns | 10.584 ns |  1.03 |    0.01 | 0.0210 |     352 B |        1.07 |
| TaskSource_EnsureTaskHandler          | True    | True  | 1,481.27 ns |  8.876 ns |  8.303 ns |  1.04 |    0.01 | 0.0210 |     376 B |        1.15 |
| TaskSource_TapTaskHandler             | True    | True  | 1,482.73 ns | 11.721 ns | 10.964 ns |  1.04 |    0.01 | 0.0210 |     368 B |        1.12 |
| TaskSource_MapErrorTaskHandler        | True    | True  |   859.34 ns |  4.824 ns |  4.512 ns |  0.60 |    0.01 | 0.0153 |     263 B |        0.80 |
| TaskSource_CompensateTaskHandler      | True    | True  |   854.92 ns |  6.539 ns |  5.796 ns |  0.60 |    0.01 | 0.0153 |     255 B |        0.78 |
| ValueTaskSource_SyncHandler           | True    | True  |   961.76 ns | 18.682 ns | 19.989 ns |  0.67 |    0.01 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_MapSyncHandler        | True    | True  |   951.98 ns | 18.151 ns | 33.644 ns |  0.67 |    0.02 | 0.0172 |     311 B |        0.95 |
| ValueTaskSource_EnsureTaskHandler     | True    | True  | 1,573.80 ns | 31.245 ns | 34.728 ns |  1.10 |    0.02 | 0.0267 |     448 B |        1.37 |
| ValueTaskSource_MapErrorSyncHandler   | True    | True  |   935.73 ns | 16.749 ns | 14.848 ns |  0.66 |    0.01 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_CompensateTaskHandler | True    | True  |   962.15 ns | 19.287 ns | 41.103 ns |  0.67 |    0.03 | 0.0191 |     327 B |        1.00 |
| ValueTaskSource_ValueTaskHandler      | True    | True  | 1,773.37 ns | 27.295 ns | 25.532 ns |  1.24 |    0.02 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler                  | True    | True  |   846.24 ns |  9.172 ns |  8.580 ns |  0.59 |    0.01 | 0.0143 |     240 B |        0.73 |
| Instance_ValueTaskHandler             | True    | True  |   901.01 ns | 11.621 ns | 10.302 ns |  0.63 |    0.01 | 0.0172 |     288 B |        0.88 |
