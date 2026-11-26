```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **18.32 ns** |  **0.091 ns** |  **0.081 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    25.51 ns |  0.075 ns |  0.071 ns |  1.39 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    51.04 ns |  0.245 ns |  0.229 ns |  2.79 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    73.69 ns |  0.110 ns |  0.103 ns |  4.02 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    31.41 ns |  0.093 ns |  0.078 ns |  1.71 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |    15.36 ns |  0.012 ns |  0.011 ns |  0.84 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **825.14 ns** |  **6.806 ns** |  **6.366 ns** |  **1.00** |    **0.01** | **0.0134** |     **223 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   815.17 ns | 10.062 ns |  9.412 ns |  0.99 |    0.01 | 0.0143 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   854.03 ns |  8.261 ns |  7.323 ns |  1.04 |    0.01 | 0.0200 |     336 B |        1.51 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   894.73 ns | 10.601 ns |  9.916 ns |  1.08 |    0.01 | 0.0200 |     343 B |        1.54 |
| Instance_TaskHandler               | False   | True  |    31.48 ns |  0.121 ns |  0.114 ns |  0.04 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |    15.11 ns |  0.009 ns |  0.007 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **25.59 ns** |  **0.080 ns** |  **0.074 ns** |  **1.00** |    **0.00** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    26.60 ns |  0.103 ns |  0.096 ns |  1.04 |    0.00 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    54.46 ns |  0.532 ns |  0.472 ns |  2.13 |    0.02 | 0.0143 |     240 B |        1.50 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    79.87 ns |  0.072 ns |  0.067 ns |  3.12 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    37.69 ns |  0.150 ns |  0.140 ns |  1.47 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    20.04 ns |  0.020 ns |  0.018 ns |  0.78 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,379.78 ns** |  **4.945 ns** |  **4.625 ns** |  **1.00** |    **0.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   824.11 ns |  6.261 ns |  5.551 ns |  0.60 |    0.00 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,630.81 ns |  6.540 ns |  5.797 ns |  1.18 |    0.01 | 0.0286 |     495 B |        1.51 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,747.23 ns | 12.539 ns | 11.116 ns |  1.27 |    0.01 | 0.0362 |     629 B |        1.92 |
| Instance_TaskHandler               | True    | True  |   810.40 ns | 12.866 ns | 12.035 ns |  0.59 |    0.01 | 0.0134 |     239 B |        0.73 |
| Instance_ValueTaskHandler          | True    | True  |   836.57 ns |  4.646 ns |  4.345 ns |  0.61 |    0.00 | 0.0162 |     287 B |        0.88 |
