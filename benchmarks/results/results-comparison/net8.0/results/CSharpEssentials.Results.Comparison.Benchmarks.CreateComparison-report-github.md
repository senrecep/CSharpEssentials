```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **17.6672 ns** | **0.1933 ns** | **0.1714 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   7.6536 ns | 0.0678 ns | 0.0634 ns |   0.43 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   |  24.3424 ns | 0.2645 ns | 0.2474 ns |   1.38 |    0.02 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 167.0927 ns | 1.3786 ns | 1.2221 ns |   9.46 |    0.11 | 0.0229 |     384 B |        6.86 |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.1647 ns** | **0.0143 ns** | **0.0127 ns** |   **1.01** |    **0.10** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   7.6524 ns | 0.0552 ns | 0.0489 ns |  46.72 |    3.42 |      - |         - |          NA |
| ErrorOr                    | True    |   0.3607 ns | 0.0192 ns | 0.0160 ns |   2.20 |    0.19 |      - |         - |          NA |
| FluentResults              | True    |  47.9400 ns | 0.4597 ns | 0.4300 ns | 292.68 |   21.51 | 0.0072 |     120 B |          NA |
