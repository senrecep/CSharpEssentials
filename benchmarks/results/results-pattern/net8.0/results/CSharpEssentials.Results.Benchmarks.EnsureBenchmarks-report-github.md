```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **20.13 ns** |  **0.129 ns** |  **0.120 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    22.68 ns |  0.105 ns |  0.098 ns |  1.13 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    42.85 ns |  0.241 ns |  0.225 ns |  2.13 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    71.00 ns |  0.139 ns |  0.130 ns |  3.53 |    0.02 | 0.0081 |     136 B |        1.70 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **860.07 ns** |  **5.005 ns** |  **4.682 ns** |  **1.00** |    **0.01** | **0.0181** |     **312 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   859.15 ns |  5.458 ns |  5.106 ns |  1.00 |    0.01 | 0.0191 |     319 B |        1.02 |
| TaskSource_TaskPredicate             | False   | True  |   891.34 ns |  5.510 ns |  5.154 ns |  1.04 |    0.01 | 0.0238 |     407 B |        1.30 |
| Instance_TaskPredicate               | False   | True  |    33.90 ns |  0.195 ns |  0.163 ns |  0.04 |    0.00 | 0.0081 |     136 B |        0.44 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **23.95 ns** |  **0.162 ns** |  **0.143 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    22.33 ns |  0.125 ns |  0.117 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    44.45 ns |  0.324 ns |  0.287 ns |  1.86 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    22.62 ns |  0.123 ns |  0.109 ns |  0.94 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **1,426.73 ns** |  **6.221 ns** |  **5.515 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   854.44 ns |  5.347 ns |  5.002 ns |  0.60 |    0.00 | 0.0153 |     262 B |        0.74 |
| TaskSource_TaskPredicate             | True    | True  | 1,657.55 ns | 10.816 ns | 10.118 ns |  1.16 |    0.01 | 0.0305 |     541 B |        1.54 |
| Instance_TaskPredicate               | True    | True  |   868.18 ns | 15.792 ns | 14.772 ns |  0.61 |    0.01 | 0.0153 |     271 B |        0.77 |
