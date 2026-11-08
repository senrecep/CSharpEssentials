```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Median      | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|------------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **13.6011 ns** | **0.2286 ns** | **0.1909 ns** |  **13.5641 ns** |  **1.000** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   0.0002 ns | 0.0005 ns | 0.0004 ns |   0.0000 ns |  0.000 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   |  20.4178 ns | 0.2100 ns | 0.1964 ns |  20.4417 ns |  1.501 |    0.02 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 105.5643 ns | 0.7081 ns | 0.5913 ns | 105.7620 ns |  7.763 |    0.11 | 0.0205 |     344 B |        6.14 |
|                            |         |             |           |           |             |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.1588 ns** | **0.0117 ns** | **0.0104 ns** |   **0.1579 ns** |   **1.00** |    **0.09** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   0.0233 ns | 0.0276 ns | 0.0258 ns |   0.0222 ns |   0.15 |    0.16 |      - |         - |          NA |
| ErrorOr                    | True    |   0.1551 ns | 0.0067 ns | 0.0059 ns |   0.1546 ns |   0.98 |    0.07 |      - |         - |          NA |
| FluentResults              | True    |  25.5255 ns | 0.5575 ns | 0.5214 ns |  25.5527 ns | 161.38 |   10.75 | 0.0067 |     112 B |          NA |
