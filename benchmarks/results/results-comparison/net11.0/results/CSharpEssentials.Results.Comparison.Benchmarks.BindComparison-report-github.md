```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.6854 ns** | **0.0138 ns** | **0.0123 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | False   |  1.3875 ns | 0.0072 ns | 0.0067 ns |  2.03 |    0.04 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |  1.9557 ns | 0.0037 ns | 0.0031 ns |  2.85 |    0.05 |      - |         - |          NA |
| ErrorOr                    | False   |  0.8181 ns | 0.0053 ns | 0.0045 ns |  1.19 |    0.02 |      - |         - |          NA |
| FluentResults              | False   | 51.9694 ns | 0.2812 ns | 0.2631 ns | 75.85 |    1.37 | 0.0124 |     208 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.7545 ns** | **0.0112 ns** | **0.0105 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | True    |  1.3927 ns | 0.0015 ns | 0.0014 ns |  1.85 |    0.02 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  3.1325 ns | 0.0050 ns | 0.0047 ns |  4.15 |    0.06 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5854 ns | 0.0054 ns | 0.0051 ns |  0.78 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 64.5618 ns | 0.3751 ns | 0.3509 ns | 85.58 |    1.23 | 0.0191 |     320 B |          NA |
