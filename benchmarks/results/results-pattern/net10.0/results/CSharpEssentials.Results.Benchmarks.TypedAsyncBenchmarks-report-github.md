```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                                | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler**    | **False**   | **False** |    **18.39 ns** |  **0.160 ns** |  **0.150 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler                | False   | False |    31.72 ns |  0.156 ns |  0.138 ns |  1.72 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler                | False   | False |    22.48 ns |  0.061 ns |  0.057 ns |  1.22 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapTaskHandler             | False   | False |    25.35 ns |  0.119 ns |  0.105 ns |  1.38 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_EnsureTaskHandler          | False   | False |    24.24 ns |  0.103 ns |  0.096 ns |  1.32 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TapTaskHandler             | False   | False |    25.41 ns |  0.096 ns |  0.090 ns |  1.38 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapErrorTaskHandler        | False   | False |    32.34 ns |  0.189 ns |  0.177 ns |  1.76 |    0.02 | 0.0091 |     152 B |        1.90 |
| TaskSource_CompensateTaskHandler      | False   | False |    32.42 ns |  0.165 ns |  0.138 ns |  1.76 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler           | False   | False |    39.91 ns |  0.017 ns |  0.016 ns |  2.17 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapSyncHandler        | False   | False |    39.25 ns |  0.057 ns |  0.050 ns |  2.13 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_EnsureTaskHandler     | False   | False |    41.50 ns |  0.091 ns |  0.080 ns |  2.26 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapErrorSyncHandler   | False   | False |    43.13 ns |  0.101 ns |  0.095 ns |  2.34 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_CompensateTaskHandler | False   | False |    54.81 ns |  0.132 ns |  0.124 ns |  2.98 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler      | False   | False |    37.65 ns |  0.082 ns |  0.068 ns |  2.05 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler                  | False   | False |    31.71 ns |  0.084 ns |  0.070 ns |  1.72 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler             | False   | False |    15.11 ns |  0.037 ns |  0.033 ns |  0.82 |    0.01 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **False**   | **True**  |   **812.46 ns** |  **6.708 ns** |  **6.274 ns** |  **1.00** |    **0.01** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler                | False   | True  |   834.66 ns |  6.093 ns |  5.699 ns |  1.03 |    0.01 | 0.0143 |     239 B |        1.07 |
| TaskSource_TaskHandler                | False   | True  |   833.49 ns |  7.870 ns |  7.362 ns |  1.03 |    0.01 | 0.0143 |     255 B |        1.14 |
| TaskSource_MapTaskHandler             | False   | True  |   831.15 ns | 11.612 ns | 10.294 ns |  1.02 |    0.01 | 0.0153 |     255 B |        1.14 |
| TaskSource_EnsureTaskHandler          | False   | True  |   836.92 ns |  6.687 ns |  6.255 ns |  1.03 |    0.01 | 0.0162 |     279 B |        1.25 |
| TaskSource_TapTaskHandler             | False   | True  |   830.90 ns |  3.497 ns |  3.100 ns |  1.02 |    0.01 | 0.0153 |     271 B |        1.21 |
| TaskSource_MapErrorTaskHandler        | False   | True  | 1,398.63 ns |  5.807 ns |  5.148 ns |  1.72 |    0.01 | 0.0210 |     368 B |        1.64 |
| TaskSource_CompensateTaskHandler      | False   | True  | 1,428.13 ns |  6.027 ns |  5.638 ns |  1.76 |    0.01 | 0.0210 |     376 B |        1.68 |
| ValueTaskSource_SyncHandler           | False   | True  |   850.49 ns |  7.244 ns |  6.776 ns |  1.05 |    0.01 | 0.0181 |     312 B |        1.39 |
| ValueTaskSource_MapSyncHandler        | False   | True  |   871.62 ns |  9.986 ns |  8.339 ns |  1.07 |    0.01 | 0.0181 |     312 B |        1.39 |
| ValueTaskSource_EnsureTaskHandler     | False   | True  |   851.61 ns |  7.982 ns |  7.467 ns |  1.05 |    0.01 | 0.0200 |     351 B |        1.57 |
| ValueTaskSource_MapErrorSyncHandler   | False   | True  |   858.96 ns |  6.682 ns |  6.250 ns |  1.06 |    0.01 | 0.0181 |     311 B |        1.39 |
| ValueTaskSource_CompensateTaskHandler | False   | True  | 1,420.52 ns |  8.972 ns |  7.953 ns |  1.75 |    0.02 | 0.0267 |     448 B |        2.00 |
| ValueTaskSource_ValueTaskHandler      | False   | True  |   867.88 ns |  6.221 ns |  5.819 ns |  1.07 |    0.01 | 0.0200 |     343 B |        1.53 |
| Instance_TaskHandler                  | False   | True  |    31.58 ns |  0.151 ns |  0.141 ns |  0.04 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler             | False   | True  |    15.15 ns |  0.012 ns |  0.010 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **True**    | **False** |    **25.59 ns** |  **0.127 ns** |  **0.113 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler                | True    | False |    26.76 ns |  0.091 ns |  0.085 ns |  1.05 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler                | True    | False |    31.81 ns |  0.169 ns |  0.159 ns |  1.24 |    0.01 | 0.0095 |     160 B |        1.00 |
| TaskSource_MapTaskHandler             | True    | False |    30.76 ns |  0.174 ns |  0.136 ns |  1.20 |    0.01 | 0.0091 |     152 B |        0.95 |
| TaskSource_EnsureTaskHandler          | True    | False |    29.31 ns |  0.115 ns |  0.090 ns |  1.15 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TapTaskHandler             | True    | False |    28.43 ns |  0.175 ns |  0.164 ns |  1.11 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_MapErrorTaskHandler        | True    | False |    25.15 ns |  0.092 ns |  0.082 ns |  0.98 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_CompensateTaskHandler      | True    | False |    23.64 ns |  0.137 ns |  0.128 ns |  0.92 |    0.01 | 0.0048 |      80 B |        0.50 |
| ValueTaskSource_SyncHandler           | True    | False |    39.93 ns |  0.032 ns |  0.029 ns |  1.56 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_MapSyncHandler        | True    | False |    40.00 ns |  0.047 ns |  0.044 ns |  1.56 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_EnsureTaskHandler     | True    | False |    46.43 ns |  0.070 ns |  0.062 ns |  1.81 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_MapErrorSyncHandler   | True    | False |    42.36 ns |  0.059 ns |  0.055 ns |  1.66 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_CompensateTaskHandler | True    | False |    38.58 ns |  0.033 ns |  0.031 ns |  1.51 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler      | True    | False |    49.06 ns |  0.082 ns |  0.076 ns |  1.92 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler                  | True    | False |    38.28 ns |  0.380 ns |  0.337 ns |  1.50 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler             | True    | False |    19.44 ns |  0.053 ns |  0.044 ns |  0.76 |    0.00 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **True**    | **True**  | **1,380.94 ns** | **12.533 ns** | **11.723 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler                | True    | True  |   842.10 ns |  5.737 ns |  5.366 ns |  0.61 |    0.01 | 0.0143 |     240 B |        0.73 |
| TaskSource_TaskHandler                | True    | True  | 1,429.73 ns |  9.187 ns |  8.594 ns |  1.04 |    0.01 | 0.0210 |     360 B |        1.10 |
| TaskSource_MapTaskHandler             | True    | True  | 1,406.57 ns |  5.040 ns |  4.714 ns |  1.02 |    0.01 | 0.0210 |     352 B |        1.07 |
| TaskSource_EnsureTaskHandler          | True    | True  | 1,428.63 ns | 25.121 ns | 23.498 ns |  1.03 |    0.02 | 0.0210 |     376 B |        1.15 |
| TaskSource_TapTaskHandler             | True    | True  | 1,432.40 ns |  6.195 ns |  5.492 ns |  1.04 |    0.01 | 0.0210 |     368 B |        1.12 |
| TaskSource_MapErrorTaskHandler        | True    | True  |   829.47 ns | 10.154 ns |  9.498 ns |  0.60 |    0.01 | 0.0153 |     263 B |        0.80 |
| TaskSource_CompensateTaskHandler      | True    | True  |   825.29 ns | 15.589 ns | 14.582 ns |  0.60 |    0.01 | 0.0143 |     255 B |        0.78 |
| ValueTaskSource_SyncHandler           | True    | True  |   865.05 ns |  7.232 ns |  6.765 ns |  0.63 |    0.01 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_MapSyncHandler        | True    | True  |   841.93 ns |  8.447 ns |  7.902 ns |  0.61 |    0.01 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_EnsureTaskHandler     | True    | True  | 1,470.69 ns | 11.972 ns |  9.997 ns |  1.07 |    0.01 | 0.0267 |     448 B |        1.37 |
| ValueTaskSource_MapErrorSyncHandler   | True    | True  |   848.64 ns |  4.516 ns |  3.771 ns |  0.61 |    0.01 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_CompensateTaskHandler | True    | True  |   851.40 ns |  7.854 ns |  7.347 ns |  0.62 |    0.01 | 0.0191 |     327 B |        1.00 |
| ValueTaskSource_ValueTaskHandler      | True    | True  | 1,549.56 ns | 12.623 ns | 11.807 ns |  1.12 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler                  | True    | True  |   807.81 ns | 10.677 ns |  9.987 ns |  0.59 |    0.01 | 0.0143 |     240 B |        0.73 |
| Instance_ValueTaskHandler             | True    | True  |   842.45 ns |  4.036 ns |  3.151 ns |  0.61 |    0.01 | 0.0172 |     288 B |        0.88 |
