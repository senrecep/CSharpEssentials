```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **0.8915 ns** | **0.0033 ns** | **0.0028 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   8.9272 ns | 0.0048 ns | 0.0042 ns |  10.01 |    0.03 |      - |         - |          NA |
| ErrorOr                    | False   |   0.7460 ns | 0.0024 ns | 0.0022 ns |   0.84 |    0.00 |      - |         - |          NA |
| FluentResults              | False   |  73.1111 ns | 0.4790 ns | 0.4481 ns |  82.01 |    0.55 | 0.0124 |     208 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.8503 ns** | **0.0004 ns** | **0.0003 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   8.9225 ns | 0.0025 ns | 0.0023 ns |  10.49 |    0.00 |      - |         - |          NA |
| ErrorOr                    | True    |   0.8504 ns | 0.0007 ns | 0.0005 ns |   1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 120.9966 ns | 2.0661 ns | 1.9327 ns | 142.30 |    2.20 | 0.0191 |     320 B |          NA |
