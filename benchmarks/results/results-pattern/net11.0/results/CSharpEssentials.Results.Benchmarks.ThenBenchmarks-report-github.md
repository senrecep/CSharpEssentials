```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                              | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_BindAsync**                | **False**   | **False** |    **15.941 ns** |  **0.1253 ns** |  **0.1172 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | False |    31.913 ns |  0.2781 ns |  0.2466 ns |  2.00 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_ThenAsync_TaskHandler      | False   | False |    16.906 ns |  0.1110 ns |  0.0867 ns |  1.06 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_BindAsync           | False   | False |    22.842 ns |  0.0325 ns |  0.0304 ns |  1.43 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ThenAsync           | False   | False |    28.922 ns |  0.0619 ns |  0.0549 ns |  1.81 |    0.01 |      - |         - |        0.00 |
| Instance_ThenAsync_ValueTaskHandler | False   | False |     9.852 ns |  0.0669 ns |  0.0593 ns |  0.62 |    0.01 |      - |         - |        0.00 |
|                                     |         |       |              |            |            |       |         |        |           |             |
| **TaskSource_BindAsync**                | **False**   | **True**  |   **556.390 ns** |  **3.4674 ns** |  **3.2434 ns** |  **1.00** |    **0.01** | **0.0143** |     **246 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | True  |   573.628 ns |  4.4688 ns |  4.1801 ns |  1.03 |    0.01 | 0.0200 |     335 B |        1.36 |
| Instance_ThenAsync_TaskHandler      | False   | True  |    14.761 ns |  0.0928 ns |  0.0822 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.33 |
| ValueTaskSource_BindAsync           | False   | True  |   608.761 ns | 12.0489 ns | 13.8756 ns |  1.09 |    0.03 | 0.0200 |     341 B |        1.39 |
| ValueTaskSource_ThenAsync           | False   | True  |   619.666 ns |  5.9945 ns |  5.3140 ns |  1.11 |    0.01 | 0.0200 |     341 B |        1.39 |
| Instance_ThenAsync_ValueTaskHandler | False   | True  |     9.631 ns |  0.0912 ns |  0.0853 ns |  0.02 |    0.00 |      - |         - |        0.00 |
|                                     |         |       |              |            |            |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **False** |    **22.614 ns** |  **0.2503 ns** |  **0.1954 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | False |    38.189 ns |  0.3050 ns |  0.2853 ns |  1.69 |    0.02 | 0.0143 |     240 B |        1.50 |
| Instance_ThenAsync_TaskHandler      | True    | False |    21.135 ns |  0.1796 ns |  0.1680 ns |  0.93 |    0.01 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_BindAsync           | True    | False |    30.943 ns |  0.0311 ns |  0.0275 ns |  1.37 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ThenAsync           | True    | False |    33.261 ns |  0.0402 ns |  0.0356 ns |  1.47 |    0.01 |      - |         - |        0.00 |
| Instance_ThenAsync_ValueTaskHandler | True    | False |    14.341 ns |  0.0278 ns |  0.0260 ns |  0.63 |    0.01 |      - |         - |        0.00 |
|                                     |         |       |              |            |            |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **True**  |   **911.194 ns** | **10.9702 ns** |  **9.7248 ns** |  **1.00** |    **0.01** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | True  | 1,055.229 ns | 13.5244 ns | 11.9890 ns |  1.16 |    0.02 | 0.0267 |     500 B |        1.42 |
| Instance_ThenAsync_TaskHandler      | True    | True  |   552.189 ns |  3.4097 ns |  3.1894 ns |  0.61 |    0.01 | 0.0143 |     247 B |        0.70 |
| ValueTaskSource_BindAsync           | True    | True  | 1,010.908 ns | 11.9904 ns | 10.6291 ns |  1.11 |    0.02 | 0.0267 |     464 B |        1.32 |
| ValueTaskSource_ThenAsync           | True    | True  | 1,172.554 ns | 23.2321 ns | 39.4499 ns |  1.29 |    0.04 | 0.0381 |     632 B |        1.80 |
| Instance_ThenAsync_ValueTaskHandler | True    | True  |   605.662 ns |  5.5572 ns |  5.1982 ns |  0.66 |    0.01 | 0.0172 |     294 B |        0.84 |
