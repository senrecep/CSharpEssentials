```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **24.542 ns** | **0.4338 ns** | **0.4057 ns** |  **1.00** |    **0.02** | **0.0062** |     **104 B** |        **1.00** |
| CSharpFunctionalExtensions | False   |  8.448 ns | 0.0074 ns | 0.0065 ns |  0.34 |    0.01 |      - |         - |        0.00 |
| ErrorOr                    | False   | 20.001 ns | 0.4260 ns | 0.4375 ns |  0.82 |    0.02 | 0.0052 |      88 B |        0.85 |
|                            |         |           |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **1.514 ns** | **0.0162 ns** | **0.0144 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  8.961 ns | 0.0048 ns | 0.0040 ns |  5.92 |    0.05 |      - |         - |          NA |
| ErrorOr                    | True    |  1.447 ns | 0.0007 ns | 0.0006 ns |  0.96 |    0.01 |      - |         - |          NA |
