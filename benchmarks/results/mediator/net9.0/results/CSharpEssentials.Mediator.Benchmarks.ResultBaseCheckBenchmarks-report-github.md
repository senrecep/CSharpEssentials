```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method            | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------ |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **ClassResponse**     | **False**   | **0.6450 ns** | **0.0016 ns** | **0.0014 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Result            | False   | 0.5385 ns | 0.0129 ns | 0.0120 ns |  0.83 |    0.02 |      - |         - |          NA |
| Typed             | False   | 8.4018 ns | 0.0658 ns | 0.0584 ns | 13.03 |    0.09 | 0.0019 |      32 B |          NA |
| Typed_Constrained | False   | 2.7338 ns | 0.0029 ns | 0.0024 ns |  4.24 |    0.01 |      - |         - |          NA |
| Result_Mediator   | False   | 0.7290 ns | 0.0020 ns | 0.0017 ns |  1.13 |    0.00 |      - |         - |          NA |
| Typed_Mediator    | False   | 5.4208 ns | 0.0104 ns | 0.0097 ns |  8.40 |    0.02 |      - |         - |          NA |
|                   |         |           |           |           |       |         |        |           |             |
| **ClassResponse**     | **True**    | **0.6504 ns** | **0.0021 ns** | **0.0018 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| Result            | True    | 0.5269 ns | 0.0086 ns | 0.0080 ns |  0.81 |    0.01 |      - |         - |          NA |
| Typed             | True    | 8.3548 ns | 0.1270 ns | 0.1188 ns | 12.85 |    0.18 | 0.0019 |      32 B |          NA |
| Typed_Constrained | True    | 2.7504 ns | 0.0073 ns | 0.0065 ns |  4.23 |    0.02 |      - |         - |          NA |
| Result_Mediator   | True    | 0.7371 ns | 0.0029 ns | 0.0027 ns |  1.13 |    0.01 |      - |         - |          NA |
| Typed_Mediator    | True    | 5.4045 ns | 0.0068 ns | 0.0057 ns |  8.31 |    0.02 |      - |         - |          NA |
