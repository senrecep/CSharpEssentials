```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **23.6150 ns** | **0.1332 ns** | **0.1246 ns** |  **1.00** |    **0.01** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  0.3424 ns | 0.0103 ns | 0.0096 ns |  0.01 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 18.4388 ns | 0.1549 ns | 0.1449 ns |  0.78 |    0.01 | 0.0052 |      88 B |        0.85 |
| FluentResults              | False   | 92.6679 ns | 0.7958 ns | 0.7444 ns |  3.92 |    0.04 | 0.0205 |     344 B |        3.31 |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.2687 ns** | **0.0001 ns** | **0.0001 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  0.2916 ns | 0.0015 ns | 0.0014 ns |  1.09 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |  0.2702 ns | 0.0006 ns | 0.0004 ns |  1.01 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 22.6842 ns | 0.1532 ns | 0.1433 ns | 84.44 |    0.52 | 0.0067 |     112 B |          NA |
