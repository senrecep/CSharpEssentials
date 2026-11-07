```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **19.70 ns** |  **0.126 ns** |  **0.118 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    25.27 ns |  0.107 ns |  0.095 ns |  1.28 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    40.86 ns |  0.302 ns |  0.282 ns |  2.07 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    19.19 ns |  0.123 ns |  0.115 ns |  0.97 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | False   | False |    47.36 ns |  0.021 ns |  0.018 ns |  2.40 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | False   | False |    26.35 ns |  0.020 ns |  0.017 ns |  1.34 |    0.01 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **848.87 ns** |  **3.816 ns** |  **3.383 ns** |  **1.00** |    **0.01** | **0.0143** |     **254 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   842.35 ns |  6.003 ns |  5.615 ns |  0.99 |    0.01 | 0.0153 |     262 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  |   856.70 ns |  3.540 ns |  3.138 ns |  1.01 |    0.01 | 0.0210 |     351 B |        1.38 |
| Instance_TaskPredicate               | False   | True  |    19.10 ns |  0.078 ns |  0.073 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.31 |
| ValueTaskSource_ValueTaskPredicate   | False   | True  |   871.40 ns |  4.970 ns |  4.649 ns |  1.03 |    0.01 | 0.0200 |     342 B |        1.35 |
| Instance_ValueTaskPredicate          | False   | True  |    28.74 ns |  0.011 ns |  0.010 ns |  0.03 |    0.00 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **21.30 ns** |  **0.163 ns** |  **0.153 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    19.76 ns |  0.101 ns |  0.094 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    41.32 ns |  0.309 ns |  0.289 ns |  1.94 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    21.66 ns |  0.100 ns |  0.089 ns |  1.02 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | True    | False |    52.96 ns |  0.056 ns |  0.053 ns |  2.49 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | True    | False |    29.87 ns |  0.016 ns |  0.014 ns |  1.40 |    0.01 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **1,356.68 ns** | **12.384 ns** | **10.341 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   842.18 ns |  4.897 ns |  4.581 ns |  0.62 |    0.01 | 0.0153 |     263 B |        0.75 |
| TaskSource_TaskPredicate             | True    | True  | 1,603.94 ns |  7.197 ns |  7.069 ns |  1.18 |    0.01 | 0.0324 |     541 B |        1.54 |
| Instance_TaskPredicate               | True    | True  |   836.45 ns |  7.181 ns |  6.717 ns |  0.62 |    0.01 | 0.0153 |     271 B |        0.77 |
| ValueTaskSource_ValueTaskPredicate   | True    | True  | 1,674.29 ns |  9.210 ns |  8.615 ns |  1.23 |    0.01 | 0.0381 |     635 B |        1.80 |
| Instance_ValueTaskPredicate          | True    | True  |   858.60 ns |  7.236 ns |  5.650 ns |  0.63 |    0.01 | 0.0172 |     295 B |        0.84 |
