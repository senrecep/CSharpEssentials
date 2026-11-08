```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **15.927 ns** |  **0.2382 ns** |  **0.2228 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    17.883 ns |  0.1619 ns |  0.1514 ns |  1.12 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    19.299 ns |  0.3080 ns |  0.2881 ns |  1.21 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    24.630 ns |  0.0171 ns |  0.0160 ns |  1.55 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |     9.033 ns |  0.0984 ns |  0.0920 ns |  0.57 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |     4.852 ns |  0.0048 ns |  0.0045 ns |  0.30 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **833.483 ns** |  **3.3154 ns** |  **3.1012 ns** | **1.000** |    **0.01** | **0.0134** |     **223 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   838.519 ns |  3.8541 ns |  3.6051 ns | 1.006 |    0.01 | 0.0134 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   834.980 ns |  4.5388 ns |  4.2456 ns | 1.002 |    0.01 | 0.0143 |     247 B |        1.11 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   848.918 ns |  7.6844 ns |  7.1880 ns | 1.019 |    0.01 | 0.0200 |     342 B |        1.53 |
| Instance_TaskHandler               | False   | True  |     9.072 ns |  0.0870 ns |  0.0814 ns | 0.011 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |     5.139 ns |  0.0046 ns |  0.0043 ns | 0.006 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **21.488 ns** |  **0.2173 ns** |  **0.1927 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.852 ns |  0.1611 ns |  0.1507 ns |  0.78 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    23.231 ns |  0.1351 ns |  0.1264 ns |  1.08 |    0.01 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    32.092 ns |  0.0291 ns |  0.0273 ns |  1.49 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |     7.511 ns |  0.0692 ns |  0.0647 ns |  0.35 |    0.00 | 0.0048 |      80 B |        0.50 |
| Instance_ValueTaskHandler          | True    | False |     2.841 ns |  0.0003 ns |  0.0003 ns |  0.13 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,363.928 ns** |  **7.2591 ns** |  **6.7902 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   829.137 ns |  3.8017 ns |  3.5561 ns |  0.61 |    0.00 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,341.255 ns |  6.2651 ns |  5.8604 ns |  0.98 |    0.01 | 0.0210 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,483.770 ns | 12.2931 ns | 11.4989 ns |  1.09 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  |   792.467 ns |  3.8331 ns |  3.5855 ns |  0.58 |    0.00 | 0.0057 |     104 B |        0.32 |
| Instance_ValueTaskHandler          | True    | True  |   791.883 ns |  5.0275 ns |  4.7027 ns |  0.58 |    0.00 | 0.0067 |     120 B |        0.37 |
