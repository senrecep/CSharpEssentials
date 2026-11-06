```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **27.9547 ns** | **0.0776 ns** | **0.0648 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  0.3016 ns | 0.0032 ns | 0.0030 ns |  0.01 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 19.5903 ns | 0.2539 ns | 0.2375 ns |  0.70 |    0.01 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 93.4794 ns | 0.7133 ns | 0.6672 ns |  3.34 |    0.02 | 0.0205 |     344 B |        6.14 |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.2711 ns** | **0.0013 ns** | **0.0011 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  0.2907 ns | 0.0004 ns | 0.0004 ns |  1.07 |    0.00 |      - |         - |          NA |
| ErrorOr                    | True    |  0.2708 ns | 0.0004 ns | 0.0004 ns |  1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 23.8158 ns | 0.1914 ns | 0.1790 ns | 87.84 |    0.72 | 0.0067 |     112 B |          NA |
