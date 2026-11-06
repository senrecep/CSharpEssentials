```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **16.110 ns** |  **0.1846 ns** |  **0.1727 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    18.513 ns |  0.1836 ns |  0.1717 ns |  1.15 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    19.792 ns |  0.1676 ns |  0.1486 ns |  1.23 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    41.168 ns |  0.1796 ns |  0.1592 ns |  2.56 |    0.03 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    23.226 ns |  0.4609 ns |  0.4312 ns |  1.44 |    0.03 | 0.0081 |     136 B |        1.70 |
| Instance_ValueTaskHandler          | False   | False |    18.000 ns |  0.0749 ns |  0.0625 ns |  1.12 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **839.886 ns** |  **2.8019 ns** |  **2.4838 ns** |  **1.00** |    **0.00** | **0.0162** |     **280 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   857.767 ns | 14.4768 ns | 13.5416 ns |  1.02 |    0.02 | 0.0172 |     296 B |        1.06 |
| TaskSource_TaskHandler             | False   | True  |   851.000 ns |  4.2464 ns |  3.9721 ns |  1.01 |    0.01 | 0.0181 |     303 B |        1.08 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   876.554 ns |  4.5027 ns |  3.9915 ns |  1.04 |    0.01 | 0.0238 |     398 B |        1.42 |
| Instance_TaskHandler               | False   | True  |    22.447 ns |  0.2292 ns |  0.2144 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.49 |
| Instance_ValueTaskHandler          | False   | True  |    17.703 ns |  0.0914 ns |  0.0855 ns |  0.02 |    0.00 | 0.0033 |      56 B |        0.20 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **22.865 ns** |  **0.2561 ns** |  **0.2395 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    17.758 ns |  0.2590 ns |  0.2296 ns |  0.78 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    24.702 ns |  0.4633 ns |  0.4334 ns |  1.08 |    0.02 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    30.290 ns |  0.1778 ns |  0.1664 ns |  1.32 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |     7.911 ns |  0.1125 ns |  0.0997 ns |  0.35 |    0.01 | 0.0048 |      80 B |        0.50 |
| Instance_ValueTaskHandler          | True    | False |     3.122 ns |  0.0208 ns |  0.0194 ns |  0.14 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,394.088 ns** |  **5.6430 ns** |  **5.2785 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   838.352 ns |  4.6579 ns |  4.3570 ns |  0.60 |    0.00 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,363.037 ns |  5.8032 ns |  5.4283 ns |  0.98 |    0.01 | 0.0210 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,499.829 ns |  5.7198 ns |  5.0704 ns |  1.08 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  |   785.422 ns |  4.4505 ns |  4.1630 ns |  0.56 |    0.00 | 0.0057 |     104 B |        0.32 |
| Instance_ValueTaskHandler          | True    | True  |   800.291 ns |  2.9258 ns |  2.5936 ns |  0.57 |    0.00 | 0.0067 |     120 B |        0.37 |
