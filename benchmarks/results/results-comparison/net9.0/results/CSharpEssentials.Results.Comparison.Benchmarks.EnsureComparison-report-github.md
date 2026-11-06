```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **13.780 ns** | **0.1361 ns** | **0.1207 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  8.452 ns | 0.0020 ns | 0.0019 ns |  0.61 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   | 22.502 ns | 0.4019 ns | 0.3760 ns |  1.63 |    0.03 | 0.0052 |      88 B |        1.57 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.160 ns** | **0.0100 ns** | **0.0089 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  8.962 ns | 0.0164 ns | 0.0145 ns |  7.73 |    0.06 |      - |         - |          NA |
| ErrorOr                    | True    |  1.295 ns | 0.0136 ns | 0.0120 ns |  1.12 |    0.01 |      - |         - |          NA |
