```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                         | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **TaskSource_BindAsync**           | **False**   | **False** |    **23.50 ns** |  **0.146 ns** |  **0.114 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_ThenAsync           | False   | False |    41.19 ns |  0.197 ns |  0.185 ns |  1.75 | 0.0095 |     160 B |        2.00 |
| Instance_ThenAsync_TaskHandler | False   | False |    20.05 ns |  0.129 ns |  0.121 ns |  0.85 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_BindAsync      | False   | False |    44.38 ns |  0.021 ns |  0.020 ns |  1.89 |      - |         - |        0.00 |
|                                |         |       |             |           |           |       |        |           |             |
| **TaskSource_BindAsync**           | **False**   | **True**  |   **839.53 ns** |  **4.443 ns** |  **4.156 ns** |  **1.00** | **0.0143** |     **247 B** |        **1.00** |
| TaskSource_ThenAsync           | False   | True  |   859.93 ns |  7.599 ns |  7.108 ns |  1.02 | 0.0200 |     335 B |        1.36 |
| Instance_ThenAsync_TaskHandler | False   | True  |    19.73 ns |  0.133 ns |  0.118 ns |  0.02 | 0.0048 |      80 B |        0.32 |
| ValueTaskSource_BindAsync      | False   | True  |   887.59 ns |  7.222 ns |  6.755 ns |  1.06 | 0.0200 |     343 B |        1.39 |
|                                |         |       |             |           |           |       |        |           |             |
| **TaskSource_BindAsync**           | **True**    | **False** |    **29.37 ns** |  **0.174 ns** |  **0.163 ns** |  **1.00** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_ThenAsync           | True    | False |    47.89 ns |  0.281 ns |  0.263 ns |  1.63 | 0.0143 |     240 B |        1.50 |
| Instance_ThenAsync_TaskHandler | True    | False |    25.80 ns |  0.168 ns |  0.157 ns |  0.88 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_BindAsync      | True    | False |    44.39 ns |  0.073 ns |  0.068 ns |  1.51 |      - |         - |        0.00 |
|                                |         |       |             |           |           |       |        |           |             |
| **TaskSource_BindAsync**           | **True**    | **True**  | **1,407.18 ns** |  **8.318 ns** |  **7.374 ns** |  **1.00** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_ThenAsync           | True    | True  | 1,610.36 ns | 16.255 ns | 15.205 ns |  1.14 | 0.0267 |     502 B |        1.43 |
| Instance_ThenAsync_TaskHandler | True    | True  |   826.49 ns |  4.264 ns |  3.989 ns |  0.59 | 0.0143 |     247 B |        0.70 |
| ValueTaskSource_BindAsync      | True    | True  | 1,534.73 ns | 11.401 ns | 10.107 ns |  1.09 | 0.0267 |     464 B |        1.32 |
