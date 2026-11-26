```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ClassResponse**     | **False**   | **3.437 ns** | **0.0194 ns** | **0.0172 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| Result            | False   | 7.089 ns | 0.0511 ns | 0.0478 ns |  2.06 |    0.02 | 0.0019 |      32 B |          NA |
| Typed             | False   | 8.608 ns | 0.0280 ns | 0.0248 ns |  2.50 |    0.01 | 0.0019 |      32 B |          NA |
| Typed_Constrained | False   | 3.022 ns | 0.0034 ns | 0.0030 ns |  0.88 |    0.00 |      - |         - |          NA |
|                   |         |          |           |           |       |         |        |           |             |
| **ClassResponse**     | **True**    | **3.767 ns** | **0.0162 ns** | **0.0144 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| Result            | True    | 6.576 ns | 0.0532 ns | 0.0498 ns |  1.75 |    0.01 | 0.0019 |      32 B |          NA |
| Typed             | True    | 7.758 ns | 0.0159 ns | 0.0124 ns |  2.06 |    0.01 | 0.0019 |      32 B |          NA |
| Typed_Constrained | True    | 3.020 ns | 0.0065 ns | 0.0057 ns |  0.80 |    0.00 |      - |         - |          NA |
