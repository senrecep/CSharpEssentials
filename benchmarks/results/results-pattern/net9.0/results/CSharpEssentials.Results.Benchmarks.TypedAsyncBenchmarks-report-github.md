```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                                | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler**    | **False**   | **False** |    **20.41 ns** |  **0.103 ns** |  **0.096 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler                | False   | False |    26.62 ns |  0.141 ns |  0.132 ns |  1.30 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler                | False   | False |    24.95 ns |  0.092 ns |  0.086 ns |  1.22 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapTaskHandler             | False   | False |    26.43 ns |  0.103 ns |  0.097 ns |  1.29 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_EnsureTaskHandler          | False   | False |    28.22 ns |  0.102 ns |  0.090 ns |  1.38 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TapTaskHandler             | False   | False |    34.97 ns |  0.346 ns |  0.306 ns |  1.71 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapErrorTaskHandler        | False   | False |    39.34 ns |  0.220 ns |  0.195 ns |  1.93 |    0.01 | 0.0091 |     152 B |        1.90 |
| TaskSource_CompensateTaskHandler      | False   | False |    36.97 ns |  0.230 ns |  0.216 ns |  1.81 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler           | False   | False |    70.15 ns |  0.150 ns |  0.140 ns |  3.44 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapSyncHandler        | False   | False |    70.54 ns |  0.074 ns |  0.066 ns |  3.46 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_EnsureTaskHandler     | False   | False |    68.59 ns |  0.122 ns |  0.114 ns |  3.36 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapErrorSyncHandler   | False   | False |    71.57 ns |  0.148 ns |  0.131 ns |  3.51 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_CompensateTaskHandler | False   | False |    77.60 ns |  0.447 ns |  0.373 ns |  3.80 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler      | False   | False |    67.23 ns |  0.090 ns |  0.084 ns |  3.29 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler                  | False   | False |    27.95 ns |  0.125 ns |  0.111 ns |  1.37 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler             | False   | False |    30.88 ns |  0.030 ns |  0.027 ns |  1.51 |    0.01 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **False**   | **True**  |   **838.21 ns** |  **6.412 ns** |  **5.998 ns** |  **1.00** |    **0.01** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler                | False   | True  |   831.12 ns |  4.724 ns |  4.419 ns |  0.99 |    0.01 | 0.0143 |     239 B |        1.07 |
| TaskSource_TaskHandler                | False   | True  |   836.44 ns |  5.551 ns |  4.921 ns |  1.00 |    0.01 | 0.0143 |     255 B |        1.14 |
| TaskSource_MapTaskHandler             | False   | True  |   838.94 ns |  6.889 ns |  6.444 ns |  1.00 |    0.01 | 0.0143 |     255 B |        1.14 |
| TaskSource_EnsureTaskHandler          | False   | True  |   843.99 ns |  4.893 ns |  4.577 ns |  1.01 |    0.01 | 0.0162 |     279 B |        1.25 |
| TaskSource_TapTaskHandler             | False   | True  |   853.07 ns |  5.084 ns |  4.755 ns |  1.02 |    0.01 | 0.0153 |     271 B |        1.21 |
| TaskSource_MapErrorTaskHandler        | False   | True  | 1,374.90 ns |  6.111 ns |  5.716 ns |  1.64 |    0.01 | 0.0210 |     368 B |        1.64 |
| TaskSource_CompensateTaskHandler      | False   | True  | 1,427.63 ns |  8.588 ns |  8.033 ns |  1.70 |    0.02 | 0.0210 |     376 B |        1.68 |
| ValueTaskSource_SyncHandler           | False   | True  |   891.28 ns |  8.838 ns |  8.267 ns |  1.06 |    0.01 | 0.0181 |     311 B |        1.39 |
| ValueTaskSource_MapSyncHandler        | False   | True  |   869.35 ns |  4.959 ns |  4.141 ns |  1.04 |    0.01 | 0.0181 |     311 B |        1.39 |
| ValueTaskSource_EnsureTaskHandler     | False   | True  |   907.64 ns |  7.164 ns |  6.701 ns |  1.08 |    0.01 | 0.0200 |     351 B |        1.57 |
| ValueTaskSource_MapErrorSyncHandler   | False   | True  |   881.53 ns |  7.858 ns |  7.351 ns |  1.05 |    0.01 | 0.0181 |     311 B |        1.39 |
| ValueTaskSource_CompensateTaskHandler | False   | True  | 1,460.20 ns | 11.523 ns | 10.779 ns |  1.74 |    0.02 | 0.0267 |     448 B |        2.00 |
| ValueTaskSource_ValueTaskHandler      | False   | True  |   888.98 ns | 10.572 ns |  9.889 ns |  1.06 |    0.01 | 0.0200 |     343 B |        1.53 |
| Instance_TaskHandler                  | False   | True  |    27.66 ns |  0.121 ns |  0.113 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler             | False   | True  |    31.02 ns |  0.079 ns |  0.074 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **True**    | **False** |    **28.36 ns** |  **0.215 ns** |  **0.202 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler                | True    | False |    25.64 ns |  0.094 ns |  0.087 ns |  0.90 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler                | True    | False |    36.60 ns |  0.258 ns |  0.241 ns |  1.29 |    0.01 | 0.0095 |     160 B |        1.00 |
| TaskSource_MapTaskHandler             | True    | False |    36.25 ns |  0.176 ns |  0.165 ns |  1.28 |    0.01 | 0.0091 |     152 B |        0.95 |
| TaskSource_EnsureTaskHandler          | True    | False |    36.53 ns |  0.086 ns |  0.076 ns |  1.29 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TapTaskHandler             | True    | False |    34.85 ns |  0.205 ns |  0.192 ns |  1.23 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_MapErrorTaskHandler        | True    | False |    26.26 ns |  0.174 ns |  0.146 ns |  0.93 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_CompensateTaskHandler      | True    | False |    24.78 ns |  0.135 ns |  0.120 ns |  0.87 |    0.01 | 0.0048 |      80 B |        0.50 |
| ValueTaskSource_SyncHandler           | True    | False |    68.92 ns |  0.112 ns |  0.105 ns |  2.43 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapSyncHandler        | True    | False |    69.51 ns |  0.118 ns |  0.099 ns |  2.45 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_EnsureTaskHandler     | True    | False |    73.49 ns |  0.113 ns |  0.106 ns |  2.59 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_MapErrorSyncHandler   | True    | False |    68.93 ns |  0.068 ns |  0.060 ns |  2.43 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_CompensateTaskHandler | True    | False |    69.70 ns |  0.093 ns |  0.087 ns |  2.46 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler      | True    | False |    73.62 ns |  0.119 ns |  0.105 ns |  2.60 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler                  | True    | False |    39.43 ns |  0.174 ns |  0.163 ns |  1.39 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler             | True    | False |    35.35 ns |  0.022 ns |  0.019 ns |  1.25 |    0.01 |      - |         - |        0.00 |
|                                       |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler**    | **True**    | **True**  | **1,382.52 ns** |  **5.419 ns** |  **5.069 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler                | True    | True  |   844.34 ns |  6.578 ns |  6.153 ns |  0.61 |    0.00 | 0.0143 |     239 B |        0.73 |
| TaskSource_TaskHandler                | True    | True  | 1,390.90 ns |  9.108 ns |  8.520 ns |  1.01 |    0.01 | 0.0210 |     360 B |        1.10 |
| TaskSource_MapTaskHandler             | True    | True  | 1,415.26 ns |  5.891 ns |  5.511 ns |  1.02 |    0.01 | 0.0210 |     352 B |        1.07 |
| TaskSource_EnsureTaskHandler          | True    | True  | 1,445.02 ns |  5.770 ns |  5.398 ns |  1.05 |    0.01 | 0.0210 |     376 B |        1.15 |
| TaskSource_TapTaskHandler             | True    | True  | 1,425.04 ns |  7.319 ns |  6.847 ns |  1.03 |    0.01 | 0.0210 |     368 B |        1.12 |
| TaskSource_MapErrorTaskHandler        | True    | True  |   826.09 ns |  4.219 ns |  3.740 ns |  0.60 |    0.00 | 0.0153 |     263 B |        0.80 |
| TaskSource_CompensateTaskHandler      | True    | True  |   836.41 ns |  5.370 ns |  5.023 ns |  0.60 |    0.00 | 0.0143 |     255 B |        0.78 |
| ValueTaskSource_SyncHandler           | True    | True  |   873.57 ns |  6.284 ns |  5.878 ns |  0.63 |    0.00 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_MapSyncHandler        | True    | True  |   866.50 ns | 10.113 ns |  9.460 ns |  0.63 |    0.01 | 0.0181 |     311 B |        0.95 |
| ValueTaskSource_EnsureTaskHandler     | True    | True  | 1,460.37 ns |  8.561 ns |  7.589 ns |  1.06 |    0.01 | 0.0267 |     448 B |        1.37 |
| ValueTaskSource_MapErrorSyncHandler   | True    | True  |   869.23 ns |  7.451 ns |  6.605 ns |  0.63 |    0.01 | 0.0181 |     312 B |        0.95 |
| ValueTaskSource_CompensateTaskHandler | True    | True  |   881.05 ns |  6.440 ns |  6.024 ns |  0.64 |    0.00 | 0.0191 |     327 B |        1.00 |
| ValueTaskSource_ValueTaskHandler      | True    | True  | 1,581.49 ns | 10.436 ns |  9.762 ns |  1.14 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler                  | True    | True  |   832.27 ns |  6.996 ns |  6.202 ns |  0.60 |    0.00 | 0.0143 |     240 B |        0.73 |
| Instance_ValueTaskHandler             | True    | True  |   866.48 ns | 12.111 ns | 11.329 ns |  0.63 |    0.01 | 0.0172 |     288 B |        0.88 |
