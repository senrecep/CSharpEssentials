```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **19.248 ns** |  **0.1392 ns** |  **0.1302 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    19.323 ns |  0.1013 ns |  0.0948 ns |  1.00 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    21.789 ns |  0.0894 ns |  0.0793 ns |  1.13 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    44.167 ns |  0.0860 ns |  0.0805 ns |  2.29 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    13.449 ns |  0.0727 ns |  0.0680 ns |  0.70 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |     5.878 ns |  0.0200 ns |  0.0187 ns |  0.31 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **836.481 ns** |  **5.4414 ns** |  **5.0899 ns** | **1.000** |    **0.01** | **0.0124** |     **223 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   839.431 ns |  3.8597 ns |  3.4215 ns | 1.004 |    0.01 | 0.0134 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   838.275 ns |  4.8502 ns |  4.5369 ns | 1.002 |    0.01 | 0.0143 |     247 B |        1.11 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   863.699 ns |  5.4672 ns |  5.1140 ns | 1.033 |    0.01 | 0.0200 |     341 B |        1.53 |
| Instance_TaskHandler               | False   | True  |    13.116 ns |  0.0565 ns |  0.0501 ns | 0.016 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |     5.154 ns |  0.0031 ns |  0.0029 ns | 0.006 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **25.359 ns** |  **0.1796 ns** |  **0.1680 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    18.269 ns |  0.1517 ns |  0.1419 ns |  0.72 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    28.793 ns |  0.2335 ns |  0.1823 ns |  1.14 |    0.01 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    47.789 ns |  0.0247 ns |  0.0231 ns |  1.88 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |     9.577 ns |  0.1326 ns |  0.1107 ns |  0.38 |    0.00 | 0.0048 |      80 B |        0.50 |
| Instance_ValueTaskHandler          | True    | False |     2.873 ns |  0.0008 ns |  0.0007 ns |  0.11 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,384.302 ns** | **17.4962 ns** | **16.3660 ns** |  **1.00** |    **0.02** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   828.556 ns |  4.6285 ns |  4.3295 ns |  0.60 |    0.01 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,377.833 ns |  6.4171 ns |  6.0026 ns |  1.00 |    0.01 | 0.0210 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,488.630 ns | 10.3627 ns |  9.6933 ns |  1.08 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  |   788.450 ns |  3.2039 ns |  2.8401 ns |  0.57 |    0.01 | 0.0057 |     104 B |        0.32 |
| Instance_ValueTaskHandler          | True    | True  |   795.752 ns |  4.7559 ns |  4.4487 ns |  0.57 |    0.01 | 0.0067 |     120 B |        0.37 |
