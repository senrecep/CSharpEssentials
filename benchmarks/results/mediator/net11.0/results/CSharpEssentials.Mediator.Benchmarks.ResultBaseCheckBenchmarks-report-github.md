```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **ClassResponse**     | **False**   | **0.4370 ns** | **0.0075 ns** | **0.0067 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Result            | False   | 0.4348 ns | 0.0071 ns | 0.0066 ns |  1.00 |    0.02 |         - |          NA |
| Typed             | False   | 0.4501 ns | 0.0100 ns | 0.0094 ns |  1.03 |    0.03 |         - |          NA |
| Typed_Constrained | False   | 2.1666 ns | 0.0095 ns | 0.0084 ns |  4.96 |    0.07 |         - |          NA |
|                   |         |           |           |           |       |         |           |             |
| **ClassResponse**     | **True**    | **0.4379 ns** | **0.0075 ns** | **0.0070 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Result            | True    | 0.4446 ns | 0.0137 ns | 0.0122 ns |  1.02 |    0.03 |         - |          NA |
| Typed             | True    | 0.4228 ns | 0.0065 ns | 0.0058 ns |  0.97 |    0.02 |         - |          NA |
| Typed_Constrained | True    | 2.4488 ns | 0.0033 ns | 0.0030 ns |  5.59 |    0.09 |         - |          NA |
