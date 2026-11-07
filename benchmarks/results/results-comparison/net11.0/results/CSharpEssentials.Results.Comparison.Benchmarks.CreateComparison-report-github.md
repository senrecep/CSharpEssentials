```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Median     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **9.2621 ns** | **0.0412 ns** | **0.0386 ns** |  **9.2565 ns** |  **1.000** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  0.0001 ns | 0.0002 ns | 0.0002 ns |  0.0000 ns |  0.000 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 15.1515 ns | 0.1516 ns | 0.1344 ns | 15.1880 ns |  1.636 |    0.02 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 90.3819 ns | 0.6773 ns | 0.6336 ns | 90.0853 ns |  9.758 |    0.08 | 0.0205 |     344 B |        6.14 |
|                            |         |            |           |           |            |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.1498 ns** | **0.0014 ns** | **0.0013 ns** |  **0.1502 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  0.2908 ns | 0.0004 ns | 0.0004 ns |  0.2907 ns |   1.94 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |  0.1486 ns | 0.0015 ns | 0.0013 ns |  0.1484 ns |   0.99 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 20.4042 ns | 0.1433 ns | 0.1340 ns | 20.4297 ns | 136.25 |    1.41 | 0.0067 |     112 B |          NA |
