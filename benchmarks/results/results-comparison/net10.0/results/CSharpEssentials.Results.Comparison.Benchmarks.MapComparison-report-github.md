```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **0.8875 ns** | **0.0031 ns** | **0.0029 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   1.9518 ns | 0.0005 ns | 0.0004 ns |   2.20 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   0.8752 ns | 0.0017 ns | 0.0015 ns |   0.99 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 118.3031 ns | 0.7430 ns | 0.6950 ns | 133.30 |    0.87 | 0.0205 |     344 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.5762 ns** | **0.0032 ns** | **0.0030 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   1.8994 ns | 0.0100 ns | 0.0093 ns |   3.30 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |   0.5773 ns | 0.0049 ns | 0.0043 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults              | True    |  56.8672 ns | 0.4025 ns | 0.3143 ns |  98.70 |    0.72 | 0.0153 |     256 B |          NA |
