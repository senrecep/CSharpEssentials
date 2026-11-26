```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ClassResponse**     | **False**   | **0.6436 ns** | **0.0013 ns** | **0.0011 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Result            | False   | 0.5134 ns | 0.0059 ns | 0.0046 ns |  0.80 |    0.01 |      - |         - |          NA |
| Typed             | False   | 8.3365 ns | 0.0535 ns | 0.0475 ns | 12.95 |    0.07 | 0.0019 |      32 B |          NA |
| Typed_Constrained | False   | 2.7570 ns | 0.0051 ns | 0.0042 ns |  4.28 |    0.01 |      - |         - |          NA |
|                   |         |           |           |           |       |         |        |           |             |
| **ClassResponse**     | **True**    | **0.6449 ns** | **0.0012 ns** | **0.0011 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Result            | True    | 0.5259 ns | 0.0082 ns | 0.0073 ns |  0.82 |    0.01 |      - |         - |          NA |
| Typed             | True    | 8.2595 ns | 0.0596 ns | 0.0529 ns | 12.81 |    0.08 | 0.0019 |      32 B |          NA |
| Typed_Constrained | True    | 2.7624 ns | 0.0051 ns | 0.0043 ns |  4.28 |    0.01 |      - |         - |          NA |
