```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **14.8156 ns** | **0.2728 ns** | **0.2552 ns** |   **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   7.6155 ns | 0.0575 ns | 0.0538 ns |   0.51 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   |  22.9566 ns | 0.4373 ns | 0.4091 ns |   1.55 |    0.04 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 143.2895 ns | 2.3505 ns | 2.1987 ns |   9.67 |    0.22 | 0.0205 |     344 B |        6.14 |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.2859 ns** | **0.0278 ns** | **0.0260 ns** |   **1.01** |    **0.12** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   5.5893 ns | 0.0434 ns | 0.0385 ns |  19.70 |    1.72 |      - |         - |          NA |
| ErrorOr                    | True    |   0.2810 ns | 0.0235 ns | 0.0220 ns |   0.99 |    0.11 |      - |         - |          NA |
| FluentResults              | True    |  38.1241 ns | 0.5250 ns | 0.4384 ns | 134.38 |   11.79 | 0.0067 |     112 B |          NA |
