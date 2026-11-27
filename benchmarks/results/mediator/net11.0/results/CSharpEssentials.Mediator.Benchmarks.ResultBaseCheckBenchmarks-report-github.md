```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **ClassResponse**     | **False**   | **0.4355 ns** | **0.0044 ns** | **0.0041 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| Result            | False   | 0.4445 ns | 0.0128 ns | 0.0120 ns |  1.02 |    0.03 |         - |          NA |
| Typed             | False   | 0.4278 ns | 0.0057 ns | 0.0048 ns |  0.98 |    0.01 |         - |          NA |
| Typed_Constrained | False   | 2.1582 ns | 0.0039 ns | 0.0036 ns |  4.96 |    0.05 |         - |          NA |
| Result_Mediator   | False   | 0.4326 ns | 0.0088 ns | 0.0078 ns |  0.99 |    0.02 |         - |          NA |
| Typed_Mediator    | False   | 1.1905 ns | 0.0305 ns | 0.0285 ns |  2.73 |    0.07 |         - |          NA |
|                   |         |           |           |           |       |         |           |             |
| **ClassResponse**     | **True**    | **0.4281 ns** | **0.0041 ns** | **0.0039 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| Result            | True    | 0.4371 ns | 0.0190 ns | 0.0178 ns |  1.02 |    0.04 |         - |          NA |
| Typed             | True    | 0.4299 ns | 0.0133 ns | 0.0118 ns |  1.00 |    0.03 |         - |          NA |
| Typed_Constrained | True    | 2.1477 ns | 0.0086 ns | 0.0077 ns |  5.02 |    0.05 |         - |          NA |
| Result_Mediator   | True    | 0.4356 ns | 0.0156 ns | 0.0146 ns |  1.02 |    0.03 |         - |          NA |
| Typed_Mediator    | True    | 1.1495 ns | 0.0297 ns | 0.0263 ns |  2.69 |    0.06 |         - |          NA |
