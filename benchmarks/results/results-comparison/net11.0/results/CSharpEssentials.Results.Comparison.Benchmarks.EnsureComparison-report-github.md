```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **10.647 ns** | **0.1292 ns** | **0.1208 ns** |  **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  1.802 ns | 0.0027 ns | 0.0025 ns |  0.17 |    0.00 |      - |         - |        0.00 |
| ErrorOr                    | False   | 17.045 ns | 0.1608 ns | 0.1504 ns |  1.60 |    0.02 | 0.0052 |      88 B |        1.57 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.298 ns** | **0.0118 ns** | **0.0104 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  2.348 ns | 0.0017 ns | 0.0015 ns |  1.81 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |  1.161 ns | 0.0081 ns | 0.0076 ns |  0.89 |    0.01 |      - |         - |          NA |
