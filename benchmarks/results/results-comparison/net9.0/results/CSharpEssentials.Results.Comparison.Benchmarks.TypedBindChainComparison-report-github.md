```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **8.628 ns** | **0.0174 ns** | **0.0145 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   6.221 ns | 0.0083 ns | 0.0069 ns |  0.72 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  29.950 ns | 0.0484 ns | 0.0404 ns |  3.47 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |  10.643 ns | 0.0064 ns | 0.0057 ns |  1.23 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.323 ns | 0.0211 ns | 0.0187 ns |  0.96 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 391.826 ns | 1.4425 ns | 1.3494 ns | 45.41 |    0.17 | 0.0610 |    1024 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |  **14.975 ns** | **0.0813 ns** | **0.0760 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   6.324 ns | 0.0111 ns | 0.0103 ns |  0.42 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  31.508 ns | 0.0461 ns | 0.0431 ns |  2.10 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   5.363 ns | 0.0045 ns | 0.0035 ns |  0.36 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  26.279 ns | 0.1090 ns | 0.1019 ns |  1.75 |    0.01 | 0.0014 |      24 B |          NA |
| FluentResults              | True    | 386.714 ns | 0.6518 ns | 0.5088 ns | 25.82 |    0.13 | 0.0591 |     992 B |          NA |
