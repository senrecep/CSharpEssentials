```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **14.817 ns** | **0.3139 ns** | **0.3969 ns** |  **1.00** |    **0.04** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  8.411 ns | 0.0035 ns | 0.0033 ns |  0.57 |    0.02 |      - |         - |        0.00 |
| ErrorOr                    | False   | 19.726 ns | 0.0750 ns | 0.0702 ns |  1.33 |    0.04 | 0.0052 |      88 B |        1.57 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.209 ns** | **0.0095 ns** | **0.0089 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  8.987 ns | 0.0026 ns | 0.0022 ns |  7.43 |    0.05 |      - |         - |          NA |
| ErrorOr                    | True    |  1.393 ns | 0.0009 ns | 0.0007 ns |  1.15 |    0.01 |      - |         - |          NA |
