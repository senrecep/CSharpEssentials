```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **13.8671 ns** | **0.2083 ns** | **0.1846 ns** |   **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   7.3223 ns | 0.0009 ns | 0.0007 ns |   0.53 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   |  21.5718 ns | 0.3903 ns | 0.3651 ns |   1.56 |    0.03 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 129.2978 ns | 0.5771 ns | 0.5399 ns |   9.33 |    0.13 | 0.0205 |     344 B |        6.14 |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.1520 ns** | **0.0036 ns** | **0.0034 ns** |   **1.00** |    **0.03** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   5.4266 ns | 0.0177 ns | 0.0165 ns |  35.71 |    0.76 |      - |         - |          NA |
| ErrorOr                    | True    |   0.1541 ns | 0.0052 ns | 0.0046 ns |   1.01 |    0.04 |      - |         - |          NA |
| FluentResults              | True    |  38.4113 ns | 0.2166 ns | 0.2026 ns | 252.80 |    5.50 | 0.0067 |     112 B |          NA |
