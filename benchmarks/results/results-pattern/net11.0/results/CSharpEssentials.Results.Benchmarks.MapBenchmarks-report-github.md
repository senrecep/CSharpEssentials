```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean       | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-----------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |  **20.197 ns** |  **0.1266 ns** |  **0.1184 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |  16.375 ns |  0.1508 ns |  0.1411 ns |  0.81 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |  16.104 ns |  0.1013 ns |  0.0947 ns |  0.80 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |  21.035 ns |  0.0185 ns |  0.0164 ns |  1.04 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |  19.211 ns |  0.0293 ns |  0.0274 ns |  0.95 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |   8.755 ns |  0.0621 ns |  0.0581 ns |  0.43 |    0.00 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |   3.696 ns |  0.0054 ns |  0.0045 ns |  0.18 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |            |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **562.385 ns** |  **7.1934 ns** |  **6.7287 ns** | **1.000** |    **0.02** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  | 547.398 ns |  3.3592 ns |  2.9778 ns | 0.973 |    0.01 | 0.0134 |     231 B |        0.97 |
| TaskSource_TaskHandler             | False   | True  | 557.357 ns |  4.4114 ns |  4.3326 ns | 0.991 |    0.01 | 0.0143 |     254 B |        1.06 |
| ValueTaskSource_SyncHandler        | False   | True  | 592.040 ns |  4.5878 ns |  4.0670 ns | 1.053 |    0.01 | 0.0181 |     309 B |        1.29 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 601.207 ns |  9.3247 ns |  8.7223 ns | 1.069 |    0.02 | 0.0191 |     325 B |        1.36 |
| Instance_TaskHandler               | False   | True  |   8.702 ns |  0.0880 ns |  0.0823 ns | 0.015 |    0.00 | 0.0048 |      80 B |        0.33 |
| Instance_ValueTaskHandler          | False   | True  |   4.271 ns |  0.0074 ns |  0.0069 ns | 0.008 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |            |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |  **21.291 ns** |  **0.3155 ns** |  **0.2952 ns** |  **1.00** |    **0.02** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |  16.395 ns |  0.1510 ns |  0.1339 ns |  0.77 |    0.01 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |  22.910 ns |  0.1712 ns |  0.1430 ns |  1.08 |    0.02 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |  17.006 ns |  0.0207 ns |  0.0194 ns |  0.80 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |  22.923 ns |  0.0403 ns |  0.0377 ns |  1.08 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |  20.254 ns |  0.2888 ns |  0.2702 ns |  0.95 |    0.02 | 0.0091 |     152 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |  12.142 ns |  0.0128 ns |  0.0120 ns |  0.57 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |            |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **963.405 ns** | **18.8073 ns** | **23.7852 ns** |  **1.00** |    **0.03** | **0.0200** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  | 568.632 ns |  5.3543 ns |  5.0084 ns |  0.59 |    0.02 | 0.0134 |     230 B |        0.68 |
| TaskSource_TaskHandler             | True    | True  | 914.234 ns |  7.8536 ns |  7.3463 ns |  0.95 |    0.02 | 0.0210 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  | 611.935 ns |  5.7784 ns |  5.4051 ns |  0.64 |    0.02 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 949.186 ns | 11.2320 ns |  9.3792 ns |  0.99 |    0.03 | 0.0257 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  | 532.150 ns |  4.0463 ns |  3.7849 ns |  0.55 |    0.01 | 0.0124 |     223 B |        0.66 |
| Instance_ValueTaskHandler          | True    | True  | 585.546 ns |  4.1918 ns |  3.7159 ns |  0.61 |    0.02 | 0.0143 |     247 B |        0.74 |
