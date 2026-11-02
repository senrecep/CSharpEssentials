```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **1.3026 ns** | **0.0015 ns** | **0.0014 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   8.9181 ns | 0.0016 ns | 0.0015 ns |   6.85 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   2.0264 ns | 0.0028 ns | 0.0026 ns |   1.56 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 162.6351 ns | 1.4074 ns | 1.3165 ns | 124.86 |    0.99 | 0.0219 |     368 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.9414 ns** | **0.0046 ns** | **0.0043 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   8.9346 ns | 0.0022 ns | 0.0020 ns |   9.49 |    0.04 |      - |         - |          NA |
| ErrorOr                    | True    |   0.9426 ns | 0.0025 ns | 0.0023 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 132.9675 ns | 0.5695 ns | 0.5327 ns | 141.25 |    0.83 | 0.0172 |     288 B |          NA |
