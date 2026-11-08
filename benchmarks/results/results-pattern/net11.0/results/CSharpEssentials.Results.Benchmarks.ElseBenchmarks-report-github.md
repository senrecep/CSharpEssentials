```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **17.987 ns** |  **0.1652 ns** |  **0.1546 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    17.547 ns |  0.1370 ns |  0.1282 ns |  0.98 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    34.373 ns |  0.3355 ns |  0.3138 ns |  1.91 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | False   | False |    22.312 ns |  0.0146 ns |  0.0130 ns |  1.24 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | False   | False |    39.301 ns |  0.2966 ns |  0.2775 ns |  2.19 |    0.02 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    17.322 ns |  0.1029 ns |  0.0962 ns |  0.96 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    31.775 ns |  0.0429 ns |  0.0402 ns |  1.77 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | False   | False |    14.437 ns |  0.0143 ns |  0.0134 ns |  0.80 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **918.404 ns** | **12.2608 ns** | **10.8689 ns** |  **1.00** |    **0.02** | **0.0191** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   553.795 ns |  3.3916 ns |  3.1725 ns |  0.60 |    0.01 | 0.0134 |     239 B |        0.71 |
| TaskSource_TaskHandler             | False   | True  | 1,154.106 ns | 21.3668 ns | 19.9865 ns |  1.26 |    0.03 | 0.0286 |     477 B |        1.42 |
| ValueTaskSource_SyncHandler        | False   | True  |   657.521 ns |  4.2234 ns |  3.9506 ns |  0.72 |    0.01 | 0.0181 |     310 B |        0.92 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,214.933 ns | 12.5639 ns | 10.4914 ns |  1.32 |    0.02 | 0.0305 |     565 B |        1.68 |
| Instance_TaskHandler               | False   | True  |   556.107 ns |  6.1625 ns |  5.4629 ns |  0.61 |    0.01 | 0.0134 |     239 B |        0.71 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,105.882 ns | 14.8164 ns | 13.8593 ns |  1.20 |    0.02 | 0.0305 |     567 B |        1.69 |
| Instance_ValueTaskHandler          | False   | True  |   591.943 ns |  7.0485 ns |  6.5932 ns |  0.64 |    0.01 | 0.0153 |     262 B |        0.78 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **15.062 ns** |  **0.1781 ns** |  **0.1666 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.623 ns |  0.1772 ns |  0.1658 ns |  1.10 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    30.731 ns |  0.6257 ns |  0.6425 ns |  2.04 |    0.05 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_SyncHandler        | True    | False |    18.555 ns |  0.0066 ns |  0.0062 ns |  1.23 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_TaskHandler        | True    | False |    35.147 ns |  0.1846 ns |  0.1727 ns |  2.33 |    0.03 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    14.359 ns |  0.1156 ns |  0.1081 ns |  0.95 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    26.701 ns |  0.0132 ns |  0.0117 ns |  1.77 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |     9.358 ns |  0.1184 ns |  0.1108 ns |  0.62 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **554.976 ns** |  **4.8413 ns** |  **4.0427 ns** |  **1.00** |    **0.01** | **0.0143** |     **239 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   549.134 ns |  7.0805 ns |  6.2767 ns |  0.99 |    0.01 | 0.0134 |     239 B |        1.00 |
| TaskSource_TaskHandler             | True    | True  |   569.037 ns |  2.8077 ns |  2.4889 ns |  1.03 |    0.01 | 0.0191 |     319 B |        1.33 |
| ValueTaskSource_SyncHandler        | True    | True  |   596.163 ns |  5.3611 ns |  4.7525 ns |  1.07 |    0.01 | 0.0181 |     309 B |        1.29 |
| ValueTaskSource_TaskHandler        | True    | True  |   623.549 ns |  8.5458 ns |  7.5757 ns |  1.12 |    0.02 | 0.0238 |     405 B |        1.69 |
| Instance_TaskHandler               | True    | True  |    17.026 ns |  0.1417 ns |  0.1325 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.33 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   615.595 ns |  3.9391 ns |  3.6846 ns |  1.11 |    0.01 | 0.0181 |     309 B |        1.29 |
| Instance_ValueTaskHandler          | True    | True  |     9.499 ns |  0.0588 ns |  0.0550 ns |  0.02 |    0.00 |      - |         - |        0.00 |
