```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **18.013 ns** |  **0.1013 ns** |  **0.0898 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    18.039 ns |  0.2014 ns |  0.1884 ns |  1.00 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    35.762 ns |  0.1815 ns |  0.1697 ns |  1.99 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | False   | False |    20.569 ns |  0.0142 ns |  0.0126 ns |  1.14 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | False   | False |    37.620 ns |  0.2021 ns |  0.1890 ns |  2.09 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    17.985 ns |  0.1480 ns |  0.1384 ns |  1.00 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    33.227 ns |  0.0483 ns |  0.0428 ns |  1.84 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | False   | False |    13.452 ns |  0.0205 ns |  0.0181 ns |  0.75 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,360.879 ns** |  **5.0707 ns** |  **4.7432 ns** |  **1.00** |    **0.00** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   824.089 ns |  2.8482 ns |  2.5249 ns |  0.61 |    0.00 | 0.0134 |     239 B |        0.71 |
| TaskSource_TaskHandler             | False   | True  | 1,587.090 ns |  8.9322 ns |  7.9182 ns |  1.17 |    0.01 | 0.0267 |     478 B |        1.42 |
| ValueTaskSource_SyncHandler        | False   | True  |   838.120 ns |  5.5612 ns |  5.2019 ns |  0.62 |    0.00 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,634.369 ns | 15.5447 ns | 13.7800 ns |  1.20 |    0.01 | 0.0305 |     566 B |        1.68 |
| Instance_TaskHandler               | False   | True  |   833.328 ns |  8.0752 ns |  7.5535 ns |  0.61 |    0.01 | 0.0134 |     239 B |        0.71 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,623.611 ns | 14.0425 ns | 13.1354 ns |  1.19 |    0.01 | 0.0343 |     570 B |        1.70 |
| Instance_ValueTaskHandler          | False   | True  |   839.446 ns |  4.0517 ns |  3.5917 ns |  0.62 |    0.00 | 0.0153 |     263 B |        0.78 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **16.181 ns** |  **0.2078 ns** |  **0.1842 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    17.549 ns |  0.1266 ns |  0.1184 ns |  1.08 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    32.074 ns |  0.2880 ns |  0.2694 ns |  1.98 |    0.03 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | True    | False |    18.491 ns |  0.0256 ns |  0.0239 ns |  1.14 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | True    | False |    35.266 ns |  0.1735 ns |  0.1623 ns |  2.18 |    0.03 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    15.478 ns |  0.1376 ns |  0.1288 ns |  0.96 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    26.111 ns |  0.0268 ns |  0.0250 ns |  1.61 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |     9.847 ns |  0.0135 ns |  0.0120 ns |  0.61 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **837.578 ns** |  **6.0339 ns** |  **5.6441 ns** |  **1.00** |    **0.01** | **0.0134** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   836.838 ns |  4.6136 ns |  4.3155 ns |  1.00 |    0.01 | 0.0134 |     239 B |        1.00 |
| TaskSource_TaskHandler             | True    | True  |   865.576 ns | 13.4387 ns | 12.5706 ns |  1.03 |    0.02 | 0.0191 |     319 B |        1.33 |
| ValueTaskSource_SyncHandler        | True    | True  |   829.655 ns |  5.2648 ns |  4.9247 ns |  0.99 |    0.01 | 0.0181 |     310 B |        1.30 |
| ValueTaskSource_TaskHandler        | True    | True  |   859.980 ns |  4.1412 ns |  3.6711 ns |  1.03 |    0.01 | 0.0238 |     406 B |        1.70 |
| Instance_TaskHandler               | True    | True  |    15.590 ns |  0.2945 ns |  0.2755 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.33 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   843.095 ns |  4.2862 ns |  4.0093 ns |  1.01 |    0.01 | 0.0181 |     311 B |        1.30 |
| Instance_ValueTaskHandler          | True    | True  |     9.840 ns |  0.0113 ns |  0.0100 ns |  0.01 |    0.00 |      - |         - |        0.00 |
