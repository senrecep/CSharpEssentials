```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **19.11 ns** |  **0.112 ns** | **0.105 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    22.21 ns |  0.104 ns | 0.097 ns |  1.16 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    41.09 ns |  0.256 ns | 0.227 ns |  2.15 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    20.33 ns |  0.077 ns | 0.072 ns |  1.06 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                      |         |       |             |           |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **850.95 ns** |  **5.857 ns** | **5.478 ns** |  **1.00** |    **0.01** | **0.0143** |     **255 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   831.94 ns |  5.985 ns | 5.599 ns |  0.98 |    0.01 | 0.0153 |     263 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  |   869.86 ns |  5.112 ns | 4.782 ns |  1.02 |    0.01 | 0.0210 |     351 B |        1.38 |
| Instance_TaskPredicate               | False   | True  |    18.55 ns |  0.160 ns | 0.150 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.31 |
|                                      |         |       |             |           |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **23.65 ns** |  **0.099 ns** | **0.093 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    20.73 ns |  0.143 ns | 0.134 ns |  0.88 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    42.59 ns |  0.292 ns | 0.259 ns |  1.80 |    0.01 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    20.91 ns |  0.113 ns | 0.106 ns |  0.88 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                      |         |       |             |           |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **1,399.89 ns** | **10.244 ns** | **8.555 ns** |  **1.00** |    **0.01** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   856.86 ns |  4.757 ns | 4.450 ns |  0.61 |    0.00 | 0.0153 |     263 B |        0.75 |
| TaskSource_TaskPredicate             | True    | True  | 1,622.02 ns |  8.382 ns | 7.430 ns |  1.16 |    0.01 | 0.0324 |     542 B |        1.54 |
| Instance_TaskPredicate               | True    | True  |   844.01 ns |  4.181 ns | 3.706 ns |  0.60 |    0.00 | 0.0153 |     271 B |        0.77 |
