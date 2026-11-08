```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                              | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_BindAsync**                | **False**   | **False** |    **22.88 ns** |  **0.183 ns** |  **0.143 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | False |    36.22 ns |  0.296 ns |  0.277 ns |  1.58 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_ThenAsync_TaskHandler      | False   | False |    18.33 ns |  0.107 ns |  0.100 ns |  0.80 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_BindAsync           | False   | False |    42.49 ns |  0.091 ns |  0.085 ns |  1.86 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ThenAsync           | False   | False |    49.04 ns |  0.040 ns |  0.037 ns |  2.14 |    0.01 |      - |         - |        0.00 |
| Instance_ThenAsync_ValueTaskHandler | False   | False |    26.35 ns |  0.028 ns |  0.025 ns |  1.15 |    0.01 |      - |         - |        0.00 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **False**   | **True**  |   **835.98 ns** |  **4.625 ns** |  **4.326 ns** |  **1.00** |    **0.01** | **0.0143** |     **247 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | True  |   862.21 ns |  3.511 ns |  3.284 ns |  1.03 |    0.01 | 0.0200 |     335 B |        1.36 |
| Instance_ThenAsync_TaskHandler      | False   | True  |    18.33 ns |  0.119 ns |  0.111 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.32 |
| ValueTaskSource_BindAsync           | False   | True  |   861.11 ns |  3.854 ns |  3.605 ns |  1.03 |    0.01 | 0.0200 |     341 B |        1.38 |
| ValueTaskSource_ThenAsync           | False   | True  |   864.41 ns |  8.615 ns |  7.194 ns |  1.03 |    0.01 | 0.0200 |     342 B |        1.38 |
| Instance_ThenAsync_ValueTaskHandler | False   | True  |    26.97 ns |  0.023 ns |  0.021 ns |  0.03 |    0.00 |      - |         - |        0.00 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **False** |    **27.88 ns** |  **0.167 ns** |  **0.156 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | False |    45.11 ns |  0.174 ns |  0.136 ns |  1.62 |    0.01 | 0.0143 |     240 B |        1.50 |
| Instance_ThenAsync_TaskHandler      | True    | False |    25.12 ns |  0.193 ns |  0.180 ns |  0.90 |    0.01 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_BindAsync           | True    | False |    49.87 ns |  0.030 ns |  0.028 ns |  1.79 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ThenAsync           | True    | False |    51.01 ns |  0.099 ns |  0.092 ns |  1.83 |    0.01 |      - |         - |        0.00 |
| Instance_ThenAsync_ValueTaskHandler | True    | False |    30.09 ns |  0.031 ns |  0.029 ns |  1.08 |    0.01 |      - |         - |        0.00 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **True**  | **1,362.37 ns** |  **5.442 ns** |  **5.090 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | True  | 1,607.90 ns | 11.307 ns | 10.577 ns |  1.18 |    0.01 | 0.0286 |     502 B |        1.43 |
| Instance_ThenAsync_TaskHandler      | True    | True  |   837.05 ns |  5.751 ns |  5.379 ns |  0.61 |    0.00 | 0.0143 |     247 B |        0.70 |
| ValueTaskSource_BindAsync           | True    | True  | 1,447.27 ns | 12.129 ns | 10.752 ns |  1.06 |    0.01 | 0.0267 |     464 B |        1.32 |
| ValueTaskSource_ThenAsync           | True    | True  | 1,660.77 ns | 10.715 ns | 10.023 ns |  1.22 |    0.01 | 0.0381 |     635 B |        1.80 |
| Instance_ThenAsync_ValueTaskHandler | True    | True  |   850.51 ns |  5.898 ns |  5.517 ns |  0.62 |    0.00 | 0.0172 |     295 B |        0.84 |
