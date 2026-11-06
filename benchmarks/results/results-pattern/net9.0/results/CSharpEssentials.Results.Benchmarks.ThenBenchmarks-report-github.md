```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                              | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_BindAsync**                | **False**   | **False** |    **22.38 ns** |  **0.156 ns** |  **0.138 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | False |    37.77 ns |  0.219 ns |  0.205 ns |  1.69 |    0.01 | 0.0095 |     160 B |        2.00 |
| Instance_ThenAsync_TaskHandler      | False   | False |    32.17 ns |  0.283 ns |  0.265 ns |  1.44 |    0.01 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_BindAsync           | False   | False |    57.20 ns |  0.119 ns |  0.111 ns |  2.56 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ThenAsync           | False   | False |    58.50 ns |  0.103 ns |  0.092 ns |  2.61 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_ThenAsync_ValueTaskHandler | False   | False |    39.56 ns |  0.117 ns |  0.110 ns |  1.77 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **False**   | **True**  |   **858.96 ns** |  **4.403 ns** |  **4.119 ns** |  **1.00** |    **0.01** | **0.0181** |     **303 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | True  |   878.97 ns |  4.059 ns |  3.797 ns |  1.02 |    0.01 | 0.0229 |     391 B |        1.29 |
| Instance_ThenAsync_TaskHandler      | False   | True  |    31.90 ns |  0.225 ns |  0.188 ns |  0.04 |    0.00 | 0.0081 |     136 B |        0.45 |
| ValueTaskSource_BindAsync           | False   | True  |   888.74 ns |  6.099 ns |  5.705 ns |  1.03 |    0.01 | 0.0238 |     398 B |        1.31 |
| ValueTaskSource_ThenAsync           | False   | True  |   889.46 ns |  7.750 ns |  7.249 ns |  1.04 |    0.01 | 0.0238 |     399 B |        1.32 |
| Instance_ThenAsync_ValueTaskHandler | False   | True  |    39.78 ns |  0.079 ns |  0.066 ns |  0.05 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **False** |    **29.15 ns** |  **0.447 ns** |  **0.419 ns** |  **1.00** |    **0.02** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | False |    48.70 ns |  0.703 ns |  0.657 ns |  1.67 |    0.03 | 0.0143 |     240 B |        1.50 |
| Instance_ThenAsync_TaskHandler      | True    | False |    27.34 ns |  0.330 ns |  0.308 ns |  0.94 |    0.02 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_BindAsync           | True    | False |    46.24 ns |  0.377 ns |  0.353 ns |  1.59 |    0.02 |      - |         - |        0.00 |
| ValueTaskSource_ThenAsync           | True    | False |    50.59 ns |  0.043 ns |  0.040 ns |  1.74 |    0.02 |      - |         - |        0.00 |
| Instance_ThenAsync_ValueTaskHandler | True    | False |    37.17 ns |  0.032 ns |  0.028 ns |  1.28 |    0.02 |      - |         - |        0.00 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **True**  | **1,364.40 ns** |  **4.327 ns** |  **4.047 ns** |  **1.00** |    **0.00** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | True  | 1,608.89 ns |  6.809 ns |  6.036 ns |  1.18 |    0.01 | 0.0286 |     502 B |        1.43 |
| Instance_ThenAsync_TaskHandler      | True    | True  |   849.89 ns |  4.601 ns |  4.303 ns |  0.62 |    0.00 | 0.0143 |     247 B |        0.70 |
| ValueTaskSource_BindAsync           | True    | True  | 1,519.60 ns |  6.161 ns |  5.763 ns |  1.11 |    0.01 | 0.0267 |     464 B |        1.32 |
| ValueTaskSource_ThenAsync           | True    | True  | 1,722.59 ns | 14.278 ns | 13.356 ns |  1.26 |    0.01 | 0.0381 |     635 B |        1.80 |
| Instance_ThenAsync_ValueTaskHandler | True    | True  |   854.18 ns |  5.800 ns |  5.425 ns |  0.63 |    0.00 | 0.0172 |     295 B |        0.84 |
