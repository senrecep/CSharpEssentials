```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **ClassResponse**     | **False**   | **0.4497 ns** | **0.0068 ns** | **0.0060 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Result            | False   | 0.3106 ns | 0.0114 ns | 0.0101 ns |  0.69 |    0.02 |         - |          NA |
| Typed             | False   | 0.7282 ns | 0.0015 ns | 0.0014 ns |  1.62 |    0.02 |         - |          NA |
| Typed_Constrained | False   | 2.7455 ns | 0.0029 ns | 0.0026 ns |  6.11 |    0.08 |         - |          NA |
| Result_Mediator   | False   | 0.2938 ns | 0.0152 ns | 0.0135 ns |  0.65 |    0.03 |         - |          NA |
| Typed_Mediator    | False   | 0.9933 ns | 0.0172 ns | 0.0161 ns |  2.21 |    0.04 |         - |          NA |
|                   |         |           |           |           |       |         |           |             |
| **ClassResponse**     | **True**    | **0.4457 ns** | **0.0100 ns** | **0.0094 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| Result            | True    | 0.3169 ns | 0.0121 ns | 0.0107 ns |  0.71 |    0.03 |         - |          NA |
| Typed             | True    | 0.7354 ns | 0.0013 ns | 0.0013 ns |  1.65 |    0.03 |         - |          NA |
| Typed_Constrained | True    | 2.7368 ns | 0.0083 ns | 0.0074 ns |  6.14 |    0.13 |         - |          NA |
| Result_Mediator   | True    | 0.3051 ns | 0.0210 ns | 0.0196 ns |  0.68 |    0.04 |         - |          NA |
| Typed_Mediator    | True    | 1.0116 ns | 0.0091 ns | 0.0085 ns |  2.27 |    0.05 |         - |          NA |
