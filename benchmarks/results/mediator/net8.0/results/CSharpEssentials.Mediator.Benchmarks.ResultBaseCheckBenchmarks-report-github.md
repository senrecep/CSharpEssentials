```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ClassResponse**     | **False**   | **3.5621 ns** | **0.0149 ns** | **0.0125 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Result            | False   | 6.8316 ns | 0.0464 ns | 0.0434 ns |  1.92 |    0.01 | 0.0019 |      32 B |          NA |
| Typed             | False   | 8.6695 ns | 0.0510 ns | 0.0477 ns |  2.43 |    0.02 | 0.0019 |      32 B |          NA |
| Typed_Constrained | False   | 3.3319 ns | 0.0072 ns | 0.0060 ns |  0.94 |    0.00 |      - |         - |          NA |
| Result_Mediator   | False   | 0.5112 ns | 0.0061 ns | 0.0057 ns |  0.14 |    0.00 |      - |         - |          NA |
| Typed_Mediator    | False   | 9.2482 ns | 0.0250 ns | 0.0233 ns |  2.60 |    0.01 |      - |         - |          NA |
|                   |         |           |           |           |       |         |        |           |             |
| **ClassResponse**     | **True**    | **3.5782 ns** | **0.0110 ns** | **0.0097 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Result            | True    | 6.8201 ns | 0.0354 ns | 0.0296 ns |  1.91 |    0.01 | 0.0019 |      32 B |          NA |
| Typed             | True    | 7.9796 ns | 0.0476 ns | 0.0445 ns |  2.23 |    0.01 | 0.0019 |      32 B |          NA |
| Typed_Constrained | True    | 3.3341 ns | 0.0063 ns | 0.0056 ns |  0.93 |    0.00 |      - |         - |          NA |
| Result_Mediator   | True    | 0.4988 ns | 0.0096 ns | 0.0090 ns |  0.14 |    0.00 |      - |         - |          NA |
| Typed_Mediator    | True    | 9.2593 ns | 0.0211 ns | 0.0197 ns |  2.59 |    0.01 |      - |         - |          NA |
