```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.5330 ns** | **0.0087 ns** | **0.0077 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |  1.9552 ns | 0.0060 ns | 0.0050 ns |   3.67 |    0.05 |      - |         - |          NA |
| ErrorOr                    | False   |  0.7964 ns | 0.0005 ns | 0.0004 ns |   1.49 |    0.02 |      - |         - |          NA |
| FluentResults              | False   | 46.6681 ns | 0.6411 ns | 0.5997 ns |  87.58 |    1.63 | 0.0124 |     208 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.5657 ns** | **0.0050 ns** | **0.0047 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  1.8096 ns | 0.0013 ns | 0.0012 ns |   3.20 |    0.03 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5762 ns | 0.0071 ns | 0.0063 ns |   1.02 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 72.7788 ns | 0.5381 ns | 0.5034 ns | 128.65 |    1.34 | 0.0191 |     320 B |          NA |
