```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **8.642 ns** | **0.0444 ns** | **0.0415 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   6.227 ns | 0.0166 ns | 0.0147 ns |  0.72 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  30.376 ns | 0.0578 ns | 0.0540 ns |  3.52 |    0.02 |      - |         - |          NA |
| ErrorOr                    | False   |   7.142 ns | 0.0070 ns | 0.0066 ns |  0.83 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.463 ns | 0.0145 ns | 0.0129 ns |  0.98 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 392.246 ns | 2.7136 ns | 2.5383 ns | 45.39 |    0.35 | 0.0610 |    1024 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |  **19.367 ns** | **0.0052 ns** | **0.0046 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   6.554 ns | 0.0059 ns | 0.0052 ns |  0.34 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  29.932 ns | 0.0350 ns | 0.0273 ns |  1.55 |    0.00 |      - |         - |          NA |
| ErrorOr                    | True    |   5.116 ns | 0.0099 ns | 0.0092 ns |  0.26 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  16.268 ns | 0.0130 ns | 0.0115 ns |  0.84 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 387.820 ns | 1.5406 ns | 1.4411 ns | 20.02 |    0.07 | 0.0591 |     992 B |          NA |
