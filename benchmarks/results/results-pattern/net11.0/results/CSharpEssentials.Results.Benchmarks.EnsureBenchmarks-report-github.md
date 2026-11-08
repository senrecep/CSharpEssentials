```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **15.79 ns** |  **0.189 ns** |  **0.177 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    18.82 ns |  0.146 ns |  0.137 ns |  1.19 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    35.21 ns |  0.335 ns |  0.297 ns |  2.23 |    0.03 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    17.09 ns |  0.179 ns |  0.168 ns |  1.08 |    0.02 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | False   | False |    52.47 ns |  0.034 ns |  0.032 ns |  3.32 |    0.04 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | False   | False |    31.86 ns |  0.026 ns |  0.025 ns |  2.02 |    0.02 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **552.31 ns** |  **4.128 ns** |  **3.861 ns** |  **1.00** |    **0.01** | **0.0143** |     **254 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   529.03 ns |  4.260 ns |  3.985 ns |  0.96 |    0.01 | 0.0153 |     262 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  |   573.24 ns |  4.519 ns |  4.006 ns |  1.04 |    0.01 | 0.0210 |     350 B |        1.38 |
| Instance_TaskPredicate               | False   | True  |    16.96 ns |  0.185 ns |  0.173 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.31 |
| ValueTaskSource_ValueTaskPredicate   | False   | True  |   643.82 ns |  4.695 ns |  4.392 ns |  1.17 |    0.01 | 0.0200 |     342 B |        1.35 |
| Instance_ValueTaskPredicate          | False   | True  |    34.24 ns |  0.016 ns |  0.015 ns |  0.06 |    0.00 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **19.86 ns** |  **0.140 ns** |  **0.131 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    18.75 ns |  0.168 ns |  0.157 ns |  0.94 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    40.48 ns |  0.222 ns |  0.208 ns |  2.04 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    19.17 ns |  0.113 ns |  0.106 ns |  0.97 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | True    | False |    53.79 ns |  0.044 ns |  0.041 ns |  2.71 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | True    | False |    33.65 ns |  0.033 ns |  0.031 ns |  1.69 |    0.01 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  |   **924.69 ns** | **12.927 ns** | **11.460 ns** |  **1.00** |    **0.02** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   553.27 ns |  8.968 ns |  8.389 ns |  0.60 |    0.01 | 0.0153 |     262 B |        0.74 |
| TaskSource_TaskPredicate             | True    | True  | 1,119.88 ns | 22.087 ns | 31.677 ns |  1.21 |    0.04 | 0.0324 |     539 B |        1.53 |
| Instance_TaskPredicate               | True    | True  |   561.14 ns |  3.919 ns |  3.666 ns |  0.61 |    0.01 | 0.0153 |     270 B |        0.77 |
| ValueTaskSource_ValueTaskPredicate   | True    | True  | 1,137.02 ns | 11.816 ns | 10.474 ns |  1.23 |    0.02 | 0.0362 |     630 B |        1.79 |
| Instance_ValueTaskPredicate          | True    | True  |   634.65 ns |  6.871 ns |  6.427 ns |  0.69 |    0.01 | 0.0172 |     293 B |        0.83 |
