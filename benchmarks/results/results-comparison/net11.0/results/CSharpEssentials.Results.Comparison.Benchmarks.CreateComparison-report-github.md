```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **18.7326 ns** | **0.1902 ns** | **0.1779 ns** |  **1.00** |    **0.01** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  0.2708 ns | 0.0010 ns | 0.0009 ns |  0.01 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 16.2151 ns | 0.3311 ns | 0.3252 ns |  0.87 |    0.02 | 0.0052 |      88 B |        0.85 |
| FluentResults              | False   | 85.7119 ns | 0.9416 ns | 0.8808 ns |  4.58 |    0.06 | 0.0205 |     344 B |        3.31 |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.2888 ns** | **0.0007 ns** | **0.0006 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  0.0672 ns | 0.0042 ns | 0.0037 ns |  0.23 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |  0.2896 ns | 0.0010 ns | 0.0009 ns |  1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 20.8668 ns | 0.1040 ns | 0.0973 ns | 72.26 |    0.36 | 0.0067 |     112 B |          NA |
