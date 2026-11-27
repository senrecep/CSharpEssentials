```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **18.06 ns** |  **0.148 ns** |  **0.116 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    22.55 ns |  0.177 ns |  0.166 ns |  1.25 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    24.04 ns |  0.130 ns |  0.122 ns |  1.33 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_MapTaskHandler          | False   | False |    27.14 ns |  0.239 ns |  0.200 ns |  1.50 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    37.90 ns |  0.084 ns |  0.070 ns |  2.10 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    31.84 ns |  0.131 ns |  0.123 ns |  1.76 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |    15.53 ns |  0.039 ns |  0.032 ns |  0.86 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **554.73 ns** |  **5.489 ns** |  **5.134 ns** |  **1.00** |    **0.01** | **0.0124** |     **223 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   573.30 ns |  4.989 ns |  4.667 ns |  1.03 |    0.01 | 0.0134 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   569.97 ns |  4.346 ns |  4.066 ns |  1.03 |    0.01 | 0.0143 |     255 B |        1.14 |
| TaskSource_MapTaskHandler          | False   | True  |   564.69 ns |  5.708 ns |  5.340 ns |  1.02 |    0.01 | 0.0143 |     254 B |        1.14 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   619.05 ns |  3.790 ns |  3.545 ns |  1.12 |    0.01 | 0.0200 |     342 B |        1.53 |
| Instance_TaskHandler               | False   | True  |    31.43 ns |  0.088 ns |  0.083 ns |  0.06 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |    15.31 ns |  0.096 ns |  0.090 ns |  0.03 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **26.20 ns** |  **0.220 ns** |  **0.183 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    23.32 ns |  0.099 ns |  0.088 ns |  0.89 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    30.27 ns |  0.124 ns |  0.110 ns |  1.16 |    0.01 | 0.0095 |     160 B |        1.00 |
| TaskSource_MapTaskHandler          | True    | False |    30.09 ns |  0.141 ns |  0.118 ns |  1.15 |    0.01 | 0.0091 |     152 B |        0.95 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    45.38 ns |  0.155 ns |  0.145 ns |  1.73 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    37.86 ns |  0.212 ns |  0.199 ns |  1.44 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    19.46 ns |  0.035 ns |  0.033 ns |  0.74 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **947.67 ns** |  **7.757 ns** |  **7.256 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   563.08 ns |  7.270 ns |  6.800 ns |  0.59 |    0.01 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  |   955.59 ns | 16.377 ns | 14.518 ns |  1.01 |    0.02 | 0.0191 |     360 B |        1.10 |
| TaskSource_MapTaskHandler          | True    | True  |   959.22 ns |  5.279 ns |  4.408 ns |  1.01 |    0.01 | 0.0210 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,173.91 ns | 16.410 ns | 15.350 ns |  1.24 |    0.02 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  |   566.36 ns |  3.169 ns |  2.965 ns |  0.60 |    0.01 | 0.0134 |     239 B |        0.73 |
| Instance_ValueTaskHandler          | True    | True  |   631.53 ns |  4.335 ns |  3.843 ns |  0.67 |    0.01 | 0.0172 |     287 B |        0.88 |
