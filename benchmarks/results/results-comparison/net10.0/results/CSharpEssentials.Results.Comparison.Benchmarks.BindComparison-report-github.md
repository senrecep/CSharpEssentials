```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.8944 ns** | **0.0057 ns** | **0.0053 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |  1.9525 ns | 0.0006 ns | 0.0005 ns |   2.18 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |  0.8771 ns | 0.0030 ns | 0.0025 ns |   0.98 |    0.01 |      - |         - |          NA |
| FluentResults              | False   | 72.7987 ns | 0.3099 ns | 0.2747 ns |  81.40 |    0.56 | 0.0124 |     208 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.5813 ns** | **0.0092 ns** | **0.0086 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  1.9860 ns | 0.0028 ns | 0.0023 ns |   3.42 |    0.05 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5817 ns | 0.0082 ns | 0.0077 ns |   1.00 |    0.02 |      - |         - |          NA |
| FluentResults              | True    | 73.3675 ns | 0.5837 ns | 0.5460 ns | 126.24 |    2.02 | 0.0191 |     320 B |          NA |
