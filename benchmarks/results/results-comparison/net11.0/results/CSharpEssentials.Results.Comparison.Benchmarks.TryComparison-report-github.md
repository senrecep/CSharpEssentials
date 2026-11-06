```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                          | Succeed | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |-------- |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**                | **False**   | **1,339.987 ns** |  **3.5061 ns** |  **3.2796 ns** |  **1.00** |    **0.00** | **0.0248** |     **416 B** |        **1.00** |
| CSharpEssentials_ErrorException | False   | 5,330.085 ns | 17.1836 ns | 15.2328 ns |  3.98 |    0.01 | 0.2747 |    4880 B |       11.73 |
| CSharpFunctionalExtensions      | False   | 1,394.204 ns |  2.4780 ns |  2.3180 ns |  1.04 |    0.00 | 0.0210 |     360 B |        0.87 |
| ErrorOr                         | False   | 1,318.470 ns |  5.7822 ns |  5.1258 ns |  0.98 |    0.00 | 0.0267 |     448 B |        1.08 |
| FluentResults                   | False   | 1,520.864 ns |  2.9915 ns |  2.4980 ns |  1.13 |    0.00 | 0.0420 |     728 B |        1.75 |
|                                 |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials**                | **True**    |     **6.631 ns** |  **0.0022 ns** |  **0.0021 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_ErrorException | True    |     6.883 ns |  0.0027 ns |  0.0024 ns |  1.04 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions      | True    |     7.445 ns |  0.0027 ns |  0.0024 ns |  1.12 |    0.00 |      - |         - |          NA |
| ErrorOr                         | True    |     6.015 ns |  0.0030 ns |  0.0024 ns |  0.91 |    0.00 |      - |         - |          NA |
| FluentResults                   | True    |    27.320 ns |  0.1465 ns |  0.1224 ns |  4.12 |    0.02 | 0.0067 |     112 B |          NA |
