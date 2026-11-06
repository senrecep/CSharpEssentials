```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                         | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_BindAsync**           | **False**   | **False** |    **26.69 ns** |  **0.201 ns** |  **0.188 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_ThenAsync           | False   | False |    42.87 ns |  0.370 ns |  0.346 ns |  1.61 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_ThenAsync_TaskHandler | False   | False |    34.37 ns |  0.607 ns |  0.567 ns |  1.29 |    0.02 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_BindAsync      | False   | False |    59.90 ns |  0.134 ns |  0.112 ns |  2.24 |    0.02 | 0.0033 |      56 B |        0.70 |
|                                |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**           | **False**   | **True**  |   **867.27 ns** |  **3.833 ns** |  **3.585 ns** |  **1.00** |    **0.01** | **0.0181** |     **303 B** |        **1.00** |
| TaskSource_ThenAsync           | False   | True  |   893.65 ns |  5.246 ns |  4.907 ns |  1.03 |    0.01 | 0.0229 |     391 B |        1.29 |
| Instance_ThenAsync_TaskHandler | False   | True  |    34.07 ns |  0.391 ns |  0.365 ns |  0.04 |    0.00 | 0.0081 |     136 B |        0.45 |
| ValueTaskSource_BindAsync      | False   | True  |   908.86 ns |  6.883 ns |  6.439 ns |  1.05 |    0.01 | 0.0229 |     399 B |        1.32 |
|                                |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**           | **True**    | **False** |    **29.97 ns** |  **0.277 ns** |  **0.259 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_ThenAsync           | True    | False |    50.87 ns |  0.261 ns |  0.244 ns |  1.70 |    0.02 | 0.0143 |     240 B |        1.50 |
| Instance_ThenAsync_TaskHandler | True    | False |    27.67 ns |  0.424 ns |  0.397 ns |  0.92 |    0.01 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_BindAsync      | True    | False |    52.74 ns |  0.205 ns |  0.192 ns |  1.76 |    0.02 |      - |         - |        0.00 |
|                                |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**           | **True**    | **True**  | **1,437.11 ns** |  **8.873 ns** |  **8.299 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_ThenAsync           | True    | True  | 1,654.09 ns |  6.668 ns |  6.237 ns |  1.15 |    0.01 | 0.0286 |     502 B |        1.43 |
| Instance_ThenAsync_TaskHandler | True    | True  |   841.66 ns |  6.160 ns |  5.762 ns |  0.59 |    0.01 | 0.0143 |     247 B |        0.70 |
| ValueTaskSource_BindAsync      | True    | True  | 1,534.54 ns | 14.971 ns | 14.004 ns |  1.07 |    0.01 | 0.0267 |     464 B |        1.32 |
