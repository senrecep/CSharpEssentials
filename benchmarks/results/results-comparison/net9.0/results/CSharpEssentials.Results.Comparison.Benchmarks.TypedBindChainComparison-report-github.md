```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **9.109 ns** | **0.0270 ns** | **0.0253 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   6.257 ns | 0.0255 ns | 0.0213 ns |  0.69 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  30.020 ns | 0.0696 ns | 0.0617 ns |  3.30 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   7.549 ns | 0.0094 ns | 0.0088 ns |  0.83 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.267 ns | 0.0081 ns | 0.0063 ns |  0.91 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 391.301 ns | 1.7762 ns | 1.6615 ns | 42.96 |    0.21 | 0.0610 |    1024 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |  **17.561 ns** | **0.0088 ns** | **0.0078 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   6.362 ns | 0.0149 ns | 0.0132 ns |  0.36 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  29.971 ns | 0.0956 ns | 0.0894 ns |  1.71 |    0.00 |      - |         - |          NA |
| ErrorOr                    | True    |   5.114 ns | 0.0052 ns | 0.0049 ns |  0.29 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  15.172 ns | 0.0136 ns | 0.0114 ns |  0.86 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 385.961 ns | 0.9780 ns | 0.8670 ns | 21.98 |    0.05 | 0.0591 |     992 B |          NA |
