```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **20.99 ns** |  **0.140 ns** |  **0.109 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    50.13 ns |  0.105 ns |  0.093 ns |  2.39 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    98.94 ns |  0.318 ns |  0.282 ns |  4.71 |    0.03 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |   139.58 ns |  0.237 ns |  0.221 ns |  6.65 |    0.03 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    34.90 ns |  0.194 ns |  0.182 ns |  1.66 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |    39.58 ns |  0.030 ns |  0.027 ns |  1.89 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **844.00 ns** |  **9.598 ns** |  **8.978 ns** |  **1.00** |    **0.01** | **0.0134** |     **224 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   853.41 ns |  8.179 ns |  7.651 ns |  1.01 |    0.01 | 0.0143 |     240 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  |   911.31 ns |  8.616 ns |  8.059 ns |  1.08 |    0.01 | 0.0200 |     336 B |        1.50 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,020.96 ns | 20.103 ns | 37.262 ns |  1.21 |    0.05 | 0.0191 |     343 B |        1.53 |
| Instance_TaskHandler               | False   | True  |    42.98 ns |  0.128 ns |  0.114 ns |  0.05 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |    41.71 ns |  0.164 ns |  0.153 ns |  0.05 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **28.38 ns** |  **0.107 ns** |  **0.095 ns** |  **1.00** |    **0.00** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    49.80 ns |  0.169 ns |  0.158 ns |  1.75 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |   114.06 ns |  0.498 ns |  0.466 ns |  4.02 |    0.02 | 0.0143 |     240 B |        1.50 |
| ValueTaskSource_ValueTaskHandler   | True    | False |   167.59 ns |  0.234 ns |  0.219 ns |  5.91 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    53.94 ns |  0.230 ns |  0.216 ns |  1.90 |    0.01 | 0.0095 |     160 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    68.78 ns |  0.106 ns |  0.099 ns |  2.42 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,396.17 ns** |  **7.771 ns** |  **6.888 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   850.43 ns | 10.019 ns |  9.372 ns |  0.61 |    0.01 | 0.0134 |     240 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,720.55 ns | 13.838 ns | 12.944 ns |  1.23 |    0.01 | 0.0286 |     495 B |        1.51 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,962.10 ns | 32.032 ns | 28.396 ns |  1.41 |    0.02 | 0.0381 |     631 B |        1.92 |
| Instance_TaskHandler               | True    | True  |   849.70 ns |  7.851 ns |  7.344 ns |  0.61 |    0.01 | 0.0143 |     240 B |        0.73 |
| Instance_ValueTaskHandler          | True    | True  |   893.56 ns | 11.003 ns |  9.754 ns |  0.64 |    0.01 | 0.0172 |     288 B |        0.88 |
