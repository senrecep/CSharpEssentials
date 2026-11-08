```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **17.13 ns** | **0.233 ns** | **0.195 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    19.22 ns | 0.119 ns | 0.111 ns |  1.12 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    36.49 ns | 0.227 ns | 0.212 ns |  2.13 |    0.03 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    18.12 ns | 0.093 ns | 0.087 ns |  1.06 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | False   | False |    53.71 ns | 0.089 ns | 0.084 ns |  3.14 |    0.03 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | False   | False |    32.16 ns | 0.030 ns | 0.028 ns |  1.88 |    0.02 |      - |         - |        0.00 |
|                                      |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **824.02 ns** | **3.777 ns** | **3.533 ns** |  **1.00** |    **0.01** | **0.0143** |     **255 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   840.35 ns | 4.333 ns | 3.841 ns |  1.02 |    0.01 | 0.0153 |     263 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  |   854.66 ns | 5.087 ns | 4.758 ns |  1.04 |    0.01 | 0.0210 |     351 B |        1.38 |
| Instance_TaskPredicate               | False   | True  |    18.06 ns | 0.109 ns | 0.102 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.31 |
| ValueTaskSource_ValueTaskPredicate   | False   | True  |   845.36 ns | 5.559 ns | 5.200 ns |  1.03 |    0.01 | 0.0200 |     343 B |        1.35 |
| Instance_ValueTaskPredicate          | False   | True  |    34.97 ns | 0.023 ns | 0.020 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|                                      |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **20.17 ns** | **0.092 ns** | **0.081 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    18.63 ns | 0.119 ns | 0.111 ns |  0.92 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    43.11 ns | 0.243 ns | 0.215 ns |  2.14 |    0.01 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    19.90 ns | 0.112 ns | 0.100 ns |  0.99 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | True    | False |    56.58 ns | 0.020 ns | 0.017 ns |  2.81 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | True    | False |    33.89 ns | 0.050 ns | 0.044 ns |  1.68 |    0.01 |      - |         - |        0.00 |
|                                      |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **1,384.06 ns** | **5.498 ns** | **4.873 ns** |  **1.00** |    **0.00** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   836.05 ns | 4.167 ns | 3.897 ns |  0.60 |    0.00 | 0.0153 |     263 B |        0.75 |
| TaskSource_TaskPredicate             | True    | True  | 1,597.06 ns | 8.943 ns | 7.928 ns |  1.15 |    0.01 | 0.0305 |     541 B |        1.54 |
| Instance_TaskPredicate               | True    | True  |   842.89 ns | 6.972 ns | 6.522 ns |  0.61 |    0.01 | 0.0153 |     271 B |        0.77 |
| ValueTaskSource_ValueTaskPredicate   | True    | True  | 1,658.60 ns | 8.064 ns | 7.149 ns |  1.20 |    0.01 | 0.0381 |     634 B |        1.80 |
| Instance_ValueTaskPredicate          | True    | True  |   836.22 ns | 3.807 ns | 3.561 ns |  0.60 |    0.00 | 0.0172 |     295 B |        0.84 |
