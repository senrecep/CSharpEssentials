```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **16.696 ns** | **0.1331 ns** | **0.1245 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    17.107 ns | 0.1205 ns | 0.1127 ns |  1.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    17.118 ns | 0.0951 ns | 0.0890 ns |  1.03 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |    20.290 ns | 0.0215 ns | 0.0201 ns |  1.22 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    19.454 ns | 0.0427 ns | 0.0378 ns |  1.17 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    12.495 ns | 0.0650 ns | 0.0608 ns |  0.75 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |     4.553 ns | 0.0046 ns | 0.0038 ns |  0.27 |      - |         - |        0.00 |
|                                    |         |       |              |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **834.107 ns** | **5.3556 ns** | **5.0097 ns** | **1.000** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   839.607 ns | 9.4576 ns | 8.8466 ns | 1.007 | 0.0134 |     231 B |        0.97 |
| TaskSource_TaskHandler             | False   | True  |   849.295 ns | 5.0382 ns | 4.4662 ns | 1.018 | 0.0143 |     255 B |        1.07 |
| ValueTaskSource_SyncHandler        | False   | True  |   856.068 ns | 7.4357 ns | 6.9554 ns | 1.026 | 0.0172 |     310 B |        1.30 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   844.736 ns | 4.8496 ns | 4.2991 ns | 1.013 | 0.0191 |     327 B |        1.37 |
| Instance_TaskHandler               | False   | True  |     9.521 ns | 0.0842 ns | 0.0747 ns | 0.011 | 0.0048 |      80 B |        0.33 |
| Instance_ValueTaskHandler          | False   | True  |     5.132 ns | 0.0042 ns | 0.0037 ns | 0.006 |      - |         - |        0.00 |
|                                    |         |       |              |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **22.626 ns** | **0.1238 ns** | **0.1158 ns** |  **1.00** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.193 ns | 0.1164 ns | 0.1089 ns |  0.72 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |    23.466 ns | 0.1621 ns | 0.1437 ns |  1.04 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |    17.219 ns | 0.0198 ns | 0.0185 ns |  0.76 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    22.455 ns | 0.0241 ns | 0.0213 ns |  0.99 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    21.447 ns | 0.1268 ns | 0.1124 ns |  0.95 | 0.0091 |     152 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    13.171 ns | 0.0117 ns | 0.0109 ns |  0.58 |      - |         - |        0.00 |
|                                    |         |       |              |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,375.482 ns** | **6.8940 ns** | **6.4487 ns** |  **1.00** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   829.721 ns | 4.2107 ns | 3.9387 ns |  0.60 | 0.0134 |     231 B |        0.69 |
| TaskSource_TaskHandler             | True    | True  | 1,363.994 ns | 6.6292 ns | 6.2010 ns |  0.99 | 0.0210 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  |   843.942 ns | 4.3250 ns | 4.0456 ns |  0.61 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,440.897 ns | 6.3022 ns | 5.8951 ns |  1.05 | 0.0248 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  |   835.298 ns | 4.7542 ns | 4.4470 ns |  0.61 | 0.0124 |     223 B |        0.66 |
| Instance_ValueTaskHandler          | True    | True  |   838.830 ns | 5.4194 ns | 5.0694 ns |  0.61 | 0.0143 |     247 B |        0.74 |
