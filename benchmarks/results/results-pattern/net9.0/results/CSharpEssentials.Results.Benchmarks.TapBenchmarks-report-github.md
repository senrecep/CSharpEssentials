```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **19.841 ns** |  **0.1160 ns** |  **0.1085 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    19.501 ns |  0.0700 ns |  0.0547 ns |  0.98 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    29.090 ns |  0.3610 ns |  0.3377 ns |  1.47 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    36.774 ns |  0.0274 ns |  0.0243 ns |  1.85 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    38.960 ns |  0.0507 ns |  0.0450 ns |  1.96 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    10.820 ns |  0.1103 ns |  0.1032 ns |  0.55 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |     6.071 ns |  0.0021 ns |  0.0019 ns |  0.31 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **838.917 ns** |  **4.8060 ns** |  **4.4956 ns** | **1.000** |    **0.01** | **0.0143** |     **255 B** |        **1.00** |
| TaskSource_Action                  | False   | True  |   839.307 ns |  4.9225 ns |  4.6045 ns | 1.000 |    0.01 | 0.0134 |     231 B |        0.91 |
| TaskSource_TaskHandler             | False   | True  |   859.676 ns |  3.3873 ns |  3.1685 ns | 1.025 |    0.01 | 0.0191 |     319 B |        1.25 |
| ValueTaskSource_Action             | False   | True  |   857.473 ns |  5.1622 ns |  4.8287 ns | 1.022 |    0.01 | 0.0181 |     309 B |        1.21 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   857.038 ns |  5.3665 ns |  5.0198 ns | 1.022 |    0.01 | 0.0200 |     341 B |        1.34 |
| Instance_TaskHandler               | False   | True  |    10.881 ns |  0.0498 ns |  0.0441 ns | 0.013 |    0.00 | 0.0048 |      80 B |        0.31 |
| Instance_ValueTaskHandler          | False   | True  |     6.069 ns |  0.0012 ns |  0.0010 ns | 0.007 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **21.457 ns** |  **0.1698 ns** |  **0.1588 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    20.219 ns |  0.1683 ns |  0.1574 ns |  0.94 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    39.652 ns |  0.1569 ns |  0.1468 ns |  1.85 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    36.717 ns |  0.0352 ns |  0.0329 ns |  1.71 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    40.643 ns |  0.0235 ns |  0.0220 ns |  1.89 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    19.468 ns |  0.1233 ns |  0.1153 ns |  0.91 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    32.642 ns |  0.0121 ns |  0.0107 ns |  1.52 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,365.864 ns** |  **4.6474 ns** |  **3.6284 ns** |  **1.00** |    **0.00** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  |   845.851 ns |  4.5187 ns |  4.2268 ns |  0.62 |    0.00 | 0.0134 |     232 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 1,620.245 ns |  9.3936 ns |  8.7868 ns |  1.19 |    0.01 | 0.0286 |     478 B |        1.36 |
| ValueTaskSource_Action             | True    | True  |   865.233 ns | 17.1381 ns | 16.8319 ns |  0.63 |    0.01 | 0.0181 |     310 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,512.882 ns |  6.8476 ns |  6.4052 ns |  1.11 |    0.01 | 0.0248 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  |   841.452 ns |  3.9914 ns |  3.7335 ns |  0.62 |    0.00 | 0.0134 |     239 B |        0.68 |
| Instance_ValueTaskHandler          | True    | True  |   846.591 ns |  5.5671 ns |  4.6488 ns |  0.62 |    0.00 | 0.0143 |     255 B |        0.72 |
