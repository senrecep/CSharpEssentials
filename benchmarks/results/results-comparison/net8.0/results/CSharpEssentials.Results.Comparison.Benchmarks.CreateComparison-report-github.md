```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **24.3507 ns** | **0.2744 ns** | **0.2566 ns** |   **1.00** |    **0.01** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   7.3484 ns | 0.0011 ns | 0.0009 ns |   0.30 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   |  18.4789 ns | 0.2194 ns | 0.2052 ns |   0.76 |    0.01 | 0.0052 |      88 B |        0.85 |
| FluentResults              | False   | 177.5352 ns | 3.5202 ns | 3.2928 ns |   7.29 |    0.15 | 0.0229 |     384 B |        3.69 |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.1669 ns** | **0.0020 ns** | **0.0018 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   7.4329 ns | 0.0008 ns | 0.0007 ns |  44.55 |    0.47 |      - |         - |          NA |
| ErrorOr                    | True    |   0.3066 ns | 0.0002 ns | 0.0002 ns |   1.84 |    0.02 |      - |         - |          NA |
| FluentResults              | True    |  40.8739 ns | 0.2147 ns | 0.2008 ns | 244.99 |    2.85 | 0.0072 |     120 B |          NA |
