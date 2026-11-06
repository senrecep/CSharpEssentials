```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **13.8171 ns** | **0.1391 ns** | **0.1233 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |   7.4449 ns | 0.0023 ns | 0.0020 ns |   0.54 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   |  19.3543 ns | 0.1296 ns | 0.1082 ns |   1.40 |    0.01 | 0.0052 |      88 B |        1.57 |
| FluentResults              | False   | 148.2368 ns | 0.7152 ns | 0.6690 ns |  10.73 |    0.10 | 0.0229 |     384 B |        6.86 |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.1687 ns** | **0.0020 ns** | **0.0018 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   7.4354 ns | 0.0022 ns | 0.0019 ns |  44.09 |    0.45 |      - |         - |          NA |
| ErrorOr                    | True    |   0.3062 ns | 0.0003 ns | 0.0003 ns |   1.82 |    0.02 |      - |         - |          NA |
| FluentResults              | True    |  42.7715 ns | 0.1421 ns | 0.1329 ns | 253.61 |    2.68 | 0.0072 |     120 B |          NA |
