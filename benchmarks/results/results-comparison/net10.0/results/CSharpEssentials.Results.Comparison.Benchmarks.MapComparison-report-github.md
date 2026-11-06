```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **0.8850 ns** | **0.0027 ns** | **0.0026 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   1.9766 ns | 0.0098 ns | 0.0092 ns |   2.23 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   0.8659 ns | 0.0045 ns | 0.0040 ns |   0.98 |    0.01 |      - |         - |          NA |
| FluentResults              | False   | 109.2260 ns | 0.5100 ns | 0.4771 ns | 123.42 |    0.63 | 0.0205 |     344 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.5791 ns** | **0.0031 ns** | **0.0026 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   1.8174 ns | 0.0066 ns | 0.0062 ns |   3.14 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |   0.6358 ns | 0.0049 ns | 0.0043 ns |   1.10 |    0.01 |      - |         - |          NA |
| FluentResults              | True    |  58.5485 ns | 0.2040 ns | 0.1808 ns | 101.11 |    0.53 | 0.0153 |     256 B |          NA |
