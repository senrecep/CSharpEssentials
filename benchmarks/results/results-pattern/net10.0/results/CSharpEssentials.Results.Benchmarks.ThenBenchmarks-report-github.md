```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                              | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **TaskSource_BindAsync**                | **False**   | **False** |    **17.98 ns** |  **0.095 ns** |  **0.084 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | False |    33.80 ns |  0.206 ns |  0.192 ns |  1.88 |    0.01 | 0.0095 |     160 B |        2.00 |
| Instance_ThenAsync_TaskHandler      | False   | False |    28.77 ns |  0.351 ns |  0.329 ns |  1.60 |    0.02 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_BindAsync           | False   | False |    41.40 ns |  0.139 ns |  0.130 ns |  2.30 |    0.01 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ThenAsync           | False   | False |    44.73 ns |  0.153 ns |  0.143 ns |  2.49 |    0.01 | 0.0033 |      56 B |        0.70 |
| Instance_ThenAsync_ValueTaskHandler | False   | False |    23.43 ns |  0.084 ns |  0.078 ns |  1.30 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **False**   | **True**  |   **846.57 ns** |  **4.185 ns** |  **3.915 ns** |  **1.00** |    **0.01** | **0.0181** |     **303 B** |        **1.00** |
| TaskSource_ThenAsync                | False   | True  |   869.60 ns | 15.069 ns | 13.359 ns |  1.03 |    0.02 | 0.0229 |     391 B |        1.29 |
| Instance_ThenAsync_TaskHandler      | False   | True  |    29.24 ns |  0.295 ns |  0.276 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.45 |
| ValueTaskSource_BindAsync           | False   | True  |   870.28 ns |  4.436 ns |  4.149 ns |  1.03 |    0.01 | 0.0238 |     399 B |        1.32 |
| ValueTaskSource_ThenAsync           | False   | True  |   863.35 ns |  4.823 ns |  4.512 ns |  1.02 |    0.01 | 0.0238 |     399 B |        1.32 |
| Instance_ThenAsync_ValueTaskHandler | False   | True  |    22.89 ns |  0.098 ns |  0.091 ns |  0.03 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **False** |    **25.59 ns** |  **0.290 ns** |  **0.271 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | False |    43.49 ns |  0.596 ns |  0.529 ns |  1.70 |    0.03 | 0.0143 |     240 B |        1.50 |
| Instance_ThenAsync_TaskHandler      | True    | False |    23.98 ns |  0.302 ns |  0.282 ns |  0.94 |    0.01 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_BindAsync           | True    | False |    29.59 ns |  0.126 ns |  0.118 ns |  1.16 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ThenAsync           | True    | False |    33.75 ns |  0.117 ns |  0.109 ns |  1.32 |    0.01 |      - |         - |        0.00 |
| Instance_ThenAsync_ValueTaskHandler | True    | False |    14.43 ns |  0.024 ns |  0.021 ns |  0.56 |    0.01 |      - |         - |        0.00 |
|                                     |         |       |             |           |           |       |         |        |           |             |
| **TaskSource_BindAsync**                | **True**    | **True**  | **1,369.53 ns** |  **4.807 ns** |  **4.261 ns** |  **1.00** |    **0.00** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_ThenAsync                | True    | True  | 1,610.64 ns |  7.251 ns |  6.783 ns |  1.18 |    0.01 | 0.0286 |     502 B |        1.43 |
| Instance_ThenAsync_TaskHandler      | True    | True  |   842.14 ns |  4.859 ns |  4.545 ns |  0.61 |    0.00 | 0.0143 |     247 B |        0.70 |
| ValueTaskSource_BindAsync           | True    | True  | 1,511.03 ns |  7.618 ns |  7.126 ns |  1.10 |    0.01 | 0.0267 |     464 B |        1.32 |
| ValueTaskSource_ThenAsync           | True    | True  | 1,678.84 ns |  9.541 ns |  8.925 ns |  1.23 |    0.01 | 0.0381 |     634 B |        1.80 |
| Instance_ThenAsync_ValueTaskHandler | True    | True  |   843.78 ns |  4.191 ns |  3.920 ns |  0.62 |    0.00 | 0.0172 |     295 B |        0.84 |
